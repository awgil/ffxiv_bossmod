namespace BossMod.Autorotation.Legacy;

// TODO: a _lot_ of that stuff should be reworked...
public abstract class CommonState(WorldState ws, Actor player, AIHints hints)
{
    public int Level => player.Level;
    public int UnlockProgress;
    public uint CurMP => player.HPMP.CurMP; // 10000 max
    public bool TargetingEnemy;
    public bool HaveTankStance;
    public float RangeToTarget; // minus both hitboxes; <= 0 means inside hitbox, <= 3 means in melee range, maxvalue if there is no target
    public float AnimationLock; // typical actions have 0.6 delay, but some (notably primal rend and potion) are >1
    public float AnimationLockDelay; // average time between action request and confirmation; this is added to effective animation lock for actions
    public float ComboTimeLeft; // 0 if not in combo, max 30
    public uint ComboLastAction;
    public float RaidBuffsLeft; // 0 if no damage-up status is up, otherwise it is time left on longest

    public float CombatTimer; // MinValue if not in combat, negative during countdown, zero or positive during combat
    public bool ForbidDOTs;
    public float ForceMovementIn;
    public float FightEndIn; // how long fight will last (we try to spend all resources before this happens)
    public float RaidBuffsIn; // estimate time when new raidbuff window starts (if it is smaller than FightEndIn, we try to conserve resources)
    public float PositionLockIn; // time left to use moving abilities (Primal Rend and Onslaught) - we won't use them if it is ==0; setting this to 2.5f will make us use PR asap
    public Positional NextPositional;
    public bool NextPositionalImminent; // true if next positional will happen on next gcd
    public bool NextPositionalCorrect; // true if correctly positioned for next positional

    // these simply point to client state
    public readonly Cooldown[] Cooldowns = ws.Client.Cooldowns;
    public readonly ActionID[] DutyActions = ws.Client.DutyActions;
    public readonly byte[] BozjaHolster = ws.Client.BozjaHolster;

    // both 2.5 max (unless slowed), reduced by gear attributes and certain status effects
    public float AttackGCDTime;
    public float SpellGCDTime;

    // find a slot containing specified duty action; returns -1 if not found
    public int FindDutyActionSlot(ActionID action) => Array.IndexOf(DutyActions, action);
    // find a slot containing specified duty action, if other duty action is the specified one; returns -1 if not found, or other action is different
    public int FindDutyActionSlot(ActionID action, ActionID other)
    {
        var slot = FindDutyActionSlot(action);
        return slot >= 0 && DutyActions[1 - slot] == other ? slot : -1;
    }

    public float GCD => Cooldowns[ActionDefinitions.GCDGroup].Remaining; // 2.5 max (decreased by SkS), 0 if not on gcd
    public float PotionCD => Cooldowns[ActionDefinitions.PotionCDGroup].Remaining; // variable max
    public float CD<AID>(AID aid) where AID : Enum => Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    public float DutyActionCD(int slot) => slot is >= 0 and < 2 ? Cooldowns[ActionDefinitions.DutyAction0CDGroup + slot].Remaining : float.MaxValue;
    public float DutyActionCD(ActionID action) => DutyActionCD(FindDutyActionSlot(action));

    // check whether weaving typical ogcd off cooldown would end its animation lock by the specified deadline
    public float OGCDSlotLength => 0.6f + AnimationLockDelay; // most actions have 0.6 anim lock delay, which allows double-weaving oGCDs between GCDs
    public bool CanWeave(float deadline) => AnimationLock + OGCDSlotLength <= deadline; // is it still possible to weave typical oGCD without missing deadline?
    // check whether weaving ogcd with specified remaining cooldown and lock time would end its animation lock by the specified deadline
    // deadline is typically either infinity (if we don't care about GCDs) or GCD (for second/only ogcd slot) or GCD-OGCDSlotLength (for first ogcd slot)
    public bool CanWeave(float cooldown, float actionLock, float deadline) => deadline < 10000 ? MathF.Max(cooldown, AnimationLock) + actionLock + AnimationLockDelay <= deadline : cooldown <= AnimationLock;
    public bool CanWeave<AID>(AID aid, float actionLock, float deadline) where AID : Enum => CanWeave(CD(aid), actionLock, deadline);

    public void UpdateCommon(Actor? target, int unlockProgress)
    {
        var vuln = Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextVulnerable(Autorot.Bossmods.ActiveModule.StateMachine) ?? (false, 10000);

        var am = Autorot.ActionManager;
        UnlockProgress = _lock.Progress();
        TargetingEnemy = target != null && target.Type is ActorType.Enemy or ActorType.Part && !target.IsAlly;
        RangeToTarget = target != null ? (target.Position - player.Position).Length() - target.HitboxRadius - player.HitboxRadius : float.MaxValue;
        AnimationLock = am.EffectiveAnimationLock;
        AnimationLockDelay = am.AnimationLockDelayEstimate;
        ComboTimeLeft = am.ComboTimeLeft;
        ComboLastAction = am.ComboLastMove;

        // all GCD skills share the same base recast time (with some exceptions that aren't relevant here)
        // so we can check Fast Blade (9) and Stone (119) recast timers to get effective sks and sps
        // regardless of current class
        AttackGCDTime = FFXIVGame.ActionManager.GetAdjustedRecastTime(FFXIVGame.ActionType.Action, 9) / 1000f;
        SpellGCDTime = FFXIVGame.ActionManager.GetAdjustedRecastTime(FFXIVGame.ActionType.Action, 119) / 1000f;

        RaidBuffsLeft = vuln.Item1 ? vuln.Item2 : 0;
        foreach (var status in player.Statuses.Where(s => IsDamageBuff(s.ID)))
        {
            RaidBuffsLeft = MathF.Max(RaidBuffsLeft, StatusDuration(status.ExpireAt));
        }
        // TODO: also check damage-taken debuffs on target


        var targetEnemy = target != null ? hints.PotentialTargets.Find(e => e.Actor == target) : null;
        var downtime = Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextDowntime(Autorot.Bossmods.ActiveModule.StateMachine) ?? (false, 0);
        var poslock = Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextPositioning(Autorot.Bossmods.ActiveModule.StateMachine) ?? (false, 10000);
        var vuln = Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextVulnerable(Autorot.Bossmods.ActiveModule.StateMachine) ?? (false, 10000);

        CombatTimer = CombatTimer();
        ForbidDOTs = targetEnemy?.ForbidDOTs ?? false;
        ForceMovementIn = MaxCastTime;
        FightEndIn = downtime.Item1 ? 0 : downtime.Item2;
        RaidBuffsIn = vuln.Item1 ? 0 : vuln.Item2;
        if (Autorot.Bossmods.ActiveModule?.PlanConfig != null) // assumption: if there is no planning support for encounter (meaning it's something trivial, like outdoor boss), don't expect any cooldowns
            RaidBuffsIn = Math.Min(RaidBuffsIn, Autorot.Bossmods.RaidCooldowns.NextDamageBuffIn(ws.CurrentTime));
        PositionLockIn = Autorot.Config.EnableMovement && !poslock.Item1 ? poslock.Item2 : 0;
        NextPositional = Positional.Any;
        NextPositionalImminent = false;
        NextPositionalCorrect = true;
    }
}
