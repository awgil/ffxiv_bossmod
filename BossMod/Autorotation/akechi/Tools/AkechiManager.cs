namespace BossMod.Autorotation.akechi.Tools;

public enum SharedTrack { AOE, Target, Hold, Count }
public enum AOEStrategy
{
    Auto,
    ForceST,
    ForceAOE
}
public enum TargetStrategy
{
    Manual,
    Self,
    MostHitWithPrimaryTarget,
    MostHitWithoutPrimaryTarget,
    PartyWithMostCurrentHP,
    PartyWithLeastCurrentHP,
    PartyWithMostMaxHP,
    PartyWithLeastMaxHP,
    PartyByAssignment,
    EnemyWithMostCurrentHP,
    EnemyWithLeastCurrentHP,
    EnemyWithMostMaxHP,
    EnemyWithLeastMaxHP,
    EnemyWithHighestPriority,
    EnemyWithClosestDistance,
    EnemyByOID
}
public enum HoldStrategy
{
    DontHold,
    HoldCooldowns,
    HoldGauge,
    HoldEverything
}

public enum GCDStrategy
{
    Automatic,
    Force,
    Delay
}
public enum OGCDStrategy
{
    Automatic,
    Force,
    AnyWeave,
    EarlyWeave,
    LateWeave,
    Delay
}

public abstract class AkechiManager<AID, TraitID>(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
        where AID : struct, Enum where TraitID : Enum

{
    #region Core
    protected WorldState WorldState => Bossmods.WorldState;
    protected PartyRolesConfig PRC => Service.Config.Get<PartyRolesConfig>();

    #region Structs
    public record struct AkechiStrategyValue()
    {
        public int Option; // index of the selected option among the Options list of the corresponding config
        public float PriorityOverride = float.NaN; // priority override for the action controlled by the config; not all configs support it, if not set the default priority is used
        public TargetStrategy Target; // target selection strategy
        public int TargetParam; // strategy-specific parameter
        public float Offset1; // x or r coordinate
        public float Offset2; // y or phi coordinate
        public string Comment = ""; // user-editable comment string
        public float ExpireIn = float.MaxValue; // time until strategy expires
    }
    public readonly record struct AkechiStrategyValues(List<StrategyConfig> Configs)
    {
        public readonly AkechiStrategyValue[] Values = Utils.MakeArray(Configs.Count, new AkechiStrategyValue());

        public readonly ref struct OptionRef(ref StrategyConfig config, ref AkechiStrategyValue value)
        {
            public readonly ref readonly StrategyConfig Config = ref config;
            public readonly ref readonly AkechiStrategyValue Value = ref value;

            public OptionType As<OptionType>() where OptionType : Enum
            {
                if (Config.OptionEnum != typeof(OptionType))
                    throw new ArgumentException($"Unexpected option type for {Config.InternalName}: expected {Config.OptionEnum.FullName}, got {typeof(OptionType).FullName}");
                return (OptionType)(object)Value.Option;
            }

            public float Priority(float defaultPrio) => float.IsNaN(Value.PriorityOverride) ? defaultPrio : Value.PriorityOverride;
            public float Priority() => Priority(Config.Options[Value.Option].DefaultPriority);
        }

        public readonly OptionRef Option<TrackIndex>(TrackIndex index) where TrackIndex : Enum
        {
            var idx = (int)(object)index;
            return new(ref Configs.Ref(idx), ref Values[idx]);
        }
    }
    #endregion

    #endregion
    #region Core Rotation
    public void QueueGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum
    => QueueGCD(aid, target, (int)(object)priority, delay);
    public void QueueOGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => QueueOGCD(aid, target, (int)(object)priority, delay);
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

    /// <summary>
    /// Checks if action is <em>Unlocked</em> based on Level and Job Quest (if required).
    /// </summary>
    protected bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid)); //Check if the desired ability is unlocked
    /// <summary>
    /// Checks if Trait is <em>Unlocked</em> based on Level and Job Quest (if required).
    /// </summary>
    protected bool Unlocked(TraitID tid) => TraitUnlocked((uint)(object)tid);
    /// <summary>
    /// <para>Checks if <em>last combo action</em> is what the user is specifying.</para>
    /// <para>NOTE: This does <em>NOT</em> check all actions, only combo actions.</para>
    /// </summary>
    protected AID ComboLastMove => (AID)(object)World.Client.ComboState.Action;
    /// <summary>
    /// Primary function for appropriately calling any <em>GCDs</em>.
    /// </summary>
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
    /// <summary>
    /// Primary function for appropriately calling any <em>OGCDs</em>.
    /// </summary>
    public void QueueOGCD(AID aid, Actor? target, int priority = 4, float delay = 0)
    {
        if (priority == 0)
            return;

        QueueAction(aid, target, ActionQueue.Priority.Medium + priority, delay);
    }


    #endregion
    #region GCD & Timers
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
    /// <para>Retrieves player's GCD length based on <em>Skill-Speed</em>.</para>
    /// <para>NOTE: This function is only recommended for jobs with <em>Skill-Speed</em>. <em>Spell-Speed</em> users are <em>unaffected</em> by this.</para>
    /// </summary>
    protected float SkSGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
    /// <summary>
    /// Retrieves player's current <em>Skill-Speed</em> stat.
    /// </summary>
    protected float SkS => ActionSpeed.Round(World.Client.PlayerStats.SpellSpeed);

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
    /// Retrieves <em>cooldown timer</em> for specified action.
    /// </summary>
    protected float CD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining; //Get remaining cooldown time for the specified action
    /// <summary>
    /// Checks if action is on cooldown based on its <em>cooldown timer</em>.
    /// </summary>
    protected bool IsOnCooldown(AID aid) => CD(aid) > 0.6f;
    /// <summary>
    /// Checks if action is off cooldown based on its <em>cooldown timer</em>.
    /// </summary>
    protected bool IsOffCooldown(AID aid) => !IsOnCooldown(aid);
    /// <summary
    /// Checks if action is on cooldown based on its <em>charges</em>.
    /// </summary>
    protected bool OnCooldown(AID aid) => MaxChargesIn(aid) > 0;
    /// <summary>
    /// Time remaining until action is <em>ready</em>.
    /// </summary>
    protected float ReadyIn(AID action) => Unlocked(action) ? ActionDefinitions.Instance.Spell(action)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) : float.MaxValue;
    /// <summary>
    /// Time remaining until action is at <em>maximum charges</em>.
    /// </summary>
    protected float MaxChargesIn(AID action) => Unlocked(action) ? ActionDefinitions.Instance.Spell(action)!.ChargeCapIn(World.Client.Cooldowns, World.Client.DutyActions, Player.Level) : float.MaxValue;
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
    #region Status
    /// <summary>
    /// Retrieves player's <em>current HP.</em>
    /// </summary>
    protected uint HP;
    /// <summary>
    /// Retrieves player's <em>current MP.</em>
    /// </summary>
    protected uint MP;

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
    /// Our targeting options pertaining to <em>smart-targeting</em> logic.
    /// </summary>
    protected Actor? TargetingOverride(TargetStrategy strategy, int param) => strategy switch
    {
        TargetStrategy.Self => Player,
        TargetStrategy.PartyWithLeastCurrentHP => WorldState.Party.WithoutSlot().Exclude(param != 0 ? null : Player).MinBy(a => a.HPMP.CurHP),
        TargetStrategy.PartyWithLeastMaxHP => WorldState.Party.WithoutSlot().Exclude(param != 0 ? null : Player).MinBy(a => a.HPMP.MaxHP),
        TargetStrategy.PartyByAssignment => PRC.SlotsPerAssignment(WorldState.Party) is var spa && param < spa.Length ? WorldState.Party[spa[param]] : null,
        TargetStrategy.EnemyWithMostCurrentHP => Player != null ? Hints.PriorityTargets.MaxBy(e => e.Actor.HPMP.CurHP)?.Actor : null,
        TargetStrategy.EnemyWithLeastCurrentHP => Player != null ? Hints.PriorityTargets.MinBy(e => e.Actor.HPMP.CurHP)?.Actor : null,
        TargetStrategy.EnemyWithMostMaxHP => Player != null ? Hints.PriorityTargets.MaxBy(e => e.Actor.HPMP.MaxHP)?.Actor : null,
        TargetStrategy.EnemyWithLeastMaxHP => Player != null ? Hints.PriorityTargets.MinBy(e => e.Actor.HPMP.MaxHP)?.Actor : null,
        TargetStrategy.EnemyWithClosestDistance => Player != null ? Hints.PriorityTargets.MinBy(e => (e.Actor.Position - Player.Position).LengthSq())?.Actor : null,
        TargetStrategy.EnemyWithHighestPriority => Player != null ? Hints.PriorityTargets.MaxBy(e => e.Priority)?.Actor : null,
        TargetStrategy.EnemyByOID => Player != null && (uint)param is var oid && oid != 0 ? Hints.PotentialTargets.Where(e => e.Actor.OID == oid).MinBy(e => (e.Actor.Position - Player.Position).LengthSq())?.Actor : null,
        _ => null
    };
    /// <summary>
    /// <para>Smart-targeting helper for selecting a <em>specific</em> target over your current target.</para>
    /// <para>Very useful for intricate planning of ability targeting in specific situations.</para>
    /// </summary>
    protected Actor? ExecuteTargetOverride(in AkechiStrategyValue strategy) => TargetingOverride(strategy.Target, strategy.TargetParam);
    /// <summary>
    /// <para>A simpler smart-targeting helper for selecting a <em>specific</em> target over your current target.</para>
    /// <para>Very useful for intricate planning of ability targeting in specific situations.</para>
    /// </summary>
    public Actor? TargetChoice(AkechiStrategyValues.OptionRef strategy) => ExecuteTargetOverride(strategy.Value); //Resolves the target choice based on the strategy
    /// <summary>
    /// Attempts to select a suitable primary target automatically.
    /// </summary>
    protected void SelectPrimaryTarget(AkechiStrategyValues strategy, ref Actor? primaryTarget, float range)
    {
        var tStrat = strategy.Option(SharedTrack.Target).As<TargetStrategy>();

        if (!IsValidEnemy(primaryTarget))
            primaryTarget = null;

        PlayerTarget = primaryTarget;

        if (tStrat is TargetStrategy.MostHitWithPrimaryTarget)
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
        AkechiStrategyValues strategy,
        Actor? primaryTarget,
        float range,
        PositionCheck isInAOE
    ) => SelectTarget(strategy, primaryTarget, range, isInAOE, (numTargets, _) => numTargets, a => a);
    /// <summary>
    /// Attempts to select the best target automatically by tracking HP.
    /// </summary>
    protected (Actor? Best, int Targets) SelectTargetByHP(AkechiStrategyValues strategy, Actor? primaryTarget, float range, PositionCheck isInAOE)
        => SelectTarget(strategy, primaryTarget, range, isInAOE, (numTargets, actor) => (numTargets, numTargets > 2 ? actor.HPMP.CurHP : 0), args => args.numTargets);
    /// <summary>
    /// Helper for <em>SelectTarget</em> functions.
    /// </summary>
    protected (Actor? Best, int Priority) SelectTarget<P>(
        AkechiStrategyValues strategy,
        Actor? primaryTarget,
        float range,
        PositionCheck isInAOE,
        PriorityFunc<P> prioritize,
        Func<P, int> simplify
    ) where P : struct, IComparable
    {
        var AOEStrat = strategy.Option(SharedTrack.AOE).As<AOEStrategy>();
        var TStrat = strategy.Option(SharedTrack.Target).As<TargetStrategy>();

        P targetPrio(Actor potentialTarget)
        {
            var numTargets = Hints.NumPriorityTargetsInAOE(enemy => isInAOE(potentialTarget, enemy.Actor));
            return prioritize(AdjustNumTargets(strategy, numTargets), potentialTarget);
        }

        if (AOEStrat == AOEStrategy.ForceST)
            TStrat = TargetStrategy.Manual;

        if (TStrat == TargetStrategy.MostHitWithPrimaryTarget)
            TStrat = primaryTarget == null ? TargetStrategy.MostHitWithoutPrimaryTarget : TargetStrategy.MostHitWithPrimaryTarget;

        var (newtarget, newprio) = strategy switch
        {
            TargetStrategy.MostHitWithPrimaryTarget => FindBetterTargetBy(primaryTarget, range, targetPrio),
            AOEStrategy.Auto => primaryTarget == null ? (null, default) : FindBetterTargetBy(
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
    protected (Actor? Target, P Timer) SelectDOTTarget<P>(AkechiStrategyValues strategy, Actor? initial, Func<Actor?, P> getTimer, int maxAllowedTargets) where P : struct, IComparable
    {
        var tStrat = strategy.Option(SharedTrack.Target).As<TargetStrategy>();
        switch (tStrat)
        {
            case TargetStrategy.Manual:
            case TargetStrategy.MostHitWithoutPrimaryTarget:
                return (initial, getTimer(initial));
            case TargetStrategy.MostHitWithPrimaryTarget:
                if (initial != null)
                    return (initial, getTimer(initial));
                break;
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
    protected int TargetsInMeleeAOE(AkechiStrategyValues strategy) => TargetsNearby(strategy, 5);
    /// <summary>
    /// Helper for caluclating number of AOE targets nearby inside <em>5 yalms.</em>
    /// </summary>
    protected int TargetsNearby(AkechiStrategyValues strategy, float range) => AdjustNumTargets(strategy, Hints.NumPriorityTargetsInAOECircle(Player.Position, range));
    /// <summary>
    /// Helper for adjusting number of targets based on <em>AOE Strategy.</em>
    /// </summary>
    protected int AdjustNumTargets(AkechiStrategyValues strategy, int count)
        => count == 0 ? 0 : strategy switch
        {
            AOEStrategy.Auto => count,
            AOEStrategy.ForceAOE => 10,
            AOEStrategy.ForceST => 0,
            _ => 0
        };
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
    /// <para>Position checker for determining the best target for an ability that deals <em>Line</em> damage within <em>Ten (10) yalms</em>.</para>
    /// </summary>
    protected PositionCheck Is10yRectTarget => (Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2);
    /// <summary>
    /// <para>Position checker for determining the best target for an ability that deals <em>Line</em> damage within <em>Fifteen (15) yalms</em>.</para>
    /// </summary>
    protected PositionCheck Is15yRectTarget => (Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 15, 2);
    /// <summary>
    /// <para>Position checker for determining the best target for an ability that deals <em>Line</em> damage within <em>Twenty-five (25) yalms</em>.</para>
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
    public Actor? BestTarget(AkechiStrategyValues.OptionRef strategy) => TargetChoice(strategy) ?? PlayerTarget; //Resolves the target choice based on the strategy or defaults to the current target

    #endregion
    #region Positionals
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

    /// <summary>
    /// Retreieves <em>effective</em> cast time by returning the action's base cast time multiplied by the player's spell-speed factor, which accounts for haste buffs (like <em>Ley Lines</em>) and slow debuffs. It also accounts for <em>Swiftcast</em>.
    /// </summary>
    protected virtual float GetCastTime(AID aid) => SelfStatusLeft(ClassShared.SID.Swiftcast, 10) > GCD ? 0 : ActionDefinitions.Instance.Spell(aid)!.CastTime * SpSGCDLength / 2.5f;

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
    /// <para>Time remaining until Movement is <em>forced.</em></para>
    /// <para>Effective for <em>Slidecasting.</em></para>
    /// </summary>
    protected float ForceMovementIn;

    /// <summary>
    /// Checks if target is valid. (e.g. not forbidden or a party member)
    /// </summary>
    private static bool IsValidEnemy(Actor? actor) => actor != null && !actor.IsAlly;

    protected Positional GetCurrentPositional(Actor target) => (Player.Position - target.Position).Normalized().Dot(target.Rotation.ToDirection()) switch
    {
        < -0.7071068f => Positional.Rear,
        < 0.7071068f => Positional.Flank,
        _ => Positional.Front
    };

    protected bool NextPositionalImminent;
    protected bool NextPositionalCorrect;

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

    private new (float Left, float In) EstimateRaidBuffTimings(Actor? primaryTarget)
    {
        // striking dummy that spawns in Explorer Mode
        if (primaryTarget?.OID != 0x2DE0)
            return (Bossmods.RaidCooldowns.DamageBuffLeft(Player), Bossmods.RaidCooldowns.NextDamageBuffIn2());

        // hack for a dummy: expect that raidbuffs appear at 7.8s and then every 120s
        var cycleTime = CombatTimer - 7.8f;
        if (cycleTime < 0)
            return (0, 7.8f - CombatTimer); // very beginning of a fight

        cycleTime %= 120;
        return cycleTime < 20 ? (20 - cycleTime, 0) : (0, 120 - cycleTime);
    }

    public abstract void Execute(AkechiStrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving);

    protected (float Left, int Stacks) Status<SID>(SID status) where SID : Enum => Player.FindStatus(status) is ActorStatus s ? (StatusDuration(s.ExpireAt), s.Extra & 0xFF) : (0, 0);
    protected float StatusLeft<SID>(SID status) where SID : Enum => Status(status).Left;
    protected int StatusStacks<SID>(SID status) where SID : Enum => Status(status).Stacks;

    protected float HPRatio(Actor actor) => (float)actor.HPMP.CurHP / Player.HPMP.MaxHP;
    protected float HPRatio() => HPRatio(Player);

    protected uint PredictedHP(Actor actor) => (uint)Math.Clamp(actor.HPMP.CurHP + World.PendingEffects.PendingHPDifference(actor.InstanceID), 0, actor.HPMP.MaxHP);
    protected float PredictedHPRatio(Actor actor) => (float)PredictedHP(actor) / actor.HPMP.MaxHP;
}

static class ModuleExtensions
{
    public static RotationModuleDefinition DefineSharedTA(this RotationModuleDefinition res)
    {
        res.Define(SharedTrack.Target).As<TargetStrategy>("Target")
            .AddOption(TargetStrategy.Manual, "Manual", "Use manual target selection")
            .AddOption(TargetStrategy.Self, "Self", "Use self as target")
            .AddOption(TargetStrategy.PartyWithMostCurrentHP, "PartyWithMostCurrentHP", "Target party member with most current HP")
            .AddOption(TargetStrategy.PartyWithLeastCurrentHP, "PartyWithLeastCurrentHP", "Target party member with least current HP")
            .AddOption(TargetStrategy.PartyWithMostMaxHP, "PartyWithMostMaxHP", "Target party member with most max HP")
            .AddOption(TargetStrategy.PartyWithLeastMaxHP, "PartyWithLeastMaxHP", "Target party member with least max HP")
            .AddOption(TargetStrategy.PartyByAssignment, "PartyByAssignment", "Target party member by assignment")
            .AddOption(TargetStrategy.EnemyWithMostCurrentHP, "EnemyWithMostCurrentHP", "Target enemy with most current HP")
            .AddOption(TargetStrategy.EnemyWithLeastCurrentHP, "EnemyWithLeastCurrentHP", "Target enemy with least current HP")
            .AddOption(TargetStrategy.EnemyWithMostMaxHP, "EnemyWithMostMaxHP", "Target enemy with most max HP")
            .AddOption(TargetStrategy.EnemyWithLeastMaxHP, "EnemyWithLeastMaxHP", "Target enemy with least max HP")
            .AddOption(TargetStrategy.EnemyWithHighestPriority, "EnemyWithHighestPriority", "Target enemy with highest priority")
            .AddOption(TargetStrategy.EnemyWithClosestDistance, "EnemyWithClosestDistance", "Target enemy with closest distance")
            .AddOption(TargetStrategy.EnemyByOID, "EnemyByOID", "Target enemy by OID");
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


    public static AOEStrategy AOE(this AkechiStrategyValues strategy) => strategy.Option(SharedTrack.AOE).As<AOEStrategy>();
    public static TargetStrategy Targeting(this AkechiStrategyValues strategy) => strategy.Option(SharedTrack.Target).As<TargetStrategy>();
    public static OffensiveStrategy Simple<Index>(this AkechiStrategyValues strategy, Index track) where Index : Enum => strategy.Option(track).As<OffensiveStrategy>();
    public static bool CooldownsOk(this AkechiStrategyValues strategy) => strategy.Option(SharedTrack.Buffs).As<OffensiveStrategy>() != OffensiveStrategy.Delay;
}
