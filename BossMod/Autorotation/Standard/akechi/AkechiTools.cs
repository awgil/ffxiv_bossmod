using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using static BossMod.AIHints;
using static FFXIVClientStructs.FFXIV.Client.Game.ActionManager;

namespace BossMod.Autorotation.akechi;

public enum SharedTrack { Targeting, Hold, Potion, Count }
public enum SoftTargetStrategy { Automatic, AutoHard, Manual }
public enum HoldStrategy { DontHold, HoldCooldowns, HoldGauge, HoldBuffs, HoldAbilities, HoldEverything }
public enum PotionStrategy { Manual, AlignWithBuffs, AlignWithRaidBuffs, Immediate }
public enum GCDStrategy { Automatic, RaidBuffsOnly, Force, Delay }
public enum OGCDStrategy { Automatic, RaidBuffsOnly, Force, AnyWeave, EarlyWeave, LateWeave, Delay }
public enum AllowOrForbid { Allow, Force, Forbid }

public abstract class AkechiTools<AID, TraitID>(RotationModuleManager manager, Actor player) : RotationModule(manager, player) where AID : struct, Enum where TraitID : Enum
{
    #region ActionQueue
    protected bool QueueAction(AID aid, Actor? target, float priority, float delay, float castTime, Vector3 targetPos = default, Angle? facingAngle = null)
    {
        var ability = ActionDefinitions.Instance.Spell(aid);
        if (ability == null ||
            (uint)(object)aid == 0 ||
            (ability.Range != 0 && target == null))
            return false;

        if (ability.AllowedTargets.HasFlag(ActionTargets.Area))
        {
            if (ability.Range == 0)
                targetPos = Player.PosRot.XYZ();
            else if (target != null)
                targetPos = target.PosRot.XYZ();
        }
        if (castTime == 0)
            castTime = 0;

        Hints.ActionsToExecute.Push(
            ActionID.MakeSpell(aid),
            target,
            priority,
            delay: delay,
            castTime: castTime,
            targetPos: targetPos,
            facingAngle: facingAngle);

        return true;
    }
    protected void QueuePot(ActionID pot) => Hints.ActionsToExecute.Push(pot, Player, ActionQueue.Priority.High);
    protected void QueuePotSTR() => QueuePot(ActionDefinitions.IDPotionStr);
    protected void QueuePotINT() => QueuePot(ActionDefinitions.IDPotionInt);
    protected void QueuePotDEX() => QueuePot(ActionDefinitions.IDPotionDex);
    protected void QueuePotMND() => QueuePot(ActionDefinitions.IDPotionMnd);

    //GCD
    protected AID NextGCD;
    protected int NextGCDPrio;

    protected void QueueGCDs(AID aid, Actor? target, int priority = 0, float delay = 0, float castTime = 0, Vector3 targetPos = default, Angle? facingAngle = null)
    {
        if (priority == 0)
            return;

        if (QueueAction(aid, target, ActionQueue.Priority.High + priority, delay, castTime, targetPos, facingAngle) && priority > NextGCDPrio)
        {
            NextGCD = aid;
            NextGCDPrio = priority;
        }
    }

    protected void QueueGCD<P>(AID aid, Actor? target, P priority, float delay = 0, float castTime = 0, Vector3 targetPos = default, Angle? facingAngle = null)
    where P : Enum => QueueGCDs(aid, target, (int)(object)priority, delay, castTime, targetPos, facingAngle);

    protected bool ShouldUseGCD(GCDStrategy strategy, Actor? target, bool ready, bool optimal = true) => ready && strategy switch
    {
        GCDStrategy.Automatic => target != null && optimal,
        GCDStrategy.RaidBuffsOnly => RaidBuffsLeft > 0f,
        GCDStrategy.Force => true,
        _ => false
    };

    //OGCD
    protected AID NextOGCD;
    protected int NextOGCDPrio;

    protected void QueueOGCDs(AID aid, Actor? target, int priority = 0, float delay = 0, float castTime = 0, Vector3 targetPos = default, Angle? facingAngle = null)
    {
        if (priority == 0)
            return;

        if (QueueAction(aid, target, ActionQueue.Priority.Low + priority, delay, castTime, targetPos, facingAngle) && priority > NextOGCDPrio)
        {
            NextOGCD = aid;
            NextOGCDPrio = priority;
        }
    }

    protected void QueueOGCD<P>(AID aid, Actor? target, P priority, float delay = 0, float castTime = 0, Vector3 targetPos = default, Angle? facingAngle = null)
        where P : Enum => QueueOGCDs(aid, target, (int)(object)priority, delay, castTime, targetPos, facingAngle);

    protected bool ShouldUseOGCD(OGCDStrategy strategy, Actor? target, bool ready, bool optimal = true) => ready && strategy switch
    {
        OGCDStrategy.Automatic => target != null && optimal,
        OGCDStrategy.RaidBuffsOnly => RaidBuffsLeft > 0f,
        OGCDStrategy.Force => true,
        OGCDStrategy.AnyWeave => CanWeaveIn,
        OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => CanLateWeaveIn,
        _ => false
    };
    #endregion

    #region States
    protected uint HP { get; private set; }
    protected uint MaxHP { get; private set; }
    protected uint MP { get; private set; }
    protected uint MaxMP { get; private set; }
    protected float AnimationLockDelay { get; private set; }
    protected float DowntimeIn { get; private set; }
    protected float? UptimeIn { get; private set; }
    protected float CombatTimer { get; private set; }
    protected float? CountdownRemaining { get; private set; }
    protected float? ComboTimer { get; private set; }
    protected bool IsMoving { get; private set; }
    protected bool CanTrueNorth { get; private set; }
    protected bool HasTrueNorth { get; private set; }
    protected bool CanSwiftcast { get; private set; }
    protected bool HasSwiftcast { get; private set; }
    protected float HPP(Actor? actor = null) => actor == null || actor.IsDead ? 0f : (float)actor.HPMP.CurHP / actor.HPMP.MaxHP * 100;
    protected float PlayerHPP => HPP(Player);
    protected bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    protected bool Unlocked(TraitID tid) => TraitUnlocked((uint)(object)tid);
    protected AID ComboLastMove => (AID)(object)World.Client.ComboState.Action;
    protected float SkSGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
    protected float SpSGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level);
    protected bool CanFitSkSGCD(float duration, int extraGCDs = 0) => GCD + SkSGCDLength * extraGCDs < duration;
    protected bool IsFirstGCD => !Player.InCombat || CombatTimer < 0.1f;
    protected bool InOddWindow(AID aid) => Cooldown(aid) is < 90 and > 30;
    protected bool InCombat(Actor? target) => Player.InCombat && target != null;
    protected float ReadyIn(AID aid) => ActionDefinitions.Instance.Spell(aid)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions);
    protected float Cooldown(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;
    protected bool ActionReady(AID aid) => Unlocked(aid) && Cooldown(aid) <= 0.5f;
    protected bool LastActionUsed(AID aid) => Manager.LastCast.Data?.IsSpell(aid) == true || Manager.LastCast.Data?.Action == ActionID.MakeSpell(aid);
    protected bool HasEffect<SID>(SID sid) where SID : Enum => Player.FindStatus(sid) != null;
    protected int StacksRemaining<SID>(Actor? target, SID sid, float duration = 1000f) where SID : Enum => StatusDetails(target, sid, Player.InstanceID, duration).Stacks;
    protected float StatusRemaining<SID>(Actor? target, SID sid, float duration = 1000f) where SID : Enum => StatusDetails(target, sid, Player.InstanceID, duration).Left;
    protected bool StatusExpiringSoon<SID>(float numGCDs, params SID[] statuses) where SID : Enum
    {
        var timer = SkSGCDLength * numGCDs;
        return statuses.Any(status => HasEffect(status) && StatusRemaining(Player, status) < timer);
    }
    protected bool CanWeaveIn => GCD >= 0.6f;
    protected bool CanEarlyWeaveIn => GCD >= 1.26f;
    protected bool CanLateWeaveIn => GCD is <= 1.25f and >= 0.6f;
    protected bool CanQuarterWeaveIn => GCD is < 0.9f and >= 0.5f;
    protected unsafe float ActualComboTimer => Instance()->Combo.Timer;
    protected unsafe bool ComboExpiringSoon(float NumGCDs)
    {
        var GCD = SkSGCDLength * NumGCDs;
        return ActualComboTimer != 0 && ActualComboTimer < GCD;
    }
    #endregion

    #region Targeting
    protected Enemy? PlayerTarget { get; private set; }
    protected delegate bool PositionCheck(Actor playerTarget, Actor targetToTest);
    protected delegate P PriorityFunc<P>(int totalTargets, Actor primaryTarget);

    protected bool TargetsInAOECircle(float range = 3f, int numTargets = 1) => Hints.NumPriorityTargetsInAOECircle(Player.Position, range) >= numTargets;
    protected Actor? SingleTargetChoice(Actor? manual, StrategyValues.OptionRef track) => ResolveTargetOverride(track.Value) ?? manual;
    protected Actor? AOETargetChoice(Actor? manual, Actor? auto, StrategyValues.OptionRef track, StrategyValues strategy) => ResolveTargetOverride(track.Value) ?? (strategy.AutoTarget() ? auto : manual);

    //position checks
    protected PositionCheck IsSplashTarget => (primary, other) => Hints.TargetInAOECircle(other, primary.Position, 5);
    protected PositionCheck Is10ySplashTarget => (primary, other) => Hints.TargetInAOECircle(other, primary.Position, 10);
    protected PositionCheck ConeTargetCheck(float range) => (primary, other) => Hints.TargetInAOECone(other, Player.Position, range, Player.DirectionTo(primary), 45.Degrees());
    protected PositionCheck Is12yConeTarget => ConeTargetCheck(12);
    protected PositionCheck LineTargetCheck(float range, float halfWidth = 2) => (primary, other) => TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), range, halfWidth);
    protected PositionCheck Is10yRectTarget => LineTargetCheck(10);
    protected PositionCheck Is25yRectTarget => LineTargetCheck(25);

    //distance checks
    protected bool DistanceFrom(Actor? target, float maxDistance) => Player.DistanceToHitbox(target) <= maxDistance;
    protected bool In3y(Actor? target) => DistanceFrom(target, 3.00f);
    protected bool In5y(Actor? target) => DistanceFrom(target, 5.00f);
    protected bool In10y(Actor? target) => DistanceFrom(target, 10.00f);
    protected bool In12y(Actor? target) => DistanceFrom(target, 12.00f);
    protected bool In15y(Actor? target) => DistanceFrom(target, 15.00f);
    protected bool In20y(Actor? target) => DistanceFrom(target, 20.00f);
    protected bool In25y(Actor? target) => DistanceFrom(target, 25.00f);

    protected void GetNextTarget(StrategyValues strategy, ref Enemy? primaryTarget, float range)
    {
        if (strategy.ManualTarget())
            return;

        var currentTarget = primaryTarget?.Actor;
        if (currentTarget == null || Player.DistanceToHitbox(currentTarget) > range)
        {
            var newTarget = Hints.PriorityTargets.FirstOrDefault(x => Player.DistanceToHitbox(x.Actor) <= range);
            if (newTarget != null)
            {
                if (strategy.AutoTargeting())
                {
                    primaryTarget = newTarget;
                    PlayerTarget = primaryTarget;
                }
                if (strategy.AutoHardTargeting())
                {
                    primaryTarget = newTarget;
                    Hints.ForcedTarget = primaryTarget?.Actor;
                }
            }
        }
    }

    protected (Enemy? Best, int Priority) GetTarget<P>
        (Enemy? primaryTarget,
        float range,
        PositionCheck isInAOE,
        PriorityFunc<P> prioritize,
        Func<P, int> simplify
        ) where P : struct, IComparable
    {
        P targetPriority(Actor potentialTarget)
            => prioritize(Hints.NumPriorityTargetsInAOE(enemy => isInAOE(potentialTarget, enemy.Actor)), potentialTarget);

        var (newTarget, newPrio) = FindBetterTargetBy(primaryTarget?.Actor, range, targetPriority);
        var simplifiedPrio = simplify(newPrio);
        return (simplifiedPrio > 0 ? Hints.FindEnemy(newTarget) : null, simplifiedPrio);
    }

    protected (Enemy? Best, int Targets) GetBestTarget(Enemy? primaryTarget, float range, PositionCheck isInAOE)
        => GetTarget(primaryTarget, range, isInAOE, (targets, _) => targets, a => a);

    protected (Enemy? Best, P Timer) GetDOTTarget<P>(Enemy? initial, Func<Actor?, P> getTimer, int maxAllowedTargets, float range = 30)
        where P : struct, IComparable
    {
        if (initial == null || maxAllowedTargets <= 0 || Player.DistanceToHitbox(initial.Actor) > range)
            return (null, getTimer(null));

        var bestTarget = initial;
        var bestTimer = getTimer(initial.Actor);
        var count = 0;

        foreach (var target in Hints.PriorityTargets)
        {
            if (target.ForbidDOTs || Player.DistanceToHitbox(target.Actor) > range)
                continue;

            if (++count > maxAllowedTargets)
                break;

            var timer = getTimer(target.Actor);
            if (timer.CompareTo(bestTimer) < 0)
            {
                bestTarget = target;
                bestTimer = timer;
            }
        }

        return (bestTarget, bestTimer);
    }

    #region PvP
    protected unsafe bool HasLOS(Actor? actor)
    {
        if (actor == null || actor.IsDeadOrDestroyed)
            return false;

        //TODO: there is weird behavior when it comes to striking dummies
        //they have a different Y coord needed for this to function correctly (works with 4), as the current correct value of 2 will not work unless up close
        //we don't care about LOS for striking dummies since they really don't matter and are mainly used for testing/observing
        //leaving this as true is ok for now, but may need a small revision in the future when someone actually cares
        if (actor.IsStrikingDummy)
            return true;

        var sourcePos = Player.Position.ToVec3() with { Y = actor.Position.ToVec3(2f).Y };
        var targetPos = actor.Position.ToVec3() with { Y = actor.Position.ToVec3(2f).Y };
        var offset = targetPos - sourcePos;
        var distance = offset.Length();
        var direction = offset / distance;
        RaycastHit hit;
        var flags = stackalloc int[] { 0x4000, 0, 0x4000, 0 };
        return !Framework.Instance()->BGCollisionModule->RaycastMaterialFilter(&hit, &sourcePos, &direction, distance, 1, flags);
    }
    public bool EnemiesTargetingSelf(int numEnemies)
        => Hints.PotentialTargets.Count(x => !x.Actor.IsDeadOrDestroyed && x.Actor.TargetID == Player.InstanceID) >= numEnemies;
    protected void GetPvPTarget(float range)
    {
        Actor? RetrieveTarget(Func<Enemy, bool> conditions)
            => Hints.PriorityTargets
                .Where(x => conditions(x) && HasLOS(x.Actor) && Player.DistanceToHitbox(x.Actor) <= range)
                .OrderBy(x => (float)x.Actor.HPMP.CurHP / x.Actor.HPMP.MaxHP)
                .FirstOrDefault()?.Actor;

        //high priority - full checks for invulnerable/resistant statuses
        var high = RetrieveTarget(x =>
            !x.Actor.IsStrikingDummy &&
            x.Actor.NameID == 0 &&
            x.Actor.FindStatus(3039) == null &&
            x.Actor.FindStatus(1302) == null &&
            x.Actor.FindStatus(1301) == null &&
            x.Actor.FindStatus(1300) == null &&
            x.Actor.FindStatus(1978) == null &&
            x.Actor.FindStatus(1240) == null &&
            x.Actor.FindStatus(ClassShared.SID.GuardPvP) == null);

        //medium priority - only Guard check
        var medium = RetrieveTarget(x =>
            !x.Actor.IsStrikingDummy &&
            x.Actor.FindStatus(ClassShared.SID.GuardPvP) == null);

        //low priority - just in range and line of sight
        var low = RetrieveTarget(x => true);

        Hints.ForcedTarget =
            (Player.Class == Class.MCH && Player.FindStatus(MCH.SID.WildfirePlayerPvP) != null) //special case for MCH - prioritize Wildfire debuffed targets
                ? Hints.PriorityTargets.FirstOrDefault(x => HasLOS(x.Actor) && Player.DistanceToHitbox(x.Actor) <= range && x.Actor.FindStatus(MCH.SID.WildfireTargetPvP) != null)?.Actor
            : high ?? medium ?? low;
    }
    #endregion

    #endregion

    #region Buffs
    protected float RaidBuffsIn { get; private set; }

    protected float RaidBuffsLeft { get; private set; }

    private new (float Left, float In) EstimateRaidBuffTimings(Actor? primaryTarget)
    {
        if (Bossmods.ActiveModule?.Info?.GroupType == BossModuleInfo.GroupType.BozjaDuel && IsSelfish(Player.Class))
            return (float.MaxValue, 0);

        if (primaryTarget?.IsStrikingDummy == true)
        {
            var cycleTime = CombatTimer - 7.8f;
            if (cycleTime < 0)
                return (0, 7.8f - CombatTimer);

            cycleTime %= 120f;
            return cycleTime < 20f ? (20f - cycleTime, 0) : (0, 120f - cycleTime);
        }

        var buffsIn = Bossmods.RaidCooldowns.NextDamageBuffIn2();
        if (buffsIn == null)
        {
            var hasBuffs = CombatTimer < 7.8f && World.Party.WithoutSlot(includeDead: true, excludeAlliance: true, excludeNPCs: true).Skip(1).Any(PartyBuffCheck);
            buffsIn = hasBuffs ? 7.8f - CombatTimer : float.MaxValue;
        }

        return (Bossmods.RaidCooldowns.DamageBuffLeft(Player, primaryTarget), buffsIn.Value);
    }
    private bool IsSelfish(Class cls) => cls is Class.VPR or Class.SAM or Class.WHM or Class.SGE or Class.DRK;
    private bool PartyBuffCheck(Actor player) => player.Class switch
    {
        Class.MNK => player.Level >= 70,
        Class.DRG => player.Level >= 52,
        Class.NIN => player.Level >= 45,
        Class.RPR => player.Level >= 72,
        Class.SMN => player.Level >= 66,
        Class.RDM => player.Level >= 58,
        Class.PCT => player.Level >= 70,
        Class.BRD => player.Level >= 50,
        Class.DNC => player.Level >= 70,
        Class.SCH => player.Level >= 66,
        Class.AST => player.Level >= 50,
        _ => false
    };
    #endregion

    #region Positionals
    protected bool NextPositionalImminent;
    protected bool NextPositionalCorrect;

    protected Positional GetCurrentPositional(Actor target) => (Player.Position - target.Position).Normalized().Dot(target.Rotation.ToDirection()) switch
    {
        < -0.7071068f => Positional.Rear,
        < 0.7071068f => Positional.Flank,
        _ => Positional.Front
    };
    protected void UpdatePositionals(Enemy? enemy, ref (Positional pos, bool imm) positional)
    {
        var tn = HasTrueNorth;
        var target = enemy?.Actor;
        if ((target?.Omnidirectional ?? true) || target?.TargetID == Player.InstanceID && target?.CastInfo == null && positional.pos != Positional.Front && target?.NameID != 541)
            positional = (Positional.Any, false);

        NextPositionalImminent = !tn && positional.imm;
        NextPositionalCorrect = tn || target == null || positional.pos switch
        {
            Positional.Flank => MathF.Abs(target.Rotation.ToDirection().Dot((Player.Position - target.Position).Normalized())) < 0.7071067f,
            Positional.Rear => target.Rotation.ToDirection().Dot((Player.Position - target.Position).Normalized()) < -0.7071068f,
            _ => true
        };
        Hints.RecommendedPositional = (target, positional.pos, NextPositionalImminent, NextPositionalCorrect);
    }

    protected bool IsOnRear(Actor target) => In3y(target) && GetCurrentPositional(target) == Positional.Rear;
    protected bool IsOnFlank(Actor target) => In3y(target) && GetCurrentPositional(target) == Positional.Flank;

    #endregion

    #region AI
    protected void GoalZoneSingle(float range)
    {
        if (PlayerTarget != null)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(PlayerTarget.Actor, range));
    }

    protected void GoalZoneCombined(StrategyValues strategy, float range, Func<WPos, float> fAoe, AID firstUnlockedAoeAction, int minAoe, float? maximumActionRange = null)
    {
        var (_, positional, imminent, _) = Hints.RecommendedPositional;

        if (!Unlocked(firstUnlockedAoeAction))
            minAoe = 50;

        if (PlayerTarget == null)
        {
            if (minAoe < 50)
                Hints.GoalZones.Add(fAoe);
        }
        else
        {
            Hints.GoalZones.Add(Hints.GoalCombined(Hints.GoalSingleTarget(PlayerTarget.Actor, positional, range), fAoe, minAoe));
            if (maximumActionRange is float r)
                Hints.GoalZones.Add(Hints.GoalSingleTarget(PlayerTarget.Actor, r, 0.5f));
        }
    }

    protected void AnyGoalZoneCombined(float range, Func<WPos, float> fAoe, AID firstUnlockedAoeAction, int minAoe, float? maximumActionRange = null)
    {
        var (_, positional, imminent, _) = Hints.RecommendedPositional;

        if (!Unlocked(firstUnlockedAoeAction))
            minAoe = 50;

        if (PlayerTarget == null)
        {
            if (minAoe < 50)
                Hints.GoalZones.Add(fAoe);
        }
        else
        {
            Hints.GoalZones.Add(Hints.GoalCombined(Hints.GoalSingleTarget(PlayerTarget.Actor, imminent ? positional : Positional.Any, range), fAoe, minAoe));
            if (maximumActionRange is float r)
                Hints.GoalZones.Add(Hints.GoalSingleTarget(PlayerTarget.Actor, r, 0.5f));
        }
    }
    #endregion

    #region Priorities
    public enum GCDPriority //Base = 4000
    {
        None = 0,
        Minimal = 50,
        ExtremelyLow = 100,
        VeryLow = 150,
        Low = 200,
        ModeratelyLow = 250,
        SlightlyLow = 300,
        BelowAverage = 350,
        Average = 400,
        AboveAverage = 450,
        SlightlyHigh = 500,
        ModeratelyHigh = 550,
        High = 600,
        VeryHigh = 650,
        ExtremelyHigh = 700,
        Severe = 800,
        VerySevere = 850,
        Critical = 750,
        VeryCritical = 900,
        Max = 999,
        Forced = 1000 //This is really high already, should never be past 5000 total tbh
    }
    protected OGCDPriority OGCDPrio(OGCDStrategy strat, OGCDPriority defaultPrio) => strat switch
    {
        OGCDStrategy.Force => defaultPrio + 2000,
        OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave => defaultPrio + 1000,
        _ => defaultPrio
    };
    public enum OGCDPriority //Base = 2000
    {
        None = 0,
        Minimal = 50,

        ExtremelyLow = 100,
        VeryLow = 150,
        Low = 200,
        ModeratelyLow = 250,
        SlightlyLow = 300,
        BelowAverage = 350,
        Average = 400,
        AboveAverage = 450,
        SlightlyHigh = 500,
        ModeratelyHigh = 550,
        High = 600,
        VeryHigh = 650,
        ExtremelyHigh = 700,
        Severe = 750,
        VerySevere = 800,
        Critical = 850,
        VeryCritical = 900,
        Max = 999,
        Forced = 1500, //makes priority higher than CDPlanner's automatic prio + 500, which is really only Medium prio
        ToGCDPriority = 2000 //converts OGCDPriority to GCDPriority by adding 2000 to the value
    }

    protected OGCDPriority ChangePriority(int lowPrio = 200, int highPrio = 400, bool emergencyCondition = true, bool convert = true)
    {
        //in case we need a condition to use this at all - else bail early
        if (!emergencyCondition)
            return OGCDPriority.None;

        //convert to GCD if needed
        if (convert && GCD <= 0.5f)
            return OGCDPriority.ToGCDPriority + 1000; //5000

        //second weave
        if (GCD is < 1.25f and > 0.5f)
            return OGCDPriority.None + highPrio; //base 2000 + high value (400 default)

        //first weave
        return OGCDPriority.None + lowPrio; //base 2000 + low value (200 default)
    }

    #endregion

    public sealed override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        NextGCD = default;
        NextGCDPrio = 0;
        HP = Player.HPMP.CurHP;
        MaxHP = Player.HPMP.MaxHP;
        MP = Player.HPMP.CurMP;
        MaxMP = Player.HPMP.MaxMP;
        PlayerTarget = Hints.FindEnemy(primaryTarget);
        AnimationLockDelay = estimatedAnimLockDelay;
        IsMoving = isMoving;
        CombatTimer = (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds;
        ComboTimer = (float)(object)World.Client.ComboState.Remaining;
        CountdownRemaining = World.Client.CountdownRemaining;
        HasTrueNorth = StatusRemaining(Player, ClassShared.SID.TrueNorth, 15) > 0.1f;
        CanTrueNorth = !HasTrueNorth && ActionUnlocked(ActionID.MakeSpell(ClassShared.AID.TrueNorth)) && World.Client.Cooldowns[ActionDefinitions.Instance.Spell(ClassShared.AID.TrueNorth)!.MainCooldownGroup].Remaining < 45.6f;
        HasSwiftcast = StatusRemaining(Player, ClassShared.SID.Swiftcast, 10) > 0.1f;
        CanSwiftcast = ActionUnlocked(ActionID.MakeSpell(ClassShared.AID.Swiftcast)) && World.Client.Cooldowns[ActionDefinitions.Instance.Spell(ClassShared.AID.Swiftcast)!.MainCooldownGroup].Remaining < 0.6f;
        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget);
        if (Manager.Planner?.EstimateTimeToNextDowntime() is (var downtimeNow, var stateLeft))
        {
            DowntimeIn = downtimeNow ? 0 : stateLeft;
            UptimeIn = downtimeNow ? stateLeft : 0;
        }
        else
        {
            DowntimeIn = float.MaxValue;
            UptimeIn = null;
        }
        Execution(strategy, PlayerTarget);
    }

    public abstract void Execution(StrategyValues strategy, Enemy? primaryTarget);
}

static class ToolsExtensions
{
    public static RotationModuleDefinition.ConfigRef<SoftTargetStrategy> DefineTargeting(this RotationModuleDefinition res)
        => res.Define(SharedTrack.Targeting).As<SoftTargetStrategy>("Targeting", "", 295)
            .AddOption(SoftTargetStrategy.Automatic, "Allow auto-selecting best target for maximum optimal DPS output - does not change hard target")
            .AddOption(SoftTargetStrategy.AutoHard, "Auto-select best target for maximum optimal DPS output - will change hard target")
            .AddOption(SoftTargetStrategy.Manual, "Forbid auto-selecting best target, instead executing only on whichever target is currently selected");

    public static RotationModuleDefinition.ConfigRef<HoldStrategy> DefineHold(this RotationModuleDefinition res)
        => res.Define(SharedTrack.Hold).As<HoldStrategy>("Hold", "Allow or Forbid Abilities", 290)
            .AddOption(HoldStrategy.DontHold, "Allow use of all cooldowns, buffs, or gauge abilities")
            .AddOption(HoldStrategy.HoldCooldowns, "Forbid use of all cooldowns only")
            .AddOption(HoldStrategy.HoldGauge, "Forbid use of all gauge abilities only")
            .AddOption(HoldStrategy.HoldBuffs, "Forbid use of all raidbuffs or buff-related abilities only")
            .AddOption(HoldStrategy.HoldAbilities, "Forbid use of all cooldowns, buffs, and gauge abilities")
            .AddOption(HoldStrategy.HoldEverything, "Forbid complete use of ALL actions; standard rotations included");

    public static RotationModuleDefinition.ConfigRef<PotionStrategy> DefinePotion(this RotationModuleDefinition res, ActionID pot)
        => res.Define(SharedTrack.Potion).As<PotionStrategy>("Pots", "Potion Usage (Tinctures, Gemdraughts, etc.)", 280)
            .AddOption(PotionStrategy.Manual, "Use potion manually")
            .AddOption(PotionStrategy.AlignWithBuffs, "Use potion when personal buffs are imminent or active")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "Use potion when party raid buffs are imminent or active")
            .AddOption(PotionStrategy.Immediate, "Use potion immediately without restriction", 270, 30)
            .AddAssociatedAction(pot);

    public static RotationModuleDefinition.ConfigRef<GCDStrategy> DefineGCD<Index, AID>
        (this RotationModuleDefinition res, Index track, AID aid, string internalName, string displayName = "", int uiPriority = 100,
        float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100)
        where Index : Enum
        where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        return res.Define(track).As<GCDStrategy>(internalName, displayName, uiPriority)
            .AddOption(GCDStrategy.Automatic, $"Automatically use {action.Name()} when optimal", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(GCDStrategy.RaidBuffsOnly, $"Automatically use {action.Name()} only when there are raid buffs active", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(GCDStrategy.Force, $"Force use of {action.Name()} ASAP", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(GCDStrategy.Delay, $"Delay use of {action.Name()}", 0, 0, ActionTargets.None, minLevel, maxLevel)
            .AddAssociatedActions(aid);
    }

    public static RotationModuleDefinition.ConfigRef<OGCDStrategy> DefineOGCD<Index, AID>
        (this RotationModuleDefinition res, Index track, AID aid, string internalName, string displayName = "", int uiPriority = 100,
        float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100)
        where Index : Enum
        where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        return res.Define(track).As<OGCDStrategy>(internalName, displayName: displayName, uiPriority: uiPriority)
            .AddOption(OGCDStrategy.Automatic, $"Automatically use {action.Name()} when optimal", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel: maxLevel)
            .AddOption(OGCDStrategy.RaidBuffsOnly, $"Use {action.Name()} when raid buffs are active", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel: maxLevel)
            .AddOption(OGCDStrategy.Force, $"Force use of {action.Name()} ASAP", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel: maxLevel)
            .AddOption(OGCDStrategy.AnyWeave, $"Force use of {action.Name()} in next possible weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel: maxLevel)
            .AddOption(OGCDStrategy.EarlyWeave, $"Force use of {action.Name()} in next possible early-weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel: maxLevel)
            .AddOption(OGCDStrategy.LateWeave, $"Force use of {action.Name()} in next possible late-weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel: maxLevel)
            .AddOption(OGCDStrategy.Delay, $"Delay use of {action.Name()}", 0, 0, ActionTargets.None, minLevel: minLevel, maxLevel: maxLevel)
            .AddAssociatedActions(aid);
    }

    public static RotationModuleDefinition.ConfigRef<AllowOrForbid> DefineAllow<Index, AID>
        (this RotationModuleDefinition res, Index track, AID aid, string internalName, string displayName = "", int uiPriority = 100,
        float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100)
        where Index : Enum
        where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        return res.Define(track).As<AllowOrForbid>(internalName, displayName: displayName, uiPriority: uiPriority)
            .AddOption(AllowOrForbid.Allow, $"Allow use of {action.Name()} when available", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel: maxLevel)
            .AddOption(AllowOrForbid.Force, $"Force use {action.Name()} ASAP", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel: maxLevel)
            .AddOption(AllowOrForbid.Forbid, $"Forbid use of {action.Name()} entirely", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel: maxLevel)
            .AddAssociatedActions(aid);
    }

    public static SoftTargetStrategy Targeting(this StrategyValues strategy) => strategy.Option(SharedTrack.Targeting).As<SoftTargetStrategy>();
    public static bool AutoTarget(this StrategyValues strategy) => strategy.Targeting() is SoftTargetStrategy.Automatic or SoftTargetStrategy.AutoHard;
    public static bool AutoTargeting(this StrategyValues strategy) => strategy.Targeting() == SoftTargetStrategy.Automatic;
    public static bool AutoHardTargeting(this StrategyValues strategy) => strategy.Targeting() == SoftTargetStrategy.AutoHard;
    public static bool ManualTarget(this StrategyValues strategy) => strategy.Targeting() == SoftTargetStrategy.Manual;

    public static HoldStrategy Hold(this StrategyValues strategy) => strategy.Option(SharedTrack.Hold).As<HoldStrategy>();
    public static bool HoldEverything(this StrategyValues strategy) => strategy.Hold() == HoldStrategy.HoldEverything;
    public static bool HoldAbilities(this StrategyValues strategy) => strategy.Hold() == HoldStrategy.HoldAbilities;
    public static bool HoldBuffs(this StrategyValues strategy) => strategy.Hold() == HoldStrategy.HoldBuffs;
    public static bool HoldCDs(this StrategyValues strategy) => strategy.Hold() == HoldStrategy.HoldCooldowns;
    public static bool HoldGauge(this StrategyValues strategy) => strategy.Hold() == HoldStrategy.HoldGauge;

    public static PotionStrategy Potion(this StrategyValues strategy) => strategy.Option(SharedTrack.Potion).As<PotionStrategy>();
}
