using BossMod.SAM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public sealed class SAM(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public enum Track { Higanbana = SharedTrack.Count, Enpi }

    public enum EnpiStrategy
    {
        Enhanced,
        None,
        Ranged
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan SAM", "Samurai", "Standard rotation (xan)|Melee", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.SAM), 100);

        def.DefineShared().AddAssociatedActions(AID.Ikishoten, AID.HissatsuSenei);

        def.Define(Track.Higanbana).As<OffensiveStrategy>("Higanbana")
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Keep Higanbana uptime against 1 or 2 targets")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Do not apply Higanbana")
            .AddOption(OffensiveStrategy.Force, "Force", "Always apply Higanbana to target");

        def.Define(Track.Enpi).As<EnpiStrategy>("Enpi")
            .AddOption(EnpiStrategy.Enhanced, "Enhanced", "Use if Enhanced Enpi is active")
            .AddOption(EnpiStrategy.None, "None", "Do not use")
            .AddOption(EnpiStrategy.Ranged, "Ranged", "Use when out of range");

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

    private Actor? BestAOETarget; // null if fuko is unlocked since it's self-targeted
    private Actor? BestLineTarget;
    private Actor? BestOgiTarget;
    private Actor? BestDotTarget;

    private float TargetDotLeft;

    protected override float GetCastTime(AID aid)
    {
        var c = base.GetCastTime(aid);
        // iaijutsu are actually affected by haste wtf?
        return Unlocked(TraitID.EnhancedIaijutsu) ? c / 1.8f * 1.3f : c;
    }

    private int NumStickers => (Ice ? 1 : 0) + (Moon ? 1 : 0) + (Flower ? 1 : 0);

    private bool Ice => Sen.HasFlag(SenFlags.Setsu);
    private bool Moon => Sen.HasFlag(SenFlags.Getsu);
    private bool Flower => Sen.HasFlag(SenFlags.Ka);

    private bool HaveFugetsu => FugetsuLeft > GCD + GetCastTime(AID.Higanbana);

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
    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
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

        (BestDotTarget, TargetDotLeft) = SelectDotTarget(strategy, primaryTarget, HiganbanaLeft, 2);

        switch (strategy.Option(Track.Higanbana).As<OffensiveStrategy>())
        {
            case OffensiveStrategy.Delay:
                TargetDotLeft = float.MaxValue;
                break;
            case OffensiveStrategy.Force:
                TargetDotLeft = 0;
                break;
        }

        var pos = GetNextPositional(strategy);
        UpdatePositionals(primaryTarget, ref pos, TrueNorthLeft > GCD);

        OGCD(strategy, primaryTarget);

        if (CountdownRemaining > 0)
        {
            if (MeikyoLeft == 0 && CountdownRemaining < 14)
                PushGCD(AID.MeikyoShisui, Player);

            if (TrueNorthLeft == 0 && Hints.PotentialTargets.Any(x => !x.Actor.Omnidirectional) && CountdownRemaining < 5)
                PushGCD(AID.TrueNorth, Player);

            return;
        }

        GoalZoneCombined(3, Hints.GoalAOECircle(NumStickers == 2 ? 8 : 5), 3, pos.Item1);

        EmergencyMeikyo(strategy, primaryTarget);
        UseKaeshi(primaryTarget);
        UseIaijutsu(primaryTarget);

        if (OgiLeft > GCD && TargetDotLeft > 10 && HaveFugetsu)
            PushGCD(AID.OgiNamikiri, BestOgiTarget);

        if (MeikyoLeft > GCD)
            PushGCD(MeikyoAction, NumAOECircleTargets > 2 ? Player : primaryTarget);

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

    private AID MeikyoAction
    {
        get
        {
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
    }

    private void UseKaeshi(Actor? primaryTarget)
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

    private (AID, Actor?) KaeshiToAID(Actor? primaryTarget, Kaeshi k) => k switch
    {
        Kaeshi.Setsugekka => (AID.KaeshiSetsugekka, primaryTarget),
        Kaeshi.TendoSetsugekka => (AID.TendoKaeshiSetsugekka, primaryTarget),
        Kaeshi.Goken => (AID.KaeshiGoken, Player),
        Kaeshi.TendoGoken => (AID.TendoKaeshiGoken, Player),
        _ => (default, null)
    };

    private void UseIaijutsu(Actor? primaryTarget)
    {
        if (!HaveFugetsu || NumStickers == 0)
            return;

        if (NumStickers == 1 && TargetDotLeft < 10 && FukaLeft > 0)
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

    private void EmergencyMeikyo(StrategyValues strategy, Actor? primaryTarget)
    {
        // special case for if we got thrust into combat with no prep
        if (MeikyoLeft == 0 && !HaveFugetsu && CombatTimer < 5 && primaryTarget != null)
            PushGCD(AID.MeikyoShisui, Player);
    }

    private (Positional, bool) GetNextPositional(StrategyValues strategy)
    {
        if (NumAOETargets > 2)
            return (Positional.Any, false);

        if (MeikyoLeft > GCD)
            return MeikyoAction switch
            {
                AID.Gekko => (Positional.Rear, true),
                AID.Kasha => (Positional.Flank, true),
                _ => (Positional.Any, false)
            };

        if (ComboLastMove == AID.Jinpu)
            return (Positional.Rear, true);

        if (ComboLastMove == AID.Shifu)
            return (Positional.Flank, true);

        if (ComboLastMove == AID.Hakaze)
        {
            var pos = GetHakazeComboAction(strategy) switch
            {
                AID.Jinpu => Positional.Rear,
                AID.Shifu => Positional.Flank,
                _ => Positional.Any
            };
            return (pos, false);
        }

        return (Positional.Any, false);
    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (primaryTarget == null || !HaveFugetsu)
            return;

        if (strategy.BuffsOk())
        {
            PushOGCD(AID.Ikishoten, Player);

            if (Zanshin > World.Client.AnimationLock && Kenki >= 50)
                PushOGCD(AID.Zanshin, BestOgiTarget);

            if (Kenki >= 25 && Zanshin == 0)
            {
                if (NumLineTargets > 1)
                    PushOGCD(AID.HissatsuGuren, BestLineTarget);

                // queue senei since guren may not be unlocked (gated by job quest)
                PushOGCD(AID.HissatsuSenei, primaryTarget);
                // queue guren since senei may not be unlocked (unlocks at level 72)
                PushOGCD(AID.HissatsuGuren, BestLineTarget);
            }
        }

        if (Meditation == 3)
            PushOGCD(AID.Shoha, BestLineTarget);

        if (Kenki >= 25 && ReadyIn(AID.HissatsuGuren) > 10 && Zanshin == 0)
        {
            if (NumAOECircleTargets > 2)
                PushOGCD(AID.HissatsuKyuten, Player);

            PushOGCD(AID.HissatsuShinten, primaryTarget);
        }

        Meikyo(strategy);
    }

    private void Meikyo(StrategyValues strategy)
    {
        if (ComboLastMove is AID.Jinpu or AID.Shifu or AID.Hakaze or AID.Gyofu or AID.Fuga or AID.Fuko)
            return;

        // TODO: DT requires early meikyo in even windows, resulting in double meikyo at 6m
        if (MeikyoLeft == 0 && Tendo == 0 && (CanWeave(MaxChargesIn(AID.MeikyoShisui), 0.6f) || CanFitGCD(RaidBuffsLeft, 3)))
            PushOGCD(AID.MeikyoShisui, Player);
    }

    private float HiganbanaLeft(Actor? p) => p == null ? float.MaxValue : StatusDetails(p, SID.Higanbana, Player.InstanceID).Left;

    private bool InConeAOE(Actor primary, Actor other) => Hints.TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 60.Degrees());
    private bool InLineAOE(Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 4);
}
