using BossMod.MCH;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public sealed class MCH(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public enum Track { Queen = SharedTrack.Count }
    public enum QueenStrategy
    {
        MinGauge,
        FullGauge,
        RaidBuffsOnly,
        Never
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan MCH", "Machinist", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.MCH), 100);

        def.DefineShared().AddAssociatedActions(AID.BarrelStabilizer, AID.Wildfire);

        def.Define(Track.Queen).As<QueenStrategy>("Queen", "Queen")
            .AddOption(QueenStrategy.MinGauge, "Min", "Summon at 50+ gauge")
            .AddOption(QueenStrategy.FullGauge, "Full", "Summon at 100 gauge")
            .AddOption(QueenStrategy.RaidBuffsOnly, "Buffed", "Delay summon until raid buffs, regardless of gauge")
            .AddOption(QueenStrategy.Never, "Never", "Do not automatically summon Queen at all")
            .AddAssociatedActions(AID.AutomatonQueen, AID.RookAutoturret);

        return def;
    }

    public int Heat; // max 100
    public int Battery; // max 100
    public float OverheatLeft; // max 10s
    public bool Overheated;
    public bool HasMinion;

    public float ReassembleLeft; // max 5s
    public float WildfireLeft; // max 10s
    public float HyperchargedLeft; // max 30s
    public float ExcavatorLeft; // max 30s
    public float FMFLeft; // max 30s

    public bool Flamethrower;

    public int NumAOETargets;
    public int NumRangedAOETargets;
    public int NumSawTargets;
    public int NumFlamethrowerTargets;

    private Actor? BestAOETarget;
    private Actor? BestRangedAOETarget;
    private Actor? BestChainsawTarget;

    private bool IsPausedForFlamethrower => Service.Config.Get<MCHConfig>().PauseForFlamethrower && Flamethrower;

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 25);

        var gauge = World.Client.GetGauge<MachinistGauge>();

        Heat = gauge.Heat;
        Battery = gauge.Battery;
        Overheated = (gauge.TimerActive & 1) != 0;
        OverheatLeft = gauge.OverheatTimeRemaining / 1000f;
        HasMinion = (gauge.TimerActive & 2) != 0;

        ReassembleLeft = StatusLeft(SID.Reassembled);
        WildfireLeft = StatusLeft(SID.WildfirePlayer);
        HyperchargedLeft = StatusLeft(SID.Hypercharged);
        ExcavatorLeft = StatusLeft(SID.ExcavatorReady);
        FMFLeft = StatusLeft(SID.FullMetalMachinist);

        Flamethrower = StatusLeft(SID.Flamethrower) > 0;

        (BestAOETarget, NumAOETargets) = SelectTarget(strategy, primaryTarget, 12, IsConeAOETarget);
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);
        (BestChainsawTarget, NumSawTargets) = SelectTarget(strategy, primaryTarget, 25, Is25yRectTarget);
        NumFlamethrowerTargets = Hints.NumPriorityTargetsInAOECone(Player.Position, 8, Player.Rotation.ToDirection(), 45.Degrees());

        OGCD(strategy, primaryTarget);

        if (IsPausedForFlamethrower)
            return;

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < 0.4)
                PushGCD(AID.AirAnchor, primaryTarget);

            return;
        }

        if (Overheated)
        {
            if (FMFLeft > GCD)
                PushGCD(AID.FullMetalField, BestRangedAOETarget);

            if (NumAOETargets > 3)
                PushGCD(AID.AutoCrossbow, BestAOETarget);

            PushGCD(AID.HeatBlast, primaryTarget);

            // we don't use any other gcds during overheat
            return;
        }

        if (ExcavatorLeft > GCD)
            PushGCD(AID.Excavator, BestRangedAOETarget);

        if (Unlocked(AID.AirAnchor))
            PushGCD(AID.AirAnchor, primaryTarget, priority: 20);

        PushGCD(AID.ChainSaw, BestChainsawTarget, 10);

        if (NumAOETargets > 2)
            PushGCD(AID.Bioblaster, BestAOETarget);

        PushGCD(AID.Drill, primaryTarget, priority: CD(AID.Drill) <= GCD ? 20 : 0);

        // TODO work out priorities
        if (FMFLeft > GCD && ExcavatorLeft == 0)
            PushGCD(AID.FullMetalField, BestRangedAOETarget);

        if (ReassembleLeft > GCD && NumAOETargets > 3)
            PushGCD(AID.Scattergun, BestAOETarget);

        // different cdgroup?
        if (!Unlocked(AID.AirAnchor))
            PushGCD(AID.HotShot, primaryTarget);

        if (NumAOETargets > 2 && Unlocked(AID.SpreadShot))
            PushGCD(AID.SpreadShot, BestAOETarget);
        else
        {
            if (ComboLastMove == AID.SlugShot)
                PushGCD(AID.CleanShot, primaryTarget);

            if (ComboLastMove == AID.SplitShot)
                PushGCD(AID.SlugShot, primaryTarget);

            PushGCD(AID.SplitShot, primaryTarget);
        }
    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (CountdownRemaining is > 0 and < 5 && ReassembleLeft == 0)
            PushOGCD(AID.Reassemble, Player);

        if (IsPausedForFlamethrower || !Player.InCombat || primaryTarget == null)
            return;

        if (ShouldWildfire(strategy))
            PushOGCD(AID.Wildfire, primaryTarget, delay: GCD - 0.8f);

        if (ShouldReassemble(strategy, primaryTarget))
            PushOGCD(AID.Reassemble, Player);

        if (ShouldStabilize(strategy))
            PushOGCD(AID.BarrelStabilizer, Player);

        UseCharges(strategy, primaryTarget);

        if (ShouldMinion(strategy, primaryTarget))
            PushOGCD(AID.RookAutoturret, Player);

        if (ShouldHypercharge(strategy))
            PushOGCD(AID.Hypercharge, Player);
    }

    private float NextToolCharge => MathF.Min(NextChargeIn(AID.Drill), MathF.Min(NextChargeIn(AID.ChainSaw), NextChargeIn(AID.AirAnchor)));
    private float NextToolCap => MathF.Min(MaxChargesIn(AID.Drill), MathF.Min(MaxChargesIn(AID.ChainSaw), MaxChargesIn(AID.AirAnchor)));

    private float MaxGaussCD => MaxChargesIn(AID.GaussRound);
    private float MaxRicochetCD => MaxChargesIn(AID.Ricochet);

    private void UseCharges(StrategyValues strategy, Actor? primaryTarget)
    {
        var gaussRoundCD = NextChargeIn(AID.GaussRound);
        var ricochetCD = NextChargeIn(AID.Ricochet);

        var canGauss = Unlocked(AID.GaussRound) && CanWeave(gaussRoundCD, 0.6f);
        var canRicochet = Unlocked(AID.Ricochet) && CanWeave(ricochetCD, 0.6f);

        if (canGauss && CanWeave(MaxGaussCD, 0.6f))
            PushOGCD(AID.GaussRound, Unlocked(AID.DoubleCheck) ? BestRangedAOETarget : primaryTarget);

        if (canRicochet && CanWeave(MaxRicochetCD, 0.6f))
            PushOGCD(AID.Ricochet, BestRangedAOETarget);

        var useAllCharges = RaidBuffsLeft > 0 || RaidBuffsIn > 9000 || Overheated || !Unlocked(AID.Hypercharge);
        if (!useAllCharges)
            return;

        // this is a little awkward but we want to try to keep the cooldowns of both actions within range of each other
        if (canGauss && canRicochet)
        {
            if (gaussRoundCD > ricochetCD)
                UseRicochet(primaryTarget);
            else
                UseGauss(primaryTarget);
        }
        else if (canGauss)
            UseGauss(primaryTarget);
        else if (canRicochet)
            UseRicochet(primaryTarget);
    }

    private void UseGauss(Actor? primaryTarget) => PushOGCD(AID.GaussRound, Unlocked(AID.DoubleCheck) ? BestRangedAOETarget : primaryTarget, -50);
    private void UseRicochet(Actor? primaryTarget) => PushOGCD(AID.Ricochet, BestRangedAOETarget, -50);

    private bool ShouldReassemble(StrategyValues strategy, Actor? primaryTarget)
    {
        if (ReassembleLeft > 0 || !Unlocked(AID.Reassemble) || Overheated || primaryTarget == null)
            return false;

        if (NumAOETargets > 3 && Unlocked(AID.SpreadShot))
            return true;

        if (RaidBuffsIn < 10 && RaidBuffsIn > GCD)
            return false;

        if (!Unlocked(AID.Drill))
            return ComboLastMove == AID.SlugShot;

        return NextToolCharge <= GCD;
    }

    private bool ShouldMinion(StrategyValues strategy, Actor? primaryTarget)
    {
        if (!Unlocked(AID.RookAutoturret) || primaryTarget == null || HasMinion || Battery < 50 || ShouldWildfire(strategy))
            return false;

        return strategy.Option(Track.Queen).As<QueenStrategy>() switch
        {
            QueenStrategy.MinGauge => true,
            QueenStrategy.FullGauge => Battery == 100,
            // allow early summon, queen doesn't start autoing for 5 seconds
            QueenStrategy.RaidBuffsOnly => RaidBuffsLeft > 10 || RaidBuffsIn < 5,
            _ => false,
        };
    }

    private bool ShouldHypercharge(StrategyValues strategy)
    {
        if (!Unlocked(AID.Hypercharge) || HyperchargedLeft == 0 && Heat < 50 || Overheated || ReassembleLeft > GCD)
            return false;

        // avoid delaying wildfire
        // TODO figure out how long we actually need to wait to ensure enough heat
        if (CD(AID.Wildfire) < 20 && !ShouldWildfire(strategy))
            return false;

        // we can't early weave if the overheat window will contain a regular GCD, because then it will expire before last HB
        if (FMFLeft > 0 && GCD > 1.1f)
            return false;

        /* A full segment of Hypercharge is exactly three GCDs worth of time, or 7.5 seconds. Because of this, you should never enter Hypercharge if Chainsaw, Drill or Air Anchor has less than eight seconds on their cooldown timers. Doing so will cause the Chainsaw, Drill or Air Anchor cooldowns to drift, which leads to a loss of DPS and will more than likely cause issues down the line in your rotation when you reach your rotational reset at Wildfire.
         */
        return NextToolCap > GCD + 7.5f;
    }

    private bool ShouldWildfire(StrategyValues strategy)
    {
        if (!Unlocked(AID.Wildfire) || !CanWeave(AID.Wildfire) || !strategy.BuffsOk())
            return false;

        // hack for opener - delay until all 4 tool charges are used
        if (CombatTimer < 60)
            return NextToolCharge > GCD;

        return FMFLeft == 0;
    }

    private bool ShouldStabilize(StrategyValues strategy)
    {
        if (!Unlocked(AID.BarrelStabilizer) || !CanWeave(AID.BarrelStabilizer) || !strategy.BuffsOk())
            return false;

        return CD(AID.Drill) > 0;
    }

    private PositionCheck IsConeAOETarget => (playerTarget, targetToTest) => Hints.TargetInAOECone(targetToTest, Player.Position, 12, Player.DirectionTo(playerTarget), 60.Degrees());
}
