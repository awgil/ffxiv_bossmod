using BossMod.SAM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public sealed class SAM(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public enum Track { Higanbana = SharedTrack.Count }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan SAM", "Samurai", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.SAM), 100);

        def.DefineShared().AddAssociatedActions(AID.Ikishoten, AID.HissatsuSenei);

        def.Define(Track.Higanbana).As<OffensiveStrategy>("Higanbana")
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Keep Higanbana uptime against 1 or 2 targets")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Do not apply Higanbana")
            .AddOption(OffensiveStrategy.Force, "Force", "Always apply Higanbana to target");

        return def;
    }

    public KaeshiAction Kaeshi;
    public byte Kenki;
    public byte Meditation;
    public SenFlags Sen;

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

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 3);

        var gauge = World.Client.GetGauge<SamuraiGauge>();
        Kaeshi = gauge.Kaeshi;
        Kenki = gauge.Kenki;
        Meditation = gauge.MeditationStacks;
        Sen = gauge.SenFlags;

        FugetsuLeft = StatusLeft(SID.Fugetsu);
        FukaLeft = StatusLeft(SID.Fuka);
        MeikyoLeft = StatusLeft(SID.MeikyoShisui);
        OgiLeft = StatusLeft(SID.OgiNamikiriReady);
        TsubameLeft = StatusLeft(SID.TsubameGaeshiReady);
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

        UpdatePositionals(primaryTarget, GetNextPositional(strategy), TrueNorthLeft > GCD);

        OGCD(strategy, primaryTarget);

        if (CountdownRemaining > 0)
        {
            if (MeikyoLeft == 0 && CountdownRemaining < 14)
                PushGCD(AID.MeikyoShisui, Player);

            if (TrueNorthLeft == 0 && Hints.PotentialTargets.Any(x => !x.Actor.Omnidirectional) && CountdownRemaining < 5)
                PushGCD(AID.TrueNorth, Player);

            return;
        }

        EmergencyMeikyo(strategy);
        UseKaeshi(primaryTarget);
        UseIaijutsu(primaryTarget);

        if (OgiLeft > GCD && TargetDotLeft > 10 && HaveFugetsu)
            PushGCD(AID.OgiNamikiri, BestOgiTarget);

        if (MeikyoLeft > GCD)
            PushGCD(MeikyoAction, NumAOETargets > 2 ? Player : primaryTarget);

        if (NumAOETargets > 2 && Unlocked(AID.Fuga))
        {
            if (ComboLastMove == AOEStarter)
            {
                if (FugetsuLeft <= FukaLeft)
                    PushGCD(AID.Mangetsu, Player);
                if (FukaLeft <= FugetsuLeft)
                    PushGCD(AID.Oka, Player);
            }

            PushGCD(AOEStarter, BestAOETarget);
        }
        else
        {
            if (ComboLastMove == AID.Jinpu)
                PushGCD(AID.Gekko, primaryTarget);
            if (ComboLastMove == AID.Shifu)
                PushGCD(AID.Kasha, primaryTarget);

            if (ComboLastMove == STStarter)
                PushGCD(GetHakazeComboAction(strategy), primaryTarget);

            PushGCD(AID.Hakaze, primaryTarget);
        }

        if (EnhancedEnpi > GCD)
            PushGCD(AID.Enpi, primaryTarget);
    }

    private AID GetHakazeComboAction(StrategyValues strategy)
    {
        if (Unlocked(AID.Jinpu) && !CanFitGCD(FugetsuLeft, 2))
            return AID.Jinpu;

        if (Unlocked(AID.Shifu) && !CanFitGCD(FukaLeft, 2))
            return AID.Shifu;

        // TODO fix loop, can't track tsubame anymore
        // if (NumStickers == 0 && GCDSUntilNextTsubame is 19 or 21)
        //     PushGCD(AID.Yukikaze, primaryTarget);

        // TODO use yukikaze if we need to re apply higanbana?

        if (Unlocked(AID.Shifu) && !Flower && FugetsuLeft > FukaLeft)
            return AID.Shifu;

        if (Unlocked(AID.Jinpu) && !Moon)
            return AID.Jinpu;

        if (Unlocked(AID.Yukikaze) && !Ice)
            return AID.Yukikaze;

        // fallback if we are full on sen but can't use midare bc of movement restrictions or w/e
        return Unlocked(AID.Jinpu) ? AID.Jinpu : AID.None;
    }

    private AID MeikyoAction
    {
        get
        {
            if (NumAOETargets > 2)
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
        switch (Kaeshi)
        {
            case KaeshiAction.Goken:
                PushGCD(AID.KaeshiGoken, Player);
                break;
            case KaeshiAction.Setsugekka:
                PushGCD(AID.KaeshiSetsugekka, primaryTarget);
                break;
            case KaeshiAction.Namikiri:
                PushGCD(AID.KaeshiNamikiri, BestOgiTarget);
                break;
            case (KaeshiAction)5:
                PushGCD(AID.TendoKaeshiGoken, Player);
                break;
            case (KaeshiAction)6:
                PushGCD(AID.TendoKaeshiSetsugekka, primaryTarget);
                break;
        }
    }

    private void UseIaijutsu(Actor? primaryTarget)
    {
        if (!HaveFugetsu)
            return;

        if (NumStickers == 1 && TargetDotLeft < 10 && FukaLeft > 0)
            PushGCD(AID.Higanbana, BestDotTarget);

        if (NumStickers == 2 && NumTenkaTargets > 2)
            PushGCD(Tendo > GCD ? AID.TendoGoken : AID.TenkaGoken, Player);

        if (NumStickers == 3)
            PushGCD(Tendo > GCD ? AID.TendoSetsugekka : AID.MidareSetsugekka, primaryTarget);
    }

    private void EmergencyMeikyo(StrategyValues strategy)
    {
        // special case for if we got thrust into combat with no prep
        if (NumStickers == 0 && MeikyoLeft == 0 && !HaveFugetsu && CombatTimer < 5)
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
                // these are ordered backwards because guren unlocks first
                if (NumLineTargets < 2)
                    PushOGCD(AID.HissatsuSenei, primaryTarget);

                PushOGCD(AID.HissatsuGuren, BestLineTarget);
            }
        }

        if (Meditation == 3)
            PushOGCD(AID.Shoha, BestLineTarget);

        if (Kenki >= 25 && CD(AID.HissatsuGuren) > 10 && Zanshin == 0)
        {
            if (NumAOECircleTargets > 2)
                PushOGCD(AID.HissatsuKyuten, Player);

            PushOGCD(AID.HissatsuShinten, primaryTarget);
        }

        if (Kaeshi == 0 && MeikyoLeft == 0 && Tendo == 0 && (NumStickers == 3 || CombatTimer < 30 || NumTenkaTargets > 2))
            PushOGCD(AID.MeikyoShisui, Player);
    }

    private float HiganbanaLeft(Actor? p) => p == null ? float.MaxValue : StatusDetails(p, SID.Higanbana, Player.InstanceID).Left;

    private bool InConeAOE(Actor primary, Actor other) => Hints.TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 60.Degrees());
    private bool InLineAOE(Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 4);
}
