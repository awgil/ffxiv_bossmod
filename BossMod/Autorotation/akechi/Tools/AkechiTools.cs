﻿using BossMod.AST;
using static BossMod.ActorCastEvent;

namespace BossMod.Autorotation.akechi.Tools;

public enum SharedTrack { AOE, Hold, Count }
public enum AOEStrategy { Automatic, ForceST, ForceAOE }
public enum HoldStrategy { DontHold, HoldCooldowns, HoldGauge, HoldEverything }
public enum GCDStrategy { Automatic, Force, Delay }
public enum OGCDStrategy { Automatic, Force, AnyWeave, EarlyWeave, LateWeave, Delay }

public abstract class AkechiTools<AID, TraitID>(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
        where AID : struct, Enum where TraitID : Enum
{
    #region Core
    protected WorldState WorldState => Bossmods.WorldState;
    protected PartyRolesConfig PRC => Service.Config.Get<PartyRolesConfig>();
    public void QueueGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum => QueueGCD(aid, target, (int)(object)priority, delay);
    public void QueueOGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum => QueueOGCD(aid, target, (int)(object)priority, delay);
    public bool QueueAction(AID aid, Actor? target, float priority, float delay)
    {
        if ((uint)(object)aid == 0)
            return false;

        var res = ActionDefinitions.Instance.Spell(aid);
        if (res == null)
            return false;

        if (res.Range != 0 && target == null)
        {
            return false;
        }

        Vector3 targetPos = default;

        if (res.AllowedTargets.HasFlag(ActionTargets.Area))
        {
            if (res.Range == 0)
                targetPos = Player.PosRot.XYZ();
            else if (target != null)
                targetPos = target.PosRot.XYZ();
        }

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority, delay: delay, targetPos: targetPos);
        return true;
    }
    public void QueueGCD(AID aid, Actor? target, int priority = 8, float delay = 0)
    {
        var NextGCDPrio = 0;
        if (priority == 0)
            return;

        if (QueueAction(aid, target, ActionQueue.Priority.High + priority, delay) && priority > NextGCDPrio)
        {
            NextGCD = aid;
        }
    }
    public void QueueOGCD(AID aid, Actor? target, int priority = 4, float delay = 0)
    {
        if (priority == 0)
            return;

        QueueAction(aid, target, ActionQueue.Priority.Medium + priority, delay);
    }
    #endregion

    #region Actions
    /// <summary>Checks if action is <em>Unlocked</em> based on Level and Job Quest (if required)</summary>
    /// <param name="aid"> User's specified <em>Action ID</em> </param>
    /// <returns>- True if conditions met, False if not</returns>
    protected bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid)); //Check if the desired ability is unlocked

    /// <summary>Checks if Trait is <em>Unlocked</em> based on Level and Job Quest (if required)</summary>
    /// <param name="aid"> User's specified <em>Action ID</em> </param>
    /// <returns>- True if conditions met, False if not</returns>
    protected bool Unlocked(TraitID tid) => TraitUnlocked((uint)(object)tid);

    /// <summary><para>Checks if <em>last combo action</em> is what the user is specifying.</para>
    /// <para>NOTE: This does <em>NOT</em> check all actions, only combo actions.</para></summary>
    /// <returns>- True if condition met, False if not</returns>
    protected AID ComboLastMove => (AID)(object)World.Client.ComboState.Action;

    /// <summary>
    /// Retreieves <em>effective</em> cast time by returning the action's base cast time multiplied by the player's spell-speed factor, which accounts for haste buffs (like <em>Ley Lines</em>) and slow debuffs. It also accounts for <em>Swiftcast</em>.
    /// </summary>
    protected virtual float GetCastTime(AID aid) => PlayerHasEffect(SID.Swiftcast, 10) ? 0 : ActionDefinitions.Instance.Spell(aid)!.CastTime * SpSGCDLength / 2.5f;

    protected float NextCastStart => World.Client.AnimationLock > GCD ? World.Client.AnimationLock + AnimationLockDelay : GCD;

    protected float GetSlidecastTime(AID aid) => MathF.Max(0, GetCastTime(aid) - 0.5f);
    protected float GetSlidecastEnd(AID aid) => NextCastStart + GetSlidecastTime(aid);

    protected virtual bool CanCast(AID aid)
    {
        var t = GetSlidecastTime(aid);
        if (t == 0)
            return true;

        return NextCastStart + t <= ForceMovementIn;
    }

    /// <summary>
    /// <para>Retrieves player's GCD length based on <em>Skill-Speed</em>.</para>
    /// <para>NOTE: This function is only recommended for jobs with <em>Skill-Speed</em>. <em>Spell-Speed</em> users are <em>unaffected</em> by this.</para>
    /// </summary>
    protected float SkSGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);

    /// <summary>
    /// Retrieves player's current <em>Skill-Speed</em> stat.
    /// </summary>
    protected float SkS => ActionSpeed.Round(World.Client.PlayerStats.SkillSpeed);

    /// <summary>
    /// <para>Retrieves player's GCD length based on <em>Spell-Speed</em>.</para>
    /// <para>NOTE: This function is only recommended for jobs with <em>Spell-Speed</em>. <em>Skill-Speed</em> users are <em>unaffected</em> by this.</para>
    /// </summary>
    protected float SpSGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level);

    /// <summary>
    /// Retrieves player's current <em>Spell-Speed</em> stat.
    /// </summary>
    protected float SpS => ActionSpeed.Round(World.Client.PlayerStats.SpellSpeed);

    /// <summary>
    /// Checks if we can fit in a <em>skill-speed based</em> GCD.
    /// </summary>
    public bool CanFitSkSGCD(float duration, int extraGCDs = 0) => GCD + SkSGCDLength * extraGCDs < duration;

    /// <summary>
    /// Checks if we can fit in a <em>spell-speed based</em> GCD.
    /// </summary>
    public bool CanFitSpSGCD(float duration, int extraGCDs = 0) => GCD + SpSGCDLength * extraGCDs < duration;

    /// <summary>
    /// <para>Checks if player is available to weave in any abilities.</para>
    /// <para>NOTE: This function is only recommended for jobs with <em>Skill-Speed</em>. <em>Spell-Speed</em> users are <em>unaffected</em> by this.</para>
    /// </summary>
    public bool CanWeave(float cooldown, float actionLock, int extraGCDs = 0, float extraFixedDelay = 0)
        => MathF.Max(cooldown, World.Client.AnimationLock) + actionLock + AnimationLockDelay <= GCD + SkSGCDLength * extraGCDs + extraFixedDelay;
    /// <summary>
    /// <para>Checks if player is available to weave in any abilities.</para>
    /// <para>NOTE: This function is only recommended for jobs with <em>Spell-Speed</em>. <em>Skill-Speed</em> users are <em>unaffected</em> by this.</para>
    /// </summary>
    public bool CanSpellWeave(float cooldown, float actionLock, int extraGCDs = 0, float extraFixedDelay = 0)
        => MathF.Max(cooldown, World.Client.AnimationLock) + actionLock + AnimationLockDelay <= GCD + SpSGCDLength * extraGCDs + extraFixedDelay;
    /// <summary>
    /// Checks if player is available to weave in any abilities.
    /// </summary>
    public bool CanWeave(AID aid, int extraGCDs = 0, float extraFixedDelay = 0)
    {
        if (!Unlocked(aid))
            return false;

        var res = ActionDefinitions.Instance[ActionID.MakeSpell(aid)]!;
        if (SkS > 100)
            return CanSpellWeave(ReadyIn(aid), res.InstantAnimLock, extraGCDs, extraFixedDelay);
        if (SpS > 100)
            return CanWeave(ReadyIn(aid), res.InstantAnimLock, extraGCDs, extraFixedDelay);

        return false;
    }
    #endregion

    #region Cooldown
    /// <summary> Retrieves the total cooldown time left on the specified action. </summary>
    /// <param name="aid"> User's specified <em>Action ID</em> </param>
    /// <returns>- The remaining cooldown duration </returns>
    protected float TotalCD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    /// <summary> Returns the charge cooldown time left on the specified action. </summary>
    /// <param name="aid"> User's specified <em>Action ID</em> </param>
    /// <returns>- The remaining cooldown duration </returns>
    protected float ChargeCD(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) : float.MaxValue;

    /// <summary> Checks if action has any charges remaining. </summary>
    /// <param name="aid"> User's specified <em>Action ID</em> </param>
    /// <returns>- True if the action has charges, False if not</returns>
    protected bool HasCharges(AID aid) => ChargeCD(aid) < 0.6f;

    /// <summary>Checks if action is on cooldown based on its <em>total cooldown timer</em>. </summary>
    /// <param name="aid"> User's specified <em>Action ID</em> </param>
    /// <returns>- True if the action is on cooldown, False if not</returns>
    protected bool IsOnCooldown(AID aid) => TotalCD(aid) > 0.6f;

    /// <summary>Checks if action is off cooldown based on its <em>total cooldown timer</em>. </summary>
    /// <param name="aid"> User's specified <em>Action ID</em> </param>
    /// <returns>- True if the action is off cooldown, False if not</returns>
    protected bool IsOffCooldown(AID aid) => !IsOnCooldown(aid);
    /// <summary>
    /// Checks if action is on cooldown based on its <em>charges</em>.
    /// </summary>
    protected bool OnCooldown(AID aid) => MaxChargesIn(aid) > 0;

    /// <summary>Checks if last action used is what the user is specifying and within however long. </summary>
    /// <param name="aid"> User's specified <em>Action ID</em> </param>
    /// <param name="variance"> How long since last action was used. </param>
    /// <returns>- True if the action is off cooldown, False if not</returns>
    protected bool LastActionUsed(AID aid, float variance = 3f) => Manager.LastCast.Data?.IsSpell(aid) == true && Manager.LastCast.Time.Second <= variance;
    /// <summary>
    /// Time remaining until action is <em>ready</em>.
    /// </summary>
    protected float ReadyIn(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) : float.MaxValue;
    /// <summary>
    /// Time remaining until action is at <em>maximum charges</em>.
    /// </summary>
    protected float MaxChargesIn(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.ChargeCapIn(World.Client.Cooldowns, World.Client.DutyActions, Player.Level) : float.MaxValue;

    #endregion

    #region GCDs, Buffs, & Timers
    protected AID NextGCD;
    protected int NextGCDPrio;
    /// <summary>
    /// Estimated delay due to <em>animation lock</em>.
    /// </summary>
    protected float AnimationLockDelay { get; private set; }
    /// <summary>
    /// Estimated time remaining until next downtime phase.
    /// </summary>
    protected float DowntimeIn { get; private set; }
    /// <summary>
    /// Estimated time remaining until next uptime phase.
    /// </summary>
    protected float? UptimeIn { get; private set; }
    /// <summary>
    /// Elapsed time in <em>seconds</em> since the start of combat.
    /// </summary>
    protected float CombatTimer { get; private set; }
    /// <summary>
    /// Time remaining on pre-pull (or any) <em>Countdown Timer</em>.
    /// </summary>
    protected float? CountdownRemaining => World.Client.CountdownRemaining;
    /// <summary>
    /// Estimated time remaining until <em>Raid Buffs</em> are available.
    /// </summary>
    protected float RaidBuffsIn { get; private set; }
    /// <summary>
    /// Time remaining on <em>Raid Buffs</em>.
    /// </summary>
    protected float RaidBuffsLeft { get; private set; }
    /// <summary>
    /// <para>Time remaining until Movement is <em>forced.</em></para>
    /// <para>Effective for <em>Slidecasting.</em></para>
    /// </summary>
    private new (float Left, float In) EstimateRaidBuffTimings(Actor? primaryTarget)
    {
        if (primaryTarget?.OID != 0x2DE0)
            return (Bossmods.RaidCooldowns.DamageBuffLeft(Player), Bossmods.RaidCooldowns.NextDamageBuffIn2());

        var cycleTime = CombatTimer - 7.8f;
        if (cycleTime < 0)
            return (0, 7.8f - CombatTimer); // very beginning of a fight

        cycleTime %= 120;
        return cycleTime < 20 ? (20 - cycleTime, 0) : (0, 120 - cycleTime);
    }
    protected float ForceMovementIn;
    #endregion

    #region Status
    /// <summary>A quick and easy helper for retrieving the <em>HP</em> of the player.
    /// <para><em>Example Given:</em> "<em>HP == 4200</em>"</para></summary>
    /// <returns>- Player's <em>current HP</em></returns>
    protected uint HP;

    /// <summary>A quick and easy helper for retrieving the <em>MP</em> of the player.
    /// <para><em>Example Given:</em> "<em>MP == 6900</em>"</para></summary>
    /// <returns>- Player's <em>current MP</em></returns>
    protected uint MP;

    /// <summary> Retrieves the amount of specified status effect's stacks remaining on any target.
    /// <para><em>NOTE:</em> The effect can be owned by anyone.</para>
    /// <para><em>Example Given:</em> "<em>StacksRemaining(Player, SID.Requiescat, 30) > 0</em>"</para></summary>
    /// <param name="target">The <em>specified Target</em> we're checking for specified status effect. (e.g. "<em>Player</em>")<para>(NOTE: can also be any target if called)</para> </param>
    /// <param name="sid">The <em>Status ID</em> of specified status effect. (e.g. "<em>SID.Requiescat</em>")</param>
    /// <param name="duration"> The <em>Total Effect Duration</em> of specified status effect. (e.g. since <em>Requiescat</em>'s buff is 30 seconds, we simply use "<em>30</em>")</param>
    /// <returns>- A value indicating if the effect exists</returns>
    protected int StacksRemaining<SID>(Actor? target, SID sid, float duration = 1000f) where SID : Enum => StatusDetails(target, sid, Player.InstanceID, duration).Stacks;

    /// <summary> Retrieves the amount of specified status effect's time left remaining on any target.
    /// <para><em>NOTE:</em> The effect can be owned by anyone.</para>
    /// <para><em>Example Given:</em> "<em>StatusRemaining(Player, SID.Requiescat, 30) > 0f</em>"</para></summary>
    /// <param name="target">The <em>specified Target</em> we're checking for specified status effect. (e.g. "<em>Player</em>")<para>(NOTE: can also be any target if called)</para> </param>
    /// <param name="sid">The <em>Status ID</em> of specified status effect. (e.g. "<em>SID.Requiescat</em>")</param>
    /// <param name="duration"> The <em>Total Effect Duration</em> of specified status effect. (e.g. since <em>Requiescat</em>'s buff is 30 seconds, we simply use "<em>30</em>")</param>
    /// <returns>- A value indicating if the effect exists</returns>
    protected float StatusRemaining<SID>(Actor? target, SID sid, float duration) where SID : Enum => StatusDetails(target, sid, Player.InstanceID, duration).Left;

    /// <summary> Checks if a specific status effect on the player exists.
    /// <para><em>NOTE:</em> The effect can be owned by anyone.</para>
    /// <para><em>Example Given:</em> "<em>PlayerHasEffect(SID.NoMercy, 20)</em>"</para></summary>
    /// <param name="sid">The <em>Status ID</em> of specified status effect. (e.g. "<em>SID.NoMercy</em>")</param>
    /// <param name="duration"> The <em>Total Effect Duration</em> of specified status effect. (e.g. since <em>No Mercy</em>'s buff is 20 seconds, we simply use "<em>20</em>")</param>
    /// <returns>- A value indicating if the effect exists</returns>
    protected bool PlayerHasEffect<SID>(SID sid, float duration) where SID : Enum => StatusRemaining(Player, sid, duration) > 0.1f;

    /// <summary> Checks if a specific status effect on any specified target exists.
    /// <para><em>NOTE:</em> The effect can be owned by anyone.</para>
    /// <para><em>Example Given:</em> "<em>TargetHasEffect(primaryTarget, SID.SonicBreak, 30)</em>"</para></summary>
    /// <param name="target">The <em>specified Target</em> we're checking for specified status effect. (e.g. "<em>primaryTarget</em>")<para>(NOTE: can even be "Player")</para> </param>
    /// <param name="sid">The <em>Status ID</em> of specified status effect. (e.g. "<em>SID.SonicBreak</em>")</param>
    /// <param name="duration"> The <em>Total Effect Duration</em> of specified status effect. (e.g. since <em>Sonic Break</em>'s debuff is 30 seconds, we simply use "<em>30</em>")</param>
    /// <returns>- A value indicating if the effect exists</returns>
    protected bool TargetHasEffect<SID>(Actor? target, SID sid, float duration = 1000f) where SID : Enum => StatusRemaining(target, sid, duration) > 0.1f;

    /// <summary> Checks if Player has any stacks of specific status effect.
    /// <para><em>NOTE:</em> The effect can be owned by anyone.</para>
    /// <para><em>Example Given:</em> "<em>PlayerHasStacks(SID.Requiescat)</em>"</para></summary>
    /// <param name="sid">The <em>Status ID</em> of specified status effect. (e.g. "<em>SID.Requiescat</em>")</param>
    /// <returns>- A value indicating if the effect exists</returns>
    protected bool PlayerHasStacks<SID>(SID sid) where SID : Enum => StacksRemaining(Player, sid) > 0;

    #endregion

    #region Targeting
    /// <summary>
    /// Checks if target is within <em>Three (3) yalms</em> in distance.
    /// </summary>
    protected bool In3y(Actor? target) => Player.DistanceToHitbox(target) <= 2.99f; //Check if the target is within melee range (3 yalms)
    /// <summary>
    /// Checks if target is within <em>Five (5) yalms</em> in distance.
    /// </summary>
    protected bool In5y(Actor? target) => Player.DistanceToHitbox(target) <= 4.99f; //Check if the target is within 5 yalms
    /// <summary>
    /// Checks if target is within <em>Ten (10) yalms</em> in distance.
    /// </summary>
    protected bool In10y(Actor? target) => Player.DistanceToHitbox(target) <= 9.99f; //Check if the target is within 10 yalms
    /// <summary>
    /// Checks if target is within <em>Fifteen (15) yalms</em> in distance.
    /// </summary>
    protected bool In15y(Actor? target) => Player.DistanceToHitbox(target) <= 14.99f; //Check if the target is within 15 yalms
    /// <summary>
    /// Checks if target is within <em>Twenty (20) yalms</em> in distance.
    /// </summary>
    protected bool In20y(Actor? target) => Player.DistanceToHitbox(target) <= 19.99f; //Check if the target is within 20 yalms
    /// <summary>
    /// Checks if target is within <em>Twenty-five (25) yalms</em> in distance.
    /// </summary>
    protected bool In25y(Actor? target) => Player.DistanceToHitbox(target) <= 24.99f; //Check if the target is within 25 yalms
    /// <summary>
    /// <para>A simpler smart-targeting helper for selecting a <em>specific</em> target over your current target.</para>
    /// <para>Very useful for intricate planning of ability targeting in specific situations.</para>
    /// </summary>
    public Actor? TargetChoice(StrategyValues.OptionRef strategy) => ResolveTargetOverride(strategy.Value); //Resolves the target choice based on the strategy
    /// <summary>
    /// Attempts to select a suitable primary target automatically.
    /// </summary>
    protected void SelectPrimaryTarget(StrategyValues strategy, ref Actor? primaryTarget, float range)
    {
        if (!IsValidEnemy(primaryTarget))
            primaryTarget = null;

        PlayerTarget = primaryTarget;
        var AOEStrat = strategy.Option(SharedTrack.AOE).As<AOEStrategy>();
        if (AOEStrat is AOEStrategy.Auto)
        {
            if (Player.DistanceToHitbox(primaryTarget) > range)
            {
                var newTarget = Hints.PriorityTargets.FirstOrDefault(x => Player.DistanceToHitbox(x.Actor) <= range)?.Actor;
                if (newTarget != null)
                    primaryTarget = newTarget;
            }
        }
    }
    /// <summary>
    /// Attempts to select the best target automatically.
    /// </summary>
    protected (Actor? Best, int Targets) SelectTarget(
        StrategyValues strategy,
        Actor? primaryTarget,
        float range,
        PositionCheck isInAOE
    ) => SelectTarget(strategy, primaryTarget, range, isInAOE, (numTargets, _) => numTargets, a => a);
    /// <summary>
    /// Attempts to select the best target automatically by tracking HP.
    /// </summary>
    protected (Actor? Best, int Targets) SelectTargetByHP(StrategyValues strategy, Actor? primaryTarget, float range, PositionCheck isInAOE)
        => SelectTarget(strategy, primaryTarget, range, isInAOE, (numTargets, actor) => (numTargets, numTargets > 2 ? actor.HPMP.CurHP : 0), args => args.numTargets);
    /// <summary>
    /// Helper for <em>SelectTarget</em> functions.
    /// </summary>
    protected (Actor? Best, int Priority) SelectTarget<P>(
        StrategyValues strategy,
        Actor? primaryTarget,
        float range,
        PositionCheck isInAOE,
        PriorityFunc<P> prioritize,
        Func<P, int> simplify
    ) where P : struct, IComparable
    {
        var AOEStrat = strategy.Option(SharedTrack.AOE).As<AOEStrategy>();
        P targetPrio(Actor potentialTarget)
        {
            var numTargets = Hints.NumPriorityTargetsInAOE(enemy => isInAOE(potentialTarget, enemy.Actor));
            return prioritize(AdjustNumTargets(strategy, numTargets), potentialTarget);
        }

        var (newtarget, newprio) = strategy switch
        {
            { } when AOEStrat == AOEStrategy.Auto => primaryTarget == null
                ? (null, Priority: default)
                : FindBetterTargetBy(
                    primaryTarget,
                    range,
                    targetPrio,
                    enemy => isInAOE(enemy.Actor, primaryTarget)
                ),
            _ => (primaryTarget, primaryTarget == null ? default : targetPrio(primaryTarget))
        };

        var newnewprio = simplify(newprio);
        return (newnewprio > 0 ? newtarget : null, newnewprio);
    }

    /// <summary>
    /// Identify an appropriate target for applying <em>DoT</em> effect. This has no impact if any <em>auto-targeting</em> is disabled.
    /// </summary>
    protected (Actor? Target, P Timer) SelectDOTTarget<P>(StrategyValues strategy, Actor? initial, Func<Actor?, P> getTimer, int maxAllowedTargets) where P : struct, IComparable
    {
        var AOEStrat = strategy.Option(SharedTrack.AOE).As<AOEStrategy>();
        switch (AOEStrat)
        {
            case AOEStrategy.ForceST:
            case AOEStrategy.ForceAOE:
            case AOEStrategy.Auto:
                return (initial, getTimer(initial));
        }

        var newTarget = initial;
        var initialTimer = getTimer(initial);
        var newTimer = initialTimer;
        var numTargets = 0;
        foreach (var dotTarget in Hints.PriorityTargets)
        {
            if (dotTarget.ForbidDOTs)
                continue;

            if (++numTargets > maxAllowedTargets)
                return (null, getTimer(null));

            var thisTimer = getTimer(dotTarget.Actor);
            if (thisTimer.CompareTo(newTimer) < 0)
            {
                newTarget = dotTarget.Actor;
                newTimer = thisTimer;
            }
        }

        return (newTarget, newTimer);
    }
    /// <summary>
    /// Calculates number of AOE targets nearby inside <em>Melee range.</em>
    /// </summary>
    protected int TargetsInMeleeAOE(StrategyValues strategy) => TargetsNearby(strategy, 5);
    /// <summary>
    /// Helper for caluclating number of AOE targets nearby inside <em>5 yalms.</em>
    /// </summary>
    protected int TargetsNearby(StrategyValues strategy, float range) => AdjustNumTargets(strategy, Hints.NumPriorityTargetsInAOECircle(Player.Position, range));
    /// <summary>
    /// Helper for adjusting number of targets based on <em>AOE Strategy.</em>
    /// </summary>
    protected int AdjustNumTargets(StrategyValues strategy, int count)
    {
        var aoeStrat = strategy.Option(SharedTrack.AOE).As<AOEStrategy>();

        return count == 0 ? 0 : aoeStrat switch
        {
            AOEStrategy.Auto => count,
            AOEStrategy.ForceAOE => 10,
            AOEStrategy.ForceST => 0,
            _ => 0
        };
    }

    #region Position Checking
    /// <summary>
    /// Checks precise positioning between <em>player target</em> and any other targets.
    /// </summary>
    protected delegate bool PositionCheck(Actor playerTarget, Actor targetToTest);
    /// <summary>
    /// <para>Calculates the <em>priority</em> of a target based on the <em>total number of targets</em> and the <em>primary target</em> itself.</para>
    /// <para>It is generic, so it can return different types based on the implementation.</para>
    /// </summary>
    protected delegate P PriorityFunc<P>(int totalTargets, Actor primaryTarget);
    /// <summary>
    /// Position checker for determining the best target for an ability that deals <em>Splash</em> damage.
    /// </summary>
    protected PositionCheck IsSplashTarget => (Actor primary, Actor other) => Hints.TargetInAOECircle(other, primary.Position, 5);
    /// <summary>
    /// Position checker for determining the best target for an ability that deals damage in a <em>Cone</em> .
    /// </summary>
    protected PositionCheck IsConeTarget => (Actor primary, Actor other) => Hints.TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 45.Degrees());
    /// <summary>
    /// <para>Position checker for determining the best target for an ability that deals damage in a <em>Line</em> within <em>Ten (10) yalms</em>.</para>
    /// </summary>
    protected PositionCheck Is10yRectTarget => (Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2);
    /// <summary>
    /// <para>Position checker for determining the best target for an ability that deals damage in a <em>Line</em> within <em>Fifteen (15) yalms</em>.</para>
    /// </summary>
    protected PositionCheck Is15yRectTarget => (Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 15, 2);
    /// <summary>
    /// Position checker for determining the best target for an ability that deals damage in a <em>Line</em> within <em>Twenty-five (25) yalms</em>
    /// </summary>
    protected PositionCheck Is25yRectTarget => (Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 25, 2);
    #endregion

    #endregion

    #region Actors
    /// <summary>
    /// Player's "actual" target; guaranteed to be an enemy.
    /// </summary>
    protected Actor? PlayerTarget { get; private set; }
    /// <summary>
    /// Finds the <em>best target</em> by simply resolving the target choice based on the strategy or by defaulting to the <em>current target</em>.
    /// </summary>
    public Actor? BestTarget(StrategyValues.OptionRef strategy) => TargetChoice(strategy) ?? PlayerTarget; //Resolves the target choice based on the strategy or defaults to the current target
    /// <summary>
    /// Checks if target is valid. (e.g. not forbidden or a party member)
    /// </summary>
    private static bool IsValidEnemy(Actor? actor) => actor != null && !actor.IsAlly;

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
    protected void UpdatePositionals(Actor? target, ref (Positional pos, bool imm) positional, bool trueNorth)
    {
        if ((target?.Omnidirectional ?? true) || target?.TargetID == Player.InstanceID && target?.CastInfo == null && positional.pos != Positional.Front && target?.NameID != 541)
            positional = (Positional.Any, false);

        NextPositionalImminent = !trueNorth && positional.imm;
        NextPositionalCorrect = trueNorth || target == null || positional.pos switch
        {
            Positional.Flank => MathF.Abs(target.Rotation.ToDirection().Dot((Player.Position - target.Position).Normalized())) < 0.7071067f,
            Positional.Rear => target.Rotation.ToDirection().Dot((Player.Position - target.Position).Normalized()) < -0.7071068f,
            _ => true
        };
        Manager.Hints.RecommendedPositional = (target, positional.pos, NextPositionalImminent, NextPositionalCorrect);
    }

    /// <summary>
    /// Finds the <em>best Positional</em> automatically.
    /// </summary>
    protected void GoalZoneCombined(float range, Func<WPos, float> fAoe, int minAoe, Positional pos = Positional.Any)
    {
        if (PlayerTarget == null)
            Hints.GoalZones.Add(fAoe);
        else
            Hints.GoalZones.Add(Hints.GoalCombined(Hints.GoalSingleTarget(PlayerTarget, pos, range), fAoe, minAoe));
    }
    #endregion

    public abstract void ExecuteIt(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving);
}

static class ModuleExtensions
{
    public static RotationModuleDefinition DefineSharedTA(this RotationModuleDefinition res)
    {
        res.Define(SharedTrack.AOE).As<AOEStrategy>("Target")
            .AddOption(AOEStrategy.Automatic, "Auto", "Use optimal rotation", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceST, "ForceST", "Force Single Target", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceAOE, "ForceAOE", "Force AOE rotation", supportedTargets: ActionTargets.Hostile);
        res.Define(SharedTrack.Hold).As<HoldStrategy>("Hold")
            .AddOption(HoldStrategy.DontHold, "DontHold", "Don't hold any cooldowns or gauge abilities")
            .AddOption(HoldStrategy.HoldCooldowns, "Hold", "Hold all cooldowns only")
            .AddOption(HoldStrategy.HoldGauge, "HoldGauge", "Hold all gauge abilities only")
            .AddOption(HoldStrategy.HoldEverything, "HoldEverything", "Hold all cooldowns and gauge abilities");
        return res;
    }

    public static RotationModuleDefinition.ConfigRef<GCDStrategy> DefineGCD<Index>(this RotationModuleDefinition res, Index track, string name, int minLevel = 1) where Index : Enum
    {
        return res.Define(track).As<GCDStrategy>(name)
            .AddOption(GCDStrategy.Automatic, "Auto", "Use when optimal", minLevel: minLevel)
            .AddOption(GCDStrategy.Force, "Force", "Use ASAP", minLevel: minLevel)
            .AddOption(GCDStrategy.Delay, "Delay", "Don't use", minLevel: minLevel);
    }
    public static RotationModuleDefinition.ConfigRef<OGCDStrategy> DefineOGCD<Index>(this RotationModuleDefinition res, Index track, string name, int minLevel = 1) where Index : Enum
    {
        return res.Define(track).As<OGCDStrategy>(name)
            .AddOption(OGCDStrategy.Automatic, "Auto", "Use when optimal", minLevel: minLevel)
            .AddOption(OGCDStrategy.Force, "Force", "Use ASAP", minLevel: minLevel)
            .AddOption(OGCDStrategy.AnyWeave, "AnyWeave", "Use in next possible weave slot", minLevel: minLevel)
            .AddOption(OGCDStrategy.EarlyWeave, "EarlyWeave", "Use in next possible early weave slot", minLevel: minLevel)
            .AddOption(OGCDStrategy.LateWeave, "LateWeave", "Use in next possible late weave slot", minLevel: minLevel)
            .AddOption(OGCDStrategy.Delay, "Delay", "Don't use", minLevel: minLevel);
    }
}
