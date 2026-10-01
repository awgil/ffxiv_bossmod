using BossMod.MCH;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KageMCH(RotationModuleManager manager, Actor player) : TypedRotationModule<KageMCH.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Hypercharge", MinLevel = 30, Action = AID.Hypercharge)]
        public Track<HyperchargeStrategy> Hypercharge;

        [Track("Wildfire", InternalName = "WF", MinLevel = 45, Action = AID.Wildfire)]
        public Track<WildfireStrategy> Wildfire;

        [Track("Barrel Stabilizer", MinLevel = 66, Action = AID.BarrelStabilizer)]
        public Track<OffensiveStrategy> Stabilizer;

        [Track("Tools", Actions = [AID.Drill, AID.HotShot, AID.AirAnchor, AID.ChainSaw, AID.Excavator, AID.Bioblaster, AID.FullMetalField])]
        public Track<OffensiveStrategy> Tools;

        [Track("Reassemble", MinLevel = 10, Action = AID.Reassemble)]
        public Track<ReassembleStrategy> Reassemble;

        [Track("Queen", MinLevel = 40, Actions = [AID.AutomatonQueen, AID.RookAutoturret, AID.QueenOverdrive, AID.RookOverdrive])]
        public Track<QueenStrategy> Queen;

        [Track("Gauss Round / Ricochet", MinLevel = 15, Actions = [AID.GaussRound, AID.Ricochet, AID.DoubleCheck, AID.Checkmate])]
        public Track<ChargeStrategy> Charges;

        [Track("Flamethrower", MinLevel = 70, Action = AID.Flamethrower)]
        public Track<FlamethrowerStrategy> Flamethrower;

        [Track("Potion")]
        public Track<PotionStrategy> Potion;

        [Track("Opener", MinLevel = 90, UiPriority = -10, Context = StrategyContext.Plan)]
        public Track<OpenerStrategy> Opener;
    }

    public enum HyperchargeStrategy
    {
        [Option("Use when no tool comes off cooldown during Overheat; save Heat for Wildfire")]
        Automatic,
        [Option("Use as soon as Heat allows", Effect = 10, MinLevel = 30)]
        ASAP,
        [Option("Do not use")]
        Delay
    }

    public enum WildfireStrategy
    {
        [Option("Use right after entering Overheat", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Use in the next weave slot", Cooldown = 120, Effect = 10, Targets = ActionTargets.Hostile, MinLevel = 45)]
        Force,
        [Option("Do not use")]
        Delay
    }

    public enum ReassembleStrategy
    {
        [Option("Use on tools")]
        Automatic,
        [Option("Use on tools and AoE fillers")]
        Any,
        [Option("Do not use")]
        Delay
    }

    public enum QueenStrategy
    {
        [Option("Summon at 90+ Battery before a Battery-generating GCD, 100 Battery, or 50+ during raid buffs")]
        Automatic,
        [Option("Summon at 50+ Battery")]
        Fifty,
        [Option("Summon at 100 Battery")]
        Hundred,
        [Option("Do not summon")]
        Delay
    }

    public enum ChargeStrategy
    {
        [Option("Spend during Overheat and raid buffs; otherwise only to avoid overcapping", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Only use to avoid overcapping", Targets = ActionTargets.Hostile)]
        Overcap,
        [Option("Do not use")]
        Delay
    }

    public enum FlamethrowerStrategy
    {
        [Option("Use in AoE while standing still")]
        Automatic,
        [Option("Do not use")]
        Delay
    }

    public enum PotionStrategy
    {
        [Option("Do not use automatically")]
        Manual,
        [Option("Use before the pull and with Barrel Stabilizer")]
        AlignWithBurst,
        [Option("Use before the pull and with raid buffs")]
        AlignWithRaidBuffs,
        [Option("Use as soon as possible")]
        Immediate
    }

    public enum OpenerStrategy
    {
        [Option("Standard: Drill, Chain Saw, Excavator, Drill, then Wildfire, Full Metal Field and Hypercharge")]
        Standard,
        [Option("Early Wildfire: Drill, Chain Saw, then Wildfire into Excavator and Hypercharge", MinLevel = 100)]
        EarlyWildfire,
        [Option("No opener-specific rules")]
        None
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Kage MCH", "Machinist", "Standard rotation (Kage)|Ranged", "Kagekazu", RotationModuleQuality.WIP, BitMask.Build(Class.MCH), 100).WithStrategies<Strategy>();
    }

    private int Heat;
    private int Battery;
    private bool Overheated;
    private bool HasMinion;

    private float ReassembleLeft;
    private float WildfireLeft;
    private float HyperchargedLeft;
    private float ExcavatorLeft;
    private float FMFLeft;

    private float RaidBuffsLeft;
    private float RaidBuffsIn;
    private float DowntimeIn;

    private int NumConeTargets;
    private Actor? BestConeTarget;
    private Actor? BestSawTarget;
    private Actor? BestSplashTarget;
    private bool AOEMode;
    private bool IsMoving;
    private float AnimLockDelay;

    private Targeting TargetMode;
    private OpenerStrategy OpenerMode;
    private AID NextGCD;
    private float NextGCDPrio;

    private float GCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
    private AID ComboLastMove => (AID)World.Client.ComboState.Action;
    private float CombatTime => Player.InCombat ? (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds : 0;
    private bool InOpener => OpenerMode != OpenerStrategy.None && CombatTime < 30;
    private bool StandardOpener => OpenerMode == OpenerStrategy.Standard && CombatTime < 30;
    private bool EarlyWFOpener => OpenerMode == OpenerStrategy.EarlyWildfire && CombatTime < 30;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        IsMoving = isMoving;
        AnimLockDelay = estimatedAnimLockDelay;
        NextGCD = AID.None;
        NextGCDPrio = 0;

        var target = Hints.FindEnemy(primaryTarget);
        if (target?.Priority is Enemy.PriorityInvincible or Enemy.PriorityForbidden)
            target = null;

        TargetMode = strategy.Targeting.Value == Targeting.AutoTryPri ? (target != null ? Targeting.AutoPrimary : Targeting.Auto) : strategy.Targeting.Value;
        if (TargetMode == Targeting.Auto && target == null)
        {
            target = Hints.PriorityTargets.Where(e => Player.DistanceToHitbox(e.Actor) <= 25).MinBy(e => Player.DistanceToHitbox(e.Actor)) ?? target;
            primaryTarget = target?.Actor;
        }

        OpenerMode = strategy.Opener.Value switch
        {
            _ when !Unlocked(AID.ChainSaw) => OpenerStrategy.None,
            OpenerStrategy.EarlyWildfire when !Unlocked(AID.Excavator) => OpenerStrategy.None,
            var o => o
        };

        var gauge = World.Client.GetGauge<MachinistGauge>();
        Heat = gauge.Heat;
        Battery = gauge.Battery;
        Overheated = (gauge.TimerActive & 1) != 0;
        HasMinion = (gauge.TimerActive & 2) != 0;

        ReassembleLeft = SelfStatusLeft(SID.Reassembled);
        WildfireLeft = SelfStatusLeft(SID.WildfirePlayer);
        HyperchargedLeft = SelfStatusLeft(SID.Hypercharged);
        ExcavatorLeft = SelfStatusLeft(SID.ExcavatorReady);
        FMFLeft = SelfStatusLeft(SID.FullMetalMachinist);

        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget);
        DowntimeIn = Manager.Planner?.EstimateTimeToNextDowntime() is (var downNow, var stateLeft) ? (downNow ? 0 : stateLeft) : float.MaxValue;

        if (SelfStatusLeft(SID.Flamethrower) > 0)
            return;

        var allowAoE = strategy.AOE.Value is AOEStrategy.AOE or AOEStrategy.ForceAOE;
        (BestConeTarget, NumConeTargets) = BestAOETarget(target, 12, allowAoE, (t, e) => TargetInAOECone(e, Player.Position, 12, Player.DirectionTo(t), 60.Degrees()));
        BestSawTarget = BestAOETarget(target, 25, allowAoE, (t, e) => TargetInAOERect(e, Player.Position, Player.DirectionTo(t), 25, 2)).Best;
        BestSplashTarget = BestAOETarget(target, 25, allowAoE, (t, e) => TargetInAOECircle(e, t.Position, 5)).Best;
        AOEMode = Unlocked(AID.SpreadShot) && strategy.AOE.Value switch
        {
            AOEStrategy.ForceAOE => true,
            AOEStrategy.AOE => NumConeTargets >= (Unlocked(AID.Scattergun) ? 3 : 2),
            _ => false
        };

        if (target != null)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(target.Actor, Player, World.Actors, AOEMode ? 12 : 25));

        if (UsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionDex, Player, ActionQueue.Priority.Medium);

        if (World.Client.CountdownRemaining is > 0 and var countdown)
        {
            if (countdown < 5 && ReassembleLeft == 0 && strategy.Reassemble.Value != ReassembleStrategy.Delay)
                PushGCD(AID.Reassemble, Player, 50);
            if (countdown < 1.15f)
                PushGCD(AID.AirAnchor, target?.Actor, 10);
            return;
        }

        if (target == null)
            return;

        if (Overheated && Unlocked(AID.HeatBlast))
            OverheatGCD(target);
        else
            NormalGCD(strategy, target);

        if (Player.InCombat)
            OGCD(strategy, target);
    }

    #region GCD

    private void OverheatGCD(Enemy target)
    {
        if (AOEMode && Unlocked(AID.AutoCrossbow) && (!Unlocked(AID.BlazingShot) || NumConeTargets >= 5))
            PushGCD(AID.AutoCrossbow, BestConeTarget ?? target.Actor, 20, faceTarget: true);
        else
            PushGCD(BestActionUnlocked(AID.BlazingShot, AID.HeatBlast), target.Actor, 20);
    }

    private void NormalGCD(in Strategy strategy, Enemy target)
    {
        var tools = strategy.Tools.Value;
        if (tools != OffensiveStrategy.Delay && target.Priority != Enemy.PriorityPointless)
        {
            var bonus = tools == OffensiveStrategy.Force ? 100 : 0;

            if (FMFLeft > GCD && UseFullMetalField())
                PushGCD(AID.FullMetalField, BestSplashTarget ?? target.Actor, 16 + bonus);

            if (ExcavatorLeft > GCD)
                PushGCD(AID.Excavator, BestSplashTarget ?? target.Actor, 15 + bonus);

            if (GCDReady(AID.ChainSaw))
                PushGCD(AID.ChainSaw, BestSawTarget ?? target.Actor, 14 + bonus, faceTarget: true);

            if (GCDReady(AID.AirAnchor) && (!AOEMode || !Unlocked(AID.Bioblaster)))
                PushGCD(AID.AirAnchor, target.Actor, 13 + bonus);

            if (!Unlocked(AID.AirAnchor) && GCDReady(AID.HotShot))
                PushGCD(AID.HotShot, target.Actor, 13 + bonus);

            var drillCapped = MaxChargesIn(AID.Drill) <= GCD;
            if (AOEMode && Unlocked(AID.Bioblaster))
            {
                if (GCDReady(AID.Bioblaster) && target.Actor.FindStatus(SID.Bioblaster, Player.InstanceID) == null)
                    PushGCD(AID.Bioblaster, BestConeTarget ?? target.Actor, 12 + bonus, faceTarget: true);
            }
            else if (GCDReady(AID.Drill))
            {
                PushGCD(AID.Drill, target.Actor, (drillCapped ? 17 : 12) + bonus);
            }
        }

        if (AOEMode)
        {
            if (strategy.Flamethrower.Value == FlamethrowerStrategy.Automatic && !IsMoving && GCDReady(AID.Flamethrower) && ReassembleLeft == 0)
                PushGCD(AID.Flamethrower, Player, 5);
            PushGCD(BestActionUnlocked(AID.Scattergun, AID.SpreadShot), BestConeTarget ?? target.Actor, 1, faceTarget: true);
            return;
        }

        if (World.Client.ComboState.Remaining > 0)
        {
            if (ComboLastMove is AID.SlugShot or AID.HeatedSlugShot && Unlocked(AID.CleanShot))
            {
                PushGCD(BestActionUnlocked(AID.HeatedCleanShot, AID.CleanShot), target.Actor, 1);
                return;
            }
            if (ComboLastMove is AID.SplitShot or AID.HeatedSplitShot && Unlocked(AID.SlugShot))
            {
                PushGCD(BestActionUnlocked(AID.HeatedSlugShot, AID.SlugShot), target.Actor, 1);
                return;
            }
        }
        PushGCD(BestActionUnlocked(AID.HeatedSplitShot, AID.SplitShot), target.Actor, 1);
    }

    // FMF goes right before the Wildfire window, or anywhere if the buff is about to expire; openers place it explicitly
    private bool UseFullMetalField()
    {
        if (StandardOpener)
            return WildfireLeft > 0;
        if (EarlyWFOpener)
            return !Overheated && HyperchargedLeft == 0 && WildfireLeft == 0;
        return ReadyIn(AID.Wildfire) <= GCD + GCDLength || WildfireLeft > 0 || FMFLeft < GCDLength * 3;
    }

    #endregion

    #region oGCD

    private void OGCD(in Strategy strategy, Enemy target)
    {
        var dying = target.Priority == Enemy.PriorityPointless;

        if (HasMinion && dying && strategy.Queen.Value != QueenStrategy.Delay)
            PushOGCD(BestActionUnlocked(AID.QueenOverdrive, AID.RookOverdrive), Player, 80);

        if (ShouldHypercharge(strategy, dying))
            PushOGCD(AID.Hypercharge, Player, 70);

        if (ShouldWildfire(strategy, dying))
            PushOGCD(AID.Wildfire, ResolveTarget(strategy.Wildfire) ?? target.Actor, 75, strategy.Wildfire.Value == WildfireStrategy.Force ? 0 : GCD - 0.8f);

        if (ShouldStabilize(strategy, dying))
            PushOGCD(AID.BarrelStabilizer, Player, 60);

        if (ShouldReassemble(strategy))
            PushOGCD(AID.Reassemble, Player, 55);

        if (ShouldQueen(strategy, dying))
            PushOGCD(AID.RookAutoturret, Player, 50);

        UseCharges(strategy, ResolveTarget(strategy.Charges) ?? BestSplashTarget ?? target.Actor, dying);
    }

    private bool ShouldHypercharge(in Strategy strategy, bool dying)
    {
        if (!Unlocked(AID.Hypercharge) || Overheated || HyperchargedLeft == 0 && Heat < 50 || !CanWeave(AID.Hypercharge))
            return false;

        switch (strategy.Hypercharge.Value)
        {
            case HyperchargeStrategy.Delay:
                return false;
            case HyperchargeStrategy.ASAP:
                return true;
        }

        // opener: Hypercharge straight into the Wildfire window
        if (StandardOpener && WildfireLeft > 0 && FMFLeft == 0 || EarlyWFOpener && WildfireLeft > 0 && ExcavatorLeft == 0)
            return true;

        // Full Metal Field and Reassembled tools go first
        if (dying || FMFLeft > 0 || ReassembleLeft > 0)
            return false;

        // Overheat lasts 5 GCDs; don't let a tool or Excavator come up during it
        var toolIn = MathF.Min(ReadyIn(AID.Drill), MathF.Min(ReadyIn(AID.AirAnchor), ReadyIn(AID.ChainSaw)));
        if (ExcavatorLeft > 0 || toolIn < GCD + GCDLength * 3 + 0.5f)
            return false;

        if (DowntimeIn < GCD + GCDLength * 5)
            return false;

        // Overheat takes ~7.5s; don't let the 1-2-3 combo expire during it
        if (World.Client.ComboState.Remaining is > 0 and < 7.6f)
            return false;

        // Wildfire window: enter Overheat right before Wildfire
        if (Unlocked(AID.Wildfire) && ReadyIn(AID.Wildfire) <= GCD && strategy.Wildfire.Value != WildfireStrategy.Delay)
            return true;

        // free Hypercharge from Barrel Stabilizer is always fine outside the Wildfire window
        if (HyperchargedLeft > 0)
            return ReadyIn(AID.Wildfire) > 10 || HyperchargedLeft < GCDLength * 2;

        // otherwise keep enough heat for the next Wildfire, unless we'd overcap
        return !Unlocked(AID.Wildfire) || ReadyIn(AID.Wildfire) > GCDLength * 15 || Heat >= 95;
    }

    private bool ShouldWildfire(in Strategy strategy, bool dying)
    {
        if (!Unlocked(AID.Wildfire) || !CanWeave(AID.Wildfire))
            return false;
        return strategy.Wildfire.Value switch
        {
            WildfireStrategy.Force => true,
            WildfireStrategy.Automatic => !dying && WildfireLeft == 0 && (Overheated || OpenerWildfire),
            _ => false
        };
    }

    // opener Wildfire goes in before Overheat: after the second Drill (standard) or after Chain Saw (early)
    private bool OpenerWildfire
        => StandardOpener && FMFLeft > 0 && ExcavatorLeft == 0 && !GCDReady(AID.Drill) && !GCDReady(AID.ChainSaw)
        || EarlyWFOpener && ExcavatorLeft > 0;

    private bool ShouldStabilize(in Strategy strategy, bool dying)
    {
        if (!Unlocked(AID.BarrelStabilizer) || !CanWeave(AID.BarrelStabilizer) || FMFLeft > 0 || HyperchargedLeft > 0)
            return false;
        return strategy.Stabilizer.Value switch
        {
            OffensiveStrategy.Force => true,
            OffensiveStrategy.Automatic => !dying && (!Unlocked(AID.Wildfire) || ReadyIn(AID.Wildfire) <= 20),
            _ => false
        };
    }

    private bool ShouldReassemble(in Strategy strategy)
    {
        if (strategy.Reassemble.Value == ReassembleStrategy.Delay || ReassembleLeft > 0 || Overheated || !CanWeave(AID.Reassemble))
            return false;

        if (StandardOpener && Unlocked(AID.Excavator))
            return NextGCD == AID.Drill && MaxChargesIn(AID.Drill) > GCD;
        if (EarlyWFOpener)
            return NextGCD == AID.ChainSaw;

        return NextGCD switch
        {
            AID.Drill or AID.AirAnchor or AID.ChainSaw or AID.Excavator => true,
            AID.CleanShot or AID.HeatedCleanShot => !Unlocked(AID.Drill),
            AID.HotShot => !Unlocked(AID.CleanShot),
            AID.SpreadShot or AID.Scattergun => strategy.Reassemble.Value == ReassembleStrategy.Any,
            _ => false
        };
    }

    private bool ShouldQueen(in Strategy strategy, bool dying)
    {
        if (!Unlocked(AID.RookAutoturret) || HasMinion || Battery < 50 || dying || !CanWeave(AID.RookAutoturret))
            return false;

        return strategy.Queen.Value switch
        {
            QueenStrategy.Fifty => true,
            QueenStrategy.Hundred => Battery == 100,
            QueenStrategy.Automatic => Battery == 100 || Battery >= 90 && BatteryFrom(NextGCD) > 0 || RaidBuffsLeft > GCD && Battery >= 50,
            _ => false
        };
    }

    private static int BatteryFrom(AID action) => action switch
    {
        AID.ChainSaw or AID.AirAnchor or AID.Excavator or AID.HotShot => 20,
        AID.CleanShot or AID.HeatedCleanShot => 10,
        _ => 0
    };

    private void UseCharges(in Strategy strategy, Actor target, bool dying)
    {
        if (strategy.Charges.Value == ChargeStrategy.Delay)
            return;

        // never sit at max charges
        foreach (var aid in (ReadOnlySpan<AID>)[AID.GaussRound, AID.Ricochet])
            if (Unlocked(aid) && MaxChargesIn(aid) <= GCD + 0.6f && CanWeave(aid))
                PushOGCD(aid, target, 45);

        if (strategy.Charges.Value != ChargeStrategy.Automatic)
            return;

        var spend = Overheated || WildfireLeft > 0 || dying || InOpener || RaidBuffsLeft > GCD || RaidBuffsIn > 9000 || !Unlocked(AID.Hypercharge);
        if (!spend)
            return;

        // alternate: whichever recharges sooner gets used first
        var gauss = MaxChargesIn(AID.GaussRound);
        var rico = MaxChargesIn(AID.Ricochet);
        if (CanWeave(AID.GaussRound))
            PushOGCD(AID.GaussRound, target, gauss <= rico ? 11 : 10);
        if (CanWeave(AID.Ricochet))
            PushOGCD(AID.Ricochet, target, rico < gauss ? 11 : 10);
    }

    #endregion

    #region Helpers

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithBurst => World.Client.CountdownRemaining is > 0 and < 2 || Player.InCombat && Unlocked(AID.BarrelStabilizer) && ReadyIn(AID.BarrelStabilizer) < 6,
        PotionStrategy.AlignWithRaidBuffs => World.Client.CountdownRemaining is > 0 and < 2 || Player.InCombat && (RaidBuffsLeft > 0 || RaidBuffsIn < 5),
        PotionStrategy.Immediate => true,
        _ => false
    };

    // pick the enemy in range whose AOE would hit the most priority targets without touching a forbidden one
    private (Actor? Best, int Count) BestAOETarget(Enemy? primary, float range, bool allowAoE, Func<Actor, Actor, bool> hits)
    {
        if (primary == null)
            return (null, 0);

        int Count(Actor center) => Hints.ForbiddenTargets.Any(e => hits(center, e.Actor)) ? 0 : Hints.PriorityTargets.Count(e => hits(center, e.Actor));

        var best = primary.Actor;
        var bestCount = Count(best);
        if (!allowAoE || TargetMode == Targeting.Manual)
            return (best, bestCount);

        foreach (var e in Hints.PriorityTargets)
        {
            if (e.Actor == primary.Actor || Player.DistanceToHitbox(e.Actor) > range || TargetMode == Targeting.AutoPrimary && !hits(e.Actor, primary.Actor))
                continue;
            var c = Count(e.Actor);
            if (c > bestCount)
                (best, bestCount) = (e.Actor, c);
        }
        return (best, bestCount);
    }

    private bool Unlocked(AID aid) => ActionUnlocked(aid);

    private float ReadyIn(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) : float.MaxValue;
    private float MaxChargesIn(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.ChargeCapIn(World.Client.Cooldowns, World.Client.DutyActions, Player.Level) : float.MaxValue;
    private bool GCDReady(AID aid) => ReadyIn(aid) < GCD + 0.05f;

    private bool CanWeave(AID aid)
    {
        if (!Unlocked(aid))
            return false;
        var def = ActionDefinitions.Instance.Spell(aid)!;
        return MathF.Max(ReadyIn(aid), World.Client.AnimationLock) + def.TotalDuration + AnimLockDelay <= GCD;
    }

    private void PushGCD(AID aid, Actor? target, int priority, bool faceTarget = false)
    {
        if (PushAction(aid, target, ActionQueue.Priority.High + priority, 0, faceTarget) && priority > NextGCDPrio)
        {
            NextGCD = aid;
            NextGCDPrio = priority;
        }
    }

    private void PushOGCD(AID aid, Actor? target, int priority, float delay = 0)
        => PushAction(aid, target, ActionQueue.Priority.Low + priority, delay, false);

    private bool PushAction(AID aid, Actor? target, float priority, float delay, bool faceTarget)
    {
        if (aid == AID.None || !Unlocked(aid))
            return false;
        var def = ActionDefinitions.Instance.Spell(aid);
        if (def == null || def.Range != 0 && target == null)
            return false;
        Angle? facing = faceTarget && target != null ? Player.AngleTo(target) : null;
        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority, delay: delay, facingAngle: facing);
        return true;
    }

    #endregion
}
