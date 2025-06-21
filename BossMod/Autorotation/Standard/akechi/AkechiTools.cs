using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

#region Shared Enums
/// <summary> <b>SharedTrack</b> enum for <b>AOE</b> and <b>Hold</b> strategies, used in damage rotations for all PvE classes and jobs. </summary>
public enum SharedTrack
{
    /// <summary> Tracks <b>single-target</b> and <b>AOE</b> rotations. </summary>
    AOE,

    /// <summary> Tracks which <b>targeting</b> strategy the user would like to use. </summary>
    Targeting,

    /// <summary> Tracks holding user's <b>rotation</b>, <b>buffs</b>, <b>gauge</b>, or <b>cooldown abilities</b> for optimal usage. </summary>
    Hold,

    /// <summary> Tracks use of <b>Potions</b>. </summary>
    Potion,

    /// <summary> Represents the total count of strategies in this track. </summary>
    Count
}

/// <summary> <b>AOEStrategy</b> enum for tracking <b>single-target</b> and <b>AOE</b> strategies.<para/>
/// <b>NOTE</b>: For jobs with combos that have no relative combo timer (e.g. <b>BLM</b>), <seealso cref="AutoFinish"/> and <seealso cref="AutoBreak"/> are essentially the same function. </summary>
public enum AOEStrategy
{
    /// <summary> Executes the most <b>optimal rotation</b> automatically.<br/>
    /// Finishes any combo <em>if</em> currently inside one.</summary>
    AutoFinish,

    /// <summary> Executes the most <b>optimal rotation</b> automatically. <br/>
    /// Breaks any combo <em>if</em> currently inside one.</summary>
    AutoBreak,

    /// <summary> Forces execution of the <b>single-target rotation</b>. </summary>
    ForceST,

    /// <summary> Forces execution of the <b>AOE rotation</b>. </summary>
    ForceAOE
}

/// <summary> <b>SoftTargetStrategy</b> enum for tracking which <b>targeting</b> strategy the user would like to use.<para/>
/// <b>NOTE</b>: This does <b>NOT</b> take priority over <seealso cref="StrategyTarget"/>. </summary>
public enum SoftTargetStrategy
{
    /// <summary> <b>Automatic</b> selection of user's target for abilities executed.</summary>
    Automatic,

    /// <summary> <b>Manual</b> selection of user's target for abilities executed.</summary>
    Manual,
}

/// <summary> <b>HoldStrategy</b> enum for tracking when to hold <b>buffs</b>, <b>gauge</b>, or <b>cooldown abilities</b>. </summary>
public enum HoldStrategy
{
    /// <summary> Uses all <b>buffs</b>, <b>gauge</b>, and <b>cooldown abilities</b> without restriction. </summary>
    DontHold,

    /// <summary> Holds all <b>cooldown-related abilities</b> only. </summary>
    HoldCooldowns,

    /// <summary> Holds all <b>gauge-related abilities</b> only. </summary>
    HoldGauge,

    /// <summary> Holds all <b>buff-related abilities</b> only. </summary>
    HoldBuffs,

    /// <summary> Holds all <b>buffs</b>, <b>cooldowns</b>, and <b>gauge abilities</b>. </summary>
    HoldAbilities,

    /// <summary> Holds <b>ALL</b> actions entirely. </summary>
    HoldEverything
}

/// <summary> <b>PotionStrategy</b> enum for tracking when to use <b>potions</b>. </summary>
public enum PotionStrategy
{
    /// <summary> Do <b>NOT</b> use <b>potions</b> automatically. </summary>
    Manual,

    /// <summary> Uses <b>potions</b> automatically when <b>personal buffs</b> are imminent. </summary>
    AlignWithBuffs,

    /// <summary> Uses <b>potions</b> automatically when <b>party raid buffs</b> are imminent. </summary>
    AlignWithRaidBuffs,

    /// <summary> Uses <b>potions</b> as soon as possible without restriction. </summary>
    Immediate
}

/// <summary> <b>GCDStrategy</b> enum for managing module-specific <b>GCD abilities</b>. </summary>
public enum GCDStrategy
{
    /// <summary> Executes the ability <b>automatically</b> based on user logic. </summary>
    Automatic,

    /// <summary> Executes the ability <b>automatically</b> based on user logic, but only if <b>raid buffs</b> are present. </summary>
    RaidBuffsOnly,

    /// <summary> <b>Forces</b> execution of the ability. </summary>
    Force,

    /// <summary> <b>Forbids</b> execution of the ability. </summary>
    Delay
}

/// <summary> <b>OGCDStrategy</b> enum for managing module-specific <b>OGCD abilities</b>. </summary>
public enum OGCDStrategy
{
    /// <summary> Executes the ability <b>automatically</b> based on user logic. </summary>
    Automatic,

    /// <summary> Executes the ability <b>automatically</b> based on user logic, but only if <b>raid buffs</b> are present. </summary>
    RaidBuffsOnly,

    /// <summary> <b>Forces</b> execution of the ability. </summary>
    Force,

    /// <summary> <b>Forces</b> execution of the ability in the very next weave window. </summary>
    AnyWeave,

    /// <summary> <b>Forces</b> execution of the ability in the very next <b>EARLY</b>-weave window. </summary>
    EarlyWeave,

    /// <summary> <b>Forces</b> execution of the ability in the very next <b>LATE</b>-weave window. </summary>
    LateWeave,

    /// <summary> <b>Forbids</b> execution of the ability. </summary>
    Delay
}

/// <summary> <b>AllowOrForbid</b> enum for managing module-specific <b>GCD</b> and <b>OGCD abilities</b>. </summary>
public enum AllowOrForbid
{
    /// <summary> <b>Allow</b> the action to be executed. </summary>
    Allow,

    /// <summary> <b>Forces</b> the action to be executed. </summary>
    Force,

    /// <summary> <b>Forbid</b> the action from being executed. </summary>
    Forbid
}
#endregion

/// <summary>The core foundation of how we execute everything, from queuing GCDs to implementing our rotation helpers, functions, and tools.<br/> This base provides a robust framework equipped with a comprehensive suite of functions designed to streamline optimization and simplify the creation of advanced rotation modules.</summary>
/// <typeparam name="AID">The user's specified <b>Action ID</b> being checked, called by <b>using <seealso cref="BossMod"/>.[class/job acronym]</b>.</typeparam>
/// <typeparam name="TraitID">The user's specified <b>Trait ID</b> being checked, called by <b>using <seealso cref="BossMod"/>.[class/job acronym]</b>.</typeparam>
/// <param name="manager">The specified <b>Rotation Module Manager</b> being used.</param>
/// <param name="player">The <b>User</b> that is executing this module.</param>
public abstract class AkechiTools<AID, TraitID>(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
        where AID : struct, Enum where TraitID : Enum
{
    #region Core Ability Execution
    /// <summary>The <b>Next GCD</b> being queued.</summary>
    protected AID NextGCD;

    /// <summary>The <b>Next GCD</b> being queued's <b>Priority</b>.</summary>
    protected int NextGCDPrio;

    #region Queuing

    #region GCD
    /// <summary>
    /// The primary helper we use for calling all our <b>GCD</b> abilities onto any actor.<para/>
    /// <b>NOTE:</b> For compatibility between <c>Actor?</c> and <c>Enemy?</c> inside one function, use <c> primaryTarget?.Actor</c>  as <c>Enemy?</c>  definition.
    /// </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <param name="priority">The user's specified <b>Priority</b>.</param>
    /// <param name="delay">The user's specified <b>application delay</b>.</param>
    /// <param name="castTime">The user's specified <b>cast time</b> for the ability.</param>
    /// <param name="targetPos">The user's specified <b>Target Position</b> for the ability.</param>
    /// <param name="facingAngle">The user's specified <b>Angle facing Target</b> for the ability.</param>
    protected void QueueGCD<P>(AID aid, Actor? target, P priority, float delay = 0, float castTime = 0, Vector3 targetPos = default, Angle? facingAngle = null) where P : Enum
                => QueueGCDs(aid, target, (int)(object)priority, delay, castTime, targetPos, facingAngle);
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
    #endregion

    #region OGCD
    /// <summary>
    /// The primary helper we use for calling all our <b>OGCD</b> abilities onto any actor.<para/>
    /// <b>NOTE:</b> For compatibility between <c>Actor?</c> and <c>Enemy?</c> inside one function, use <c> primaryTarget?.Actor</c>  as <c>Enemy?</c>  definition.
    /// </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <param name="priority">The user's specified <b>Priority</b>.</param>
    /// <param name="delay">The user's specified <b>application delay</b>.</param>
    /// <param name="castTime">The user's specified <b>cast time</b> for the ability.</param>
    /// <param name="targetPos">The user's specified <b>Target Position</b> for the ability.</param>
    /// <param name="facingAngle">The user's specified <b>Angle facing Target</b> for the ability.</param>
    protected void QueueOGCD<P>(AID aid, Actor? target, P priority, float delay = 0, float castTime = 0, Vector3 targetPos = default, Angle? facingAngle = null) where P : Enum
                => QueueOGCDs(aid, target, (int)(object)priority, delay, castTime, targetPos, facingAngle);
    protected void QueueOGCDs(AID aid, Actor? target, int priority = 1, float delay = 0, float castTime = 0, Vector3 targetPos = default, Angle? facingAngle = null)
    {
        if (priority == 0)
            return;

        QueueAction(aid, target, ActionQueue.Priority.Low + priority, delay, castTime, targetPos, facingAngle);
    }
    #endregion

    protected bool QueueAction(AID aid, Actor? target, float priority, float delay, float castTime, Vector3 targetPos = default, Angle? facingAngle = null)
    {
        var ability = ActionDefinitions.Instance.Spell(aid);

        if ((uint)(object)aid == 0)
            return false;
        if (ability == null)
            return false;
        if (ability.Range != 0 && target == null)
        {
            return false;
        }
        if (ability.AllowedTargets.HasFlag(ActionTargets.Area))
        {
            if (ability.Range == 0)
                targetPos = Player.PosRot.XYZ();
            else if (target != null)
                targetPos = target.PosRot.XYZ();
        }
        if (castTime == 0)
            castTime = 0;

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority, delay: delay, castTime: castTime, targetPos: targetPos, facingAngle: facingAngle);
        return true;
    }
    #endregion

    #endregion

    #region HP/MP/Shield
    /// <summary>Retrieves the <b>current HP</b> value of the player.</summary>
    protected uint HP { get; private set; }

    /// <summary>Retrieves the <b>maximum HP</b> value of the player.</summary>
    protected uint MaxHP { get; private set; }

    /// <summary>Retrieves the <b>current MP</b> value of the player.</summary>
    protected uint MP { get; private set; }

    /// <summary>Retrieves the <b>maximum MP</b> value of the player.</summary>
    protected uint MaxMP { get; private set; }

    /// <summary>Retrieves the <b>current Shield</b> value of the player.</summary>
    protected uint Shield { get; private set; }

    /// <summary>Retrieves the <b>current HP</b> value of a specified actor.</summary>
    /// <param name="actor">The specified <b>target actor</b>.</param>
    protected uint TargetCurrentHP(Actor actor) => actor.HPMP.CurHP;

    /// <summary>Retrieves the <b>current Shield</b> value of a specified actor.</summary>
    /// <param name="actor">The <b>target actor</b>.</param>
    protected uint TargetCurrentShield(Actor actor) => actor.HPMP.Shield;

    /// <summary>Checks if a specified actor has any current <b>active Shield</b>.</summary>
    /// <param name="actor">The <b>target actor</b>.</param>
    protected bool TargetHasShield(Actor actor) => actor.HPMP.Shield > 0.1f;

    /// <summary>Retrieves the <b>player's current HP percentage</b>.</summary>
    protected float PlayerHPP => TargetHPP(Player);

    /// <summary>Retrieves the <b>current HP percentage</b> of a specified actor.</summary>
    /// <param name="actor">The <b>target actor</b>.</param>
    protected static float TargetHPP(Actor? actor = null)
    {
        if (actor is null || actor.IsDead)
            return 0f;

        var HPP = (float)actor.HPMP.CurHP / actor.HPMP.MaxHP * 100f;
        return Math.Clamp(HPP, 0f, 100f);
    }
    #endregion

    #region Actions
    /// <summary>Checks if specified <b>action</b> is <b>Unlocked</b> based on <b>Level</b> and <b>Job Quest</b> (if required).</summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    protected bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));

    /// <summary>Checks if specified <b>trait</b> is <b>Unlocked</b> based on <b>Level</b> and <b>Job Quest</b> (if required).</summary>
    /// <param name="tid">The <b>Trait ID</b> being checked.</param>
    protected bool Unlocked(TraitID tid) => TraitUnlocked((uint)(object)tid);

    /// <summary>Checks the <b>last combo action</b> is what the user is specifying.</summary>
    protected AID ComboLastMove => (AID)(object)World.Client.ComboState.Action;

    /// <summary>Retrieves player's <b>GCD length</b> based on <b>Skill-Speed</b>.</summary>
    protected float SkSGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);

    /// <summary>Retrieves player's <b>GCD length</b> based on <b>Spell-Speed</b>.</summary>
    protected float SpSGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level);

    /// <summary>Checks if we can fit in a <b>skill-speed based</b> GCD into a deadline.</summary>
    /// <param name="duration">The <b>duration</b> to check against.</param>
    /// <param name="extraGCDs">How many <b>extra GCDs</b> the user can fit in.</param>
    protected bool CanFitSkSGCD(float duration, int extraGCDs = 0) => GCD + SkSGCDLength * extraGCDs < duration;

    /// <summary>Checks if we can fit in a <b>spell-speed based</b> GCD into a deadline.</summary>
    /// <param name="duration">The <b>duration</b> to check against.</param>
    /// <param name="extraGCDs">How many <b>extra GCDs</b> the user can fit in.</param>
    protected bool CanFitSpSGCD(float duration, int extraGCDs = 0) => GCD + SpSGCDLength * extraGCDs < duration;

    /// <summary>Checks if user is in <b>pre-pull</b> stage.</summary>
    protected bool IsFirstGCD => !Player.InCombat || CombatTimer < 0.1f;

    /// <summary>Checks if user can <b>Weave in</b> any <b>abilities</b>.</summary>
    protected bool CanWeaveIn => GCD >= 0.6f;

    /// <summary>Checks if user can <b>Early Weave in</b> any <b>abilities</b>.</summary>
    protected bool CanEarlyWeaveIn => GCD >= 1.26f;

    /// <summary>Checks if user can <b>Late Weave in</b> any <b>abilities</b>.</summary>
    protected bool CanLateWeaveIn => GCD is <= 1.25f and >= 0.6f;

    /// <summary>Checks if user can <b>Quarter Weave in</b> any <b>abilities</b>.</summary>
    protected bool CanQuarterWeaveIn => GCD is < 0.9f and >= 0.5f;

    /// <summary>Checks if Player is in an <b>odd minute window</b> by checking job's specified <b>120s ability</b> explicitly.</summary>
    protected bool InOddWindow(AID aid) => CDRemaining(aid) is < 90 and > 30;
    #endregion

    #region Cooldown
    /// <summary>Retrieves the total <b>cooldown</b> time left on the specified <b>action</b>. </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    protected float CDRemaining(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    /// <summary>Returns the <b>charge cooldown</b> time left on the specified <b>action</b>. </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    protected float ReadyIn(AID aid) => ActionDefinitions.Instance.Spell(aid)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions);

    /// <summary>Checks if <b>specified action</b> is ready to be used based on if it's <b>Unlocked</b> and its <b>total cooldown timer</b>. </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    protected bool ActionReady(AID aid) => Unlocked(aid) && CDRemaining(aid) <= 0.5f;

    /// <summary>Checks if last <b>action</b> used is what the user is specifying. </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    protected bool LastActionUsed(AID aid) => Manager.LastCast.Data?.IsSpell(aid) == true || Manager.LastCast.Data?.Action == ActionID.MakeSpell(aid);

    /// <summary>Retrieves time remaining until specified <b>action</b> is at <b>max charges</b>. </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    protected float MaxChargesIn(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.ChargeCapIn(World.Client.Cooldowns, World.Client.DutyActions, Player.Level) : float.MaxValue;
    #endregion

    #region Status
    /// <summary>Retrieves the amount of specified <b>status effect's stacks</b> remaining on any target.
    /// <para><b>NOTE:</b> The effect MUST be owned by the <b>Player</b>.</para></summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <param name="sid"> The user's specified <b>Status ID</b> being checked.</param>
    /// <param name="duration"> The <b>Total Effect Duration</b> of specified <b>Status ID</b> being checked.</param>
    protected int StacksRemaining<SID>(Actor? target, SID sid, float duration = 1000f) where SID : Enum => StatusDetails(target, sid, Player.InstanceID, duration).Stacks;

    /// <summary>Retrieves the amount of specified <b>status effect's time</b> left remaining on any target.
    /// <para><b>NOTE:</b> The effect <b>MUST</b> be owned by the <b>Player</b>.</para></summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <param name="sid"> The user's specified <b>Status ID</b> being checked.</param>
    /// <param name="duration"> The <b>Total Effect Duration</b> of specified <b>Status ID</b> being checked.</param>
    protected float StatusRemaining<SID>(Actor? target, SID sid, float duration = 1000f) where SID : Enum => StatusDetails(target, sid, Player.InstanceID, duration).Left;

    /// <summary>Checks if a specific <b>status effect</b> on the <b>Player</b> exists.
    /// <para><b>NOTE:</b> The effect can be owned by <b>anyone</b>; Player, Party, Alliance, NPCs, or even enemies.</para></summary>
    /// <param name="sid"> The user's specified <b>Status ID</b> being checked.</param>
    protected bool HasEffect<SID>(SID sid) where SID : Enum => Player.FindStatus(sid) != null;

    /// <summary>Checks if a specific <b>status effect</b> on any specified <b>Target</b> exists.
    /// <para><b>NOTE:</b> The effect <b>MUST</b> be owned by the <b>Player</b>.</para></summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <param name="sid"> The user's specified <b>Status ID</b> being checked.</param>
    /// <param name="duration"> The <b>Total Effect Duration</b> of specified <b>Status ID</b> being checked.</param>
    protected bool TargetHasEffect<SID>(Actor? target, SID sid) where SID : Enum => target?.FindStatus(sid) != null;
    #endregion

    #region Targeting

    #region Position Checks

    #region Core
    /// <summary>Checks precise positioning between <b>player target</b> and any other targets.</summary>
    protected delegate bool PositionCheck(Actor playerTarget, Actor targetToTest);

    /// <summary>Calculates the <b>priority</b> of a target based on the <b>total number of targets</b> and the <b>primary target</b> itself.<br/>It is generic, so it can return different types based on the implementation.</summary>
    protected delegate P PriorityFunc<P>(int totalTargets, Actor primaryTarget);
    #endregion

    #region Splash
    /// <summary>Position checker for determining the best target for an ability that deals <b>Splash</b> damage.</summary>
    protected PositionCheck IsSplashTarget => (primary, other) => Hints.TargetInAOECircle(other, primary.Position, 5);
    #endregion

    #region Cones
    //some use-cases for these are mainly for BLU modules, since the ranges for their abilities are all over the place. (e.g. 4y & 16y specifically)

    /// <summary>Creates a <b>Position Check</b> for <b>Cone AOE</b> attacks with the given range.</summary>
    private PositionCheck ConeTargetCheck(float range) => (primary, other) =>
    {
        var playerDir = Player.DirectionTo(primary);
        return Hints.TargetInAOECone(other, Player.Position, range, playerDir, 45.Degrees());
    };

    /// <summary>Checks if the target is within a <b>4-yalm Cone</b> range.</summary>
    protected PositionCheck Is4yConeTarget => ConeTargetCheck(4);

    /// <summary>Checks if the target is within a <b>6-yalm Cone</b> range.</summary>
    protected PositionCheck Is6yConeTarget => ConeTargetCheck(6);

    /// <summary>Checks if the target is within an <b>8-yalm Cone</b> range.</summary>
    protected PositionCheck Is8yConeTarget => ConeTargetCheck(8);

    /// <summary>Checks if the target is within a <b>10-yalm Cone</b> range.</summary>
    protected PositionCheck Is10yConeTarget => ConeTargetCheck(10);

    /// <summary>Checks if the target is within a <b>12-yalm Cone</b> range.</summary>
    protected PositionCheck Is12yConeTarget => ConeTargetCheck(12);

    /// <summary>Checks if the target is within a <b>15-yalm Cone</b> range.</summary>
    protected PositionCheck Is15yConeTarget => ConeTargetCheck(15);

    /// <summary>Checks if the target is within a <b>16-yalm Cone</b> range.</summary>
    protected PositionCheck Is16yConeTarget => ConeTargetCheck(16);
    #endregion

    #region Lines (AOE Rectangles)
    /// <summary>Creates a <b>Position Check</b> for <b>Line AOE (AOE Rectangle)</b> attacks with the given range.</summary>
    private PositionCheck LineTargetCheck(float range) => (primary, other) =>
    {
        var playerDir = Player.DirectionTo(primary); // Cache calculation
        return Hints.TargetInAOERect(other, Player.Position, playerDir, range, 2);
    };

    /// <summary>Checks if the target is within a <b>10-yalm AOE Rect</b> range.</summary>
    protected PositionCheck Is10yRectTarget => LineTargetCheck(10);

    /// <summary>Checks if the target is within a <b>15-yalm AOE Rect</b> range.</summary>
    protected PositionCheck Is15yRectTarget => LineTargetCheck(15);

    /// <summary>Checks if the target is within a <b>20-yalm AOE Rect</b> range.</summary>
    protected PositionCheck Is20yRectTarget => LineTargetCheck(20);

    /// <summary>Checks if the target is within a <b>25-yalm AOE Rect</b> range.</summary>
    protected PositionCheck Is25yRectTarget => LineTargetCheck(25);
    #endregion

    #endregion

    #region Range Checks
    /// <summary>Checks if target is within the specified distance in <b>yalms</b>.</summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <param name="maxDistance">The maximum distance threshold.</param>
    protected bool InRange(Actor? target, float maxDistance) => Player.DistanceToHitbox(target) <= maxDistance - 0.01f;

    /// <summary>Checks if target is within the Melee-range distance in <b>yalms</b>.</summary>
    protected bool InMeleeRange(Actor? target) => InRange(target, 3.5f);

    /// <summary>Checks if the target is within <b>0-yalm</b> range.</summary>
    protected bool In0y(Actor? target) => InRange(target, 0.01f);

    /// <summary>Checks if the target is within <b>3-yalm</b> range.</summary>
    protected bool In3y(Actor? target) => InRange(target, 3.00f);

    /// <summary>Checks if the target is within <b>5-yalm</b> range.</summary>
    protected bool In5y(Actor? target) => InRange(target, 5.00f);

    /// <summary>Checks if the target is within <b>10-yalm</b> range.</summary>
    protected bool In10y(Actor? target) => InRange(target, 10.00f);

    /// <summary>Checks if the target is within <b>12-yalm</b> range.</summary>
    protected bool In12y(Actor? target) => InRange(target, 12.00f);

    /// <summary>Checks if the target is within <b>15-yalm</b> range.</summary>
    protected bool In15y(Actor? target) => InRange(target, 15.00f);

    /// <summary>Checks if the target is within <b>20-yalm</b> range.</summary>
    protected bool In20y(Actor? target) => InRange(target, 20.00f);

    /// <summary>Checks if the target is within <b>25-yalm</b> range.</summary>
    protected bool In25y(Actor? target) => InRange(target, 25.00f);
    #endregion

    #region Smart-Targeting
    /// <summary>A simpler smart-targeting helper for picking a <b>specific</b> target over your current target.<br/>Very useful for intricate planning of ability targeting in specific situations.</summary>
    /// <param name="track">The user's selected strategy's option <b>Track</b>, retrieved from module's enums and definitions. (e.g. <b>strategy.Option(Track.NoMercy)</b>)</param>
    /// <returns></returns>
    protected Actor? TargetChoice(StrategyValues.OptionRef track) => ResolveTargetOverride(track.Value);

    /// <summary>Attempts to <b>select</b> a suitable <b>primary PvE target</b> automatically.<para/>
    /// <b>NOTE</b>: This function is solely used for <b>auto-targeting enemies</b> without having a target selected or for <b>AI</b> usage. Please use appropriately.</summary>
    /// <param name="strategy">The user's selected <b>strategy</b>.</param>
    /// <param name="primaryTarget">The user's current <b>target</b>.</param>
    /// <param name="range">The <b>max range</b> to consider a new target.</param>
    protected void GetNextTarget(StrategyValues strategy, ref Enemy? primaryTarget, float range)
    {
        if (primaryTarget?.Actor == null || Player.DistanceToHitbox(primaryTarget.Actor) > range)
        {
            var AOEStrat = strategy.Option(SharedTrack.AOE).As<AOEStrategy>();
            if (AOEStrat is AOEStrategy.AutoFinish or AOEStrategy.AutoBreak)
            {
                var newTarget = Hints.PriorityTargets.FirstOrDefault(x => Player.DistanceToHitbox(x.Actor) <= range);
                if (newTarget != null)
                    primaryTarget = newTarget;
            }
        }
    }

    /// <summary>Attempts to <b>select</b> the most suitable <b>PvP target</b> automatically, prioritizing the target with the <b>lowest HP percentage</b> within range.<para/>
    /// <b>NOTE</b>: This function is solely used for finding the best <b>PvP target</b> without having to manually scan and click on other targets. Please use appropriately.</summary>
    /// <param name="primaryTarget">The user's current <b>target</b>.</param>
    /// <param name="range">The <b>max range</b> to consider a new target.</param>
    protected void GetPvPTarget(float range)
    {
        var bestTarget = Hints.PriorityTargets
            .Where(x => Player.DistanceToHitbox(x.Actor) <= range && x.Actor.FindStatus(ClassShared.SID.Guard) == null)
            .OrderBy(x => (float)x.Actor.HPMP.CurHP / x.Actor.HPMP.MaxHP)
            .FirstOrDefault();
        Hints.ForcedTarget = bestTarget?.Actor;
    }

    /// <summary>Targeting function for indicating when <b>AOE Circle</b> abilities should be used based on nearby targets.</summary>
    /// <param name="range">The radius of the <b>AOE Circle</b> ability from the Player.</param>
    /// <returns>
    /// A tuple with boolean values indicating whether the <b>AOE Circle</b> should be used based on the <b>number of targets nearby</b>:<br/>
    /// <b>- OnAny</b>: At least 1 target is inside.<br/>
    /// <b>- OnTwoOrMore</b>: At least 2 targets are inside.<br/>
    /// <b>- OnThreeOrMore</b>: At least 3 targets are inside.<br/>
    /// <b>- OnFourOrMore</b>: At least 4 targets are inside.<br/>
    /// <b>- OnFiveOrMore</b>: At least 5 targets are inside.
    /// </returns>
    protected (bool OnAny, bool OnTwoOrMore, bool OnThreeOrMore, bool OnFourOrMore, bool OnFiveOrMore) ShouldUseAOECircle(float range)
    {
        var numTargets = Hints.NumPriorityTargetsInAOECircle(Player.Position, range);
        return (numTargets > 0, numTargets > 1, numTargets > 2, numTargets > 3, numTargets > 4);
    }

    /// <summary>This function attempts to pick the best target automatically.</summary>
    /// <param name="primaryTarget">The user's current <b>selected Target</b>.</param>
    /// <param name="range">The <b>range</b> within which to evaluate potential targets.</param>
    /// <param name="isInAOE">A flag indicating if the target is within the <b>Area of Effect</b> (AOE).</param>
    protected (Enemy? Best, int Targets) GetBestTarget(Enemy? primaryTarget, float range, PositionCheck isInAOE)
        => GetTarget(primaryTarget, range, isInAOE, (numTargets, _) => numTargets, a => a);

    /// <summary>This function picks the target based on HP, modified by how many targets are in the AOE.</summary>
    /// <param name="primaryTarget">The user's current <b>selected Target</b>.</param>
    /// <param name="range">The <b>range</b> within which to evaluate potential targets.</param>
    /// <param name="isInAOE">A flag indicating if the target is within the <b>Area of Effect</b> (AOE).</param>
    protected (Enemy? Best, int Targets) GetBestHPTarget(Enemy? primaryTarget, float range, PositionCheck isInAOE)
        => GetTarget(primaryTarget, range, isInAOE, (numTargets, enemy) => (numTargets, numTargets > 2 ? enemy.HPMP.CurHP : 0), args => args.numTargets);

    /// <summary>Main function for picking a target, generalized for any prioritization and simplification logic.</summary>
    /// <typeparam name="P"></typeparam>
    /// <param name="primaryTarget">The user's current <b>selected Target</b>.</param>
    /// <param name="range">The <b>range</b> within which to evaluate potential targets.</param>
    /// <param name="isInAOE">A flag indicating if the target is within the <b>Area of Effect</b> (AOE).</param>
    /// <param name="prioritize">A flag indicating whether <b>prioritization</b> of certain targets should occur.</param>
    /// <param name="simplify">A flag indicating whether the <b>simplification</b> of target selection should apply.</param>
    protected (Enemy? Best, int Priority) GetTarget<P>(Enemy? primaryTarget, float range, PositionCheck isInAOE, PriorityFunc<P> prioritize, Func<P, int> simplify) where P : struct, IComparable
    {
        P targetPrio(Actor potentialTarget)
        {
            var numTargets = Hints.NumPriorityTargetsInAOE(enemy => isInAOE(potentialTarget, enemy.Actor));
            return prioritize(numTargets, potentialTarget); // Prioritize based on number of targets in AOE and the potential target
        }

        var (newtarget, newprio) = FindBetterTargetBy(primaryTarget?.Actor, range, targetPrio);
        var newnewprio = simplify(newprio);
        return (newnewprio > 0 ? Hints.FindEnemy(newtarget) : null, newnewprio);
    }

    /// <summary>Identify an appropriate target for applying <b>DoT</b> effect. This has no impact if any <b>auto-targeting</b> is disabled.</summary>
    /// <typeparam name="P">The type representing the timer value, which must be a struct and implement <see cref="IComparable"/>.</typeparam>
    /// <param name="initial">The <b>initial</b> target to consider for applying the <b>DoT</b> effect.</param>
    /// <param name="getTimer">A function that retrieves the <b>timer</b> value associated with a given <b>actor</b>.</param>
    /// <param name="maxAllowedTargets">The maximum number of valid targets to evaluate.</param>
    protected (Enemy? Best, P Timer) GetDOTTarget<P>(Enemy? initial, Func<Actor?, P> getTimer, int maxAllowedTargets, float range = 30) where P : struct, IComparable
    {
        if (initial == null || maxAllowedTargets <= 0 || Player.DistanceToHitbox(initial.Actor) > range)
        {
            return (null, getTimer(null));
        }

        var newTarget = initial;
        var initialTimer = getTimer(initial?.Actor);
        var newTimer = initialTimer;
        var numTargets = 0;

        foreach (var dotTarget in Hints.PriorityTargets)
        {
            if (dotTarget.ForbidDOTs || Player.DistanceToHitbox(dotTarget.Actor) > range)
                continue;

            if (++numTargets > maxAllowedTargets)
                return (newTarget, newTimer);

            var thisTimer = getTimer(dotTarget.Actor);

            if (thisTimer.CompareTo(newTimer) < 0)
            {
                newTarget = dotTarget;
                newTimer = thisTimer;
            }
        }

        return (newTarget, newTimer);
    }

    /// <summary>A simpler smart-targeting helper for picking a <b>specific</b> target over your current target.<br/>This function specifically is for <b>Single-Target</b> actions.</summary>
    /// <param name="manual">Player's current target</param>
    /// <param name="track">The ability's specified track</param>
    protected Actor? SingleTargetChoice(Actor? manual, StrategyValues.OptionRef track) => TargetChoice(track) ?? manual;

    /// <summary>A simpler smart-targeting helper for picking a <b>specific</b> target over your current target.<br/>This function specifically is for <b>AOE</b> actions.</summary>
    /// <param name="manual">Player's current target</param>
    /// <param name="auto">The best target to be auto-selected</param>
    /// <param name="track">The ability's specified track</param>
    protected Actor? AOETargetChoice(Actor? manual, Actor? auto, StrategyValues.OptionRef track, StrategyValues strategy) => TargetChoice(track) ?? (strategy.Targeting() == SoftTargetStrategy.Automatic ? auto : manual);
    #endregion

    #endregion

    #region Positionals
    protected bool NextPositionalImminent;
    protected bool NextPositionalCorrect;

    /// <summary>Retrieves the current positional of the target based on target's position and rotation.</summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    protected Positional GetCurrentPositional(Actor target) => (Player.Position - target.Position).Normalized().Dot(target.Rotation.ToDirection()) switch
    {
        < -0.7071068f => Positional.Rear,
        < 0.7071068f => Positional.Flank,
        _ => Positional.Front
    };

    /// <summary>Checks if player is on specified target's <b>Rear Positional</b>.</summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    protected bool IsOnRear(Actor target) => In3y(target) && GetCurrentPositional(target) == Positional.Rear;

    /// <summary>Checks if player is on specified target's <b>Flank Positional</b>.</summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    protected bool IsOnFlank(Actor target) => In3y(target) && GetCurrentPositional(target) == Positional.Flank;

    /// <summary>Updates the positional recommendations based on the current target and the positional requirement.</summary>
    /// <param name="enemy">The user's current enemy scanned for user's current positional.</param>
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

    #endregion

    #region AI
    /// <summary>Establishes a goal-zone for a single target within a specified range.<br/>
    /// Primarily utilized by <b>caster-DPS</b> jobs that lack a dedicated maximize-AOE function.</summary>
    /// <param name="range">The range within which the goal zone is applied.</param>
    protected void GoalZoneSingle(float range)
    {
        if (PlayerTarget != null)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(PlayerTarget.Actor, range));
    }

    /// <summary>Defines a goal-zone using a combined strategy, factoring in AOE considerations.</summary>
    /// <param name="strategy">The strategy that influences the goal zone logic.</param>
    /// <param name="range">The base range for the goal zone.</param>
    /// <param name="fAoe">A function determining the area of effect.</param>
    /// <param name="firstUnlockedAoeAction">The first available AOE action.</param>
    /// <param name="minAoe">The minimum number of targets required to trigger AOE.</param>
    /// <param name="maximumActionRange">An optional parameter specifying the maximum action range.</param>
    protected void GoalZoneCombined(StrategyValues strategy, float range, Func<WPos, float> fAoe, AID firstUnlockedAoeAction, int minAoe, float? maximumActionRange = null)
    {
        var (_, positional, imminent, _) = Hints.RecommendedPositional;

        if (!strategy.ForceAOE() && !Unlocked(firstUnlockedAoeAction))
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

    #region Misc
    /// <summary>Estimates the delay caused by <b>animation lock</b>.</summary>
    protected float AnimationLockDelay { get; private set; }

    /// <summary>Estimates the time remaining until the <b>next Down-time phase</b>.</summary>
    protected float DowntimeIn { get; private set; }

    /// <summary>Estimates the time remaining until the <b>next Up-time phase</b>.</summary>
    protected float? UptimeIn { get; private set; }

    /// <summary>Elapsed time in <b>seconds</b> since the start of combat.</summary>
    protected float CombatTimer { get; private set; }

    /// <summary>Time remaining on pre-pull (or any) <b>Countdown Timer</b>.</summary>
    protected float? CountdownRemaining { get; private set; }

    /// <summary>Time remaining on current <b>Combo timer</b>.</summary>
    protected float? ComboTimer { get; private set; }

    /// <summary>Checks if player is currently <b>moving</b>.</summary>
    protected bool IsMoving { get; private set; }

    /// <summary>Player's <b>actual</b> target; guaranteed to be an enemy.</summary>
    protected Enemy? PlayerTarget { get; private set; }

    /// <summary>Checks if player is inside combat and has a primary target.</summary>
    protected bool InsideCombatWith(Actor? target) => Player.InCombat && target != null;

    /// <summary>Checks if party-wide <b>Raid Buffs </b> are imminent.</summary>
    protected float RaidBuffsIn { get; private set; }

    /// <summary>Checks time remaining on party-wide <b>Raid Buffs</b>.</summary>
    protected float RaidBuffsLeft { get; private set; }

    /// <summary>A function for helping check estimations on Raid Buffs.</summary>
    private new (float Left, float In) EstimateRaidBuffTimings(Actor? target)
    {
        if (Bossmods.ActiveModule?.Info?.GroupType is BossModuleInfo.GroupType.BozjaDuel && IsSelfish(Player.Class))
            return (float.MaxValue, 0);

        //dummy shit
        if (target?.IsStrikingDummy == true)
        {
            var cycle = CombatTimer - 7.8f;
            if (cycle < 0)
                return (0, 7.8f - CombatTimer);

            cycle %= 120;
            return cycle < 20 ? (20 - cycle, 0) : (0, 120 - cycle);
        }

        var buffsIn = Bossmods.RaidCooldowns.NextDamageBuffIn2();
        buffsIn ??= CombatTimer < 7.8f && World.Party.WithoutSlot(includeDead: true, excludeAlliance: true, excludeNPCs: true).Skip(1).Any(PartyBuffCheck) ? 7.8f - CombatTimer : float.MaxValue;
        return (Bossmods.RaidCooldowns.DamageBuffLeft(Player, target), buffsIn.Value);
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

    /// <summary>Simplified check for if a specified OGCD is ready and if the strategy allows for it.</summary>
    protected bool ShouldUseGCD(bool ready, GCDStrategy strategy, Actor? target, bool optimal = false) => ready && strategy switch
    {
        GCDStrategy.Automatic => target != null && optimal,
        GCDStrategy.RaidBuffsOnly => RaidBuffsLeft > 0f,
        GCDStrategy.Force => true,
        _ => false
    };

    /// <summary>Simplified check for if a specified OGCD is ready and if the strategy allows for it.</summary>
    protected bool ShouldUseOGCD(OGCDStrategy strategy, Actor? target, bool ready, bool optimal = true)
    {
        return ready && strategy switch
        {
            OGCDStrategy.Automatic => target != null && optimal,
            OGCDStrategy.RaidBuffsOnly => RaidBuffsLeft > 0f,
            OGCDStrategy.Force => true,
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            _ => false
        };
    }
    #endregion

    #region Shared Abilities
    /// <summary>Checks if player is able to execute the melee-DPS shared ability: <b>True North</b></summary>
    protected bool CanTrueNorth { get; private set; }

    /// <summary>Checks if player is under the effect of the melee-DPS shared ability: <b>True North</b></summary>
    protected bool HasTrueNorth { get; private set; }

    /// <summary>Checks if player is able to execute the caster-DPS shared ability: <b>Swiftcast</b></summary>
    protected bool CanSwiftcast { get; private set; }

    /// <summary>Checks if player is under the effect of the caster-DPS shared ability: <b>Swiftcast</b></summary>
    protected bool HasSwiftcast { get; private set; }

    /// <summary>Checks if player is able to execute the ranged-DPS shared ability: <b>Peloton</b></summary>
    protected bool CanPeloton { get; private set; }

    /// <summary>Checks if player is under the effect of the ranged-DPS shared ability: <b>Peloton</b></summary>
    protected bool HasPeloton { get; private set; }
    #endregion

    #region Priorities
    protected GCDPriority GCDPrio(GCDStrategy strat, GCDPriority defaultPrio) => strat is GCDStrategy.Force ? GCDPriority.Forced : defaultPrio;
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
    protected OGCDPriority OGCDPrio(OGCDStrategy strat, OGCDPriority defaultPrio) => strat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave ? OGCDPriority.Forced : defaultPrio;
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
        Forced = 1500 //makes priority higher than CDPlanner's automatic prio + 500, which is really only Medium prio
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
        Shield = Player.HPMP.Shield;
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
        HasPeloton = HasEffect(ClassShared.SID.Peloton);
        CanPeloton = !Player.InCombat && !HasPeloton && ActionUnlocked(ActionID.MakeSpell(ClassShared.AID.Peloton)) && World.Client.Cooldowns[ActionDefinitions.Instance.Spell(ClassShared.AID.Peloton)!.MainCooldownGroup].Remaining < 0.6f;
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

    /// <summary>The core function responsible for orchestrating the execution of all abilities and strategies.<br/></summary>
    /// <param name="strategy">The user's specified <b>Strategy</b>.</param>
    /// <param name="primaryTarget">The user's specified <b>Enemy</b>.</param>
    public abstract void Execution(StrategyValues strategy, Enemy? primaryTarget);
}

static class ModuleExtensions
{
    #region Shared Definitions
    /// <summary>Defines our shared <b>AOE</b> (rotation) strategies.</summary>
    /// <param name="res">The definitions of our base module's strategies.</param>
    public static RotationModuleDefinition.ConfigRef<AOEStrategy> DefineAOE(this RotationModuleDefinition res)
    {
        return res.Define(SharedTrack.AOE).As<AOEStrategy>("AOE", uiPriority: 300)
            .AddOption(AOEStrategy.AutoFinish, "Auto (Finish combo)", "Automatically execute optimal rotation based on targets; finishes combo if possible", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.AutoBreak, "Auto (Break combo)", "Automatically execute optimal rotation based on targets; breaks combo if necessary", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceST, "Force ST", "Force Single-Target rotation execution", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceAOE, "Force AOE", "Force AOE rotation execution", supportedTargets: ActionTargets.Hostile | ActionTargets.Self);
    }

    /// <summary>Defines our shared <b>soft-Targeting</b> strategies.</summary>
    public static RotationModuleDefinition.ConfigRef<SoftTargetStrategy> DefineTargeting(this RotationModuleDefinition res)
    {
        return res.Define(SharedTrack.Targeting).As<SoftTargetStrategy>("SoftTarget", uiPriority: 295)
            .AddOption(SoftTargetStrategy.Automatic, "Automatic", "Auto-select best target for maximum optimal DPS output")
            .AddOption(SoftTargetStrategy.Manual, "Manual", "Do not auto-select best target, instead executing only on whichever target is currently selected");
    }

    /// <summary>Defines our shared <b>Hold</b> strategies.</summary>
    /// <param name="res">The definitions of our base module's strategies.</param>
    public static RotationModuleDefinition.ConfigRef<HoldStrategy> DefineHold(this RotationModuleDefinition res)
    {
        return res.Define(SharedTrack.Hold).As<HoldStrategy>("Hold", uiPriority: 290)
            .AddOption(HoldStrategy.DontHold, "Dont Hold", "Allow use of all cooldowns, buffs, or gauge abilities")
            .AddOption(HoldStrategy.HoldCooldowns, "Hold", "Forbid use of all cooldowns only")
            .AddOption(HoldStrategy.HoldGauge, "Hold Gauge", "Forbid use of all gauge abilities only")
            .AddOption(HoldStrategy.HoldBuffs, "Hold Buffs", "Forbid use of all raidbuffs or buff-related abilities only")
            .AddOption(HoldStrategy.HoldAbilities, "Hold Abilities", "Forbid use of all cooldowns, buffs, and gauge abilities")
            .AddOption(HoldStrategy.HoldEverything, "Hold Everything", "Forbid complete use of ALL actions; rotations included");
    }

    /// <summary>Defines our shared <b>Potion</b> strategies.</summary>
    /// <param name="res">The definitions of our base module's strategies.</param>
    public static RotationModuleDefinition.ConfigRef<PotionStrategy> DefinePotion(this RotationModuleDefinition res, ActionID pot)
    {
        return res.Define(SharedTrack.Potion).As<PotionStrategy>("Potion", uiPriority: 280)
            .AddOption(PotionStrategy.Manual, "Manual", "Use potion manually")
            .AddOption(PotionStrategy.AlignWithBuffs, "With Buffs", "Use potion when personal buffs are imminent or active")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "With RaidBuffs", "Use potion when party raid buffs are imminent or active")
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use potion immediately without restriction", 270, 30)
            .AddAssociatedAction(pot);
    }

    /// <summary>A quick and easy helper for shortcutting how we define our <b>GCD</b> abilities.</summary>
    /// <param name="track">The <b>Track</b> for the ability that the user is specifying; ability tracked <b>must</b> be inside the module's <b>Track</b> enum for target selection.</param>
    /// <param name="internalName">The <b>Internal Name</b> for the ability that the user is specifying; we usually want to put the full name of the ability here, as this will show up as the main name representing this option. (e.g. "No Mercy")</param>
    /// <param name="displayName">The <b>Display Name</b> for the ability that the user is specifying; we usually want to put some sort of abbreviation here, as this will show up as the secondary name representing this option. (e.g. "NM" or "N.Mercy")</param>
    /// <param name="uiPriority">The priority for specified ability inside the UI. (e.g. Higher the number = More to the left (in CDPlanner) or top (in Autorotation) menus)</param>
    /// <param name="cooldown"><para>The <b>Cooldown</b> for the ability that the user is specifying; 0 if none.</para><para><c><b>NOTE</b></c>: For charge abilities, this will check for its Charge CD, not its Total CD.</para></param>
    /// <param name="effectDuration">The <b>Effect Duration</b> for the ability that the user is specifying; 0 if none.</param>
    /// <param name="supportedTargets">The <b>Targets Supported</b> for the ability that the user is specifying.</param>
    /// <param name="minLevel">The <b>Minimum Level</b> required for the ability that the user is specifying.</param>
    /// <param name="maxLevel">The <b>Maximum Level</b> required for the ability that the user is specifying.</param>
    public static RotationModuleDefinition.ConfigRef<GCDStrategy> DefineGCD<Index, AID>(this RotationModuleDefinition res, Index track, AID aid, string internalName, string displayName = "", int uiPriority = 100, float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100)
        where Index : Enum
        where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        return res.Define(track).As<GCDStrategy>(internalName, displayName: displayName, uiPriority: uiPriority)
            .AddOption(GCDStrategy.Automatic, "Auto", $"Automatically use {action.Name()} when optimal", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(GCDStrategy.RaidBuffsOnly, "With Buffs", $"Use {action.Name()} when raid buffs are active", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(GCDStrategy.Force, "Force", $"Force use {action.Name()} ASAP", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
            .AddOption(GCDStrategy.Delay, "Delay", $"Do NOT use {action.Name()}", 0, 0, ActionTargets.None, minLevel, maxLevel)
            .AddAssociatedActions(aid);
    }

    /// <summary>A quick and easy helper for shortcutting how we define our <b>OGCD</b> abilities.</summary>
    /// <param name="track">The <b>Track</b> for the ability that the user is specifying; ability tracked <b>must</b> be inside the module's <b>Track</b> enum for target selection.</param>
    /// <param name="internalName">The <b>Internal Name</b> for the ability that the user is specifying; we usually want to put the full name of the ability here, as this will show up as the main name representing this option. (e.g. "No Mercy")</param>
    /// <param name="displayName">The <b>Display Name</b> for the ability that the user is specifying; we usually want to put some sort of abbreviation here, as this will show up as the secondary name representing this option. (e.g. "NM" or "N.Mercy")</param>
    /// <param name="uiPriority">The priority for specified ability inside the UI. (e.g. Higher the number = More to the left (in CDPlanner) or top (in Autorotation) menus)</param>
    /// <param name="cooldown"><para>The <b>Cooldown</b> for the ability that the user is specifying; 0 if none.</para><para><c><b>NOTE</b></c>: For charge abilities, this will check for its Charge CD, not its Total CD.</para></param>
    /// <param name="effectDuration">The <b>Effect Duration</b> for the ability that the user is specifying; 0 if none.</param>
    /// <param name="supportedTargets">The <b>Targets Supported</b> for the ability that the user is specifying.</param>
    /// <param name="minLevel">The <b>Minimum Level</b> required for the ability that the user is specifying.</param>
    /// <param name="maxLevel">The <b>Maximum Level</b> required for the ability that the user is specifying.</param>
    public static RotationModuleDefinition.ConfigRef<OGCDStrategy> DefineOGCD<Index, AID>(this RotationModuleDefinition res, Index track, AID aid, string internalName, string displayName = "", int uiPriority = 100, float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100)
        where Index : Enum
        where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        return res.Define(track).As<OGCDStrategy>(internalName, displayName: displayName, uiPriority: uiPriority)
            .AddOption(OGCDStrategy.Automatic, "Auto", $"Automatically use {action.Name()} when optimal", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.RaidBuffsOnly, "With Buffs", $"Use {action.Name()} when raid buffs are active", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.Force, "Force", $"Force use {action.Name()} ASAP", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.AnyWeave, "AnyWeave", $"Force use {action.Name()} in next possible weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.EarlyWeave, "EarlyWeave", $"Force use {action.Name()} in next possible early-weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.LateWeave, "LateWeave", $"Force use {action.Name()} in next possible late-weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.Delay, "Delay", $"Do NOT use {action.Name()}", 0, 0, ActionTargets.None, minLevel: minLevel, maxLevel)
            .AddAssociatedActions(aid);
    }

    public static RotationModuleDefinition.ConfigRef<AllowOrForbid> DefineAllow<Index, AID>(this RotationModuleDefinition res, Index track, AID aid, string internalName, string displayName = "", int uiPriority = 100, float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100)
        where Index : Enum
        where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        return res.Define(track).As<AllowOrForbid>(internalName, displayName: displayName, uiPriority: uiPriority)
            .AddOption(AllowOrForbid.Allow, "Allow", $"Allow use of {action.Name()} when available", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(AllowOrForbid.Force, "Force", $"Force use {action.Name()} ASAP", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(AllowOrForbid.Forbid, "Forbid", $"Forbid use of {action.Name()} entirely", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddAssociatedActions(aid);
    }
    #endregion

    #region Global Helpers

    #region ST/AOE
    /// <summary>A global helper for easily retrieving the user's <b>Rotation</b> strategy. See <seealso cref="AOEStrategy"/> for more details.</summary>
    public static AOEStrategy Rotation(this StrategyValues strategy) => strategy.Option(SharedTrack.AOE).As<AOEStrategy>();

    /// <summary>A global helper for automatically executing the best optimal rotation; finishes combo if possible. See <seealso cref="AOEStrategy"/> for more details.</summary>
    public static bool AutoFinish(this StrategyValues strategy) => strategy.Rotation() == AOEStrategy.AutoFinish;

    /// <summary>A global helper for automatically executing the best optimal rotation; breaks combo if necessary. See <seealso cref="AOEStrategy"/> for more details.</summary>
    public static bool AutoBreak(this StrategyValues strategy) => strategy.Rotation() == AOEStrategy.AutoBreak;

    /// <summary>A global helper for force-executing the single-target rotation. See <seealso cref="AOEStrategy"/> for more details.</summary>
    public static bool ForceST(this StrategyValues strategy) => strategy.Rotation() == AOEStrategy.ForceST;

    /// <summary>A global helper for force-executing the AOE rotation. See <seealso cref="AOEStrategy"/> for more details.</summary>
    public static bool ForceAOE(this StrategyValues strategy) => strategy.Rotation() == AOEStrategy.ForceAOE;
    #endregion

    #region Targeting
    /// <summary>A global helper for easily retrieving the user's <b>Targeting</b> strategy. See <seealso cref="SoftTargetStrategy"/> for more details.</summary>
    public static SoftTargetStrategy Targeting(this StrategyValues strategy) => strategy.Option(SharedTrack.Targeting).As<SoftTargetStrategy>();

    /// <summary>A global helper for automatically selecting the best target. See <seealso cref="SoftTargetStrategy"/> for more details.</summary>
    public static bool AutoTarget(this StrategyValues strategy) => strategy.Targeting() == SoftTargetStrategy.Automatic;

    /// <summary>A global helper for manually selecting the best target. See <seealso cref="SoftTargetStrategy"/> for more details.</summary>
    public static bool ManualTarget(this StrategyValues strategy) => strategy.Targeting() == SoftTargetStrategy.Manual;
    #endregion

    #region Hold
    public static HoldStrategy Hold(this StrategyValues strategy) => strategy.Option(SharedTrack.Hold).As<HoldStrategy>();

    /// <summary>A global helper for forbidding ALL actions, rotations <em>and</em> abilities. See <seealso cref="HoldStrategy"/> for more details.</summary>
    public static bool HoldEverything(this StrategyValues strategy) => strategy.Hold() == HoldStrategy.HoldEverything;

    /// <summary>A global helper for forbidding ALL available abilities that are buff, gauge, or cooldown related. See <seealso cref="HoldStrategy"/> for more details.</summary>
    public static bool HoldAbilities(this StrategyValues strategy) => strategy.Hold() == HoldStrategy.HoldAbilities;

    /// <summary>A global helper for forbidding ALL available abilities that are related to raidbuffs. See <seealso cref="HoldStrategy"/> for more details.</summary>
    public static bool HoldBuffs(this StrategyValues strategy) => strategy.Hold() == HoldStrategy.HoldBuffs;

    /// <summary>A global helper for forbidding ALL available abilities that have any sort of cooldown attached to it. See <seealso cref="HoldStrategy"/> for more details.</summary>
    public static bool HoldCDs(this StrategyValues strategy) => strategy.Hold() == HoldStrategy.HoldCooldowns;

    /// <summary>A global helper for forbidding ALL available abilities that are related to the job's gauge. See <seealso cref="HoldStrategy"/> for more details.</summary>
    public static bool HoldGauge(this StrategyValues strategy) => strategy.Hold() == HoldStrategy.HoldGauge;
    #endregion

    #region Potion
    public static PotionStrategy Potion(this StrategyValues strategy) => strategy.Option(SharedTrack.Potion).As<PotionStrategy>();
    public static bool AlignPotionWithBuffs(this StrategyValues strategy) => strategy.Potion() == PotionStrategy.AlignWithBuffs;
    public static bool AlignPotionWithRaidBuffs(this StrategyValues strategy) => strategy.Potion() == PotionStrategy.AlignWithRaidBuffs;
    public static bool UsePotionImmediately(this StrategyValues strategy) => strategy.Potion() == PotionStrategy.Immediate;
    public static bool UsePotionManually(this StrategyValues strategy) => strategy.Potion() == PotionStrategy.Manual;
    #endregion

    #endregion
}
