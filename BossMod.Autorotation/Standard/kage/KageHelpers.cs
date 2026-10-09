namespace BossMod.Autorotation.kage;

public static class TimeToKill
{
    private const float SampleInterval = 0.5f;
    private const float Window = 15;
    private const float MinObserved = 3;

    private static readonly Dictionary<ulong, Queue<(DateTime time, uint hp)>> _samples = [];
    private static DateTime _lastUpdate;

    public static void Update(WorldState ws, AIHints hints)
    {
        if ((ws.CurrentTime - _lastUpdate).TotalSeconds < SampleInterval && ws.CurrentTime >= _lastUpdate)
            return;
        _lastUpdate = ws.CurrentTime;

        foreach (var e in hints.PotentialTargets)
        {
            if (!_samples.TryGetValue(e.Actor.InstanceID, out var q))
                _samples[e.Actor.InstanceID] = q = new();
            q.Enqueue((ws.CurrentTime, e.Actor.HPMP.CurHP));
            while (q.Count > 0 && (ws.CurrentTime - q.Peek().time).TotalSeconds > Window)
                q.Dequeue();
        }

        foreach (var id in _samples.Keys.Where(id => ws.Actors.Find(id) is not { IsDead: false }).ToList())
            _samples.Remove(id);
    }

    public static float? Estimate(Actor enemy)
    {
        if (!_samples.TryGetValue(enemy.InstanceID, out var q) || q.Count < 2)
            return null;
        var (t0, hp0) = q.Peek();
        var dt = (float)(_lastUpdate - t0).TotalSeconds;
        if (dt < MinObserved)
            return null;
        var lost = (float)hp0 - enemy.HPMP.CurHP;
        return lost <= 0 ? float.MaxValue : enemy.HPMP.CurHP / (lost / dt);
    }

    public static bool WillLive(Actor enemy, float seconds)
        => enemy.IsStrikingDummy || (Estimate(enemy) is { } ttk ? ttk >= seconds : enemy.HPRatio > 0.05f);

    public static bool InBossFight(BossModule? module, Actor? target) => module != null || target?.IsStrikingDummy == true;

    public static bool IsBossTier(BossModule? module, AIHints hints, Actor target)
        => target.IsStrikingDummy || module?.PrimaryActor is { IsDeadOrDestroyed: false, IsTargetable: true } boss
            && (target == boss || target.HPMP.CurHP >= boss.HPMP.CurHP || (hints.FindEnemy(target)?.Priority ?? 0) > (hints.FindEnemy(boss)?.Priority ?? 0));

    public static bool BurstWorthIt(BossModule? module, AIHints hints, Actor target, float payoff)
    {
        if (module?.PrimaryActor is { IsDeadOrDestroyed: false, IsTargetable: true } && !IsBossTier(module, hints, target))
            return false;
        return WillLive(target, payoff);
    }
}

// raidwides / tankbusters from the module's timeline, for modules that don't predict damage
public static class StateTimeline
{
    public static IEnumerable<(DateTime at, StateMachine.StateHint hint)> Upcoming(BossModule? module, WorldState ws, float horizon = 30)
    {
        var sm = module?.StateMachine;
        var s = sm?.ActiveState;
        if (sm == null || s == null)
            yield break;
        var t = MathF.Max(0, s.Duration - sm.TimeSinceTransition);
        for (var i = 0; i < 256 && s != null && t <= horizon; i++)
        {
            if ((s.EndHint & (StateMachine.StateHint.Raidwide | StateMachine.StateHint.Tankbuster)) != 0)
                yield return (ws.FutureTime(t), s.EndHint);
            if (s.NextStates?.Length != 1)
                yield break;
            s = s.NextStates[0];
            t += s.Duration;
        }
    }

    public static IEnumerable<DateTime> Raidwides(BossModule? module, WorldState ws, AIHints hints, float horizon = 30)
    {
        var predicted = hints.PredictedDamage.Where(d => d.Type is AIHints.PredictedDamageType.Raidwide or AIHints.PredictedDamageType.Shared).Select(d => d.Activation).ToList();
        foreach (var p in predicted)
            yield return p;
        foreach (var (at, hint) in Upcoming(module, ws, horizon))
            if (hint.HasFlag(StateMachine.StateHint.Raidwide) && !predicted.Any(p => Math.Abs((p - at).TotalSeconds) < 3))
                yield return at;
    }

    public static IEnumerable<(Actor target, DateTime at)> Tankbusters(BossModule? module, WorldState ws, AIHints hints, float horizon = 30)
    {
        var predicted = hints.PredictedDamage.Where(d => d.Type == AIHints.PredictedDamageType.Tankbuster)
            .SelectMany(d => ws.Party.WithSlot().IncludedInMask(d.Players).Select(p => (target: p.Item2, at: d.Activation))).ToList();
        foreach (var p in predicted)
            yield return p;
        if (module?.PrimaryActor is not { } boss || ws.Actors.Find(boss.TargetID) is not { } tank || ws.Party.FindSlot(tank.InstanceID) < 0)
            yield break;
        foreach (var (at, hint) in Upcoming(module, ws, horizon))
            if (hint.HasFlag(StateMachine.StateHint.Tankbuster) && !predicted.Any(p => Math.Abs((p.at - at).TotalSeconds) < 3))
                yield return (tank, at);
    }
}

// BossMod's estimate reads "buffs now" until someone uses one (forever if nobody has a raid buff)
public static class RaidBuffs
{
    public const float OpenerBuffs = 7.8f;

    public static (float Left, float In) Estimate(BossModuleManager bossmods, WorldState world, Actor player, Actor? target, float combatTime)
    {
        if (target?.IsStrikingDummy == true)
        {
            var cycle = combatTime - OpenerBuffs;
            if (cycle < 0)
                return (0, -cycle);
            cycle %= 120;
            return cycle < 20 ? (20 - cycle, 0) : (0, 120 - cycle);
        }

        var left = bossmods.RaidCooldowns.DamageBuffLeft(player, target);
        if (bossmods.RaidCooldowns.NextDamageBuffIn2() is { } next)
            return (left, next);
        if (!world.Party.WithoutSlot(includeDead: true, excludeAlliance: true, excludeNPCs: true).Any(p => p != player && HasPartyBuff(p)))
            return (left, float.MaxValue);
        return (left, MathF.Max(0, OpenerBuffs - combatTime));
    }

    private static bool HasPartyBuff(Actor p) => p.Class switch
    {
        Class.MNK => p.Level >= 70,
        Class.DRG => p.Level >= 52,
        Class.NIN => p.Level >= 45,
        Class.RPR => p.Level >= 72,
        Class.SMN => p.Level >= 66,
        Class.RDM => p.Level >= 58,
        Class.PCT => p.Level >= 70,
        Class.BRD => p.Level >= 50,
        Class.DNC => p.Level >= 70,
        Class.SCH => p.Level >= 66,
        Class.AST => p.Level >= 50,
        _ => false
    };
}

public static class BurstPlanner
{
    public const float BuffWeight = 1.15f;

    public static bool SpendNow(float have, float income, float cap, float minSpend, float margin = 5)
    {
        if (have < minSpend || have + income <= cap)
            return false;
        return have + BuffWeight * MathF.Min(cap, income) > BuffWeight * cap + margin;
    }

    public static float Overlap(float start, float length, float buffStart, float buffLength)
        => MathF.Max(0, MathF.Min(start + length, buffStart + buffLength) - MathF.Max(start, buffStart));
}

public abstract class KageRotation<TStrategy>(RotationModuleManager manager, Actor player) : TypedRotationModule<TStrategy>(manager, player) where TStrategy : struct
{
    protected float AnimLockDelay;
    protected Targeting TargetMode;
    protected float DowntimeIn;
    protected float RaidBuffsLeft;
    protected float RaidBuffsIn;
    protected bool InBossFight;

    protected virtual bool UsesSpellSpeed => false;
    protected float GCDLength => ActionSpeed.GCDRounded(UsesSpellSpeed ? World.Client.PlayerStats.SpellSpeed : World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
    protected float CombatTime => Player.InCombat ? (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds : 0;
    protected bool NoRaidBuffs => RaidBuffsIn > 9000 && RaidBuffsLeft == 0;
    protected bool HasRaidBuffJobs => InBossFight && !NoRaidBuffs;
    protected bool PotionPrepull => World.Client.CountdownRemaining is > 0 and < 2;
    protected bool PotionWithRaidBuffs => PotionPrepull || Player.InCombat && (RaidBuffsLeft > 0 || RaidBuffsIn < 5);

    protected AIHints.Enemy? SelectTarget(Targeting targeting, ref Actor? primaryTarget, float estimatedAnimLockDelay, float autoRange)
    {
        AnimLockDelay = estimatedAnimLockDelay;
        TimeToKill.Update(World, Hints);

        var target = Hints.FindEnemy(primaryTarget);
        if (target?.Priority is AIHints.Enemy.PriorityInvincible or AIHints.Enemy.PriorityForbidden || target?.Priority == AIHints.Enemy.PriorityPointless && Hints.PriorityTargets.Any())
            target = null;

        TargetMode = targeting == Targeting.AutoTryPri ? (target != null ? Targeting.AutoPrimary : Targeting.Auto) : targeting;
        if (TargetMode == Targeting.Auto && target == null)
        {
            target = Hints.PriorityTargets.Where(e => Player.DistanceToHitbox(e.Actor) <= autoRange).MinBy(e => Player.DistanceToHitbox(e.Actor));
            primaryTarget = target?.Actor;
        }

        DowntimeIn = Manager.Planner?.EstimateTimeToNextDowntime() is (var downNow, var stateLeft) ? (downNow ? 0 : stateLeft) : float.MaxValue;
        (RaidBuffsLeft, RaidBuffsIn) = RaidBuffs.Estimate(Bossmods, World, Player, primaryTarget, CombatTime);
        InBossFight = TimeToKill.InBossFight(Bossmods.ActiveModule, target?.Actor ?? primaryTarget);
        return target;
    }

    // wall bosses often lack the omnidirectional flag
    protected bool HasPositionals(Actor target)
    {
        if (target.Omnidirectional)
            return false;
        var rear = target.Position - target.Rotation.ToDirection() * (target.HitboxRadius + 1.5f);
        return Hints.PathfindMapBounds.Contains(rear - Hints.PathfindMapCenter);
    }

    protected bool Unlocked<AID>(AID aid) where AID : Enum => ActionUnlocked(ActionID.MakeSpell(aid));
    protected float ReadyIn<AID>(AID aid) where AID : Enum => ReadyIn(ActionID.MakeSpell(aid));
    protected float ReadyIn(ActionID action) => ActionUnlocked(action) && ActionDefinitions.Instance[action] is { } def ? def.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) : float.MaxValue;
    protected bool GCDReady<AID>(AID aid) where AID : Enum => ReadyIn(aid) < GCD + 0.05f;

    protected bool CanWeave<AID>(AID aid) where AID : Enum => CanWeave(ActionID.MakeSpell(aid));
    protected bool CanWeave(ActionID action)
    {
        if (!ActionUnlocked(action) || ActionDefinitions.Instance[action] is not { } def)
            return false;
        return MathF.Max(def.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions), World.Client.AnimationLock) + def.TotalDuration + AnimLockDelay <= GCD;
    }

    protected bool PushAction(ActionID action, Actor? target, float priority, float delay = 0, float castTime = 0, Angle? facing = null)
    {
        if (action.ID == 0 || !ActionUnlocked(action) || ActionDefinitions.Instance[action] is not { } def || def.Range != 0 && target == null)
            return false;
        var targetPos = def.AllowedTargets.HasFlag(ActionTargets.Area) ? (def.Range == 0 ? Player.PosRot.XYZ() : target?.PosRot.XYZ() ?? default) : default;
        Hints.ActionsToExecute.Push(action, def.Range == 0 ? Player : target, priority, delay: delay, castTime: castTime, targetPos: targetPos, facingAngle: facing);
        return true;
    }

    protected bool PushOGCD<AID>(AID aid, Actor? target, int priority, float delay = 0) where AID : Enum
        => PushAction(ActionID.MakeSpell(aid), target, ActionQueue.Priority.Low + priority, delay);

    protected (Actor? Best, int Count) BestAOETarget(AIHints.Enemy? primary, float range, bool allowAoE, Func<Actor, Actor, bool> hits)
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
}
