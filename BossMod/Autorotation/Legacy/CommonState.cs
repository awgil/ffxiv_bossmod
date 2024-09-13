namespace BossMod.Autorotation.Legacy;

// TODO: a _lot_ of this stuff should be reworked...
public abstract class CommonState(RotationModule module)
{
    public RotationModule Module = module;
    public int Level => Module.Player.Level;
    public uint CurMP => Module.Player.HPMP.CurMP; // 10000 max
    public bool TargetingEnemy;
    public bool HaveTankStance;
    public float RangeToTarget; // minus both hitboxes; <= 0 means inside hitbox, <= 3 means in melee range, maxvalue if there is no target
    public float AnimationLock; // typical actions have 0.6 delay, but some (notably primal rend and potion) are >1
    public float AnimationLockDelay; // average time between action request and confirmation; this is added to effective animation lock for actions
    public float ComboTimeLeft => Module.World.Client.ComboState.Remaining; // 0 if not in combo, max 30
    public uint ComboLastAction => Module.World.Client.ComboState.Action;
    public float RaidBuffsLeft; // 0 if no damage-up status is up, otherwise it is time left on longest

    public float? CountdownRemaining => Module.World.Client.CountdownRemaining;
    public bool ForbidDOTs;
    public float FightEndIn; // how long fight will last (we try to spend all resources before this happens)
    public float RaidBuffsIn; // estimate time when new raidbuff window starts (if it is smaller than FightEndIn, we try to conserve resources)
    public float PositionLockIn; // time left to use moving abilities (Primal Rend and Onslaught) - we won't use them if it is ==0; setting this to 2.5f will make us use PR asap
    public Positional NextPositional;
    public bool NextPositionalImminent; // true if next positional will happen on next gcd
    public bool NextPositionalCorrect; // true if correctly positioned for next positional

    // both 2.5 max (unless slowed), reduced by gear attributes and certain status effects
    public float AttackGCDTime;
    public float SpellGCDTime;

    // find a slot containing specified duty action; returns -1 if not found
    public int FindDutyActionSlot(ActionID action) => Array.FindIndex(Module.World.Client.DutyActions, d => d.Action == action);
    // find a slot containing specified duty action, if other duty action is the specified one; returns -1 if not found, or other action is different
    public int FindDutyActionSlot(ActionID action, ActionID other)
    {
        var slot = FindDutyActionSlot(action);
        return slot >= 0 && Module.World.Client.DutyActions[1 - slot].Action == other ? slot : -1;
    }

    public float GCD => Module.World.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining; // 2.5 max (decreased by SkS), 0 if not on gcd
    public float PotionCD => Module.World.Client.Cooldowns[ActionDefinitions.PotionCDGroup].Remaining; // variable max
    public float CD<AID>(AID aid) where AID : Enum => Module.World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    public float DutyActionCD(int slot) => slot is >= 0 and < 2 ? Module.World.Client.Cooldowns[ActionDefinitions.DutyAction0CDGroup + slot].Remaining : float.MaxValue;
    public float DutyActionCD(ActionID action) => DutyActionCD(FindDutyActionSlot(action));

    // check whether weaving typical ogcd off cooldown would end its animation lock by the specified deadline
    public float OGCDSlotLength => 0.6f + AnimationLockDelay; // most actions have 0.6 anim lock delay, which allows double-weaving oGCDs between GCDs
    public bool CanWeave(float deadline) => AnimationLock + OGCDSlotLength <= deadline; // is it still possible to weave typical oGCD without missing deadline?
    // check whether weaving ogcd with specified remaining cooldown and lock time would end its animation lock by the specified deadline
    // deadline is typically either infinity (if we don't care about GCDs) or GCD (for second/only ogcd slot) or GCD-OGCDSlotLength (for first ogcd slot)
    public bool CanWeave(float cooldown, float actionLock, float deadline) => deadline < 10000 ? MathF.Max(cooldown, AnimationLock) + actionLock + AnimationLockDelay <= deadline : cooldown <= AnimationLock;
    public bool CanWeave<AID>(AID aid, float actionLock, float deadline) where AID : Enum => CanWeave(CD(aid), actionLock, deadline);

    public void UpdateCommon(Actor? target, float estimatedAnimLockDelay)
    {
        var vuln = Module.Manager.Planner?.EstimateTimeToNextVulnerable() ?? (false, 10000);
        var downtime = Module.Manager.Planner?.EstimateTimeToNextDowntime() ?? (false, 0);
        var poslock = Module.Manager.Planner?.EstimateTimeToNextPositioning() ?? (false, 10000);

        TargetingEnemy = target != null && target.Type is ActorType.Enemy or ActorType.Part && !target.IsAlly;
        RangeToTarget = Module.Player.DistanceToHitbox(target);
        AnimationLock = (Module.Player.CastInfo?.RemainingTime ?? 0) + Module.World.Client.AnimationLock;
        AnimationLockDelay = estimatedAnimLockDelay;

        RaidBuffsLeft = vuln.Item1 ? vuln.Item2 : 0;
        foreach (var status in Module.Player.Statuses.Where(s => IsDamageBuff(s.ID)))
        {
            RaidBuffsLeft = MathF.Max(RaidBuffsLeft, StatusDuration(status.ExpireAt));
        }
        // TODO: also check damage-taken debuffs on target

        var targetEnemy = target != null ? Module.Hints.PotentialTargets.Find(e => e.Actor == target) : null;
        ForbidDOTs = targetEnemy?.ForbidDOTs ?? false;
        FightEndIn = downtime.Item1 ? 0 : downtime.Item2;
        RaidBuffsIn = vuln.Item1 ? 0 : vuln.Item2;
        if (Module.Bossmods.ActiveModule?.Info?.PlanLevel > 0) // assumption: if there is no planning support for encounter (meaning it's something trivial, like outdoor boss), don't expect any cooldowns
            RaidBuffsIn = Math.Min(RaidBuffsIn, Module.Bossmods.RaidCooldowns.NextDamageBuffIn());
        PositionLockIn = !poslock.Item1 ? poslock.Item2 : 0;
        NextPositional = Positional.Any;
        NextPositionalImminent = false;
        NextPositionalCorrect = true;

        // all GCD skills share the same base recast time (with some exceptions that aren't relevant here)
        AttackGCDTime = ActionSpeed.GCDRounded(Module.World.Client.PlayerStats.SkillSpeed, Module.World.Client.PlayerStats.Haste, Module.Player.Level);
        SpellGCDTime = ActionSpeed.GCDRounded(Module.World.Client.PlayerStats.SpellSpeed, Module.World.Client.PlayerStats.Haste, Module.Player.Level);
    }

    public void UpdatePositionals(Actor? target, (Positional pos, bool imm) positional, bool trueNorth)
    {
        var ignore = trueNorth || (target?.Omnidirectional ?? true);
        NextPositional = positional.pos;
        NextPositionalImminent = !ignore && positional.imm;
        NextPositionalCorrect = ignore || target == null || positional.pos switch
        {
            Positional.Flank => MathF.Abs(target.Rotation.ToDirection().Dot((Module.Player.Position - target.Position).Normalized())) < 0.7071067f,
            Positional.Rear => target.Rotation.ToDirection().Dot((Module.Player.Position - target.Position).Normalized()) < -0.7071068f,
            _ => true
        };
        Module.Manager.Hints.RecommendedPositional = (target, NextPositional, NextPositionalImminent, NextPositionalCorrect);
    }

    public float StatusDuration(DateTime expireAt) => Math.Max((float)(expireAt - Module.World.CurrentTime).TotalSeconds, 0.0f);

    // this also checks pending statuses
    // note that we check pending statuses first - otherwise we get the same problem with double refresh if we try to refresh early (we find old status even though we have pending one)
    public (float Left, int Stacks) StatusDetails(Actor? actor, uint sid, ulong sourceID, float pendingDuration = 1000)
    {
        if (actor == null)
            return (0, 0);
        var pending = Module.World.PendingEffects.PendingStatus(actor.InstanceID, sid, sourceID);
        if (pending != null)
            return (pendingDuration, pending.Value);
        var status = actor.FindStatus(sid, sourceID);
        return status != null ? (StatusDuration(status.Value.ExpireAt), status.Value.Extra & 0xFF) : (0, 0);
    }
    public (float Left, int Stacks) StatusDetails<SID>(Actor? actor, SID sid, ulong sourceID, float pendingDuration = 1000) where SID : Enum => StatusDetails(actor, (uint)(object)sid, sourceID, pendingDuration);

    // check whether specified status is a damage buff
    // see https://i.redd.it/xrtgpras94881.png
    // TODO: AST card buffs?, enemy debuffs?, single-target buffs (DRG dragon sight, DNC devilment)
    public bool IsDamageBuff(uint statusID) => statusID == 49 || RaidCooldowns.IsDamageBuff(statusID); // medicated or raidbuff
}
