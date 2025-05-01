using BossMod.SAM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class SAM(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public enum Track { Higanbana = SharedTrack.Count, Enpi, Meikyo, Opener }

    public enum EnpiStrategy
    {
        Enhanced,
        None,
        Ranged
    }

    public enum MeikyoStrategy
    {
        Auto,
        Delay,
        Force,
        HoldOne
    }

    public enum OpenerStrategy
    {
        Standard,
        KashaStandard,
        GekkoBana
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan SAM", "Samurai", "Standard rotation (xan)|Melee", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.SAM), 100);

        def.DefineShared().AddAssociatedActions(AID.Ikishoten, AID.HissatsuSenei);

        def.Define(Track.Higanbana).As<OffensiveStrategy>("Higanbana")
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Refresh every 60s according to standard rotation", supportedTargets: ActionTargets.Hostile)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Don't apply")
            .AddOption(OffensiveStrategy.Force, "Force", "Apply to target ASAP, regardless of remaining duration", supportedTargets: ActionTargets.Hostile);

        def.Define(Track.Enpi).As<EnpiStrategy>("Enpi")
            .AddOption(EnpiStrategy.Enhanced, "Enhanced", "Use if Enhanced Enpi is active")
            .AddOption(EnpiStrategy.None, "None", "Do not use")
            .AddOption(EnpiStrategy.Ranged, "Ranged", "Use when out of range");

        def.Define(Track.Meikyo).As<MeikyoStrategy>("Meikyo")
            .AddOption(MeikyoStrategy.Auto, "Auto", "Use every minute or so")
            .AddOption(MeikyoStrategy.Delay, "Delay", "Don't use")
            .AddOption(MeikyoStrategy.Force, "Force", "Use ASAP (unless already active)")
            .AddOption(MeikyoStrategy.HoldOne, "HoldOne", "Only use if charges are capped");

        def.Define(Track.Opener).As<OpenerStrategy>("Opener")
            .AddOption(OpenerStrategy.Standard, "Standard", "Standard opener; Gekko (damage buff), Kasha, Midare, Higanbana, Ogi")
            .AddOption(OpenerStrategy.KashaStandard, "KashaStandard", "Standard opener, but use Kasha first for immediate haste buff")
            .AddOption(OpenerStrategy.GekkoBana, "GekkoBana", "Apply Higanbana immediately");

        return def;
    }

    public enum Kaeshi
    {
        None,
        Goken,
        Setsugekka,
        TendoGoken,
        TendoSetsugekka
    }

    public byte Kenki;
    public byte Meditation;
    public SenFlags Sen;
    public (float Left, Kaeshi Action) KaeshiAction;

    public bool KaeshiNamikiri;

    public float FugetsuLeft; // damage buff, max 40s
    public float FukaLeft; // haste buff, max 40s
    public float MeikyoLeft; // max 20s
    public float OgiLeft; // max 30s
    public float TsubameLeft; // max 30s
    public float EnhancedEnpi; // max 15s
    public float Zanshin; // max 30s
    public float Tendo; // max 30s

    public int NumAOECircleTargets; // 5y circle around self, but if fuko isn't unlocked, then...
    public int NumAOETargets; // 8y/120deg cone if we don't have fuko
    public int NumTenkaTargets; // 8y circle instead of 5
    public int NumLineTargets; // shoha+guren
    public int NumOgiTargets; // 8y/120deg cone

    public AID AOEStarter => Unlocked(AID.Fuko) ? AID.Fuko : AID.Fuga;
    public AID STStarter => Unlocked(AID.Gyofu) ? AID.Gyofu : AID.Hakaze;

    private Enemy? BestAOETarget; // null if fuko is unlocked since it's self-targeted
    private Enemy? BestLineTarget;
    private Enemy? BestOgiTarget;
    private Enemy? BestDotTarget;

    private float TargetDotLeft;

    protected override float GetCastTime(AID aid)
    {
        var c = base.GetCastTime(aid);
        return c > 0 && Unlocked(TraitID.EnhancedIaijutsu) ? 1.3f : c;
    }

    private int NumStickers => (Ice ? 1 : 0) + (Moon ? 1 : 0) + (Flower ? 1 : 0);

    private bool Ice => Sen.HasFlag(SenFlags.Setsu);
    private bool Moon => Sen.HasFlag(SenFlags.Getsu);
    private bool Flower => Sen.HasFlag(SenFlags.Ka);

    private bool HaveFugetsu => FugetsuLeft > GCD + GetCastTime(AID.Higanbana);
    private bool HaveFuka => FukaLeft > GCD;

    private (float Left, Kaeshi Action) GetKaeshiAction()
    {
        var goken = StatusLeft(SID.KaeshiGoken);
        if (goken > 0)
            return (goken, Kaeshi.Goken);
        var sets = StatusLeft(SID.KaeshiSetsugekka);
        if (sets > 0)
            return (sets, Kaeshi.Setsugekka);
        var tgoken = StatusLeft(SID.TendoKaeshiGoken);
        if (tgoken > 0)
            return (tgoken, Kaeshi.TendoGoken);
        var tsets = StatusLeft(SID.TendoKaeshiSetsugekka);
        if (tsets > 0)
            return (tsets, Kaeshi.TendoSetsugekka);
        return (0, Kaeshi.None);
    }

    // TODO: fix GCD priorities - use kaeshi as fallback action (during forced movement, etc)
    // use kaeshi goken asap in aoe? we usually arent holding for buffs with 3 targets
    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 3);

        var gauge = World.Client.GetGauge<SamuraiGauge>();
        KaeshiAction = GetKaeshiAction();
        // other kaeshi are distinguished by status ID now
        KaeshiNamikiri = gauge.Kaeshi == FFXIVClientStructs.FFXIV.Client.Game.Gauge.KaeshiAction.Namikiri;
        Kenki = gauge.Kenki;
        Meditation = gauge.MeditationStacks;
        Sen = gauge.SenFlags;

        FugetsuLeft = StatusLeft(SID.Fugetsu);
        FukaLeft = StatusLeft(SID.Fuka);
        MeikyoLeft = StatusLeft(SID.MeikyoShisui);
        OgiLeft = StatusLeft(SID.OgiNamikiriReady);
        EnhancedEnpi = StatusLeft(SID.EnhancedEnpi);
        Zanshin = StatusLeft(SID.ZanshinReady);
        Tendo = StatusLeft(SID.Tendo);

        (BestOgiTarget, NumOgiTargets) = SelectTarget(strategy, primaryTarget, 8, InConeAOE);

        NumAOECircleTargets = NumMeleeAOETargets(strategy);
        if (Unlocked(AID.Fuko))
            (BestAOETarget, NumAOETargets) = (null, NumAOECircleTargets);
        else
            (BestAOETarget, NumAOETargets) = (BestOgiTarget, NumOgiTargets);

        NumTenkaTargets = NumNearbyTargets(strategy, 8);
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 10, InLineAOE);

        var dotTarget = Hints.FindEnemy(ResolveTargetOverride(strategy.Option(Track.Higanbana).Value)) ?? primaryTarget;

        (BestDotTarget, TargetDotLeft) = SelectDotTarget(strategy, dotTarget, HiganbanaLeft, 2);

        switch (strategy.Option(Track.Higanbana).As<OffensiveStrategy>())
        {
            case OffensiveStrategy.Delay:
                TargetDotLeft = float.MaxValue;
                break;
            case OffensiveStrategy.Force:
                TargetDotLeft = 0;
                break;
        }

        var opener = strategy.Option(Track.Opener).As<OpenerStrategy>();

        var meikyoCutoff = opener == OpenerStrategy.GekkoBana ? 11 : 14;

        if (CountdownRemaining > 0)
        {
            if (MeikyoLeft == 0 && CountdownRemaining < meikyoCutoff)
                PushGCD(AID.MeikyoShisui, Player);

            if (TrueNorthLeft == 0 && Hints.PotentialTargets.Any(x => !x.Actor.Omnidirectional) && CountdownRemaining < 5)
                PushGCD(AID.TrueNorth, Player);

            if (MeikyoLeft > CountdownRemaining && CountdownRemaining < 0.76f)
                PushGCD(opener == OpenerStrategy.KashaStandard ? AID.Kasha : AID.Gekko, primaryTarget);

            return;
        }

        EmergencyMeikyo(strategy, primaryTarget);
        UseKaeshi(primaryTarget);
        UseIaijutsu(strategy, primaryTarget);

        if (OgiLeft > GCD && TargetDotLeft > 10 && HaveFugetsu && HaveFuka && (RaidBuffsLeft > GCD || RaidBuffsIn > 1000))
            PushGCD(AID.OgiNamikiri, BestOgiTarget);

        if (MeikyoLeft > GCD)
            PushGCD(GetMeikyoAction(strategy), NumAOECircleTargets > 2 ? null : primaryTarget);

        if (ComboLastMove == AOEStarter && NumAOECircleTargets > 0)
        {
            if (FugetsuLeft <= FukaLeft)
                PushGCD(AID.Mangetsu, Player);
            if (FukaLeft <= FugetsuLeft)
                PushGCD(AID.Oka, Player);
        }

        if (ComboLastMove == AID.Jinpu)
            PushGCD(AID.Gekko, primaryTarget);
        if (ComboLastMove == AID.Shifu)
            PushGCD(AID.Kasha, primaryTarget);

        if (ComboLastMove == STStarter)
            PushGCD(GetHakazeComboAction(strategy), primaryTarget);

        // note that this is intentionally checking for number of "nearby" targets even if our AOE starter is a cone AOE
        if (NumAOECircleTargets > 2 && Unlocked(AID.Fuga))
            PushGCD(AOEStarter, BestAOETarget);
        else
            PushGCD(AID.Hakaze, primaryTarget);

        var enpiprio = strategy.Option(Track.Enpi).As<EnpiStrategy>() switch
        {
            EnpiStrategy.Enhanced => EnhancedEnpi > GCD ? 2 : 0,
            EnpiStrategy.Ranged => 2,
            _ => 0,
        };

        PushGCD(AID.Enpi, primaryTarget, enpiprio);

        var pos = GetNextPositional(strategy);
        UpdatePositionals(primaryTarget, ref pos);

        OGCD(strategy, primaryTarget);

        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(NumStickers == 2 ? 8 : 5), AID.Fuga, 3, 20);
    }

    private AID GetHakazeComboAction(StrategyValues strategy)
    {
        if (Unlocked(AID.Jinpu) && !CanFitGCD(FugetsuLeft, 2))
            return AID.Jinpu;

        if (Unlocked(AID.Shifu) && !CanFitGCD(FukaLeft, 2))
            return AID.Shifu;

        if (Unlocked(AID.Yukikaze) && !Ice)
            return AID.Yukikaze;

        if (Unlocked(AID.Shifu) && !Flower && FugetsuLeft > FukaLeft)
            return AID.Shifu;

        if (Unlocked(AID.Jinpu) && !Moon)
            return AID.Jinpu;

        // fallback if we are full on sen but can't use midare bc of movement restrictions or w/e
        return Unlocked(AID.Yukikaze) ? AID.Yukikaze : AID.None;
    }

    private AID GetMeikyoAction(StrategyValues strategy)
    {
        var opener = strategy.Option(Track.Opener).As<OpenerStrategy>();

        if (CombatTimer < 10 && FugetsuLeft == 0 && FukaLeft == 0 && opener == OpenerStrategy.KashaStandard)
            return AID.Kasha;

        if (NumAOECircleTargets > 2)
        {
            // priority 0: damage buff
            if (FugetsuLeft == 0)
                return AID.Mangetsu;

            return (Moon, Flower) switch
            {
                // refresh buff running out first
                (false, false) => FugetsuLeft <= FukaLeft ? AID.Mangetsu : AID.Oka,
                (true, false) => AID.Oka,
                _ => AID.Mangetsu,
            };
        }
        else
        {
            // priority 0: damage buff
            if (FugetsuLeft == 0)
                return AID.Gekko;

            return (Moon, Flower) switch
            {
                // refresh buff running out first
                (false, false) => FugetsuLeft <= FukaLeft ? AID.Gekko : AID.Kasha,
                (false, true) => AID.Gekko,
                (true, false) => AID.Kasha,
                // only use yukikaze to get sen, as it's the weakest ender
                _ => Ice ? AID.Gekko : AID.Yukikaze,
            };
        }
    }

    private void UseKaeshi(Enemy? primaryTarget)
    {
        // namikiri combo is broken by other gcds, other followups are not
        if (KaeshiNamikiri)
            PushGCD(AID.KaeshiNamikiri, BestOgiTarget);

        var (aid, target) = KaeshiToAID(primaryTarget, KaeshiAction.Action);
        if (aid == default)
            return;

        if (RaidBuffsLeft > GCD || !CanFitGCD(KaeshiAction.Left, 1))
            PushGCD(aid, target);
    }

    private (AID, Enemy?) KaeshiToAID(Enemy? primaryTarget, Kaeshi k) => k switch
    {
        Kaeshi.Setsugekka => (AID.KaeshiSetsugekka, primaryTarget),
        Kaeshi.TendoSetsugekka => (AID.TendoKaeshiSetsugekka, primaryTarget),
        Kaeshi.Goken => (AID.KaeshiGoken, null),
        Kaeshi.TendoGoken => (AID.TendoKaeshiGoken, null),
        _ => (default, null)
    };

    private void UseIaijutsu(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (!HaveFugetsu || NumStickers == 0)
            return;

        var opener = strategy.Option(Track.Opener).As<OpenerStrategy>();

        if (NumStickers == 1 && !CanFitGCD(TargetDotLeft, 1) && (FukaLeft > 0 || opener == OpenerStrategy.GekkoBana && CombatTimer < 10))
            PushGCD(AID.Higanbana, BestDotTarget);

        void kaeshi()
        {
            var (a, k) = KaeshiToAID(primaryTarget, KaeshiAction.Action);
            if (a == default)
                return;

            PushGCD(a, k);
        }

        if (NumStickers == 2 && NumTenkaTargets > 2)
        {
            kaeshi();
            PushGCD(Tendo > GCD ? AID.TendoGoken : AID.TenkaGoken, Player);
        }

        if (NumStickers == 3)
        {
            kaeshi();
            PushGCD(Tendo > GCD ? AID.TendoSetsugekka : AID.MidareSetsugekka, primaryTarget);
        }
    }

    private void EmergencyMeikyo(StrategyValues strategy, Enemy? primaryTarget)
    {
        // special case for if we got thrust into combat with no prep
        if (MeikyoLeft == 0 && !HaveFugetsu && CombatTimer < 5 && primaryTarget != null)
            PushGCD(AID.MeikyoShisui, Player);
    }

    private (Positional, bool) GetNextPositional(StrategyValues strategy)
    {
        if (NumAOETargets > 2 || !Unlocked(AID.Gekko))
            return (Positional.Any, false);

        if (NextGCD == AID.Gekko)
            return (Positional.Rear, true);
        else if (NextGCD == AID.Kasha)
            return (Positional.Flank, true);
        else if (FugetsuLeft <= FukaLeft)
            return (Positional.Rear, false);
        else if (Unlocked(AID.Kasha))
            return (Positional.Flank, false);

        return (Positional.Any, false);
    }

    private void OGCD(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (primaryTarget == null || !HaveFugetsu || !Player.InCombat)
            return;

        if (strategy.BuffsOk() && Kenki <= 50)
            PushOGCD(AID.Ikishoten, Player);

        Meikyo(strategy);

        if (Kenki >= 25 && (RaidBuffsLeft > AnimLock || RaidBuffsIn > (Unlocked(TraitID.EnhancedHissatsu) ? 40 : 100)))
        {
            if (NumLineTargets > 1)
                PushOGCD(AID.HissatsuGuren, BestLineTarget);

            // queue senei since guren may not be unlocked (gated by job quest)
            PushOGCD(AID.HissatsuSenei, primaryTarget);

            // queue guren since senei may not be unlocked (unlocks at level 72)
            if (!Unlocked(AID.HissatsuSenei))
                PushOGCD(AID.HissatsuGuren, BestLineTarget);
        }

        if (Kenki >= 50 && Zanshin > 0 && ReadyIn(AID.HissatsuSenei) > 30)
            PushOGCD(AID.Zanshin, BestOgiTarget);

        if (Meditation == 3 && (RaidBuffsLeft > AnimLock || GrantsMeditation(NextGCD)))
            PushOGCD(AID.Shoha, BestLineTarget);

        var saveKenki = RaidBuffsLeft <= AnimLock && RaidBuffsIn < 1000 || Zanshin > 0 || ReadyIn(AID.HissatsuSenei) < 10;

        if (Kenki >= (saveKenki ? 90 : 25))
        {
            if (NumAOECircleTargets > 2)
                PushOGCD(AID.HissatsuKyuten, Player);

            PushOGCD(AID.HissatsuShinten, primaryTarget);
        }

        if (NextPositionalImminent && !NextPositionalCorrect)
            PushOGCD(AID.TrueNorth, Player, -10, GCD - 0.8f);
    }

    private bool GrantsMeditation(AID aid) => aid is AID.MidareSetsugekka or AID.TenkaGoken or AID.Higanbana or AID.TendoSetsugekka or AID.TendoGoken or AID.OgiNamikiri;

    private void Meikyo(StrategyValues strategy)
    {
        if (MeikyoLeft > GCD)
            return;

        var midCombo = ComboLastMove is AID.Jinpu or AID.Shifu or AID.Hakaze or AID.Gyofu or AID.Fuga or AID.Fuko;

        var use = strategy.Option(Track.Meikyo).As<MeikyoStrategy>() switch
        {
            MeikyoStrategy.Auto => !midCombo && Tendo == 0 && (CanWeave(MaxChargesIn(AID.MeikyoShisui), 0.6f) || CanFitGCD(RaidBuffsLeft, 3)),
            MeikyoStrategy.HoldOne => !midCombo && Tendo == 0 && CanWeave(MaxChargesIn(AID.MeikyoShisui), 0.6f),
            MeikyoStrategy.Force => true,
            _ => false
        };

        if (use)
            PushOGCD(AID.MeikyoShisui, Player);
    }

    private float HiganbanaLeft(Actor? p) => p == null ? float.MaxValue : StatusDetails(p, SID.Higanbana, Player.InstanceID).Left;

    private bool InConeAOE(Actor primary, Actor other) => Hints.TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 60.Degrees());
    private bool InLineAOE(Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 4);
}
