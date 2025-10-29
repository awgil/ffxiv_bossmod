using BossMod.SAM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class SAM(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player, PotionType.Strength)
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
        GekkoBana,
        KashaBana
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan SAM", "Samurai", "Standard rotation (xan)|Melee", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.SAM), 100);

        def.DefineShared().AddAssociatedActions(AID.Ikishoten, AID.HissatsuSenei);

        def.Define(Track.Higanbana).As<OffensiveStrategy>("Higanbana")
            .AddOption(OffensiveStrategy.Automatic, "Refresh every 60s according to standard rotation", supportedTargets: ActionTargets.Hostile)
            .AddOption(OffensiveStrategy.Delay, "Don't apply")
            .AddOption(OffensiveStrategy.Force, "Apply to target ASAP, regardless of remaining duration", supportedTargets: ActionTargets.Hostile)
            .AddAssociatedActions(AID.Higanbana);

        def.Define(Track.Enpi).As<EnpiStrategy>("Enpi")
            .AddOption(EnpiStrategy.Enhanced, "Use if Enhanced Enpi is active")
            .AddOption(EnpiStrategy.None, "Do not use")
            .AddOption(EnpiStrategy.Ranged, "Use when out of range")
            .AddAssociatedActions(AID.Enpi);

        def.Define(Track.Meikyo).As<MeikyoStrategy>("Meikyo")
            .AddOption(MeikyoStrategy.Auto, "Use every minute or so")
            .AddOption(MeikyoStrategy.Delay, "Don't use")
            .AddOption(MeikyoStrategy.Force, "Use ASAP (unless already active)")
            .AddOption(MeikyoStrategy.HoldOne, "Only use if charges are capped")
            .AddAssociatedActions(AID.MeikyoShisui);

        def.Define(Track.Opener).As<OpenerStrategy>("Opener")
            .AddOption(OpenerStrategy.Standard, "Standard opener; Gekko (damage buff), Kasha, Midare, Higanbana, Ogi")
            .AddOption(OpenerStrategy.KashaStandard, "Standard opener, but use Kasha first for immediate haste buff")
            .AddOption(OpenerStrategy.GekkoBana, "Apply Higanbana immediately")
            .AddOption(OpenerStrategy.KashaBana, "Use Kasha, then (unbuffed) Higanbana");

        return def;
    }

    public enum IaiRepeat
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
    public (float Left, IaiRepeat Action) Tsubame;

    public bool OgiRepeat;

    public float DamageUpLeft; // damage buff, max 40s
    public float HasteLeft; // haste buff, max 40s
    public (float Left, int Stacks) Meikyo; // max 20s/3
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

    // fugetsu needs to cover end of iaijutsu cast
    private bool HaveDmg => DamageUpLeft > GCD + GetCastTime(AID.Higanbana);
    // haste doesn't really, it takes effect at start of GCD
    private bool HaveHaste => HasteLeft > GCD;

    public enum GCDPriority
    {
        None = 0,
        Enpi = 50,
        Standard = 100,
        Combo = 150,
        ComboEnd = 200,
        Iaijutsu = 700,
        Tsubame = 750,
        Ogi1 = 800,
        PreDotRefresh = 830,
        Ogi2 = 850,
        Higanbana = 900
    }

    private (float Left, IaiRepeat Action) GetTsubameAction()
    {
        var goken = StatusLeft(SID.KaeshiGoken);
        if (goken > 0)
            return (goken, IaiRepeat.Goken);
        var sets = StatusLeft(SID.KaeshiSetsugekka);
        if (sets > 0)
            return (sets, IaiRepeat.Setsugekka);
        var tgoken = StatusLeft(SID.TendoKaeshiGoken);
        if (tgoken > 0)
            return (tgoken, IaiRepeat.TendoGoken);
        var tsets = StatusLeft(SID.TendoKaeshiSetsugekka);
        if (tsets > 0)
            return (tsets, IaiRepeat.TendoSetsugekka);
        return (0, IaiRepeat.None);
    }

    // TODO: fix GCD priorities - use kaeshi as fallback action (during forced movement, etc)
    // use kaeshi goken asap in aoe?
    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 3);

        var gauge = World.Client.GetGauge<SamuraiGauge>();
        Tsubame = GetTsubameAction();
        OgiRepeat = gauge.Kaeshi == KaeshiAction.Namikiri;
        Kenki = gauge.Kenki;
        Meditation = gauge.MeditationStacks;
        Sen = gauge.SenFlags;

        DamageUpLeft = Status(SID.Fugetsu, 40).Left;
        HasteLeft = Status(SID.Fuka, 40).Left;
        Meikyo = Status(SID.MeikyoShisui);
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

        var meikyoCutoff = opener.EarlyBana() ? 11 : 14;

        if (CountdownRemaining > 0)
        {
            if (Meikyo.Left == 0 && CountdownRemaining < meikyoCutoff)
                PushGCD(AID.MeikyoShisui, Player, GCDPriority.Standard);

            if (TrueNorthLeft == 0 && Hints.PotentialTargets.Any(x => !x.Actor.Omnidirectional) && CountdownRemaining < 5)
                PushGCD(AID.TrueNorth, Player, GCDPriority.Standard);

            if (Meikyo.Left > CountdownRemaining && CountdownRemaining < 0.76f)
                PushGCD(opener.EarlyKasha() ? AID.Kasha : AID.Gekko, primaryTarget, GCDPriority.ComboEnd);

            return;
        }

        EmergencyMeikyo(strategy, primaryTarget);
        UseTsubame(primaryTarget);
        UseIaijutsu(strategy, primaryTarget);

        if (OgiLeft > GCD && CanFitGCD(TargetDotLeft, 1) && HaveDmg && HaveHaste && (RaidBuffsLeft > GCD || RaidBuffsIn > 1000))
        {
            // technically the remaining duration we need is ((1 + stacks) * GCD) + (application delay for next GCD) but that's at the mercy of network latency
            if (Meikyo.Left == 0 || CanFitGCD(Meikyo.Left, 2 + Meikyo.Stacks))
                PushGCD(AID.OgiNamikiri, BestOgiTarget, GCDPriority.Ogi1);
        }

        if (Meikyo.Left > GCD)
            PushGCD(GetMeikyoAction(strategy), NumAOECircleTargets > 2 ? null : primaryTarget, GCDPriority.Standard);

        if (ComboLastMove == AOEStarter && NumAOECircleTargets > 0)
        {
            if (DamageUpLeft <= HasteLeft)
                PushGCD(AID.Mangetsu, Player, GCDPriority.Standard);
            if (HasteLeft <= DamageUpLeft)
                PushGCD(AID.Oka, Player, GCDPriority.Standard);
        }

        var comboEndPrio = CanFitGCD(TargetDotLeft, 2) ? GCDPriority.Standard : GCDPriority.PreDotRefresh;

        if (ComboLastMove == AID.Jinpu)
            PushGCD(AID.Gekko, primaryTarget, comboEndPrio);
        if (ComboLastMove == AID.Shifu)
            PushGCD(AID.Kasha, primaryTarget, comboEndPrio);

        if (ComboLastMove == STStarter)
        {
            var act = GetHakazeComboAction(strategy);
            PushGCD(act, primaryTarget, act == AID.Yukikaze ? comboEndPrio : GCDPriority.Standard);
        }

        // note that this is intentionally checking for number of "nearby" targets even if our AOE starter is a cone AOE
        if (NumAOECircleTargets > 2 && Unlocked(AID.Fuga))
            PushGCD(AOEStarter, BestAOETarget, GCDPriority.Standard);
        else
            PushGCD(AID.Hakaze, primaryTarget, GCDPriority.Standard);

        var enpiprio = strategy.Option(Track.Enpi).As<EnpiStrategy>() switch
        {
            EnpiStrategy.Enhanced => EnhancedEnpi > GCD ? GCDPriority.Enpi : GCDPriority.None,
            EnpiStrategy.Ranged => GCDPriority.Enpi,
            _ => GCDPriority.None,
        };

        PushGCD(AID.Enpi, primaryTarget, enpiprio);

        var pos = GetNextPositional(strategy);
        UpdatePositionals(primaryTarget, ref pos);

        OGCD(strategy, primaryTarget);

        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(NumStickers == 2 ? 8 : 5), AID.Fuga, 3, 20);
    }

    private AID GetHakazeComboAction(StrategyValues strategy)
    {
        if (Unlocked(AID.Jinpu) && !CanFitGCD(DamageUpLeft, 2))
            return AID.Jinpu;

        if (Unlocked(AID.Shifu) && !CanFitGCD(HasteLeft, 2))
            return AID.Shifu;

        // TODO: pick a longer combo route if we need to delay at least 4 more GCDs for bana refresh
        if (Unlocked(AID.Yukikaze) && !Ice)
            return AID.Yukikaze;

        // TODO select the more convenient GCD based on closest positional
        if (Unlocked(AID.Shifu) && !Flower)
            return AID.Shifu;

        if (Unlocked(AID.Jinpu) && !Moon)
            return AID.Jinpu;

        // fallback if we are full on sen but can't use midare bc of movement restrictions or w/e
        return Unlocked(AID.Yukikaze) ? AID.Yukikaze : AID.None;
    }

    private AID GetMeikyoAction(StrategyValues strategy)
    {
        var opener = strategy.Option(Track.Opener).As<OpenerStrategy>();

        if (CombatTimer < 10 && DamageUpLeft == 0 && HasteLeft == 0 && opener.EarlyKasha())
            return AID.Kasha;

        if (NumAOECircleTargets > 2)
        {
            // priority 0: damage buff
            if (DamageUpLeft == 0)
                return AID.Mangetsu;

            return (Moon, Flower) switch
            {
                // refresh buff running out first
                (false, false) => DamageUpLeft <= HasteLeft ? AID.Mangetsu : AID.Oka,
                (true, false) => AID.Oka,
                _ => AID.Mangetsu,
            };
        }
        else
        {
            // priority 0: damage buff
            if (!CanFitGCD(DamageUpLeft, 1))
                return AID.Gekko;

            if (!CanFitGCD(HasteLeft, 1))
                return AID.Kasha;

            if (!Flower)
                return AID.Kasha;

            if (!Moon)
                return AID.Gekko;

            return AID.Yukikaze;
        }
    }

    private void UseTsubame(Enemy? primaryTarget)
    {
        // namikiri combo is broken by all GCDs EXCEPT for non-tsubame iaijutsu, meaning we can use e.g. ogi 1 -> bana -> ogi 2 for alignment
        // TODO rotation does not currently do this
        if (OgiRepeat)
            PushGCD(AID.KaeshiNamikiri, BestOgiTarget, GCDPriority.Ogi2);

        var (aid, target) = TsubameAction(primaryTarget, Tsubame.Action);
        if (aid == default)
            return;

        if (RaidBuffsLeft > GCD
            || RaidBuffsIn > 1000
            || !CanFitGCD(Tsubame.Left, 1)
            || PotionLeft > GCD && !CanFitGCD(PotionLeft, 1))
            PushGCD(aid, target, GCDPriority.Tsubame);
    }

    private (AID, Enemy?) TsubameAction(Enemy? primaryTarget, IaiRepeat k) => k switch
    {
        IaiRepeat.Setsugekka => (AID.KaeshiSetsugekka, primaryTarget),
        IaiRepeat.TendoSetsugekka => (AID.TendoKaeshiSetsugekka, primaryTarget),
        IaiRepeat.Goken => (AID.KaeshiGoken, null),
        IaiRepeat.TendoGoken => (AID.TendoKaeshiGoken, null),
        _ => (default, null)
    };

    private void UseIaijutsu(StrategyValues strategy, Enemy? primaryTarget)
    {
        var opener = strategy.Option(Track.Opener).As<OpenerStrategy>();

        if (NumStickers == 1
            && !CanFitGCD(TargetDotLeft, 1) // dot expiring
            && (
                HaveHaste && HaveDmg // standard buffs
                || opener.EarlyBana() && CombatTimer < 10 // forced early bana
            ))
            PushGCD(AID.Higanbana, BestDotTarget, GCDPriority.Higanbana);

        // we don't cast any other iaijutsu without having fugetsu up first, since it takes max 2 GCDs to apply
        if (!HaveDmg)
            return;

        void tsubame()
        {
            var (a, k) = TsubameAction(primaryTarget, Tsubame.Action);
            if (a != default)
                PushGCD(a, k, GCDPriority.Tsubame);
        }

        var needAOETargets = Tendo > GCD ? 4 : 3;
        if (NumStickers == 2 && NumTenkaTargets >= needAOETargets)
        {
            tsubame();
            PushGCD(Tendo > GCD ? AID.TendoGoken : AID.TenkaGoken, Player, GCDPriority.Iaijutsu);
        }

        if (NumStickers == 3)
        {
            tsubame();
            PushGCD(Tendo > GCD ? AID.TendoSetsugekka : AID.MidareSetsugekka, primaryTarget, GCDPriority.Iaijutsu);
        }
    }

    private void EmergencyMeikyo(StrategyValues strategy, Enemy? primaryTarget)
    {
        // special case for if we got thrust into combat with no prep
        if (Meikyo.Left == 0 && !HaveDmg && CombatTimer < 5 && primaryTarget != null)
            PushGCD(AID.MeikyoShisui, Player, GCDPriority.Higanbana);
    }

    private (Positional, bool) GetNextPositional(StrategyValues strategy)
    {
        if (NumAOETargets > 2 || !Unlocked(AID.Gekko))
            return (Positional.Any, false);

        switch (NextGCD)
        {
            case AID.Gekko:
                return (Positional.Rear, true);
            case AID.Kasha:
                return (Positional.Flank, true);
            case AID.Jinpu:
                return (Positional.Rear, false);
            case AID.Shifu:
                return (Positional.Flank, false);
            default:
                if (Unlocked(AID.Kasha) && !Flower)
                    return (Positional.Flank, false);
                if (Unlocked(AID.Gekko) && !Moon)
                    return (Positional.Rear, false);
                return (Positional.Any, false);
        }
    }

    private void OGCD(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (primaryTarget == null || !HaveDmg || !Player.InCombat)
            return;

        // most important ogcd for alignment
        UseMeikyo(strategy);

        // accidentally overcapping shoha will probably cause the second one to fall out of buffs, resulting in huge potency loss
        if (Meditation == 3 && (RaidBuffsLeft > AnimLock || GrantsMeditation(NextGCD)))
            PushOGCD(AID.Shoha, BestLineTarget);

        if (strategy.BuffsOk() && Kenki <= 50)
            PushOGCD(AID.Ikishoten, Player);

        if (Kenki >= 25 && (RaidBuffsLeft > AnimLock || RaidBuffsIn > (Unlocked(TraitID.EnhancedHissatsu) ? 40 : 100)))
        {
            if (NumLineTargets > 2)
                PushOGCD(AID.HissatsuGuren, BestLineTarget);

            // queue senei since guren may not be unlocked (gated by job quest)
            PushOGCD(AID.HissatsuSenei, primaryTarget);

            // queue guren since senei may not be unlocked (unlocks at level 72)
            if (!Unlocked(AID.HissatsuSenei))
                PushOGCD(AID.HissatsuGuren, BestLineTarget);
        }

        if (Kenki >= 50 && Zanshin > 0 && ReadyIn(AID.HissatsuSenei) > 30)
            PushOGCD(AID.Zanshin, BestOgiTarget);

        var saveKenki = RaidBuffsLeft <= AnimLock && RaidBuffsIn < 1000 || Zanshin > 0 || ReadyIn(AID.HissatsuSenei) < 10;

        if (Kenki >= (saveKenki ? 80 : 25))
        {
            if (NumAOECircleTargets > 2)
                PushOGCD(AID.HissatsuKyuten, Player);

            PushOGCD(AID.HissatsuShinten, primaryTarget);
        }

        if (NextPositionalImminent && !NextPositionalCorrect)
            PushOGCD(AID.TrueNorth, Player, -10, GCD - 0.8f);
    }

    private bool GrantsMeditation(AID aid) => aid is AID.MidareSetsugekka or AID.TenkaGoken or AID.Higanbana or AID.TendoSetsugekka or AID.TendoGoken or AID.OgiNamikiri;

    private void UseMeikyo(StrategyValues strategy)
    {
        if (Meikyo.Left > GCD)
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

internal static class StrategyExt
{
    public static bool EarlyBana(this SAM.OpenerStrategy strat) => strat is SAM.OpenerStrategy.GekkoBana or SAM.OpenerStrategy.KashaBana;
    public static bool EarlyKasha(this SAM.OpenerStrategy strat) => strat is SAM.OpenerStrategy.KashaBana or SAM.OpenerStrategy.KashaStandard;
}
