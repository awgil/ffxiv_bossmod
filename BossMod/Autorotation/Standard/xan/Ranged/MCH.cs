using BossMod.MCH;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class MCH(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID, MCH.Strategy>(manager, player, PotionType.Dexterity)
{
    public struct Strategy : IStrategyCommon
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;
        [Track("Barrel Stabilizer", MinLevel = 66, Action = AID.BarrelStabilizer)]
        public Track<OffensiveStrategy> Buffs;

        [Track("Queen", MinLevel = 40, Actions = [AID.AutomatonQueen, AID.RookAutoturret])]
        public Track<QueenStrategy> Queen;

        [Track("Wildfire", InternalName = "WF", MinLevel = 45, Actions = [AID.Wildfire, AID.Detonator])]
        public Track<WildfireStrategy> Wildfire;

        [Track(Action = AID.Hypercharge, MinLevel = 30)]
        public Track<OffensiveStrategy> Hypercharge;

        // not including tool-related actions that have a buff status instead of a cd group
        [Track(Actions = [AID.Drill, AID.HotShot, AID.AirAnchor, AID.ChainSaw, AID.Bioblaster])]
        public Track<ToolStrategy> Tools;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public enum QueenStrategy
    {
        [Option("Summon at 50+ gauge", Targets = ActionTargets.Hostile)]
        MinGauge,
        [Option("Summon at full gauge", Targets = ActionTargets.Hostile)]
        FullGauge,
        [Option("Only summon during raid buffs, regardless of gauge", Targets = ActionTargets.Hostile)]
        RaidBuffsOnly,
        [Option("Do not summon")]
        Never
    }
    public enum WildfireStrategy
    {
        [Option("Use ASAP; delay in opener until tools are used", Targets = ActionTargets.Hostile)]
        ASAP,
        [Option("Do not use")]
        Delay,
        [Option("Delay until Hypercharge window", Targets = ActionTargets.Hostile)]
        Hypercharge
    }

    public enum ToolStrategy
    {
        [Option("Use ASAP", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Do not use")]
        Delay
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("xan MCH", "Machinist", "Standard rotation (xan)|Ranged", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.MCH), 100).WithStrategies<Strategy>();
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

    public override void Exec(in Strategy strategy, Enemy? primaryTarget)
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

        // minion chooses target based on the first target hit with any non-autoattack action after summon
        // ideally the summon is early-weaved, so we can immediately leg graze and then switch back to chosen target
        // TODO: this obviously won't work properly if the next action is a GCD that is cdplanned to hit something else; a consistent solution would be to force a clip with Leg Graze, but that sounds really frustrating for users
        if (Manager.LastCast.Data?.Action.ID is (uint)AID.AutomatonQueen or (uint)AID.RookAutoturret)
        {
            if (ResolveTargetOverride(strategy.Queen.TrackRaw) is { } target)
            {
                primaryTarget = Hints.FindEnemy(target);
                PushOGCD(AID.LegGraze, target);
            }
        }

        if (primaryTarget != null)
        {
            var aoebreakpoint = Overheated && Unlocked(AID.AutoCrossbow) ? 6 : 3;
            GoalZoneCombined(strategy, 25, Hints.GoalAOECone(primaryTarget.Actor, 12, 60.Degrees()), AID.SpreadShot, aoebreakpoint);
        }

        if (Overheated && Unlocked(AID.HeatBlast))
        {
            if (FMFLeft > GCD)
                PushGCD(AID.FullMetalField, BestRangedAOETarget);

            if (NumAOETargets > 5)
                PushGCD(AID.AutoCrossbow, BestAOETarget);

            PushGCD(BestActionUnlocked(AID.BlazingShot, AID.HeatBlast), primaryTarget);
        }
        else
        {
            var toolOk = strategy.Tools.Value == ToolStrategy.Automatic;
            var toolTarget = ResolveTargetOverride(strategy.Tools);

            if (toolOk)
            {
                if (ExcavatorLeft > GCD)
                    PushGCD(AID.Excavator, toolTarget ?? BestRangedAOETarget);

                if (GCDReady(AID.AirAnchor))
                    PushGCD(AID.AirAnchor, toolTarget ?? primaryTarget, priority: 20);

                if (GCDReady(AID.ChainSaw))
                    PushGCD(AID.ChainSaw, toolTarget ?? BestChainsawTarget, 10);

                if (GCDReady(AID.Bioblaster) && NumAOETargets > 2)
                    PushGCD(AID.Bioblaster, toolTarget ?? BestAOETarget, priority: MaxChargesIn(AID.Bioblaster) <= GCD ? 20 : 2);

                if (GCDReady(AID.Drill))
                    PushGCD(AID.Drill, toolTarget ?? primaryTarget, priority: MaxChargesIn(AID.Drill) <= GCD ? 20 : 2);

                // different cdgroup fsr
                if (!Unlocked(AID.AirAnchor) && GCDReady(AID.HotShot))
                    PushGCD(AID.HotShot, toolTarget ?? primaryTarget);

                // TODO work out priorities
                if (FMFLeft > GCD && ExcavatorLeft == 0)
                    PushGCD(AID.FullMetalField, toolTarget ?? BestRangedAOETarget);
            }

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

    private void OGCD(in Strategy strategy, Enemy? primaryTarget)
    {
        if (CountdownRemaining == null && !Player.InCombat && Player.DistanceToHitbox(primaryTarget) <= 25 && ReassembleLeft == 0 && ShouldReassemble(strategy, primaryTarget))
            PushGCD(AID.Reassemble, Player, priority: 50);

        if (!Player.InCombat || primaryTarget == null)
            return;

        if (GetWildfireTarget(strategy, primaryTarget) is { } tar)
            PushOGCD(AID.Wildfire, tar, delay: GCD - 0.8f);

        if (ShouldReassemble(strategy, primaryTarget))
            PushOGCD(AID.Reassemble, Player);

        if (ShouldStabilize(strategy, primaryTarget))
            PushOGCD(AID.BarrelStabilizer, Player);

        UseCharges(strategy, primaryTarget);

        if (ShouldMinion(strategy, primaryTarget))
            PushOGCD(AID.RookAutoturret, Player);

        if (ShouldHypercharge(strategy, primaryTarget))
            PushOGCD(AID.Hypercharge, Player);
    }

    private float NextToolCharge => MathF.Min(ReadyIn(AID.Drill), MathF.Min(ReadyIn(AID.ChainSaw), ReadyIn(AID.AirAnchor)));
    private float NextToolCap => MathF.Min(MaxChargesIn(AID.Drill), MathF.Min(MaxChargesIn(AID.ChainSaw), MaxChargesIn(AID.AirAnchor)));

    private float MaxGaussCD => MaxChargesIn(AID.GaussRound);
    private float MaxRicochetCD => MaxChargesIn(AID.Ricochet);

    private void UseCharges(in Strategy strategy, Enemy? primaryTarget)
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

    private bool ShouldReassemble(in Strategy strategy, Enemy? primaryTarget)
    {
        if (ReassembleLeft > 0 || !Unlocked(AID.Reassemble) || Overheated || primaryTarget == null || primaryTarget?.Priority == Enemy.PriorityPointless)
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

    private bool ShouldMinion(in Strategy strategy, Enemy? primaryTarget)
    {
        if (!Unlocked(AID.RookAutoturret) || primaryTarget == null || HasMinion || Battery < 50 || GetWildfireTarget(strategy, primaryTarget) != null || primaryTarget?.Priority < 0)
            return false;

        var almostFull = Battery == 90 && BatteryFromAction(NextGCD) == 20;

        return strategy.Queen.Value switch
        {
            QueenStrategy.MinGauge => true,
            QueenStrategy.FullGauge => Battery == 100 || almostFull,
            // allow early summon, queen doesn't start autoing for 5 seconds
            QueenStrategy.RaidBuffsOnly => RaidBuffsLeft > 10 || RaidBuffsIn < 5,
            _ => false,
        };
    }

    private bool ShouldHypercharge(in Strategy strategy, Enemy? primaryTarget)
    {
        // strategy-independent preconditions, hypercharge cannot be used at all in these cases
        if (!Unlocked(AID.Hypercharge) || HyperchargedLeft == 0 && Heat < 50 || Overheated)
            return false;

        // don't want to use reassemble on heat blast, even if strategy is Force, since presumably next GCD will be a tool charge
        if (ReassembleLeft > GCD)
            return false;

        // primary target is dying
        if (primaryTarget?.Priority < 0)
            return false;

        switch (strategy.Hypercharge.Value)
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
        if (ReadyIn(AID.Wildfire) < 20 && GetWildfireTarget(strategy, primaryTarget) == null)
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

    private Enemy? GetWildfireTarget(in Strategy strategy, Enemy? primaryTarget)
    {
        var wf = strategy.Wildfire;
        var wfTarget = ResolveTargetOverride(wf) ?? primaryTarget;

        if (!Unlocked(AID.Wildfire) || !CanWeave(AID.Wildfire) || wf == WildfireStrategy.Delay)
            return null;

        if (wfTarget?.Priority < 0)
            return null;

        if (wf == WildfireStrategy.Hypercharge)
            return Overheated || HyperchargedLeft > 0 || Heat >= 50 ? wfTarget : null;

        // hack for opener - delay until all 4 tool charges are used
        if (CombatTimer < 60)
            return NextToolCharge > GCD ? wfTarget : null;

        return FMFLeft == 0 ? wfTarget : null;
    }

    private bool ShouldStabilize(in Strategy strategy, Enemy? primaryTarget)
    {
        if (!Unlocked(AID.BarrelStabilizer) || !CanWeave(AID.BarrelStabilizer) || strategy.Buffs == OffensiveStrategy.Delay || primaryTarget?.Priority < 0)
            return false;

        return OnCooldown(AID.Drill);
    }

    private PositionCheck IsConeAOETarget => (playerTarget, targetToTest) => Hints.TargetInAOECone(targetToTest, Player.Position, 12, Player.DirectionTo(playerTarget), 60.Degrees());
}
