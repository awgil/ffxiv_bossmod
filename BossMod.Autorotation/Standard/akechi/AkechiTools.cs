using BossMod.Data;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using static BossMod.AIHints;
using static FFXIVClientStructs.FFXIV.Client.Game.ActionManager;

namespace BossMod.Autorotation.akechi;

public enum SharedTrack { Targeting, Hold, Potion, Count }
public enum SoftTargetStrategy { Automatic, AutoHard, Manual }
public enum HoldStrategy { DontHold, HoldCooldowns, HoldGauge, HoldBuffs, HoldAbilities, HoldEverything }
public enum PotionStrategy { Manual, AlignWithBuffs, AlignWithRaidBuffs, Immediate }
public enum GCDStrategy { Automatic, Force, Delay }
public enum OGCDStrategy { Automatic, Force, AnyWeave, EarlyWeave, LateWeave, Delay }
public enum AllowOrForbid { Allow, Force, Forbid }

public abstract class AkechiTools<AID, TraitID>(RotationModuleManager manager, Actor player) : RotationModule(manager, player) where AID : struct, Enum where TraitID : Enum
{
    #region Action Queue
    protected AID NextGCD;
    private int NextGCDPrio;

    private bool QueueAction(AID aid, Actor? target, float priority, float delay, float castTime, Vector3 targetPos, Angle? facingAngle)
    {
        var ability = ActionDefinitions.Instance.Spell(aid);
        var blocked = ability == null || (uint)(object)aid == 0 || !ability.IsUnlocked(World, Player)
            || ability.Range != 0 && target == null
            || ability.Category == ActionCategory.Ability && Player.FindStatus(1092) != null //amnesia
            || Hints.FindEnemy(target)?.Priority is Enemy.PriorityInvincible or Enemy.PriorityForbidden;
        if (blocked)
            return false;

        if (ability!.AllowedTargets.HasFlag(ActionTargets.Area))
            targetPos = ability.Range == 0 ? Player.PosRot.XYZ() : target?.PosRot.XYZ() ?? targetPos;

        if (castTime == 0)
            castTime = GetSlidecastTime(aid);

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority, delay: delay, castTime: castTime, targetPos: targetPos, facingAngle: facingAngle);
        return true;
    }

    protected virtual float GetCastTime(AID aid)
    {
        var def = ActionDefinitions.Instance.Spell(aid);
        var instant = def == null || def.Category == ActionCategory.Spell && SwiftcastLeft > GCD;
        return instant ? 0 : def!.CastTime * (def.Category == ActionCategory.Spell ? SpSGCDLength : SkSGCDLength) / 2.5f;
    }
    protected float GetSlidecastTime(AID aid) => MathF.Max(0, GetCastTime(aid) - 0.5f);

    protected void QueueGCD<P>(AID aid, Actor? target, P priority, float delay = 0, float castTime = 0, Vector3 targetPos = default, Angle? facingAngle = null) where P : Enum
    {
        var prio = (int)(object)priority;
        if (prio != 0 && QueueAction(aid, target, ActionQueue.Priority.High + prio, delay, castTime, targetPos, facingAngle) && prio > NextGCDPrio)
            (NextGCD, NextGCDPrio) = (aid, prio);
    }
    protected void QueueOGCD<P>(AID aid, Actor? target, P priority, float delay = 0, float castTime = 0, Vector3 targetPos = default, Angle? facingAngle = null) where P : Enum
    {
        var prio = (int)(object)priority;
        if (prio != 0)
            QueueAction(aid, target, ActionQueue.Priority.Low + prio, delay, castTime, targetPos, facingAngle);
    }
    protected void QueuePotion(StrategyValues strategy, ActionID potion, bool alignedWithBuffs, float priority = ActionQueue.Priority.High, float delay = 0)
    {
        var condition = strategy.Potion() switch
        {
            PotionStrategy.AlignWithBuffs => Player.InCombat && alignedWithBuffs,
            PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 5 || RaidBuffsLeft > 0),
            PotionStrategy.Immediate => true,
            _ => false
        };
        if (condition)
            Hints.ActionsToExecute.Push(potion, Player, priority, delay: delay);
    }

    protected bool ShouldUseGCD(GCDStrategy strategy, Actor? target, bool ready, bool optimal = true) => ready && strategy switch
    {
        GCDStrategy.Automatic => target != null && optimal,
        GCDStrategy.Force => true,
        _ => false
    };
    protected bool ShouldUseOGCD(OGCDStrategy strategy, Actor? target, bool ready, bool optimal = true) => ready && strategy switch
    {
        OGCDStrategy.Automatic => target != null && optimal,
        OGCDStrategy.Force => true,
        OGCDStrategy.AnyWeave => CanWeaveIn,
        OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => CanLateWeaveIn,
        _ => false
    };
    #endregion

    #region State
    protected uint HP { get; private set; }
    protected uint MaxHP { get; private set; }
    protected uint MP { get; private set; }
    protected uint MaxMP { get; private set; }
    protected bool IsMoving { get; private set; }
    protected float CombatTimer { get; private set; }
    protected float? CountdownRemaining { get; private set; }
    protected float DowntimeIn { get; private set; }
    protected float RaidBuffsIn { get; private set; }
    protected float RaidBuffsLeft { get; private set; }
    protected bool HasTrueNorth { get; private set; }
    protected bool CanTrueNorth { get; private set; }
    private float SwiftcastLeft { get; set; }
    protected bool HasSwiftcast { get; private set; }
    protected bool CanSwiftcast { get; private set; }
    protected Enemy? PrimaryEnemy { get; private set; }
    protected Actor? PrimaryTarget => PrimaryEnemy?.Actor;
    protected float SkSGCDLength { get; private set; }
    private float SpSGCDLength { get; set; }

    protected float ComboTimer => World.Client.ComboState.Remaining;
    protected AID ComboLastMove => (AID)(object)World.Client.ComboState.Action;
    protected unsafe float ActualComboTimer => Instance()->Combo.Timer;
    protected unsafe AID LastComboAction => (AID)(object)Instance()->Combo.Action;

    protected bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    protected bool Unlocked(TraitID tid) => TraitUnlocked((uint)(object)tid);
    protected float Cooldown(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;
    protected float ReadyIn(AID aid) => ActionDefinitions.Instance.Spell(aid)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions);
    protected bool ActionReady(AID aid) => Unlocked(aid) && Cooldown(aid) <= 0.5f;
    protected bool LastActionUsed(AID aid) => Manager.LastCast.Data?.IsSpell(aid) == true || Manager.LastCast.Data?.Action == ActionID.MakeSpell(aid);
    protected bool InOddWindow(AID aid) => Cooldown(aid) is < 90 and > 30;

    private static readonly Dictionary<AID, int> MPCosts = [];
    protected virtual int MPCost(AID aid)
    {
        if (!MPCosts.TryGetValue(aid, out var cost))
            MPCosts[aid] = cost = Service.LuminaRow<Lumina.Excel.Sheets.Action>((uint)(object)aid) is { PrimaryCostType: 3 } row ? row.PrimaryCostValue * 100 : 0;
        return cost;
    }
    protected bool HasMP(AID aid) => MP >= MPCost(aid);

    protected bool CanFitSkSGCD(float duration, int extraGCDs = 0) => GCD + SkSGCDLength * extraGCDs < duration;
    protected bool IsFirstGCD => !Player.InCombat || CombatTimer < 0.1f;
    protected bool InCombat(Actor? target) => Player.InCombat && target != null;
    protected bool NoPrepullWithin(float seconds) => !Player.InCombat && (CountdownRemaining is not { } countdown || countdown <= seconds);

    protected bool CanWeaveIn => GCD >= 0.6f;
    protected bool CanEarlyWeaveIn => GCD >= 1.26f;
    protected bool CanLateWeaveIn => GCD is <= 1.25f and >= 0.6f;
    protected bool CanQuarterWeaveIn => GCD is < 0.9f and >= 0.5f;

    protected float Status<SID>(SID sid, float duration = 1000f, Actor? on = null) where SID : Enum => StatusDetails(on ?? Player, sid, Player.InstanceID, duration).Left;
    protected int Stacks<SID>(SID sid, float duration = 1000f, Actor? on = null) where SID : Enum => StatusDetails(on ?? Player, sid, Player.InstanceID, duration).Stacks;
    protected bool HasStatus<SID>(SID sid) where SID : Enum => Player.FindStatus(sid) != null;
    #endregion

    #region Targeting
    protected delegate bool PositionCheck(Actor playerTarget, Actor targetToTest);

    protected static PositionCheck IsSplashTarget { get; } = (primary, other) => TargetInAOECircle(other, primary.Position, 5);
    protected static PositionCheck Is10ySplashTarget { get; } = (primary, other) => TargetInAOECircle(other, primary.Position, 10);
    protected PositionCheck ConeTargetCheck(float range) => (primary, other) => TargetInAOECone(other, Player.Position, range, Player.DirectionTo(primary), 45.Degrees());
    protected PositionCheck Is12yConeTarget => ConeTargetCheck(12);
    protected PositionCheck LineTargetCheck(float range, float halfWidth = 2) => (primary, other) => TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), range, halfWidth);
    protected PositionCheck Is10yRectTarget => LineTargetCheck(10);
    protected PositionCheck Is15yRectTarget => LineTargetCheck(15);
    protected PositionCheck Is25yRectTarget => LineTargetCheck(25);

    protected bool DistanceFrom(Actor? target, float maxDistance) => Player.DistanceToHitbox(target) <= maxDistance;
    protected bool In3y(Actor? target) => DistanceFrom(target, 3);
    protected bool In5y(Actor? target) => DistanceFrom(target, 5);
    protected bool In10y(Actor? target) => DistanceFrom(target, 10);
    protected bool In12y(Actor? target) => DistanceFrom(target, 12);
    protected bool In15y(Actor? target) => DistanceFrom(target, 15);
    protected bool In20y(Actor? target) => DistanceFrom(target, 20);
    protected bool In25y(Actor? target) => DistanceFrom(target, 25);
    protected bool DashSafe(Actor? target) => target != null && ActionPredicate.IsDashSafe(Player.Position, target.Position, Hints);
    protected bool WorthDot(Actor? target, float seconds) => target != null && kage.TimeToKill.WorthDot(Bossmods.ActiveModule, target, seconds);

    protected Actor? SingleTargetChoice(Actor? manual, StrategyValues.OptionRef track) => ResolveTarget(track.Value) ?? manual;
    protected Actor? AOETargetChoice(Actor? manual, Actor? auto, StrategyValues.OptionRef track, StrategyValues strategy) => ResolveTarget(track.Value) ?? (strategy.AutoTarget() ? auto : manual);

    private List<Enemy> Enemies => Hints.PotentialTargets;
    private int NumPriorityTargets;
    private int FirstForbiddenTarget;

    private void CountTargets()
    {
        NumPriorityTargets = 0;
        while (NumPriorityTargets < Enemies.Count && Enemies[NumPriorityTargets].Priority == Hints.HighestPotentialTargetPriority)
            ++NumPriorityTargets;
        FirstForbiddenTarget = Enemies.Count;
        while (FirstForbiddenTarget > 0 && Enemies[FirstForbiddenTarget - 1].Priority <= Enemy.PriorityUndesirable)
            --FirstForbiddenTarget;
    }

    protected bool TargetsInAOECircle(float range = 3f, int numTargets = 1)
    {
        var origin = Player.Position;
        var forbidden = false;
        for (var i = FirstForbiddenTarget; i < Enemies.Count && !forbidden; ++i)
            forbidden = TargetInAOECircle(Enemies[i].Actor, origin, range);

        var count = 0;
        for (int i = 0, end = NumPriorityTargets; i < end && count < numTargets && !forbidden; ++i)
            if (TargetInAOECircle(Enemies[i].Actor, origin, range))
                ++count;
        return forbidden ? numTargets <= 0 : count >= numTargets;
    }

    protected void GetNextTarget(StrategyValues strategy, ref Enemy? primaryTarget, float range)
    {
        var mode = strategy.Targeting();
        Enemy? newTarget = null;
        var search = mode != SoftTargetStrategy.Manual && (primaryTarget == null || !DistanceFrom(primaryTarget.Actor, range));
        for (int i = 0, end = search ? NumPriorityTargets : 0; i < end && newTarget == null; ++i)
            if (DistanceFrom(Enemies[i].Actor, range))
                newTarget = Enemies[i];

        if (newTarget != null)
        {
            primaryTarget = newTarget;
            if (mode == SoftTargetStrategy.Automatic)
                GoalTarget = newTarget;
            else
                Hints.ForcedTarget = newTarget.Actor;
        }
    }

    protected (Enemy? Best, int Targets) GetBestTarget(Enemy? primaryTarget, float range, PositionCheck isInAOE)
    {
        var numPriority = NumPriorityTargets;
        var firstForbidden = FirstForbiddenTarget;
        var primary = primaryTarget?.Actor;
        var primaryUndesirable = primaryTarget?.Priority == Enemy.PriorityUndesirable;

        bool InRange(Actor actor) => actor.Position.InCircle(Player.Position, range + actor.HitboxRadius + 0.5f);

        int NumTargets(Actor candidate)
        {
            var exempt = primaryUndesirable && candidate == primary;
            var numForbidden = 0;
            var blocked = false;
            for (var i = firstForbidden; i < Enemies.Count && !blocked; ++i)
                blocked = isInAOE(candidate, Enemies[i].Actor) && (!exempt || ++numForbidden > 1);

            var numOk = 0;
            for (var i = 0; i < numPriority && !blocked; ++i)
                if (isInAOE(candidate, Enemies[i].Actor))
                    ++numOk;

            return blocked ? 0 : numForbidden == 0 ? Math.Max(1, numOk) : numOk == 0 ? 1 : 0;
        }

        var best = primary != null && InRange(primary) ? primary : null;
        var bestTargets = best != null ? NumTargets(best) : 0;
        for (var i = 0; i < numPriority && bestTargets < numPriority; ++i)
        {
            var candidate = Enemies[i].Actor;
            var targets = candidate != best && InRange(candidate) ? NumTargets(candidate) : 0;
            if (targets > bestTargets)
                (best, bestTargets) = (candidate, targets);
        }
        return (bestTargets > 0 ? Hints.FindEnemy(best) : null, bestTargets);
    }

    protected (Enemy? Best, P Timer) GetDOTTarget<P>(Enemy? initial, Func<Actor?, P> getTimer, int maxAllowedTargets, float range = 30) where P : struct, IComparable
    {
        var valid = initial != null && maxAllowedTargets > 0 && DistanceFrom(initial.Actor, range);
        var best = valid ? initial : null;
        var bestTimer = getTimer(best?.Actor);
        var checkedTargets = 0;
        for (int i = 0, end = valid ? NumPriorityTargets : 0; i < end && checkedTargets < maxAllowedTargets; ++i)
        {
            var target = Enemies[i];
            if (target.ForbidDOTs || !DistanceFrom(target.Actor, range))
                continue;

            ++checkedTargets;
            var timer = getTimer(target.Actor);
            if (timer.CompareTo(bestTimer) < 0)
                (best, bestTimer) = (target, timer);
        }
        return (best, bestTimer);
    }
    #endregion

    #region PvP
    private static readonly uint[] PvPImmunities = [3039, 1302, 1301, 1300, 1978, 1240];
    private static readonly uint[] TomelithOIDs = [0x1E9961, 0x1E9962, 0x1E9963, 0x1E9964, 0x1E9965, 0x1E9966, 0x1E9967, 0x1E9968, 0x1E995A, 0x1E995B, 0x1E995C, 0x1E995D, 0x1E995E, 0x1E995F];

    protected unsafe bool HasLOS(Actor? actor)
    {
        if (actor == null || actor.IsDeadOrDestroyed)
            return false;

        var sourcePos = Player.Position.ToVec3(2f);
        var targetPos = actor.Position.ToVec3(2f);
        var offset = targetPos - sourcePos;
        var distance = offset.Length();
        var direction = offset / MathF.Max(distance, 0.001f);
        RaycastHit hit;
        var flags = stackalloc int[] { 0x4000, 0, 0x4000, 0 };
        return actor.IsStrikingDummy || distance < 0.001f || !Framework.Instance()->BGCollisionModule->RaycastMaterialFilter(&hit, &sourcePos, &direction, distance, 1, flags);
    }

    protected int EnemiesTargetingPlayer
    {
        get
        {
            var count = 0;
            foreach (var enemy in Enemies)
                if (!enemy.Actor.IsDeadOrDestroyed && enemy.Actor.TargetID == Player.InstanceID)
                    ++count;
            return count;
        }
    }
    protected bool HPBelow(float ratio) => Player.PendingHPRatio > 0 && Player.PendingHPRatio < ratio;
    protected bool HPAtOrBelow(float ratio) => Player.PendingHPRatio > 0 && Player.PendingHPRatio <= ratio;
    protected Actor? FindActor(uint oid)
    {
        Actor? found = null;
        foreach (var actor in World.Actors)
        {
            if (actor.OID != oid)
                continue;
            found = actor;
            break;
        }
        return found;
    }

    private readonly List<Enemy> PvPTargets = [];

    protected void GetPvPTarget(float range, bool wantDuosTarget = false)
    {
        PvPTargets.Clear();
        foreach (var enemy in Enemies)
            if (DistanceFrom(enemy.Actor, range) && HasLOS(enemy.Actor))
                PvPTargets.Add(enemy);

        var tomelith = FindTomelith();
        var best = PvPTargets.Count == 0 ? null
            : LowestHP(a => a.OID is 0x15E8 or 0xC19 or 0x47BD || tomelith != null && a.TargetID == tomelith.InstanceID)
            ?? (Player.Class == Class.MCH && Player.FindStatus(MCH.SID.WildfirePlayerPvP) != null ? FirstPriority(static a => a.FindStatus(MCH.SID.WildfireTargetPvP) != null) : null)
            ?? LowestHP(static a => !a.IsStrikingDummy && a.NameID == 0 && !IsGuarded(a) && !IsImmune(a))
            ?? FirstPriority(static a => a.OID == 0x15E7)
            ?? LowestHP(static a => !a.IsStrikingDummy && !IsGuarded(a))
            ?? LowestHP(static _ => true);

        if (best != null && wantDuosTarget && Service.TargetManager.FocusTarget is { } focus)
        {
            var focusTarget = focus.TargetObjectId;
            best = LowestHP(a => a.InstanceID == focusTarget) ?? best;
        }

        Hints.ForcedTarget = best?.Actor;
    }

    protected Enemy? HighestHPPvPTarget(float range)
    {
        Enemy? best = null;
        foreach (var enemy in Enemies)
            if (DistanceFrom(enemy.Actor, range) && HasLOS(enemy.Actor) && (best == null || enemy.Actor.PendingHPRatio > best.Actor.PendingHPRatio))
                best = enemy;
        return best;
    }

    private Enemy? LowestHP(Func<Actor, bool> condition)
    {
        Enemy? best = null;
        foreach (var enemy in PvPTargets)
            if (condition(enemy.Actor) && (best == null || enemy.Actor.PendingHPRatio < best.Actor.PendingHPRatio))
                best = enemy;
        return best;
    }
    private Enemy? FirstPriority(Func<Actor, bool> condition)
    {
        Enemy? found = null;
        foreach (var enemy in PvPTargets)
        {
            if (enemy.Priority != Hints.HighestPotentialTargetPriority || !condition(enemy.Actor))
                continue;
            found = enemy;
            break;
        }
        return found;
    }
    private Actor? FindTomelith()
    {
        Actor? found = null;
        foreach (var actor in World.Actors)
        {
            if (Array.IndexOf(TomelithOIDs, actor.OID) < 0)
                continue;
            found = actor;
            break;
        }
        return found;
    }

    protected static bool IsGuarded(Actor actor) => actor.FindStatus(ClassShared.SID.GuardPvP) != null;
    private static bool IsImmune(Actor actor)
    {
        var immune = false;
        foreach (var id in PvPImmunities)
            immune |= actor.FindStatus(id) != null;
        return immune;
    }
    #endregion

    #region Raid Buffs
    private const float OpenerBuffsAt = 7.8f;
    private const float BuffDuration = 20f;
    private const float BuffCooldown = 120f;
    private const float BuffSpread = 9f;
    private const float OverdueGrace = 10f;
    private DateTime LastDowntime;

    private (float Left, float In) EstimateBuffWindows(Actor? primaryTarget)
    {
        var duel = Bossmods.ActiveModule?.Info?.GroupType == BossModuleInfo.GroupType.BozjaDuel && Player.Class is Class.VPR or Class.SAM or Class.WHM or Class.SGE or Class.DRK;
        return duel ? (float.MaxValue, 0) : primaryTarget?.IsStrikingDummy == true ? DummyBuffWindows() : PartyBuffWindows(primaryTarget);
    }
    private (float Left, float In) DummyBuffWindows()
    {
        var cycleTime = CombatTimer - OpenerBuffsAt;
        var cycle = cycleTime % BuffCooldown;
        return cycleTime < 0 ? (0, -cycleTime) : cycle < BuffDuration ? (BuffDuration - cycle, 0) : (0, BuffCooldown - cycle);
    }
    private (float Left, float In) PartyBuffWindows(Actor? primaryTarget)
    {
        var cooldowns = Bossmods.RaidCooldowns.DamageCooldowns;
        var left = Bossmods.RaidCooldowns.DamageBuffLeft(Player, primaryTarget);
        var uptime = MathF.Min(CombatTimer, (float)(World.CurrentTime - LastDowntime).TotalSeconds);
        var justPressed = false;
        var next = float.MaxValue;
        foreach (var cooldown in cooldowns)
        {
            var availableIn = AvailableIn(cooldown.AvailableAt);
            justPressed |= availableIn > BuffCooldown - 2;
            if (availableIn > -OverdueGrace || uptime <= OverdueGrace)
                next = MathF.Min(next, availableIn);
        }
        var waitForOthers = left > 0 ? WaitForMoreBuffs() : 0;
        var opener = CombatTimer <= OpenerBuffsAt + OverdueGrace && PartyHasRaidBuffer();
        return waitForOthers > 0 ? (0, waitForOthers)
            : cooldowns.Count == 0 ? (left, opener ? MathF.Max(0, OpenerBuffsAt - CombatTimer) : float.MaxValue)
            : left == 0 && justPressed ? (0, BuffSpread)
            : (left, next == float.MaxValue && left > 0 ? 0 : MathF.Max(0, next));
    }
    private float WaitForMoreBuffs()
    {
        float? sinceFirst = null;
        var neverPressed = false;
        var soonest = float.MaxValue;
        var cooldowns = Bossmods.RaidCooldowns.DamageCooldowns;
        foreach (var (slot, member) in World.Party.WithSlot(excludeAlliance: true))
        {
            if (slot >= PartyState.MaxPartySize || !HasRaidBuff(member))
                continue;
            float? availableIn = null;
            foreach (var cooldown in cooldowns)
                if (cooldown.Slot == slot)
                    availableIn = MathF.Min(availableIn ?? float.MaxValue, AvailableIn(cooldown.AvailableAt));
            if (availableIn > BuffCooldown - BuffDuration)
                sinceFirst = MathF.Max(sinceFirst ?? 0, BuffCooldown - availableIn.Value);
            else if (availableIn is not { } pending)
                neverPressed = true;
            else if (pending > -OverdueGrace)
                soonest = MathF.Min(soonest, pending);
        }
        var waitLeft = BuffSpread - (sinceFirst ?? BuffSpread);
        return waitLeft > 0 && (neverPressed || soonest < waitLeft) ? waitLeft : 0;
    }
    private float AvailableIn(DateTime at) => (float)(at - World.CurrentTime).TotalSeconds;
    private bool PartyHasRaidBuffer()
    {
        var found = false;
        foreach (var member in World.Party.WithoutSlot(includeDead: true, excludeAlliance: true, excludeNPCs: true))
            found |= member != Player && HasRaidBuff(member);
        return found;
    }
    private static bool HasRaidBuff(Actor player) => player.Level >= player.Class switch
    {
        Class.MNK or Class.PCT or Class.DNC => 70,
        Class.DRG => 52,
        Class.NIN => 45,
        Class.RPR => 72,
        Class.SMN or Class.SCH => 66,
        Class.RDM => 58,
        Class.BRD or Class.AST => 50,
        _ => int.MaxValue
    };
    #endregion

    #region Positionals
    protected bool NextPositionalImminent;
    protected bool NextPositionalCorrect;

    private Positional GetCurrentPositional(Actor target) => target.Omnidirectional
        ? Positional.Any
        : (Player.Position - target.Position).Normalized().Dot(target.Rotation.ToDirection()) switch
        {
            < -0.7071068f => Positional.Rear,
            < 0.7071068f => Positional.Flank,
            _ => Positional.Front
        };

    private bool HasPositionals(Actor target)
    {
        var rear = target.Position - target.Rotation.ToDirection() * (target.HitboxRadius + 1.5f);
        return !target.Omnidirectional && Hints.PathfindMapBounds.Contains(rear - Hints.PathfindMapCenter);
    }

    protected bool IsOnRear(Actor? target) => target != null && In3y(target) && GetCurrentPositional(target) == Positional.Rear;
    protected bool IsOnFlank(Actor? target) => target != null && In3y(target) && GetCurrentPositional(target) == Positional.Flank;

    protected void UpdatePositionals(Enemy? enemy, ref (Positional pos, bool imm) positional)
    {
        var target = enemy?.Actor;
        var facingPlayer = target is { CastInfo: null, IsStrikingDummy: false } && target.TargetID == Player.InstanceID;
        if (target != null && (!HasPositionals(target) || facingPlayer || enemy!.Priority < 0))
            positional = (Positional.Any, false);

        NextPositionalImminent = !HasTrueNorth && positional.imm;
        NextPositionalCorrect = HasTrueNorth || target == null || positional.pos is not (Positional.Flank or Positional.Rear) || GetCurrentPositional(target) == positional.pos;
        Hints.RecommendedPositional = (target, positional.pos, NextPositionalImminent, NextPositionalCorrect);
    }
    #endregion

    #region Goal Zones
    private Enemy? GoalTarget;

    protected void GoalZoneSingle(float range)
    {
        if (GoalTarget != null)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(GoalTarget.Actor, Player, World.Actors, range));
    }
    protected void GoalZoneCombined(StrategyValues strategy, float range, Func<WPos, float> fAoe, AID firstUnlockedAoeAction, int minAoe, float? maximumActionRange = null)
        => AddGoalZone(range, fAoe, firstUnlockedAoeAction, minAoe, maximumActionRange, Hints.RecommendedPositional.Pos);
    protected void AnyGoalZoneCombined(float range, Func<WPos, float> fAoe, AID firstUnlockedAoeAction, int minAoe, float? maximumActionRange = null)
    {
        var (_, positional, imminent, _) = Hints.RecommendedPositional;
        AddGoalZone(range, fAoe, firstUnlockedAoeAction, minAoe, maximumActionRange, imminent ? positional : Positional.Any);
    }
    private void AddGoalZone(float range, Func<WPos, float> fAoe, AID firstUnlockedAoeAction, int minAoe, float? maximumActionRange, Positional positional)
    {
        var aoe = Unlocked(firstUnlockedAoeAction) ? minAoe : 50;
        if (GoalTarget == null && aoe < 50)
            Hints.GoalZones.Add(fAoe);
        if (GoalTarget != null)
            Hints.GoalZones.Add(GoalCombined(Hints.GoalSingleTarget(GoalTarget.Actor, positional, Player, World.Actors, range), fAoe, aoe));
        if (GoalTarget != null && maximumActionRange is float r)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(GoalTarget.Actor, Player, World.Actors, r, 0.5f));
    }
    #endregion

    #region Priorities
    public enum GCDPriority
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
        Critical = 750,
        Severe = 800,
        VerySevere = 850,
        VeryCritical = 900,
        Max = 999,
        Forced = 1000
    }

    public enum OGCDPriority
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
        Forced = 1500,
        ToGCDPriority = 2000
    }

    protected static OGCDPriority OGCDPrio(OGCDStrategy strategy, OGCDPriority defaultPrio) => strategy switch
    {
        OGCDStrategy.Force => defaultPrio + 2000,
        OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave => defaultPrio + 1000,
        _ => defaultPrio
    };

    protected OGCDPriority ChangePriority(int lowPrio = 200, int highPrio = 400, bool emergencyCondition = true, bool convert = true)
        => !emergencyCondition ? OGCDPriority.None
        : convert && GCD <= 0.5f ? OGCDPriority.ToGCDPriority + 999
        : (OGCDPriority)(GCD is < 1.25f and > 0.5f ? highPrio : lowPrio);
    #endregion

    public sealed override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        NextGCD = default;
        NextGCDPrio = 0;
        HP = Player.HPMP.CurHP;
        MaxHP = Player.HPMP.MaxHP;
        MP = (uint)Math.Clamp(Player.PendingMPRaw, 0, Player.HPMP.MaxMP);
        MaxMP = Player.HPMP.MaxMP;
        PrimaryEnemy = GoalTarget = Hints.FindEnemy(primaryTarget);
        IsMoving = isMoving;
        CombatTimer = (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds;
        CountdownRemaining = World.Client.CountdownRemaining;
        SkSGCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
        SpSGCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level);
        CountTargets();
        HasTrueNorth = Status(ClassShared.SID.TrueNorth, 10) > GCD;
        CanTrueNorth = !HasTrueNorth && ActionUnlocked(ClassShared.AID.TrueNorth) && SharedCooldown(ClassShared.AID.TrueNorth) < 45.6f;
        SwiftcastLeft = Utils.MaxAll(Status(ClassShared.SID.Swiftcast, 10), AnySourceStatusLeft(ClassShared.SID.LostChainspell), AnySourceStatusLeft(PhantomSID.OccultQuick));
        HasSwiftcast = SwiftcastLeft > 0.1f;
        CanSwiftcast = ActionUnlocked(ClassShared.AID.Swiftcast) && SharedCooldown(ClassShared.AID.Swiftcast) < 0.6f;
        DowntimeIn = Manager.Planner?.EstimateTimeToNextDowntime() is (var downtimeNow, var stateLeft) ? (downtimeNow ? 0 : stateLeft) : float.MaxValue;
        if (DowntimeIn == 0)
            LastDowntime = World.CurrentTime;
        (RaidBuffsLeft, RaidBuffsIn) = EstimateBuffWindows(primaryTarget);
        kage.TimeToKill.Update(World, Hints);
        Execution(strategy, PrimaryEnemy);
    }

    private float AnySourceStatusLeft<SID>(SID sid) where SID : Enum => Player.FindStatus(sid) is ActorStatus s ? StatusDuration(s.ExpireAt) : 0;
    private float SharedCooldown(ClassShared.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    public abstract void Execution(StrategyValues strategy, Enemy? primaryTarget);
}

static class ToolsExtensions
{
    public static RotationModuleDefinition.ConfigRef<SoftTargetStrategy> DefineTargeting(this RotationModuleDefinition res)
        => res.Define(SharedTrack.Targeting).As<SoftTargetStrategy>("Targeting", "", 295)
            .AddOption(SoftTargetStrategy.Automatic, "Allow auto-selecting best target for maximum optimal DPS output - does not change hard target")
            .AddOption(SoftTargetStrategy.AutoHard, "Allow auto-selecting best target for maximum optimal DPS output - will change hard target")
            .AddOption(SoftTargetStrategy.Manual, "Forbid auto-selecting best target, instead executing only on whichever target is currently selected");

    public static RotationModuleDefinition.ConfigRef<HoldStrategy> DefineHold(this RotationModuleDefinition res)
        => res.Define(SharedTrack.Hold).As<HoldStrategy>("Hold", "Allow or Forbid Abilities", 290)
            .AddOption(HoldStrategy.DontHold, "Allow use of all cooldowns, buffs, or gauge abilities")
            .AddOption(HoldStrategy.HoldCooldowns, "Forbid use of all cooldowns")
            .AddOption(HoldStrategy.HoldGauge, "Forbid use of all gauge abilities")
            .AddOption(HoldStrategy.HoldBuffs, "Forbid use of all raidbuffs or buff-related abilities")
            .AddOption(HoldStrategy.HoldAbilities, "Forbid use of all cooldowns, buffs, and gauge abilities")
            .AddOption(HoldStrategy.HoldEverything, "Forbid complete use of ALL actions - standard rotations included");

    public static RotationModuleDefinition.ConfigRef<PotionStrategy> DefinePotion(this RotationModuleDefinition res, ActionID pot)
        => res.Define(SharedTrack.Potion).As<PotionStrategy>("Pots", "Potion Usage (Tinctures, Gemdraughts, etc.)", 280)
            .AddOption(PotionStrategy.Manual, "Do not use potion automatically")
            .AddOption(PotionStrategy.AlignWithBuffs, "Automatically use potion when personal buffs are imminent or active")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "Automatically use potion when party raid buffs are imminent or active")
            .AddOption(PotionStrategy.Immediate, "Force potion ASAP", 270, 30)
            .AddAssociatedAction(pot);

    public static RotationModuleDefinition.ConfigRef<GCDStrategy> DefineGCD<Index, AID>(this RotationModuleDefinition res, Index track, AID aid, string internalName, string displayName = "", int uiPriority = 100,
        float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100) where Index : Enum where AID : Enum
    {
        var name = ActionID.MakeSpell(aid).Name();
        return res.Define(track).As<GCDStrategy>(internalName, displayName, uiPriority)
            .AddOption(GCDStrategy.Automatic, $"Automatically use {name}", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(GCDStrategy.Force, $"Force {name} ASAP", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(GCDStrategy.Delay, $"Delay {name}", 0, 0, ActionTargets.None, minLevel, maxLevel)
            .AddAssociatedActions(aid);
    }

    public static RotationModuleDefinition.ConfigRef<OGCDStrategy> DefineOGCD<Index, AID>(this RotationModuleDefinition res, Index track, AID aid, string internalName, string displayName = "", int uiPriority = 100,
        float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100) where Index : Enum where AID : Enum
    {
        var name = ActionID.MakeSpell(aid).Name();
        return res.Define(track).As<OGCDStrategy>(internalName, displayName, uiPriority)
            .AddOption(OGCDStrategy.Automatic, $"Automatically use {name}", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(OGCDStrategy.Force, $"Force {name} ASAP", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(OGCDStrategy.AnyWeave, $"Force {name} in next possible weave slot", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(OGCDStrategy.EarlyWeave, $"Force {name} in next possible early-weave slot", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(OGCDStrategy.LateWeave, $"Force {name} in next possible late-weave slot", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(OGCDStrategy.Delay, $"Delay {name}", 0, 0, ActionTargets.None, minLevel, maxLevel)
            .AddAssociatedActions(aid);
    }

    public static RotationModuleDefinition.ConfigRef<AllowOrForbid> DefineAllow<Index, AID>(this RotationModuleDefinition res, Index track, AID aid, string internalName, string displayName = "", int uiPriority = 100,
        float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100) where Index : Enum where AID : Enum
    {
        var name = ActionID.MakeSpell(aid).Name();
        return res.Define(track).As<AllowOrForbid>(internalName, displayName, uiPriority)
            .AddOption(AllowOrForbid.Allow, $"Allow {name} when available", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(AllowOrForbid.Force, $"Force use {name} ASAP", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(AllowOrForbid.Forbid, $"Forbid {name} usage entirely", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddAssociatedActions(aid);
    }

    public static SoftTargetStrategy Targeting(this StrategyValues strategy) => strategy.Option(SharedTrack.Targeting).As<SoftTargetStrategy>();
    public static bool AutoTarget(this StrategyValues strategy) => strategy.Targeting() != SoftTargetStrategy.Manual;
    public static bool AutoHardTargeting(this StrategyValues strategy) => strategy.Targeting() == SoftTargetStrategy.AutoHard;

    public static HoldStrategy Hold(this StrategyValues strategy) => strategy.Option(SharedTrack.Hold).As<HoldStrategy>();
    public static bool HoldEverything(this StrategyValues strategy) => strategy.Hold() == HoldStrategy.HoldEverything;
    public static bool AllowAbilities(this StrategyValues strategy) => strategy.Hold() is not (HoldStrategy.HoldAbilities or HoldStrategy.HoldEverything);
    public static bool AllowCDs(this StrategyValues strategy) => strategy.AllowAbilities() && strategy.Hold() != HoldStrategy.HoldCooldowns;
    public static bool AllowBuffs(this StrategyValues strategy) => strategy.AllowCDs() && strategy.Hold() != HoldStrategy.HoldBuffs;
    public static bool AllowGauge(this StrategyValues strategy) => strategy.AllowAbilities() && strategy.Hold() != HoldStrategy.HoldGauge;

    public static PotionStrategy Potion(this StrategyValues strategy) => strategy.Option(SharedTrack.Potion).As<PotionStrategy>();
}
