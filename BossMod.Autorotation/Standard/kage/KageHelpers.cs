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

// BossMod's estimate reads "buffs now" until someone uses one (forever if nobody has a raid buff)
public static class RaidBuffs
{
    public const float OpenerBuffs = 7.8f;
    // a buff this overdue after this much boss uptime isn't coming (buffer dead or not pressing it); waiting longer would stall every hold-for-buffs check
    private const float OverdueGrace = 10;

    public static (float Left, float In) Estimate(BossModuleManager bossmods, WorldState world, Actor player, Actor? target, float combatTime, float uptime)
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
            return (left, left == 0 && next < -OverdueGrace && uptime > OverdueGrace ? float.MaxValue : MathF.Max(0, next));
        if (combatTime > OpenerBuffs + OverdueGrace || !world.Party.WithoutSlot(includeDead: true, excludeAlliance: true, excludeNPCs: true).Any(p => p != player && HasPartyBuff(p)))
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

public abstract class KageRotation<TStrategy>(RotationModuleManager manager, Actor player) : TypedRotationModule<TStrategy>(manager, player) where TStrategy : struct
{
    protected float AnimLockDelay;
    protected Targeting TargetMode;
    protected float DowntimeIn;
    protected float RaidBuffsLeft;
    protected float RaidBuffsIn;
    protected bool InBossFight;
    // highest-priority GCD pushed this frame
    protected ActionID NextGCDAction { get; private set; }
    private int NextGCDPriority;
    private DateTime LastDowntime;

    protected virtual bool UsesSpellSpeed => false;
    protected float GCDLength => ActionSpeed.GCDRounded(UsesSpellSpeed ? World.Client.PlayerStats.SpellSpeed : World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
    protected float CombatTime => Player.InCombat ? (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds : 0;
    protected float ComboLeft => World.Client.ComboState.Remaining;
    protected int MP => (int)Player.HPMP.CurMP;
    protected bool NoRaidBuffs => RaidBuffsIn > 9000 && RaidBuffsLeft == 0;
    protected bool HasRaidBuffJobs => InBossFight && !NoRaidBuffs;
    protected bool PotionPrepull => World.Client.CountdownRemaining is > 0 and < 2;
    protected bool PotionWithRaidBuffs => PotionPrepull || Player.InCombat && (RaidBuffsLeft > 0 || RaidBuffsIn < 5);

    protected AIHints.Enemy? SelectTarget(Targeting targeting, ref Actor? primaryTarget, float estimatedAnimLockDelay, float autoRange)
    {
        AnimLockDelay = estimatedAnimLockDelay;
        NextGCDAction = default;
        NextGCDPriority = 0;
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
        if (DowntimeIn == 0)
            LastDowntime = World.CurrentTime;
        (RaidBuffsLeft, RaidBuffsIn) = RaidBuffs.Estimate(Bossmods, World, Player, primaryTarget, CombatTime, MathF.Min(CombatTime, (float)(World.CurrentTime - LastDowntime).TotalSeconds));
        InBossFight = Bossmods.ActiveModule != null || primaryTarget?.IsStrikingDummy == true;
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

    protected Positional CurrentPositional(Actor target)
    {
        var dir = target.Rotation.ToDirection().Dot((Player.Position - target.Position).Normalized());
        return dir < -0.7071068f ? Positional.Rear : dir < 0.7071068f ? Positional.Flank : Positional.Front;
    }

    // an enemy we're tanking keeps facing us outside its casts
    protected void RecommendPositional(AIHints.Enemy target, Positional pos, bool imminent, bool useTrueNorth)
    {
        var actor = target.Actor;
        if (!HasPositionals(actor) || actor.TargetID == Player.InstanceID && actor.CastInfo == null && !actor.IsStrikingDummy || target.Priority < 0)
            (pos, imminent) = (Positional.Any, false);

        var tn = SelfStatusLeft(ClassShared.SID.TrueNorth) > GCD;
        var correct = tn || pos == Positional.Any || CurrentPositional(actor) == pos;
        Hints.RecommendedPositional = (actor, pos, imminent && !tn, correct);

        if (useTrueNorth && imminent && !correct)
            PushOGCD(ClassShared.AID.TrueNorth, Player, 20, GCD - 0.8f);
    }

    protected bool RaidwideWithin(float seconds) => StateTimeline.Raidwides(Bossmods.ActiveModule, World, Hints).Any(t => t >= World.CurrentTime && t <= World.FutureTime(seconds));

    protected bool DotWorthIt(AIHints.Enemy e)
    {
        if (e.ForbidDOTs || e.Priority < 0 && !TimeToKill.IsBossTier(Bossmods.ActiveModule, Hints, e.Actor) || DowntimeIn < 15)
            return false;
        return TimeToKill.WillLive(e.Actor, 15);
    }

    // a dot we just cast counts as fresh until it lands, so it isn't cast twice
    protected float MaxDotLeft<SID>(Actor target, params SID[] sids) where SID : Enum
    {
        var left = 0f;
        foreach (var sid in sids)
            left = MathF.Max(left, StatusDetails(target, sid, Player.InstanceID, 30).Left);
        return left;
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

    // cast times scale with speed like the GCD
    protected float ScaledCastTime<AID>(AID aid) where AID : Enum => ActionDefinitions.Instance.Spell(aid) is { CastTime: > 0 } def ? def.CastTime * GCDLength / 2.5f : 0;

    // moving during the last 0.5s doesn't interrupt a cast
    protected static float SlideCast(float castTime) => MathF.Max(0, castTime - 0.5f);

    protected bool PushAction(ActionID action, Actor? target, float priority, float delay = 0, float castTime = 0, Angle? facing = null)
    {
        if (action.ID == 0 || !ActionUnlocked(action) || ActionDefinitions.Instance[action] is not { } def || def.Range != 0 && target == null)
            return false;
        var targetPos = def.AllowedTargets.HasFlag(ActionTargets.Area) ? (def.Range == 0 ? Player.PosRot.XYZ() : target?.PosRot.XYZ() ?? default) : default;
        Hints.ActionsToExecute.Push(action, def.Range == 0 ? Player : target, priority, delay: delay, castTime: castTime, targetPos: targetPos, facingAngle: facing);
        return true;
    }

    protected void PushGCD<AID>(AID aid, Actor? target, int priority, float castTime = 0, Angle? facing = null) where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        if (PushAction(action, target, ActionQueue.Priority.High + priority, castTime: SlideCast(castTime), facing: facing) && priority > NextGCDPriority)
            (NextGCDAction, NextGCDPriority) = (action, priority);
    }

    protected bool PushOGCD<AID>(AID aid, Actor? target, int priority, float delay = 0) where AID : Enum
        => PushAction(ActionID.MakeSpell(aid), target, ActionQueue.Priority.Low + priority, delay);

    protected static bool UseAOE(AOEStrategy strategy, int targets, int minTargets) => strategy switch
    {
        AOEStrategy.ForceAOE => true,
        AOEStrategy.AOE => targets >= minTargets,
        _ => false
    };

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
