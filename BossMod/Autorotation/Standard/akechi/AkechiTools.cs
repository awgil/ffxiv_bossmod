using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

#region Shared Enums: Strategies
/// <summary>
/// The <b>SharedTrack</b> enum used for <b>AOE</b> and <b>Hold</b> strategies, typically for modules featuring damage rotations.
/// <br>This enum defines tracks that can be used for all PvE classes and jobs, such as strategies containing executing standard rotations or explicitly holding abilities.</br>
/// <para><b>Example Given:</b>
/// <br>- <c>public enum Track { NoMercy = SharedTrack.Count }</c></br></para>
/// <para><b>Explanation:</b>
/// <br>- <c><b> Track</b></c> is the enum for tracking specific abilities on user's specific rotation module.</br>
/// <br>- <c><b> NoMercy</b></c> is the example enum of a specific ability being tracked on user's specific rotation module.</br>
/// <br>- <c><b> SharedTrack.Count</b></c> is the shared track being used for executing our shared strategies listed above, called using <seealso cref="AkechiTools{AID, TraitID}"/>.</br></para>
/// </summary>
/// <returns>- All strategies listed under <seealso cref="SharedTrack"/> into user's rotation module</returns>
public enum SharedTrack
{
    /// <summary>
    /// The main strategy for tracking <b>single-target</b> and <b>AOE</b> rotations.
    /// </summary>
    /// <returns>- All strategies listed under <seealso cref="AOEStrategy"/> into user's rotation module</returns>
    AOE,

    /// <summary>
    /// The main strategy used for tracking when to <b>hold any buffs, gauge, or cooldown abilties</b> for optimal usage.
    /// </summary>
    /// <returns>- All strategies listed under <seealso cref="HoldStrategy"/> into user's rotation module</returns>
    Hold,

    /// <summary>
    /// Represents the total count of strategies available inside this specific track. We generally never actually use this as a strategy since there isn't any logic really linked to this besides the counting.
    /// </summary>
    Count
}

/// <summary>
/// The <b>Default Strategy</b> enum used for tracking <b>single-target</b> and <b>AOE</b> strategies.<para/>
/// <b>Example Given:</b><br/>
/// - <c>strategy.Option(SharedTrack.AOE).As&lt;<seealso cref="AOEStrategy"/>&gt;();</c><para/>
/// <b>Explanation:</b><br/>
/// - "<c><b>strategy</b></c>" is the parameter for tracking a specific strategy for a specific ability in the user's rotation module.<br/>
/// - "<c><b>Option</b></c>" are the relative options for user's specific strategy.<br/>
/// - "<c><b>(SharedTrack.AOE)</b></c>" is the enum representing all options relating to this custom strategy being tracked in the user's rotation module.<br/>
/// - "<c><b>.As&lt;<seealso cref="AOEStrategy"/>&gt;();</b></c>" is the relative strategy for user's specific ability being tracked in the user's rotation module.
/// </summary>
/// <returns>- All strategies listed under <seealso cref="AOEStrategy"/> into user's rotation module</returns>
public enum AOEStrategy
{
    /// <summary>
    /// The default strategy used for <b>automatically executing</b> the rotation.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.Automatic(StrategyValues),"/>, or also as `strategy.Automatic()` in some cases.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(SharedTrack.AOE).As&lt;<seealso cref="AOEStrategy"/>&gt;() == <seealso cref="Automatic"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(SharedTrack.AOE).As&lt;<seealso cref="AOEStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="AOEStrategy"/>.<br/>
    /// - "<c><b><seealso cref="Automatic"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The most optimal rotation <b>automatically executed</b>.</returns>
    Automatic,

    /// <summary>
    /// The main strategy used for <b>force-executing</b> the <b>single-target</b> rotation.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.ForceST(StrategyValues),"/>, or also as `strategy.ForceST()` in some cases.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(SharedTrack.AOE).As&lt;<seealso cref="AOEStrategy"/>&gt;() == <seealso cref="ForceST"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(SharedTrack.AOE).As&lt;<seealso cref="AOEStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="AOEStrategy"/>.<br/>
    /// - "<c><b><seealso cref="ForceST"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The single-target rotation <b>force-executed</b>, regardless of any conditions.</returns>
    ForceST,

    /// <summary>
    /// The main strategy used for <b>force-executing</b> the <b>AOE</b> rotation.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.ForceAOE(StrategyValues),"/>, or also as `strategy.ForceAOE()` in some cases.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(SharedTrack.AOE).As&lt;<seealso cref="AOEStrategy"/>&gt;() == <seealso cref="ForceAOE"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(SharedTrack.AOE).As&lt;<seealso cref="AOEStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="AOEStrategy"/>.<br/>
    /// - "<c><b><seealso cref="ForceAOE"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The AOE rotation <b>force-executed</b>, regardless of any conditions.</returns>
    ForceAOE
}

/// <summary>
/// The <b>Default Strategy</b> enum used for tracking when to <b>hold any buffs, gauge, or cooldown abilties</b> for optimal usage.<para/>
/// <b>Example Given:</b><br/>
/// - <c>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;();</c><para/>
/// <b>Explanation:</b><br/>
/// - "<c><b>strategy</b></c>" is the parameter for tracking a specific strategy for a specific ability in the user's rotation module.<br/>
/// - "<c><b>Option</b></c>" are the relative options for user's specific strategy.<br/>
/// - "<c><b>(SharedTrack.Hold)</b></c>" is the enum representing all options relating to this custom strategy being tracked in the user's rotation module.<br/>
/// - "<c><b>.As&lt;<seealso cref="HoldStrategy"/>&gt;();</b></c>" is the relative strategy for user's specific ability being tracked in the user's rotation module.
/// </summary>
/// <returns>- All strategies listed under <seealso cref="HoldStrategy"/> into user's rotation module</returns>
public enum HoldStrategy
{
    /// <summary>
    /// The default strategy used for <b>not holding any buffs, gauge, or cooldown abilties</b>.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.DontHold(StrategyValues),"/>, or also as `strategy.DontHold()` in some cases.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;() == <seealso cref="DontHold"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="HoldStrategy"/>.<br/>
    /// - "<c><b><seealso cref="DontHold"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The availability of <b>using all buffs, gauge, or cooldown abilties</b>.</returns>
    DontHold,

    /// <summary>
    /// The main strategy used for <b>only holding</b> any ability that is <b>cooldown</b>-related.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.HoldCDs(StrategyValues),"/>, or also as `strategy.HoldCDs()` in some cases.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;() == <seealso cref="HoldCooldowns"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="HoldStrategy"/>.<br/>
    /// - "<c><b><seealso cref="HoldCooldowns"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The availability of <b>using all buffs and gauge abilties</b>, but <b>forbidden from using any cooldowns</b>.</returns>
    HoldCooldowns,

    /// <summary>
    /// The main strategy used for <b>only holding</b> any ability that is <b>gauge</b>-related.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.HoldGauge(StrategyValues),"/>, or also as `strategy.HoldGauge()` in some cases.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;() == <seealso cref="HoldGauge"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="HoldStrategy"/>.<br/>
    /// - "<c><b><seealso cref="HoldGauge"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The availability of <b>using all buffs and cooldowns</b>, but <b>forbidden from using any gauge abilties</b>.</returns>
    HoldGauge,

    /// <summary>
    /// The main strategy used for <b>only holding</b> any ability that is <b>buff</b>-related.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.HoldBuffs(StrategyValues),"/>, or also as `strategy.HoldBuffs()` in some cases.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;() == <seealso cref="HoldBuffs"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="HoldStrategy"/>.<br/>
    /// - "<c><b><seealso cref="HoldBuffs"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The availability of <b>using all cooldowns and gauge abilties</b>, but <b>forbidden from using any buffs</b>.</returns>
    HoldBuffs,

    /// <summary>
    /// The main strategy used for <b>holding</b> any ability that is <b>buff, cooldown, or gauge</b>-related.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.HoldAll(StrategyValues),"/>, or also as `strategy.HoldAll()` in some cases.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;() == <seealso cref="HoldEverything"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="HoldStrategy"/>.<br/>
    /// - "<c><b><seealso cref="HoldEverything"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The forbiddance from using all <b>buffs, cooldowns and gauge</b> abilties.</returns>
    HoldEverything
}

/// <summary>
/// The <b>Default Strategy</b> enum used for allowing or forbidding use of module-specific GCD abilities.<para/>
/// <b>Example Given:</b><br/>
/// - <c>strategy.Option(Track.SonicBreak).As&lt;<seealso cref="GCDStrategy"/>&gt;()</c><para/>
/// <b>Explanation:</b><br/>
/// - "<c><b>strategy</b></c>" is the parameter for tracking a specific strategy for a specific ability in the user's rotation module.<br/>
/// - "<c><b>Option</b></c>" are the relative options for user's specific strategy.<br/>
/// - "<c><b>(Track.SonicBreak)</b></c>" is the user's module-specific GCD ability enum being tracked.<br/>
/// - "<c><b>.As&lt;<seealso cref="GCDStrategy"/>&gt;()</b></c>" is the relative Default strategy for user's module-specific GCD ability being tracked in the user's rotation module.
/// </summary>
/// <returns>- All strategies listed under <seealso cref="GCDStrategy"/>.</returns>
public enum GCDStrategy
{
    /// <summary>
    /// The default strategy used for automatically executing user's module-specific GCD ability.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(Track.SonicBreak).As&lt;<seealso cref="GCDStrategy"/>&gt;() == <seealso cref="Automatic"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(Track.SonicBreak).As&lt;<seealso cref="GCDStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="GCDStrategy"/>.<br/>
    /// - "<c><b><seealso cref="Automatic"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- <b>Automatic execution</b> of ability based on user's logic.</returns>
    Automatic,

    /// <summary>
    /// The main strategy used for force-executing user's module-specific GCD ability.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(Track.SonicBreak).As&lt;<seealso cref="GCDStrategy"/>&gt;() == <seealso cref="Force"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(Track.SonicBreak).As&lt;<seealso cref="GCDStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="GCDStrategy"/>.<br/>
    /// - "<c><b><seealso cref="Force"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- <b>Forced execution</b> of user's module-specific GCD ability.</returns>
    Force,

    /// <summary>
    /// The main strategy used for forbidding use of user's module-specific GCD ability.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(Track.SonicBreak).As&lt;<seealso cref="GCDStrategy"/>&gt;() == <seealso cref="Delay"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(Track.SonicBreak).As&lt;<seealso cref="GCDStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="GCDStrategy"/>.<br/>
    /// - "<c><b><seealso cref="Delay"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- <b>Forbiddance of execution</b> for user's module-specific GCD ability.</returns>
    Delay
}

/// <summary>
/// The <b>Default Strategy</b> enum used for allowing or forbidding use of module-specific GCD abilities.<para/>
/// <b>Example Given:</b><br/>
/// - <c>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;()</c><para/>
/// <b>Explanation:</b><br/>
/// - "<c><b>strategy</b></c>" is the parameter for tracking a specific strategy for a specific ability in the user's rotation module.<br/>
/// - "<c><b>Option</b></c>" are the relative options for user's specific strategy.<br/>
/// - "<c><b>(Track.NoMercy)</b></c>" is the user's module-specific OGCD ability enum being tracked.<br/>
/// - "<c><b>.As&lt;<seealso cref="OGCDStrategy"/>&gt;()</b></c>" is the relative Default strategy for user's module-specific OGCD ability being tracked in the user's rotation module.
/// </summary>
/// <returns>- All strategies listed under <seealso cref="OGCDStrategy"/>.</returns>
public enum OGCDStrategy
{
    /// <summary>
    /// The default strategy used for automatically executing user's module-specific OGCD ability.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;() == <seealso cref="Automatic"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="OGCDStrategy"/>.<br/>
    /// - "<c><b><seealso cref="Automatic"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- <b>Automatic execution</b> of ability based on user's logic.</returns>
    Automatic,

    /// <summary>
    /// The main strategy used for force-executing user's module-specific OGCD ability.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;() == <seealso cref="Force"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="OGCDStrategy"/>.<br/>
    /// - "<c><b><seealso cref="Force"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- <b>Forced execution</b> of user's module-specific OGCD ability.</returns>
    Force,

    /// <summary>
    /// The main strategy used for force-executing user's module-specific OGCD ability in the very next weave window.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;() == <seealso cref="AnyWeave"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="OGCDStrategy"/>.<br/>
    /// - "<c><b><seealso cref="AnyWeave"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- <b>Forced execution</b> of user's module-specific OGCD ability inside next weave.</returns>
    AnyWeave,

    /// <summary>
    /// The main strategy used for force-executing user's module-specific OGCD ability in the first-half of very next weave window.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;() == <seealso cref="EarlyWeave"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="OGCDStrategy"/>.<br/>
    /// - "<c><b><seealso cref="EarlyWeave"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- <b>Forced execution</b> of user's module-specific OGCD ability inside next early-weave.</returns>
    EarlyWeave,

    /// <summary>
    /// The main strategy used for force-executing user's module-specific OGCD ability in the second-half of very next weave window.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;() == <seealso cref="LateWeave"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="OGCDStrategy"/>.<br/>
    /// - "<c><b><seealso cref="LateWeave"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- <b>Forced execution</b> of user's module-specific OGCD ability inside next late-weave.</returns>
    LateWeave,

    /// <summary>
    /// The main strategy used for forbidding use of user's module-specific OGCD ability.<para/>
    /// Example:<br/>
    /// - <c>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;() == <seealso cref="Delay"/></c><para/>
    /// Explanation:<br/>
    /// - "<c><b>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;()</b></c>" is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="OGCDStrategy"/>.<br/>
    /// - "<c><b><seealso cref="Delay"/></b></c>" is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- <b>Forbiddance of execution</b> for user's module-specific OGCD ability.</returns>
    Delay
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
    /// The primary helper we use for calling all our <b>GCD</b> abilities onto any actor.
    /// <br>This also handles <b>Ground Target</b> abilities, such as <c>BLM:LeyLines</c> or <c>NIN:Shukuchi</c></br><para/>
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
                => QueueGCD(aid, target, (int)(object)priority, delay, castTime, targetPos, facingAngle);
    protected void QueueGCD(AID aid, Actor? target, int priority = 2, float delay = 0, float castTime = 0, Vector3 targetPos = default, Angle? facingAngle = null)
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
    /// The primary helper we use for calling all our <b>OGCD</b> abilities onto any actor.<br/>
    /// This also handles <b>Ground Target</b> abilities, such as <c>BLM:LeyLines</c> or <c>NIN:Shukuchi</c><para/>
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
                => QueueOGCD(aid, target, (int)(object)priority, delay, castTime, targetPos, facingAngle);
    protected void QueueOGCD(AID aid, Actor? target, int priority = 1, float delay = 0, float castTime = 0, Vector3 targetPos = default, Angle? facingAngle = null)
    {
        if (priority == 0)
            return;

        QueueAction(aid, target, ActionQueue.Priority.Low + priority, delay, castTime, targetPos, facingAngle);
    }
    #endregion

    protected bool QueueAction(AID aid, Actor? target, float priority, float delay, float castTime, Vector3 targetPos = default, Angle? facingAngle = null)
    {
        var def = ActionDefinitions.Instance.Spell(aid);

        if ((uint)(object)aid == 0)
            return false;

        if (def == null)
            return false;

        if (def.Range != 0 && target == null)
        {
            return false;
        }

        //Ground Targeting abilities
        if (def.AllowedTargets.HasFlag(ActionTargets.Area))
        {
            if (def.Range == 0)
                targetPos = Player.PosRot.XYZ();
            else if (target != null)
                targetPos = target.PosRot.XYZ();
        }

        //TODO: is this necessary? idk maybe for melees
        if (castTime == 0)
            castTime = 0;

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority, delay: delay, castTime: castTime, targetPos: targetPos, facingAngle: facingAngle);
        return true;
    }
    #endregion

    #endregion

    #region HP/MP/Shield
    /// <summary>
    /// A quick and easy helper for retrieving the <b>current HP value</b> of the player.<para/>
    /// Example:<br/>
    /// - <c>HP >= 6900</c><para/>
    /// Explanation:<br/>
    ///    - "<c><b>HP</b></c>" represents the <b>current HP value</b> of the player.<br/>
    /// - "<c><b>>= 6900</b></c>" is the example conditional expression specified by the user.<br/>
    /// </summary>
    /// <returns>- A <b>value</b> representing the Player's <b>current HP</b></returns>
    protected uint HP { get; private set; }

    /// <summary>
    /// A quick and easy helper for retrieving the <b>current MP value</b> of the player.<para/>
    /// Example:<br/>
    /// - <c>MP != 4200</c><para/>
    /// Explanation:<br/>
    /// - "<c><b>MP</b></c>" represents the <b>current MP value</b> of the player.<br/>
    /// - "<c><b>!= 4200</b></c>" is the example conditional expression specified by the user.<br/>
    /// </summary>
    /// <returns>- A <b>value</b> representing the Player's <b>current MP</b></returns>
    protected uint MP { get; private set; }

    /// <summary>
    /// A quick and easy helper for retrieving the <b>current Shield value</b> of the player.<para/>
    /// Example:<br/>
    /// - <c>Shield > 0</c><para/>
    /// Explanation:<br/>
    /// - "<c><b>Shield</b></c>" represents the <b>current Shield value</b> of the player.<br/>
    /// - "<c><b>> 0</b></c>" is the example conditional expression specified by the user.<br/>
    /// </summary>
    /// <returns>- A <b>value</b> representing the Player's <b>current Shield</b></returns>
    protected uint Shield { get; private set; }

    /// <summary>
    /// A quick and easy helper for retrieving the <b>Current HP</b> of any specified actor, whether it is the player or any other target user desires.<para/>
    /// Example:<br/>
    /// - <c>TargetCurrentHP(primaryTarget) > 0</c><para/>
    /// Explanation:<br/>
    /// - "<c><b>TargetCurrentHP</b></c>" represents the <b>current HP value</b> of the specified actor.<br/>
    /// - "<c><b>(primaryTarget)</b></c>" represents the specified actor being checked.<br/>
    /// - "<c><b>> 0</b></c>" is the example conditional expression specified by the user.<br/>
    /// </summary>
    /// <param name="actor">Any specified player, ally, or target</param>
    /// <returns>- A <b>value</b> representing the <b>current HP</b> of user's specified actor</returns>
    protected uint TargetCurrentHP(Actor actor) => actor.HPMP.CurHP;

    /// <summary>
    /// A quick and easy helper for retrieving the <b>Current Shield</b> of any specified actor, whether it is the player or any other target user desires.<para/>
    /// Example:<br/>
    /// - <c>TargetCurrentShield(primaryTarget) > 0</c><para/>
    /// Explanation:<br/>
    /// - "<c><b>TargetCurrentShield</b></c>" represents the <b>current Shield value</b> of the specified actor.<br/>
    /// - "<c><b>(primaryTarget)</b></c>" represents the specified actor being checked.<br/>
    /// - "<c><b>> 0</b></c>" is the example conditional expression specified by the user.<br/>
    /// </summary>
    /// <param name="actor">Any specified player, ally, or target</param>
    /// <returns>- A <b>value</b> representing the <b>current Shield of user's specified actor</b>.</returns>
    protected uint TargetCurrentShield(Actor actor) => actor.HPMP.Shield;

    /// <summary>
    /// A quick and easy helper for checking if specified actor has any <b>current Shield</b> present, whether it is the player or any other target user desires.<para/>
    /// Example:<br/>
    /// - <c>TargetHasShield(primaryTarget)</c><para/>
    /// Explanation:<br/>
    /// - "<c><b>TargetHasShield</b></c>" checks if the specified actor has any <b>current Shield value</b>.<br/>
    /// - "<c><b>(primaryTarget)</b></c>" represents the specified actor being checked.<br/>
    /// </summary>
    /// <param name="actor">Any specified player, ally, or target</param>
    protected bool TargetHasShield(Actor actor) => actor.HPMP.Shield > 0.1f;

    /// <summary>
    /// A quick and easy helper for retrieving the <b>Current HP Percentage</b> of any specified actor, whether it is the player or any other target user desires.<para/>
    /// Example:<br/>
    /// - <c>TargetHPP(primaryTarget) > 50</c><para/>
    /// Explanation:<br/>
    /// - "<c><b>TargetHPP</b></c>" represents the <b>current HP Percentage value</b> of the specified actor.<br/>
    /// - "<c><b>(primaryTarget)</b></c>" represents the specified actor being checked.<br/>
    /// - "<c><b>> 50</b></c>" is the example conditional expression specified by the user.<br/>
    /// </summary>
    /// <param name="target">Any specified player, ally, or target</param>
    /// <returns>- A <b>value</b> representing the <b>current HP Percentage (%) of user's specified actor</b>.</returns>
    public static float TargetHPP(Actor? target = null)
    {
        if (target is null || target.IsDead)
            return 0f;

        if (target is Actor actor)
        {
            var HPP = (float)actor.HPMP.CurHP / actor.HPMP.MaxHP * 100f;
            return Math.Clamp(HPP, 0f, 100f);
        }

        return 0f;
    }
    #endregion

    #region Actions
    /// <summary>
    /// Checks if specified action is <b>Unlocked</b> based on <b>Level</b> and <b>Job Quest</b> (if required).<para/>
    /// Example:<br/>
    /// - <c>Unlocked(AID.GnashingFang)</c><para/>
    /// Explanation:<br/>
    /// - "<c><b>Unlocked</b></c>" is the function.<br/>
    /// - "<c><b>(AID.GnashingFang)</b></c>" is the example ability being checked, called <b>using <seealso cref="BossMod"/>.<seealso cref="GNB"/></b>.
    /// </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    /// <returns>- <c><b>TRUE</b></c> if the <b>specified action is Unlocked</b>.<br/>
    /// - <c><b>FALSE</b></c> if the <b>specified action is still locked or Job Quest is unfinished</b>.</returns>
    protected bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid)); //Check if the desired ability is unlocked

    /// <summary>
    /// Checks if specified trait is <b>Unlocked</b> based on <b>Level</b> and <b>Job Quest</b> (if required).<para/>
    /// Example:<br/>
    /// - <c>Unlocked(TraitID.EnhancedBloodfest)</c><para/>
    /// Explanation:<br/>
    /// - "<c><b>Unlocked</b></c>" is the function.<br/>
    /// - "<b><c>(TraitID.EnhancedBloodfest)</c></b>" is the example trait being checked, called <b>using <seealso cref="BossMod"/>.<seealso cref="GNB"/></b>.
    /// </summary>
    /// <param name="tid"> The user's specified <b>Trait ID</b> being checked.</param>
    /// <returns>- <c><b>TRUE</b></c> if the <b>specified action is Unlocked</b>.<br/>
    /// - <c><b>FALSE</b></c> if the <b>specified action is still locked or Job Quest is unfinished</b>.</returns>
    protected bool Unlocked(TraitID tid) => TraitUnlocked((uint)(object)tid);

    /// <summary>
    /// Checks if <b>last combo action</b> is what the user is specifying.<br/>
    /// <c><b>NOTE</b></c>: This does <c><b>NOT</b></c> check all actions, only combo actions.<para/>
    /// Example:<br/>
    /// - <c>ComboLastMove == AID.BrutalShell</c><para/>
    /// Explanation:<br/>
    /// - "<c><b>ComboLastMove</b></c>" is the function.<br/>
    /// - "<c><b>AID.BrutalShell</b></c>" is the example specified combo action being checked, called <b>using <seealso cref="BossMod"/>.<seealso cref="GNB"/></b>.
    /// </summary>
    /// <returns>- <c><b>TRUE</b></c> if the last combo action is what the user is specifying.<br/>
    /// - <c><b>FALSE</b></c> if otherwise or last action was not a combo action.</returns>
    protected AID ComboLastMove => (AID)(object)World.Client.ComboState.Action;

    /// <summary>
    /// Checks the <b>time left remaining</b> inside current combo before expiration.<para/>
    /// <c><b>NOTE</b></c>: This does <c><b>NOT</b></c> check all actions, only combo actions.<para/>
    /// </summary>
    protected float ComboTimer => (float)(object)World.Client.ComboState.Remaining;

    /// <summary>
    /// Retrieves <b>actual cast time</b> of a specified action.<para/>
    /// Example:<br/>
    /// - <b>ActualCastTime(AID.Fire3) > 0</b><para/>
    /// Explanation:<br/>
    /// - "<c><b>ActualCastTime</b></c>" is the function.<br/>
    /// - "<c><b>AID.Fire3</b></c>" is the example specified action being checked, called <b>using <seealso cref="BossMod"/>.<seealso cref="BLM"/></b>.<br/>
    /// - "<c><b>> 0</b></c>" is the example conditional expression specified by the user.<br/>
    /// </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    /// <returns>- A <b>value</b> representing the current <b>real-time Cast Time</b> of user's specified action</returns>
    protected virtual float ActualCastTime(AID aid) => ActionDefinitions.Instance.Spell(aid)!.CastTime;

    /// <summary>
    /// Retrieves <b>effective cast time</b> of a specified action.<para/>
    /// Example:<br/>
    /// - <b>EffectiveCastTime(AID.Fire3) > 0</b><para/>
    /// Explanation:<br/>
    /// - "<c><b>EffectiveCastTime</b></c>" is the function.<br/>
    /// - "<c><b>AID.Fire3</b></c>" is the example specified action being checked, called <b>using <seealso cref="BossMod"/>.<seealso cref="BLM"/></b>.<br/>
    /// - "<c><b>> 0</b></c>" is the example conditional expression specified by the user.<br/>
    /// </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    /// <returns>- A <b>value</b> representing the current <b>effective Cast Time</b> of user's specified action</returns>
    protected virtual float EffectiveCastTime(AID aid) => PlayerHasEffect(ClassShared.SID.Swiftcast, 10) ? 0 : ActualCastTime(aid) * SpSGCDLength / 2.5f;

    /// <summary><para>Retrieves player's GCD length based on <b>Skill-Speed</b>.</para>
    /// <para><c><b>NOTE</b></c>: This function is only recommended for jobs with <b>Skill-Speed</b>. <b>Spell-Speed</b> users are <b>unaffected</b> by this function.</para></summary>
    /// <returns>- A <b>value</b> representing the player's current <b>GCD Length</b></returns>
    protected float SkSGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);

    /// <summary>Retrieves player's current <b>Skill-Speed</b> stat.</summary>
    /// <returns>- A <b>value</b> representing the player's current <b>Skill-Speed</b></returns>
    protected float SkS => ActionSpeed.Round(World.Client.PlayerStats.SkillSpeed);

    /// <summary><para>Retrieves player's GCD length based on <b>Spell-Speed</b>.</para>
    /// <para><c><b>NOTE</b></c>: This function is only recommended for jobs with <b>Spell-Speed</b>. <b>Skill-Speed</b> users are <b>unaffected</b> by this function.</para></summary>
    /// <returns>- A <b>value</b> representing the player's current <b>GCD Length</b></returns>
    protected float SpSGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level);

    /// <summary>Retrieves player's current <b>Spell-Speed</b> stat.</summary>
    /// <returns>- A <b>value</b> representing the player's current <b>Spell-Speed</b></returns>
    protected float SpS => ActionSpeed.Round(World.Client.PlayerStats.SpellSpeed);

    /// <summary>Checks if we can fit in a <b>skill-speed based</b> GCD.</summary>
    /// <param name="duration"> </param>
    /// <param name="extraGCDs"> How many extra GCDs the user can fit in.</param>
    /// <returns>- <c><b>TRUE</b></c> if we can fit in a <b>skill-speed based</b> GCD <para>- <c><b>FALSE</b></c> if otherwise</para></returns>
    protected bool CanFitSkSGCD(float duration, int extraGCDs = 0) => GCD + SkSGCDLength * extraGCDs < duration;

    /// <summary>Checks if we can fit in a <b>spell-speed based</b> GCD.</summary>
    /// <param name="duration"> </param>
    /// <param name="extraGCDs"> How many extra GCDs the user can fit in.</param>
    /// <returns>- <c><b>TRUE</b></c> if we can fit in a <b>spell-speed based</b> GCD <para>- <c><b>FALSE</b></c> if otherwise</para></returns>
    protected bool CanFitSpSGCD(float duration, int extraGCDs = 0) => GCD + SpSGCDLength * extraGCDs < duration;

    /// <summary><para>Checks if player is available to weave in any abilities.</para>
    /// <para><c><b>NOTE</b></c>: This function is only recommended for jobs with <b>Skill-Speed</b>. <b>Spell-Speed</b> users are <b>unaffected</b> by this.</para></summary>
    /// <param name="cooldown"> The cooldown time of the action specified.</param>
    /// <param name="actionLock"> The animation lock time of the action specified.</param>
    /// <param name="extraGCDs"> How many extra GCDs the user can fit in.</param>
    /// <param name="extraFixedDelay"> How much extra delay the user can add in, in seconds.</param>
    /// <returns>- <c><b>TRUE</b></c> if we can weave in a <b>skill-speed based</b> GCD <para>- <c><b>FALSE</b></c> if otherwise</para></returns>
    protected bool CanWeave(float cooldown, float actionLock, int extraGCDs = 0, float extraFixedDelay = 0)
        => MathF.Max(cooldown, World.Client.AnimationLock) + actionLock + AnimationLockDelay <= GCD + SkSGCDLength * extraGCDs + extraFixedDelay;

    /// <summary><para>Checks if player is available to weave in any spells.</para>
    /// <para><c><b>NOTE</b></c>: This function is only recommended for jobs with <b>Spell-Speed</b>. <b>Skill-Speed</b> users are <b>unaffected</b> by this.</para></summary>
    /// <param name="cooldown"> The cooldown time of the action specified.</param>
    /// <param name="actionLock"> The animation lock time of the action specified.</param>
    /// <param name="extraGCDs"> How many extra GCDs the user can fit in.</param>
    /// <param name="extraFixedDelay"> How much extra delay the user can add in, in seconds.</param>
    /// <returns>- <c><b>TRUE</b></c> if we can weave in a <b>spell-speed based</b> GCD <para>- <c><b>FALSE</b></c> if otherwise</para></returns>
    protected bool CanSpellWeave(float cooldown, float actionLock, int extraGCDs = 0, float extraFixedDelay = 0)
        => MathF.Max(cooldown, World.Client.AnimationLock) + actionLock + AnimationLockDelay <= GCD + SpSGCDLength * extraGCDs + extraFixedDelay;

    /// <summary><para>Checks if player is available to weave in any abilities.</para>
    /// <para><c><b>NOTE</b></c>: This function is only recommended for jobs with <b>Skill-Speed</b>. <b>Spell-Speed</b> users are <b>unaffected</b> by this.</para></summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    /// <param name="extraGCDs"> How many extra GCDs the user can fit in.</param>
    /// <param name="extraFixedDelay"> How much extra delay the user can add in, in seconds.</param>
    /// <returns>- <c><b>TRUE</b></c> if we can weave in any abilities <para>- <c><b>FALSE</b></c> if otherwise</para></returns>
    protected bool CanWeave(AID aid, int extraGCDs = 0, float extraFixedDelay = 0)
    {
        if (!Unlocked(aid))
            return false;

        var res = ActionDefinitions.Instance[ActionID.MakeSpell(aid)]!;
        return SkS > 100
            ? CanSpellWeave(ChargeCD(aid), res.InstantAnimLock, extraGCDs, extraFixedDelay)
            : SpS > 100 && CanWeave(ChargeCD(aid), res.InstantAnimLock, extraGCDs, extraFixedDelay);
    }

    /// <summary>Checks if user is in pre-pull stage; useful for <b>First GCD</b> openings.</summary>
    /// <returns>- <c><b>TRUE</b></c> if user is in pre-pull stage or fully not in combat<para>- <c><b>FALSE</b></c> if otherwise</para></returns>
    protected bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f;

    /// <summary>Checks if user can <b>Weave in</b> any abilities.<c><b>NOTE</b></c>: This function is pretty sub-optimal, but gets the job done. <b>CanWeave() </b>is much more intricate if user really wants it.</para></summary>
    protected bool CanWeaveIn => GCD is <= 2.49f and >= 0.01f;

    /// <summary>Checks if user can <b>Early Weave in</b> any abilities.<para><c><b>NOTE</b></c>: This function is pretty sub-optimal, but gets the job done. <b>CanWeave() </b>is much more intricate if user really wants it.</para></summary>
    protected bool CanEarlyWeaveIn => GCD is <= 2.49f and >= 1.26f;

    /// <summary>Checks if user can <b>Late Weave in</b> any abilities.<para><c><b>NOTE</b></c>: This function is pretty sub-optimal, but gets the job done. <b>CanWeave() </b>is much more intricate if user really wants it.</para></summary>
    protected bool CanLateWeaveIn => GCD is <= 1.25f and >= 0.01f;

    /// <summary>Checks if user can <b>Quarter Weave in</b> any abilities.<para><c><b>NOTE</b></c>: This function is pretty sub-optimal, but gets the job done. <b>CanWeave() </b>is much more intricate if user really wants it.</para></summary>
    protected bool CanQuarterWeaveIn => GCD is < 0.9f and >= 0.01f;
    #endregion

    #region Cooldown
    /// <summary> Retrieves the total cooldown time left on the specified action. </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    /// <returns>- A <b>value</b> representing the current <b>cooldown</b> on user's specified <b>Action ID</b></returns>
    protected float TotalCD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    /// <summary> Returns the charge cooldown time left on the specified action. </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    /// <returns>- A <b>value</b> representing the current <b>charge cooldown</b> on user's specified <b>Action ID</b></returns>
    protected float ChargeCD(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) : float.MaxValue;

    /// <summary> Checks if action is ready to be used based on if it's <b>Unlocked</b> and its <b>charge cooldown timer</b>. </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    /// <returns>- <c><b>TRUE</b></c> if the specified Action is <b>Unlocked</b> and <b>off cooldown.</b> <para>- <c><b>FALSE</b></c> if <b>locked</b> or still on <b>cooldown</b></para></returns>
    protected bool ActionReady(AID aid) => Unlocked(aid) && ChargeCD(aid) < 0.6f;

    /// <summary> Checks if action has any charges remaining. </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    /// <returns>- <c><b>TRUE</b></c> if the specified Action has any <b>charges</b> available <para>- <c><b>FALSE</b></c> if locked or still on cooldown</para></returns>
    protected bool HasCharges(AID aid) => ChargeCD(aid) < 0.6f;

    /// <summary>Checks if action is on cooldown based on its <b>total cooldown timer</b>. </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    /// <returns>- <c><b>TRUE</b></c> if the specified Action is <b>still on cooldown</b> <para>- <c><b>FALSE</b></c> if <b>off cooldown</b></para></returns>
    protected bool IsOnCooldown(AID aid) => TotalCD(aid) > 0.6f;

    /// <summary>Checks if action is off cooldown based on its <b>total cooldown timer</b>. </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    /// <returns>- <c><b>TRUE</b></c> if the specified Action is <b>off cooldown</b> <para>- <c><b>FALSE</b></c> if <b>still on cooldown</b></para></returns>
    protected bool IsOffCooldown(AID aid) => !IsOnCooldown(aid);

    /// <summary>Checks if action is off cooldown based on its <b>charge cooldown timer</b>. </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    /// <returns>- <c><b>TRUE</b></c> if the specified Action is <b>off cooldown</b> <para>- <c><b>FALSE</b></c> if <b>still on cooldown</b></para></returns>
    protected bool OnCooldown(AID aid) => MaxChargesIn(aid) > 0;

    /// <summary>Checks if last action used is what the user is specifying and within however long. </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    /// <returns>- <c><b>TRUE</b></c> if the specified Action was <b>just used</b> <para>- <c><b>FALSE</b></c> if was <b>not just used</b></para></returns>
    protected bool LastActionUsed(AID aid) => Manager.LastCast.Data?.IsSpell(aid) == true;

    /// <summary>Retrieves time remaining until specified action is at max charges. </summary>
    /// <param name="aid"> The user's specified <b>Action ID</b> being checked.</param>
    protected float MaxChargesIn(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.ChargeCapIn(World.Client.Cooldowns, World.Client.DutyActions, Player.Level) : float.MaxValue;

    #endregion

    #region Status
    /// <summary> Retrieves the amount of specified status effect's stacks remaining on any target.
    /// <para><c><b>NOTE</b></c>: The effect MUST be owned by the Player.</para>
    /// <para><b>Example Given:</b> "<b>StacksRemaining(Player, SID.Requiescat, 30) > 0</b>"</para></summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <param name="sid"> The user's specified <b>Status ID</b> being checked.</param>
    /// <param name="duration"> The <b>Total Effect Duration</b> of specified <b>Status ID</b> being checked.</param>
    /// <returns>- A value indicating how many stacks exist</returns>
    protected int StacksRemaining<SID>(Actor? target, SID sid, float duration = 1000f) where SID : Enum => StatusDetails(target, sid, Player.InstanceID, duration).Stacks;

    /// <summary> Retrieves the amount of specified status effect's time left remaining on any target.
    /// <para><c><b>NOTE</b></c>: The effect MUST be owned by the Player.</para>
    /// <para><b>Example Given:</b> "<b>StatusRemaining(Player, SID.Requiescat, 30) > 0f</b>"</para></summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <param name="sid"> The user's specified <b>Status ID</b> being checked.</param>
    /// <param name="duration"> The <b>Total Effect Duration</b> of specified <b>Status ID</b> being checked.</param>
    /// <returns>- A value indicating how much time left on existing effect</returns>
    protected float StatusRemaining<SID>(Actor? target, SID sid, float duration) where SID : Enum => StatusDetails(target, sid, Player.InstanceID, duration).Left;

    /// <summary> Checks if a specific status effect on the player exists.
    /// <para><c><b>NOTE</b></c>: The effect MUST be owned by the Player.</para>
    /// <para><b>Example Given:</b> "<b>PlayerHasEffect(SID.NoMercy, 20)</b>"</para></summary>
    /// <param name="sid"> The user's specified <b>Status ID</b> being checked.</param>
    /// <param name="duration"> The <b>Total Effect Duration</b> of specified <b>Status ID</b> being checked.</param>
    /// <returns>- A value indicating if the effect exists</returns>
    protected bool PlayerHasEffect<SID>(SID sid, float duration = 1000f) where SID : Enum => StatusRemaining(Player, sid, duration) > 0.1f;

    /// <summary> Checks if a specific status effect on the player exists.
    /// <para><c><b>NOTE</b></c>: The effect can be owned by anyone; Player, Party, Alliance, NPCs or even enemies</para>
    /// <para><b>Example Given:</b> "<b>PlayerHasEffectAny(SID.Troubadour)</b>"</para></summary>
    /// <param name="sid"> The user's specified <b>Status ID</b> being checked.</param>
    /// <param name="duration"> The <b>Total Effect Duration</b> of specified <b>Status ID</b> being checked.</param>
    /// <returns>- A value indicating if the effect exists</returns>
    protected bool PlayerHasAnyEffect<SID>(SID sid) where SID : Enum => Player.FindStatus(sid) != null;

    /// <summary> Checks if a specific status effect on any specified target exists.
    /// <para><c><b>NOTE</b></c>: The effect MUST be owned by the Player.</para>
    /// <para><b>Example Given:</b> "<b>TargetHasEffect(primaryTarget, SID.SonicBreak, 30)</b>"</para></summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <param name="sid"> The user's specified <b>Status ID</b> being checked.</param>
    /// <param name="duration"> The <b>Total Effect Duration</b> of specified <b>Status ID</b> being checked.</param>
    /// <returns>- A value indicating if the effect exists</returns>
    protected bool TargetHasEffect<SID>(Actor? target, SID sid, float duration = 1000f) where SID : Enum => StatusRemaining(target, sid, duration) > 0.1f;

    /// <summary> Checks if a specific status effect on any specified target exists.
    /// <para><c><b>NOTE</b></c>: The effect can be owned by anyone; Player, Party, Alliance, NPCs or even enemies</para>
    /// <para><b>Example Given:</b> "<b>TargetHasAnyEffect(primaryTarget, SID.MeditativeBrotherhood)</b>"</para></summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <param name="sid"> The user's specified <b>Status ID</b> being checked.</param>
    /// <returns>- A value indicating if the effect exists</returns>
    protected bool TargetHasAnyEffect<SID>(Actor? target, SID sid) where SID : Enum => target?.FindStatus(sid) != null;

    /// <summary> Checks if Player has any stacks of specific status effect.
    /// <para><c><b>NOTE</b></c>: The effect MUST be owned by the Player.</para>
    /// <para><b>Example Given:</b> "<b>PlayerHasStacks(SID.Requiescat)</b>"</para></summary>
    /// <param name="sid"> The user's specified <b>Status ID</b> being checked.</param>
    /// <returns>- A value indicating if the effect exists</returns>
    protected bool PlayerHasStacks<SID>(SID sid) where SID : Enum => StacksRemaining(Player, sid) > 0;

    #endregion

    #region Targeting

    #region Position Checks
    /// <summary>
    /// Checks precise positioning between <b>player target</b> and any other targets.
    /// </summary>
    protected delegate bool PositionCheck(Actor playerTarget, Actor targetToTest);
    /// <summary>
    /// <para>Calculates the <b>priority</b> of a target based on the <b>total number of targets</b> and the <b>primary target</b> itself.</para>
    /// <para>It is generic, so it can return different types based on the implementation.</para>
    /// </summary>
    protected delegate P PriorityFunc<P>(int totalTargets, Actor primaryTarget);
    /// <summary>
    /// Position checker for determining the best target for an ability that deals <b>Splash</b> damage.
    /// </summary>
    protected PositionCheck IsSplashTarget => (primary, other) => Hints.TargetInAOECircle(other, primary.Position, 5);
    /// <summary>
    /// Position checker for determining the best target for an ability that deals damage in a <b>Cone</b> .
    /// </summary>
    protected PositionCheck IsConeTarget => (primary, other) => Hints.TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 45.Degrees());
    /// <summary>
    /// <para>Position checker for determining the best target for an ability that deals damage in a <b>Line</b> within <b>Ten (10) yalms</b>.</para>
    /// </summary>
    protected PositionCheck Is10yRectTarget => (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2);
    /// <summary>
    /// <para>Position checker for determining the best target for an ability that deals damage in a <b>Line</b> within <b>Fifteen (15) yalms</b>.</para>
    /// </summary>
    protected PositionCheck Is15yRectTarget => (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 15, 2);
    /// <summary>
    /// Position checker for determining the best target for an ability that deals damage in a <b>Line</b> within <b>Twenty-five (25) yalms</b>
    /// </summary>
    protected PositionCheck Is25yRectTarget => (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 25, 2);
    #endregion

    /// <summary>
    /// Checks if target is within <b>Zero (0) yalms</b> in distance, or if Player is inside hitbox.
    /// </summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <returns></returns>
    protected bool In0y(Actor? target) => Player.DistanceToHitbox(target) <= 0.00f;

    /// <summary>
    /// Checks if target is within <b>Three (3) yalms</b> in distance.
    /// </summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <returns></returns>
    protected bool In3y(Actor? target) => Player.DistanceToHitbox(target) <= 2.99f;

    /// <summary>
    /// Checks if target is within <b>Five (5) yalms</b> in distance.
    /// </summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <returns></returns>
    protected bool In5y(Actor? target) => Player.DistanceToHitbox(target) <= 4.99f;

    /// <summary>
    /// Checks if target is within <b>Ten (10) yalms</b> in distance.
    /// </summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <returns></returns>
    protected bool In10y(Actor? target) => Player.DistanceToHitbox(target) <= 9.99f;

    /// <summary>
    /// Checks if target is within <b>Fifteen (15) yalms</b> in distance.
    /// </summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <returns></returns>
    protected bool In15y(Actor? target) => Player.DistanceToHitbox(target) <= 14.99f;

    /// <summary>
    /// Checks if target is within <b>Twenty (20) yalms</b> in distance.
    /// </summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <returns></returns>
    protected bool In20y(Actor? target) => Player.DistanceToHitbox(target) <= 19.99f;

    /// <summary>
    /// Checks if target is within <b>Twenty-five (25) yalms</b> in distance.
    /// </summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <returns></returns>
    protected bool In25y(Actor? target) => Player.DistanceToHitbox(target) <= 24.99f;

    /// <summary>
    /// <para>A simpler smart-targeting helper for picking a <b>specific</b> target over your current target.</para>
    /// <para>Very useful for intricate planning of ability targeting in specific situations.</para>
    /// </summary>
    /// <param name="track">The user's picked strategy's option <b>Track</b>, retrieved from module's enums and definitions. (e.g. <b>strategy.Option(Track.NoMercy)</b>)</param>
    /// <returns></returns>
    protected Actor? TargetChoice(StrategyValues.OptionRef track) => ResolveTargetOverride(track.Value); //Resolves the target choice based on the strategy

    /// <summary>Targeting function for indicating when or not <b>AOE Circle</b> abilities should be used based on targets nearby.</summary>
    /// <param name="range">The range of the <b>AOE Circle</b> ability, or radius from center of Player; this should be adjusted accordingly to user's module specific to job's abilities.</param>
    /// <returns>- A tuple with the following booleans:
    /// <para><b>-- OnTwoOrMore</b>: A boolean indicating if there are two (2) or more targets inside Player's <b>AOE Circle</b>.</para>
    /// <para><b>-- OnThreeOrMore</b>: A boolean indicating if there are three (3) or more targets inside Player's <b>AOE Circle</b>.</para>
    /// <para><b>-- OnFourOrMore</b>: A boolean indicating if there are four (4) or more targets inside Player's <b>AOE Circle</b>.</para>
    /// <para><b>-- OnFiveOrMore</b>: A boolean indicating if there are five (5) or more targets inside Player's <b>AOE Circle</b>.</para></returns>
    protected (bool OnTwoOrMore, bool OnThreeOrMore, bool OnFourOrMore, bool OnFiveOrMore) ShouldUseAOECircle(float range)
    {
        var OnTwoOrMore = Hints.NumPriorityTargetsInAOECircle(Player.Position, range) > 1;
        var OnThreeOrMore = Hints.NumPriorityTargetsInAOECircle(Player.Position, range) > 2;
        var OnFourOrMore = Hints.NumPriorityTargetsInAOECircle(Player.Position, range) > 3;
        var OnFiveOrMore = Hints.NumPriorityTargetsInAOECircle(Player.Position, range) > 4;

        return (OnTwoOrMore, OnThreeOrMore, OnFourOrMore, OnFiveOrMore);
    }

    /// <summary>
    /// This function attempts to pick a suitable primary target automatically, even if a target is not already picked.
    /// </summary>
    /// <param name="strategy">The user's picked <b>Strategy</b></param>
    /// <param name="primaryTarget">The user's current <b>specified Target</b>.</param>
    /// <param name="range"></param>
    protected void GetPrimaryTarget(StrategyValues strategy, ref Enemy? primaryTarget, float range)
    {
        var AOEStrat = strategy.Option(SharedTrack.AOE).As<AOEStrategy>();
        if (AOEStrat is AOEStrategy.Automatic)
        {
            if (Player.DistanceToHitbox(primaryTarget?.Actor) > range)
            {
                var newTarget = Hints.PriorityTargets.FirstOrDefault(x => Player.DistanceToHitbox(x.Actor) <= range);
                if (newTarget != null)
                    primaryTarget = newTarget;
            }
        }
    }

    /// <summary>
    /// This function attempts to pick the best target automatically.
    /// </summary>
    /// <param name="strategy">The user's picked <b>Strategy</b></param>
    /// <param name="primaryTarget">The user's current <b>picked Target</b>.</param>
    /// <param name="range"></param>
    /// <param name="isInAOE"></param>
    /// <returns></returns>
    protected (Enemy? Best, int Targets) GetBestTarget(Enemy? primaryTarget, float range, PositionCheck isInAOE) => GetTarget(primaryTarget, range, isInAOE, (numTargets, _) => numTargets, a => a);

    /// <summary>
    /// This function picks the target based on HP, modified by how many targets are in the AOE.
    /// </summary>
    /// <param name="strategy"></param>
    /// <param name="primaryTarget">The user's current <b>picked Target</b>.</param>
    /// <param name="range"></param>
    /// <param name="isInAOE"></param>
    /// <returns></returns>
    protected (Enemy? Best, int Targets) GetTargetByHP(Enemy? primaryTarget, float range, PositionCheck isInAOE) => GetTarget(primaryTarget, range, isInAOE, (numTargets, enemy) => (numTargets, numTargets > 2 ? enemy.HPMP.CurHP : 0), args => args.numTargets);

    /// <summary>
    /// Main function for picking a target, generalized for any prioritization and simplification logic.
    /// </summary>
    /// <typeparam name="P"></typeparam>
    /// <param name="strategy"></param>
    /// <param name="primaryTarget">The user's current <b>picked Target</b>.</param>
    /// <param name="range"></param>
    /// <param name="isInAOE"></param>
    /// <param name="prioritize"></param>
    /// <param name="simplify"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Identify an appropriate target for applying <b>DoT</b> effect. This has no impact if any <b>auto-targeting</b> is disabled.
    /// </summary>
    /// <typeparam name="P"></typeparam>
    /// <param name="strategy"></param>
    /// <param name="initial"></param>
    /// <param name="getTimer"></param>
    /// <param name="maxAllowedTargets"></param>
    /// <returns></returns>
    protected (Enemy? Target, P Timer) GetDOTTarget<P>(Enemy? initial, Func<Actor?, P> getTimer, int maxAllowedTargets) where P : struct, IComparable
    {
        // Check if the initial target is null or if the maxAllowedTargets is 0, in which case no valid target can be selected
        if (initial == null || maxAllowedTargets <= 0)
        {
            return (null, getTimer(null));
        }

        var newTarget = initial;
        var initialTimer = getTimer(initial?.Actor);
        var newTimer = initialTimer;
        var numTargets = 0;

        foreach (var dotTarget in Hints.PriorityTargets)
        {
            // Skip targets that forbid DOTs
            if (dotTarget.ForbidDOTs)
                continue;

            // If we exceed the max number of allowed targets, stop and return the current best
            if (++numTargets > maxAllowedTargets)
                return (newTarget, newTimer);

            // Get the timer for the current target
            var thisTimer = getTimer(dotTarget.Actor);

            // Update the new target and timer if the current one is better (has a smaller timer value)
            if (thisTimer.CompareTo(newTimer) < 0)
            {
                newTarget = dotTarget;
                newTimer = thisTimer;
            }
        }

        return (newTarget, newTimer);
    }
    #endregion

    #region Actors
    /// <summary>
    /// Player's "actual" target; guaranteed to be an enemy.
    /// </summary>
    protected Enemy? PlayerTarget { get; private set; }

    //TODO: implement this soon
    protected Actor? AnyTarget { get; private set; }
    #endregion

    #region Positionals
    /// <summary>
    /// Retrieves the current positional of the target based on target's position and rotation.
    /// </summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <returns></returns>
    protected Positional GetCurrentPositional(Actor target) => (Player.Position - target.Position).Normalized().Dot(target.Rotation.ToDirection()) switch //Check current positional based on target
    {
        < -0.7071068f => Positional.Rear, //value for Rear positional
        < 0.7071068f => Positional.Flank, //value for Flank positional
        _ => Positional.Front //default to Front positional if not on Rear or Flank
    };

    /// <summary>
    /// Checks if player is on specified target's <b>Rear Positional</b>.
    /// </summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <returns></returns>
    protected bool IsOnRear(Actor target) => GetCurrentPositional(target) == Positional.Rear;

    /// <summary>
    /// Checks if player is on specified target's <b>Flank Positional</b>.
    /// </summary>
    /// <param name="target">The user's specified <b>Target</b> being checked.</param>
    /// <returns></returns>
    protected bool IsOnFlank(Actor target) => GetCurrentPositional(target) == Positional.Flank;
    #endregion

    #region AI
    /// <summary>
    /// Establishes a goal-zone for a single target within a specified range.<br/>
    /// Primarily utilized by <b>caster-DPS</b> jobs that lack a dedicated maximize-AOE function.
    /// </summary>
    /// <param name="range">The range within which the goal zone is applied.</param>
    protected void GoalZoneSingle(float range)
    {
        if (PlayerTarget != null)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(PlayerTarget.Actor, range));
    }

    /// <summary>
    /// Defines a goal-zone using a combined strategy, factoring in AOE considerations.
    /// </summary>
    /// <param name="strategy">The strategy values that influence the goal zone logic.</param>
    /// <param name="range">The base range for the goal zone.</param>
    /// <param name="fAoe">A function determining the area of effect.</param>
    /// <param name="firstUnlockedAoeAction">The first available AOE action.</param>
    /// <param name="minAoe">The minimum number of targets required to trigger AOE.</param>
    /// <param name="positional">The positional requirement for the goal zone (default: any).</param>
    /// <param name="maximumActionRange">An optional parameter specifying the maximum action range.</param>
    protected void GoalZoneCombined(StrategyValues strategy, float range, Func<WPos, float> fAoe, AID firstUnlockedAoeAction, int minAoe, Positional positional = Positional.Any, float? maximumActionRange = null)
    {
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
    protected void AnyGoalZoneCombined(float range, Func<WPos, float> fAoe, AID firstUnlockedAoeAction, int minAoe, Positional positional = Positional.Any, float? maximumActionRange = null)
    {
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

    #endregion

    #region Misc
    /// <summary>Estimates the delay caused by <b>animation lock</b>.</summary>
    /// <returns>- A <b>value</b> representing the current <b>Animation Lock Delay</b></returns>
    protected float AnimationLockDelay { get; private set; }

    /// <summary>Estimates the time remaining until the <b>next Down-time phase</b>.</summary>
    protected float DowntimeIn { get; private set; }

    /// <summary>Elapsed time in <b>seconds</b> since the start of combat.</summary>
    protected float CombatTimer { get; private set; }

    /// <summary>Time remaining on pre-pull (or any) <b>Countdown Timer</b>.</summary>
    protected float? CountdownRemaining { get; private set; }

    /// <summary>Checks if player is currently <b>moving</b>.</summary>
    protected bool IsMoving { get; private set; }
    #endregion

    #region Shared Abilities
    /// <summary>
    /// Checks if player is able to execute the melee-DPS shared ability: <b>True North</b>
    /// </summary>
    protected bool CanTrueNorth { get; private set; }

    /// <summary>
    /// Checks if player is under the effect of the melee-DPS shared ability: <b>True North</b>
    /// </summary>
    protected bool HasTrueNorth { get; private set; }

    /// <summary>
    /// Checks if player is able to execute the caster-DPS shared ability: <b>Swiftcast</b>
    /// </summary>
    protected bool CanSwiftcast { get; private set; }

    /// <summary>
    /// Checks if player is under the effect of the caster-DPS shared ability: <b>Swiftcast</b>
    /// </summary>
    protected bool HasSwiftcast { get; private set; }

    /// <summary>
    /// Checks if player is able to execute the ranged-DPS shared ability: <b>Peloton</b>
    /// </summary>
    protected bool CanPeloton { get; private set; }

    /// <summary>
    /// Checks if player is under the effect of the ranged-DPS shared ability: <b>Peloton</b>
    /// </summary>
    protected bool HasPeloton { get; private set; }
    #endregion

    public sealed override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        NextGCD = default;
        NextGCDPrio = 0;
        HP = Player.HPMP.CurHP;
        MP = Player.HPMP.CurMP;
        Shield = Player.HPMP.Shield;
        PlayerTarget = Hints.FindEnemy(primaryTarget);
        AnimationLockDelay = estimatedAnimLockDelay;
        IsMoving = isMoving;
        DowntimeIn = Manager.Planner?.EstimateTimeToNextDowntime().Item2 ?? float.MaxValue;
        CombatTimer = (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds;
        CountdownRemaining = World.Client.CountdownRemaining;
        CanTrueNorth = ActionUnlocked(ActionID.MakeSpell(ClassShared.AID.TrueNorth)) && World.Client.Cooldowns[ActionDefinitions.Instance.Spell(ClassShared.AID.TrueNorth)!.MainCooldownGroup].Remaining < 45.6f;
        HasTrueNorth = StatusRemaining(Player, ClassShared.SID.TrueNorth, 15) > 0.1f;
        CanSwiftcast = ActionUnlocked(ActionID.MakeSpell(ClassShared.AID.Swiftcast)) && World.Client.Cooldowns[ActionDefinitions.Instance.Spell(ClassShared.AID.Swiftcast)!.MainCooldownGroup].Remaining < 0.6f;
        HasSwiftcast = StatusRemaining(Player, ClassShared.SID.Swiftcast, 10) > 0.1f;
        CanPeloton = !Player.InCombat && ActionUnlocked(ActionID.MakeSpell(ClassShared.AID.Peloton)) && World.Client.Cooldowns[ActionDefinitions.Instance.Spell(ClassShared.AID.Peloton)!.MainCooldownGroup].Remaining < 0.6f;
        HasPeloton = PlayerHasAnyEffect(ClassShared.SID.Peloton);

        if (Player.MountId is not (103 or 117 or 128))
            Execution(strategy, PlayerTarget);
    }

    /// <summary>
    /// The core function responsible for orchestrating the execution of all abilities and strategies.<br/>
    /// </summary>
    /// <param name="strategy">The user's specified <b>Strategy</b>.</param>
    /// <param name="primaryTarget">The user's specified <b>Enemy</b>.</param>
    /// <returns>- <b>Primary execution</b> of user's rotation module.</returns>
    public abstract void Execution(StrategyValues strategy, Enemy? primaryTarget);
}

static class ModuleExtensions
{
    #region Shared Definitions
    /// <summary>Defines our shared <b>AOE</b> (rotation) and <b>Hold</b> strategies.</summary>
    /// <param name="res">The definitions of our base module's strategies.</param>
    /// <returns>- Options for shared custom strategies to be used via <b>AutoRotation</b> or <b>Cooldown Planner</b></returns>
    public static RotationModuleDefinition DefineShared(this RotationModuleDefinition res)
    {
        res.Define(SharedTrack.AOE).As<AOEStrategy>("AOE", uiPriority: 300)
            .AddOption(AOEStrategy.Automatic, "Auto", "Automatically execute optimal rotation based on targets", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceST, "ForceST", "Force-execute Single Target", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceAOE, "ForceAOE", "Force-execute AOE rotation", supportedTargets: ActionTargets.Hostile);

        res.Define(SharedTrack.Hold).As<HoldStrategy>("Hold", uiPriority: 290)
            .AddOption(HoldStrategy.DontHold, "DontHold", "Allow use of all cooldowns, buffs, or gauge abilities")
            .AddOption(HoldStrategy.HoldCooldowns, "Hold", "Forbid use of all cooldowns only")
            .AddOption(HoldStrategy.HoldGauge, "HoldGauge", "Forbid use of all gauge abilities only")
            .AddOption(HoldStrategy.HoldBuffs, "HoldBuffs", "Forbid use of all raidbuffs or buff-related abilities only")
            .AddOption(HoldStrategy.HoldEverything, "HoldEverything", "Forbid use of all cooldowns, buffs, and gauge abilities");
        return res;
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
    /// <returns>- Basic GCD options for any specified ability to be used via <b>AutoRotation</b> or <b>Cooldown Planner</b></returns>
    public static RotationModuleDefinition.ConfigRef<GCDStrategy> DefineGCD<Index, AID>(this RotationModuleDefinition res, Index track, AID aid, string internalName, string displayName = "", int uiPriority = 100, float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100)
        where Index : Enum
        where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        return res.Define(track).As<GCDStrategy>(internalName, displayName: displayName, uiPriority: uiPriority)
            .AddOption(GCDStrategy.Automatic, "Auto", $"Automatically use {action.Name()} when optimal", cooldown, effectDuration, supportedTargets, minLevel, maxLevel)
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
    /// <returns>- Basic OGCD options for any specified ability to be used via <b>AutoRotation</b> or <b>Cooldown Planner</b></returns>
    public static RotationModuleDefinition.ConfigRef<OGCDStrategy> DefineOGCD<Index, AID>(this RotationModuleDefinition res, Index track, AID aid, string internalName, string displayName = "", int uiPriority = 100, float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100)
        where Index : Enum
        where AID : Enum
    {
        var action = ActionID.MakeSpell(aid);
        return res.Define(track).As<OGCDStrategy>(internalName, displayName: displayName, uiPriority: uiPriority)
            .AddOption(OGCDStrategy.Automatic, "Auto", $"Automatically use {action.Name()} when optimal", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.Force, "Force", $"Force use {action.Name()} ASAP", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.AnyWeave, "AnyWeave", $"Force use {action.Name()} in next possible weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.EarlyWeave, "EarlyWeave", $"Force use {action.Name()} in next possible early-weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.LateWeave, "LateWeave", $"Force use {action.Name()} in next possible late-weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.Delay, "Delay", $"Do NOT use {action.Name()}", 0, 0, ActionTargets.None, minLevel: minLevel, maxLevel)
            .AddAssociatedActions(aid);
    }
    #endregion

    #region Global Helpers
    /// <summary>A global helper for automatically executing the best optimal rotation. See <seealso cref="AOEStrategy"/> for more details.</summary>
    /// <returns>- <c><b>TRUE</b></c> if <seealso cref="AOEStrategy"/> is set to <seealso cref="AOEStrategy.Automatic"/>
    /// <para>- <c><b>FALSE</b></c> if set to any other option</para></returns>
    public static bool Automatic(this StrategyValues strategy) => strategy.Option(SharedTrack.AOE).As<AOEStrategy>() is AOEStrategy.Automatic;

    /// <summary>A global helper for force-executing the single-target rotation. See <seealso cref="AOEStrategy"/> for more details.</summary>
    /// <returns>- <c><b>TRUE</b></c> if <seealso cref="AOEStrategy"/> is set to <seealso cref="AOEStrategy.ForceST"/>
    /// <para>- <c><b>FALSE</b></c> if set to any other option</para></returns>
    public static bool ForceST(this StrategyValues strategy) => strategy.Option(SharedTrack.AOE).As<AOEStrategy>() is AOEStrategy.ForceST;

    /// <summary>A global helper for force-executing the AOE rotation. See <seealso cref="AOEStrategy"/> for more details.</summary>
    /// <returns>- <c><b>TRUE</b></c> if <seealso cref="AOEStrategy"/> is set to <seealso cref="AOEStrategy.ForceAOE"/>
    /// <para>- <c><b>FALSE</b></c> if set to any other option</para></returns>
    public static bool ForceAOE(this StrategyValues strategy) => strategy.Option(SharedTrack.AOE).As<AOEStrategy>() == AOEStrategy.ForceAOE;

    /// <summary>A global helper for forbidding ALL available abilities that are buff, gauge, or cooldown related. See <seealso cref="HoldStrategy"/> for more details.</summary>
    /// <returns>- <c><b>TRUE</b></c> if <seealso cref="HoldStrategy"/> is set to <seealso cref="HoldStrategy.HoldEverything"/>
    /// <para>- <c><b>FALSE</b></c> if set to any other option</para></returns>
    public static bool HoldAll(this StrategyValues strategy) => strategy.Option(SharedTrack.Hold).As<HoldStrategy>() == HoldStrategy.HoldEverything;

    /// <summary>A global helper for forbidding ALL available abilities that are related to raidbuffs. See <seealso cref="HoldStrategy"/> for more details.</summary>
    /// <returns>- <c><b>TRUE</b></c> if <seealso cref="HoldStrategy"/> is set to <seealso cref="HoldStrategy.HoldBuffs"/>
    /// <para>- <c><b>FALSE</b></c> if set to any other option</para></returns>
    public static bool HoldBuffs(this StrategyValues strategy) => strategy.Option(SharedTrack.Hold).As<HoldStrategy>() == HoldStrategy.HoldBuffs;

    /// <summary>A global helper for forbidding ALL available abilities that have any sort of cooldown attached to it. See <seealso cref="HoldStrategy"/> for more details.</summary>
    /// <returns>- <c><b>TRUE</b></c> if <seealso cref="HoldStrategy"/> is set to <seealso cref="HoldStrategy.HoldCooldowns"/>
    /// <para>- <c><b>FALSE</b></c> if set to any other option</para></returns>
    public static bool HoldCDs(this StrategyValues strategy) => strategy.Option(SharedTrack.Hold).As<HoldStrategy>() == HoldStrategy.HoldCooldowns;

    /// <summary>A global helper for forbidding ALL available abilities that are related to the job's gauge. See <seealso cref="HoldStrategy"/> for more details.</summary>
    /// <returns>- <c><b>TRUE</b></c> if <seealso cref="HoldStrategy"/> is set to <seealso cref="HoldStrategy.HoldGauge"/>
    /// <para>- <c><b>FALSE</b></c> if set to any other option</para></returns>
    public static bool HoldGauge(this StrategyValues strategy) => strategy.Option(SharedTrack.Hold).As<HoldStrategy>() == HoldStrategy.HoldGauge;

    /// <summary>A global helper for allowing ALL available abilities that are buff, gauge, or cooldown related. This is the default option for this strategy. See <seealso cref="HoldStrategy"/> for more details.</summary>
    /// <returns>- <c><b>TRUE</b></c> if <seealso cref="HoldStrategy"/> is set to <seealso cref="HoldStrategy.DontHold"/>
    /// <para>- <c><b>FALSE</b></c> if set to any other option</para></returns>
    public static bool DontHold(this StrategyValues strategy) => strategy.Option(SharedTrack.Hold).As<HoldStrategy>() == HoldStrategy.DontHold;
    #endregion
}
