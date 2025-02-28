using BossMod.MCH;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class MCH(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public enum Track { Queen = SharedTrack.Count, Wildfire, Hypercharge, Tools }
    public enum QueenStrategy
    {
        MinGauge,
        FullGauge,
        RaidBuffsOnly,
        Never
    }
    public enum WildfireStrategy
    {
        ASAP,
        Delay,
        Hypercharge
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan MCH", "Machinist", "Standard rotation (xan)|Ranged", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.MCH), 100);

        def.DefineShared().AddAssociatedActions(AID.BarrelStabilizer);

        def.Define(Track.Queen).As<QueenStrategy>("Queen", "Queen")
            .AddOption(QueenStrategy.MinGauge, "Min", "Summon at 50+ gauge")
            .AddOption(QueenStrategy.FullGauge, "Full", "Summon at 100 gauge")
            .AddOption(QueenStrategy.RaidBuffsOnly, "Buffed", "Delay summon until raid buffs, regardless of gauge")
            .AddOption(QueenStrategy.Never, "Never", "Do not automatically summon Queen at all")
            .AddAssociatedActions(AID.AutomatonQueen, AID.RookAutoturret);

        def.Define(Track.Wildfire).As<WildfireStrategy>("WF", "Wildfire")
            .AddOption(WildfireStrategy.ASAP, "ASAP", "Use as soon as possible (delay in opener until after Full Metal Field)")
            .AddOption(WildfireStrategy.Delay, "Delay", "Do not use")
            .AddOption(WildfireStrategy.Hypercharge, "Hypercharge", "Delay until Hypercharge window");

        def.DefineSimple(Track.Hypercharge, "Hypercharge").AddAssociatedActions(AID.Hypercharge);
        def.DefineSimple(Track.Tools, "Tools").AddAssociatedActions(AID.Drill, AID.AirAnchor, AID.ChainSaw, AID.Bioblaster);

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

    private Enemy? BestAOETarget;
    private Enemy? BestRangedAOETarget;
    private Enemy? BestChainsawTarget;

    private bool IsPausedForFlamethrower => Service.Config.Get<MCHConfig>().PauseForFlamethrower && Flamethrower;

    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
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

        if (IsPausedForFlamethrower)
            return;

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < 5 && ReassembleLeft == 0)
                PushGCD(AID.Reassemble, Player);

            if (CountdownRemaining < 1.15f)
            {
                PushGCD(AID.AirAnchor, primaryTarget);
                PushGCD(AID.Drill, primaryTarget);
            }

            return;
        }

        if (primaryTarget != null)
        {
            var aoebreakpoint = Overheated && Unlocked(AID.AutoCrossbow) ? 4 : 3;
            GoalZoneCombined(strategy, 25, Hints.GoalAOECone(primaryTarget.Actor, 12, 60.Degrees()), AID.SpreadShot, aoebreakpoint);
        }

        if (Overheated && Unlocked(AID.HeatBlast))
        {
            if (FMFLeft > GCD)
                PushGCD(AID.FullMetalField, BestRangedAOETarget);

            if (NumAOETargets > 2)
                PushGCD(AID.AutoCrossbow, BestAOETarget);

            PushGCD(BestActionUnlocked(AID.BlazingShot, AID.HeatBlast), primaryTarget);
        }
        else
        {
            if (ExcavatorLeft > GCD)
                PushGCD(AID.Excavator, BestRangedAOETarget);

            var toolOk = strategy.Simple(Track.Tools) != OffensiveStrategy.Delay;

            if (toolOk)
            {
                if (ReadyIn(AID.AirAnchor) <= GCD)
                    PushGCD(AID.AirAnchor, primaryTarget, priority: 20);

                if (ReadyIn(AID.ChainSaw) <= GCD)
                    PushGCD(AID.ChainSaw, BestChainsawTarget, 10);

                if (ReadyIn(AID.Bioblaster) <= GCD && NumAOETargets > 1)
                    PushGCD(AID.Bioblaster, BestAOETarget, priority: MaxChargesIn(AID.Bioblaster) <= GCD ? 20 : 2);

                if (ReadyIn(AID.Drill) <= GCD)
                    PushGCD(AID.Drill, primaryTarget, priority: MaxChargesIn(AID.Drill) <= GCD ? 20 : 2);

                // different cdgroup fsr
                if (!Unlocked(AID.AirAnchor) && ReadyIn(AID.HotShot) <= GCD)
                    PushGCD(AID.HotShot, primaryTarget);
            }

            // TODO work out priorities
            if (FMFLeft > GCD && ExcavatorLeft == 0)
                PushGCD(AID.FullMetalField, BestRangedAOETarget);

            var breakpoint = Unlocked(AID.Scattergun) ? 2 : 1;

            if (NumAOETargets > breakpoint && Unlocked(AID.SpreadShot))
                PushGCD(BestActionUnlocked(AID.Scattergun, AID.SpreadShot), BestAOETarget);
            else
            {
                if (ComboLastMove == AID.SlugShot)
                    PushGCD(BestActionUnlocked(AID.HeatedCleanShot, AID.CleanShot), primaryTarget);

                if (ComboLastMove == AID.SplitShot)
                    PushGCD(BestActionUnlocked(AID.HeatedSlugShot, AID.SlugShot), primaryTarget);

                PushGCD(BestActionUnlocked(AID.HeatedSplitShot, AID.SplitShot), primaryTarget);
            }
        }

        OGCD(strategy, primaryTarget);
    }

    private void OGCD(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (CountdownRemaining == null && !Player.InCombat && Player.DistanceToHitbox(primaryTarget) <= 25 && ReassembleLeft == 0 && ShouldReassemble(strategy, primaryTarget))
            PushGCD(AID.Reassemble, Player, priority: 50);

        if (!Player.InCombat || primaryTarget == null)
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

    private float NextToolCharge => MathF.Min(ReadyIn(AID.Drill), MathF.Min(ReadyIn(AID.ChainSaw), ReadyIn(AID.AirAnchor)));
    private float NextToolCap => MathF.Min(MaxChargesIn(AID.Drill), MathF.Min(MaxChargesIn(AID.ChainSaw), MaxChargesIn(AID.AirAnchor)));

    private float MaxGaussCD => MaxChargesIn(AID.GaussRound);
    private float MaxRicochetCD => MaxChargesIn(AID.Ricochet);

    private void UseCharges(StrategyValues strategy, Enemy? primaryTarget)
    {
        // checking for max charges
        if (CanWeave(MaxGaussCD, 0.6f))
            PushOGCD(AID.GaussRound, Unlocked(AID.DoubleCheck) ? BestRangedAOETarget : primaryTarget);
        if (CanWeave(MaxRicochetCD, 0.6f))
            PushOGCD(AID.Ricochet, BestRangedAOETarget);

        var useAllCharges = RaidBuffsLeft > 0 || RaidBuffsIn > 9000 || Overheated || !Unlocked(AID.Hypercharge);
        if (!useAllCharges)
            return;

        var gelapse = World.Client.Cooldowns[14].Elapsed;
        var relapse = World.Client.Cooldowns[15].Elapsed;

        UseGauss(primaryTarget, gelapse > relapse ? 1 : 0);
        UseRicochet(primaryTarget, relapse > gelapse ? 1 : 0);
    }

    private void UseGauss(Enemy? primaryTarget, int charges) => Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.GaussRound), (Unlocked(AID.DoubleCheck) ? BestRangedAOETarget : primaryTarget)?.Actor, ActionQueue.Priority.Low - 50 + charges);
    private void UseRicochet(Enemy? primaryTarget, int charges) => Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.Ricochet), BestRangedAOETarget?.Actor, ActionQueue.Priority.Low - 50 + charges);

    private bool ShouldReassemble(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (ReassembleLeft > 0 || !Unlocked(AID.Reassemble) || Overheated || primaryTarget == null)
            return false;

        if (AlwaysReassemble(NextGCD))
            return true;

        return NextGCD switch
        {
            // AOE actions (TODO review usage, the wording in the balance guide is contradictory)
            AID.SpreadShot or AID.Scattergun or AID.AutoCrossbow => true,
            // highest potency before 58
            AID.CleanShot => !Unlocked(AID.Drill),
            // highest potency before 26
            AID.HotShot => !Unlocked(AID.CleanShot),

            _ => false
        };
    }

    private bool AlwaysReassemble(AID action) => action is AID.Drill or AID.AirAnchor or AID.ChainSaw or AID.Excavator;

    private int BatteryFromAction(AID action) => action switch
    {
        AID.ChainSaw or AID.AirAnchor or AID.Excavator or AID.HotShot => 20,
        AID.CleanShot or AID.HeatedCleanShot => 10,
        _ => 0
    };

    private bool ShouldMinion(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (!Unlocked(AID.RookAutoturret) || primaryTarget == null || HasMinion || Battery < 50 || ShouldWildfire(strategy))
            return false;

        var almostFull = Battery == 90 && BatteryFromAction(NextGCD) == 20;

        return strategy.Option(Track.Queen).As<QueenStrategy>() switch
        {
            QueenStrategy.MinGauge => true,
            QueenStrategy.FullGauge => Battery == 100 || almostFull,
            // allow early summon, queen doesn't start autoing for 5 seconds
            QueenStrategy.RaidBuffsOnly => RaidBuffsLeft > 10 || RaidBuffsIn < 5,
            _ => false,
        };
    }

    private bool ShouldHypercharge(StrategyValues strategy)
    {
        // strategy-independent preconditions, hypercharge cannot be used at all in these cases 
        if (!Unlocked(AID.Hypercharge) || HyperchargedLeft == 0 && Heat < 50 || Overheated)
            return false;

        // don't want to use reassemble on heat blast, even if strategy is Force, since presumably next GCD will be a tool charge
        if (ReassembleLeft > GCD)
            return false;

        switch (strategy.Simple(Track.Hypercharge))
        {
            case OffensiveStrategy.Force:
                return true;
            case OffensiveStrategy.Delay:
                return false;
            default:
                break;
        }

        // avoid delaying wildfire
        // TODO figure out how long we actually need to wait to ensure enough heat
        if (ReadyIn(AID.Wildfire) < 20 && !ShouldWildfire(strategy))
            return false;

        // we can't early weave if the overheat window will contain a regular GCD, because then it will expire before last HB
        if (FMFLeft > 0 && GCD > 1.1f)
            return false;

        if (DowntimeIn < GCD + 6)
            return false;

        /* A full segment of Hypercharge is exactly three GCDs worth of time, or 7.5 seconds. Because of this, you should never enter Hypercharge if Chainsaw, Drill or Air Anchor has less than eight seconds on their cooldown timers. Doing so will cause the Chainsaw, Drill or Air Anchor cooldowns to drift, which leads to a loss of DPS and will more than likely cause issues down the line in your rotation when you reach your rotational reset at Wildfire.
         */
        return NextToolCap > GCD + 7.5f;
    }

    private bool ShouldWildfire(StrategyValues strategy)
    {
        var wfStrat = strategy.Option(Track.Wildfire).As<WildfireStrategy>();

        if (!Unlocked(AID.Wildfire) || !CanWeave(AID.Wildfire) || wfStrat == WildfireStrategy.Delay)
            return false;

        if (wfStrat == WildfireStrategy.Hypercharge)
            return Overheated || HyperchargedLeft > 0 || Heat >= 50;

        // hack for opener - delay until all 4 tool charges are used
        if (CombatTimer < 60)
            return NextToolCharge > GCD;

        return FMFLeft == 0;
    }

    private bool ShouldStabilize(StrategyValues strategy)
    {
        if (!Unlocked(AID.BarrelStabilizer) || !CanWeave(AID.BarrelStabilizer) || !strategy.BuffsOk())
            return false;

        return OnCooldown(AID.Drill);
    }

    private PositionCheck IsConeAOETarget => (playerTarget, targetToTest) => Hints.TargetInAOECone(targetToTest, Player.Position, 12, Player.DirectionTo(playerTarget), 60.Degrees());
}
