using static BossMod.AIHints;

namespace BossMod.Autorotation.Standard.akechi;
/// <summary>
/// The <em>SharedTrack</em> enum used for <em>AOE</em> and <em>Hold</em> strategies, typically for modules featuring damage rotations.
/// <br>This enum defines tracks that can be used for all PvE-related abilities, such as strategies containing standard rotations or holding abilities.</br>
/// <para><em>Example Given:</em>
/// <br>- <c>public enum Track { NoMercy = SharedTrack.Count }</c></br></para>
/// <para><em>Explanation:</em>
/// <br>- <b><em> Track</em></b> is the enum for tracking specific abilities on user's specific rotation module.</br>
/// <br>- <b><em> NoMercy</em></b> is the example enum of a specific ability being tracked on user's specific rotation module.</br>
/// <br>- <b><em> SharedTrack.Count</em></b> is the shared track being used for executing our shared strategies listed above, called using <seealso cref="AkechiTools{AID, TraitID}"/>.</br></para>
/// </summary>
/// <returns>- All strategies listed under <seealso cref="SharedTrack"/> into user's rotation module</returns>
public enum SharedTrack
{
    /// <summary>
    /// The main strategy for tracking <em>single-target</em> and <em>AOE</em> rotations.
    /// </summary>
    /// <returns>- All strategies listed under <seealso cref="AOEStrategy"/> into user's rotation module</returns>
    AOE,

    /// <summary>
    /// The main strategy used for tracking when to <em>hold any buffs, gauge, or cooldown abilties</em> for optimal usage.
    /// </summary>
    /// <returns>- All strategies listed under <seealso cref="HoldStrategy"/> into user's rotation module</returns>
    Hold,

    /// <summary>
    /// Represents the total count of strategies available inside this specific track. We generally never actually use this as a strategy since there isn't any logic really linked to this besides the counting.
    /// </summary>
    Count
}

/// <summary>
/// The <em>Default Strategy</em> enum used for tracking <em>single-target</em> and <em>AOE</em> strategies.<para/>
/// <em>Example Given:</em><br/>
/// - <c>strategy.Option(SharedTrack.AOE).As&lt;<seealso cref="AOEStrategy"/>&gt;();</c><para/>
/// <em>Explanation:</em><br/>
/// - <b><em>'strategy'</em></b> is the parameter for tracking a specific strategy for a specific ability in the user's rotation module.<br/>
/// - <b><em>'Option'</em></b> are the relative options for user's specific strategy.<br/>
/// - <b><em>'(SharedTrack.AOE)'</em></b> is the enum representing all options relating to this custom strategy being tracked in the user's rotation module.<br/>
/// - <b><em>'.As&lt;<seealso cref="AOEStrategy"/>&gt;();'</em></b> is the relative strategy for user's specific ability being tracked in the user's rotation module.
/// </summary>
/// <returns>- All strategies listed under <seealso cref="AOEStrategy"/> into user's rotation module</returns>
public enum AOEStrategy
{
    /// <summary>
    /// The default strategy used for <em>automatically executing</em> the rotation.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.Automatic(StrategyValues),"/>, or also as `strategy.Automatic()` in some cases.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(SharedTrack.AOE).As&lt;<seealso cref="AOEStrategy"/>&gt;() == <seealso cref="Automatic"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(SharedTrack.AOE).As&lt;<seealso cref="AOEStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="AOEStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="Automatic"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The most optimal rotation <em>automatically executed</em>.</returns>
    Automatic,

    /// <summary>
    /// The main strategy used for <em>force-executing</em> the <em>single-target</em> rotation.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.ForceST(StrategyValues),"/>, or also as `strategy.ForceST()` in some cases.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(SharedTrack.AOE).As&lt;<seealso cref="AOEStrategy"/>&gt;() == <seealso cref="ForceST"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(SharedTrack.AOE).As&lt;<seealso cref="AOEStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="AOEStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="ForceST"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The single-target rotation <em>force-executed</em>, regardless of any conditions.</returns>
    ForceST,

    /// <summary>
    /// The main strategy used for <em>force-executing</em> the <em>AOE</em> rotation.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.ForceAOE(StrategyValues),"/>, or also as `strategy.ForceAOE()` in some cases.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(SharedTrack.AOE).As&lt;<seealso cref="AOEStrategy"/>&gt;() == <seealso cref="ForceAOE"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(SharedTrack.AOE).As&lt;<seealso cref="AOEStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="AOEStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="ForceAOE"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The AOE rotation <em>force-executed</em>, regardless of any conditions.</returns>
    ForceAOE
}

/// <summary>
/// The <em>Default Strategy</em> enum used for tracking when to <em>hold any buffs, gauge, or cooldown abilties</em> for optimal usage.<para/>
/// <em>Example Given:</em><br/>
/// - <c>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;();</c><para/>
/// <em>Explanation:</em><br/>
/// - <b><em>'strategy'</em></b> is the parameter for tracking a specific strategy for a specific ability in the user's rotation module.<br/>
/// - <b><em>'Option'</em></b> are the relative options for user's specific strategy.<br/>
/// - <b><em>'(SharedTrack.Hold)'</em></b> is the enum representing all options relating to this custom strategy being tracked in the user's rotation module.<br/>
/// - <b><em>'.As&lt;<seealso cref="HoldStrategy"/>&gt;();'</em></b> is the relative strategy for user's specific ability being tracked in the user's rotation module.
/// </summary>
/// <returns>- All strategies listed under <seealso cref="HoldStrategy"/> into user's rotation module</returns>
public enum HoldStrategy
{
    /// <summary>
    /// The default strategy used for <em>not holding any buffs, gauge, or cooldown abilties</em>.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.DontHold(StrategyValues),"/>, or also as `strategy.DontHold()` in some cases.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;() == <seealso cref="DontHold"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="HoldStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="DontHold"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The availability of <em>using all buffs, gauge, or cooldown abilties</em>.</returns>
    DontHold,

    /// <summary>
    /// The main strategy used for <em>only holding</em> any ability that is <b>cooldown</b>-related.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.HoldCDs(StrategyValues),"/>, or also as `strategy.HoldCDs()` in some cases.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;() == <seealso cref="HoldCooldowns"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="HoldStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="HoldCooldowns"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The availability of <em>using all buffs and gauge abilties</em>, but <b>forbidden from using any cooldowns</b>.</returns>
    HoldCooldowns,

    /// <summary>
    /// The main strategy used for <em>only holding</em> any ability that is <b>gauge</b>-related.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.HoldGauge(StrategyValues),"/>, or also as `strategy.HoldGauge()` in some cases.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;() == <seealso cref="HoldGauge"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="HoldStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="HoldGauge"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The availability of <em>using all buffs and cooldowns</em>, but <b>forbidden from using any gauge abilties</b>.</returns>
    HoldGauge,

    /// <summary>
    /// The main strategy used for <em>only holding</em> any ability that is <b>buff</b>-related.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.HoldBuffs(StrategyValues),"/>, or also as `strategy.HoldBuffs()` in some cases.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;() == <seealso cref="HoldBuffs"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="HoldStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="HoldBuffs"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The availability of <em>using all cooldowns and gauge abilties</em>, but <b>forbidden from using any buffs</b>.</returns>
    HoldBuffs,

    /// <summary>
    /// The main strategy used for <em>holding</em> any ability that is <b>buff, cooldown, or gauge</b>-related.<br/>
    /// This can also be called using <seealso cref="ModuleExtensions.HoldAll(StrategyValues),"/>, or also as `strategy.HoldAll()` in some cases.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;() == <seealso cref="HoldEverything"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(SharedTrack.Hold).As&lt;<seealso cref="HoldStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="HoldStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="HoldEverything"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- The forbiddance from using all <b>buffs, cooldowns and gauge</b> abilties.</returns>
    HoldEverything
}

/// <summary>
/// The <em>Default Strategy</em> enum used for allowing or forbidding use of module-specific GCD abilities.<para/>
/// <em>Example Given:</em><br/>
/// - <c>strategy.Option(Track.SonicBreak).As&lt;<seealso cref="GCDStrategy"/>&gt;()</c><para/>
/// <em>Explanation:</em><br/>
/// - <b><em>'strategy'</em></b> is the parameter for tracking a specific strategy for a specific ability in the user's rotation module.<br/>
/// - <b><em>'Option'</em></b> are the relative options for user's specific strategy.<br/>
/// - <b><em>'(Track.SonicBreak)'</em></b> is the user's module-specific GCD ability enum being tracked.<br/>
/// - <b><em>'.As&lt;<seealso cref="GCDStrategy"/>&gt;()'</em></b> is the relative Default strategy for user's module-specific GCD ability being tracked in the user's rotation module.
/// </summary>
/// <returns>- All strategies listed under <seealso cref="GCDStrategy"/>.</returns>
public enum GCDStrategy
{
    /// <summary>
    /// The default strategy used for automatically executing user's module-specific GCD ability.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(Track.SonicBreak).As&lt;<seealso cref="GCDStrategy"/>&gt;() == <seealso cref="Automatic"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(Track.SonicBreak).As&lt;<seealso cref="GCDStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="GCDStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="Automatic"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- Automatic execution of ability based on user's logic.</returns>
    Automatic,

    /// <summary>
    /// The main strategy used for force-executing user's module-specific GCD ability.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(Track.SonicBreak).As&lt;<seealso cref="GCDStrategy"/>&gt;() == <seealso cref="Force"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(Track.SonicBreak).As&lt;<seealso cref="GCDStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="GCDStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="Force"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- Forced execution of user's module-specific GCD ability.</returns>
    Force,

    /// <summary>
    /// The main strategy used for forbidding use of user's module-specific GCD ability.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(Track.SonicBreak).As&lt;<seealso cref="GCDStrategy"/>&gt;() == <seealso cref="Delay"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(Track.SonicBreak).As&lt;<seealso cref="GCDStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="GCDStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="Delay"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- Forbiddance of execution for user's module-specific GCD ability.</returns>
    Delay
}

/// <summary>
/// The <em>Default Strategy</em> enum used for allowing or forbidding use of module-specific GCD abilities.<para/>
/// <em>Example Given:</em><br/>
/// - <c>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;()</c><para/>
/// <em>Explanation:</em><br/>
/// - <b><em>'strategy'</em></b> is the parameter for tracking a specific strategy for a specific ability in the user's rotation module.<br/>
/// - <b><em>'Option'</em></b> are the relative options for user's specific strategy.<br/>
/// - <b><em>'(Track.NoMercy)'</em></b> is the user's module-specific OGCD ability enum being tracked.<br/>
/// - <b><em>'.As&lt;<seealso cref="OGCDStrategy"/>&gt;()'</em></b> is the relative Default strategy for user's module-specific OGCD ability being tracked in the user's rotation module.
/// </summary>
/// <returns>- All strategies listed under <seealso cref="OGCDStrategy"/>.</returns>
public enum OGCDStrategy
{
    /// <summary>
    /// The default strategy used for automatically executing user's module-specific OGCD ability.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;() == <seealso cref="Automatic"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="OGCDStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="Automatic"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- Automatic execution of ability based on user's logic.</returns>
    Automatic,

    /// <summary>
    /// The main strategy used for force-executing user's module-specific OGCD ability.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;() == <seealso cref="Force"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="OGCDStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="Force"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- Forced execution of user's module-specific OGCD ability.</returns>
    Force,

    /// <summary>
    /// The main strategy used for force-executing user's module-specific OGCD ability in the very next weave window.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;() == <seealso cref="AnyWeave"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="OGCDStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="AnyWeave"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- Forced execution of user's module-specific OGCD ability inside next weave.</returns>
    AnyWeave,

    /// <summary>
    /// The main strategy used for force-executing user's module-specific OGCD ability in the first-half of very next weave window.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;() == <seealso cref="EarlyWeave"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="OGCDStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="EarlyWeave"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- Forced execution of user's module-specific OGCD ability inside next early-weave.</returns>
    EarlyWeave,

    /// <summary>
    /// The main strategy used for force-executing user's module-specific OGCD ability in the second-half of very next weave window.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;() == <seealso cref="LateWeave"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="OGCDStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="LateWeave"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- Forced execution of user's module-specific OGCD ability inside next late-weave.</returns>
    LateWeave,

    /// <summary>
    /// The main strategy used for forbidding use of user's module-specific OGCD ability.<para/>
    /// <em>Example Given:</em><br/>
    /// - <c>strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;() == <seealso cref="Delay"/></c><para/>
    /// <em>Explanation:</em><br/>
    /// - <b><em>'strategy.Option(Track.NoMercy).As&lt;<seealso cref="OGCDStrategy"/>&gt;()'</em></b> is the full function (or <c>local variable</c> if user desires) for calling <seealso cref="OGCDStrategy"/>.<br/>
    /// - <b><em>'<seealso cref="Delay"/>'</em></b> is the chosen option for this specific strategy.<br/>
    /// </summary>
    /// <returns>- Forbiddance of execution for user's module-specific OGCD ability.</returns>
    Delay
}

/// <summary>The core foundation of how we execute everything, from queuing GCDs to implementing our rotation helpers, functions, and tools.<br/> This base provides a robust framework equipped with a comprehensive suite of functions designed to streamline optimization and simplify the creation of advanced rotation modules.</summary>
/// <typeparam name="AID">The user's specified <em>Action ID</em> being checked, called by <b>using BossMod.(class/job acronym)</b></typeparam>
/// <typeparam name="TraitID">The user's specified <em>Trait ID</em> being checked, called by <b>using BossMod.(class/job acronym)</b></typeparam>
/// <param name="manager">The specified <em>Rotation Module Manager</em> being used.</param>
/// <param name="player">The <em>User</em> that is executing this module.</param>
public abstract class AkechiTools<AID, TraitID>(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
        where AID : struct, Enum where TraitID : Enum
{
    #region Core Ability Execution
    /// <summary>The <em>Next GCD</em> being queued.</summary>
    protected AID NextGCD;

    /// <summary>The <em>Next GCD</em> being queued's <em>Priority</em>.</summary>
    protected int NextGCDPrio;

    #region Queuing
    #region GCD
    /// <summary>
    /// The primary helper we use for calling all our <b>GCD</b> abilities onto any actor.
    /// <br>This also handles <em>Ground Target</em> abilities, such as <c>BLM:LeyLines</c> or <c>NIN:Shukuchi</c></br>
    /// </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <param name="target">The user's specified <em>Target</em>.</param>
    /// <param name="priority">The user's specified <em>Priority</em>.</param>
    /// <param name="delay">The user's specified <em>application delay</em>.</param>
    /// <param name="castTime">The user's specified <em>cast time</em> for the ability.</param>
    protected void QueueGCD<P>(AID aid, Actor? target, P priority, float delay = 0, float castTime = 0) where P : Enum
    => QueueGCD(aid, target, (int)(object)priority, delay, castTime);

    /// <summary>
    /// The primary helper we use for calling all our <b>GCD</b> abilities onto any enemy.
    /// <br>This also handles <em>Ground Target</em> abilities, such as <c>BLM:LeyLines</c> or <c>NIN:Shukuchi</c></br>
    /// </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <param name="target">The user's specified <em>Target</em>.</param>
    /// <param name="priority">The user's specified <em>Priority</em>.</param>
    /// <param name="delay">The user's specified <em>application delay</em>.</param>
    /// <param name="castTime">The user's specified <em>cast time</em> for the ability.</param>
    protected void QueueGCD<P>(AID aid, Enemy? target, P priority, float delay = 0, float castTime = 0) where P : Enum => QueueGCD(aid, target?.Actor, (int)(object)priority, delay, castTime);
    protected void QueueGCD(AID aid, Enemy? target, int priority = 2, float delay = 0, float castTime = 0) => QueueGCD(aid, target?.Actor, priority, delay, castTime);
    protected void QueueGCD(AID aid, Actor? target, int priority = 2, float delay = 0, float castTime = 0)
    {
        if (priority == 0)
            return;

        if (QueueAction(aid, target, ActionQueue.Priority.High + priority, delay, castTime) && priority > NextGCDPrio)
        {
            NextGCD = aid;
            NextGCDPrio = priority;
        }
    }
    #endregion

    #region OGCD
    /// <summary>
    /// The primary helper we use for calling all our <b>OGCD</b> abilities onto any actor.<br/>
    /// This also handles <em>Ground Target</em> abilities, such as <c>BLM:LeyLines</c> or <c>NIN:Shukuchi</c>
    /// <b>NOTE:</b> For compatibility between <c>Actor?</c> and <c>Enemy?</c> inside one function, use `primarytarget?.Actor` as `Enemy?` definition.
    /// </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <param name="target">The user's specified <em>Target</em>.</param>
    /// <param name="priority">The user's specified <em>Priority</em>.</param>
    /// <param name="delay">The user's specified <em>application delay</em>.</param>
    /// <param name="castTime">The user's specified <em>cast time</em> for the ability.</param>
    protected void QueueOGCD<P>(AID aid, Actor? target, P priority, float delay = 0, float castTime = 0) where P : Enum => QueueOGCD(aid, target, (int)(object)priority, delay, castTime);

    /// <summary>
    /// The primary helper we use for calling all our <b>OGCD</b> abilities onto any enemy.<br/>
    /// This also handles <em>Ground Target</em> abilities, such as <c>BLM:LeyLines</c> or <c>NIN:Shukuchi</c><br/>
    /// <b>NOTE:</b> For compatibility between <c>Actor?</c> and <c>Enemy?</c> inside one function, use `primarytarget?.Actor` as `Enemy?` definition.
    /// </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <param name="target">The user's specified <em>Target</em>.</param>
    /// <param name="priority">The user's specified <em>Priority</em>.</param>
    /// <param name="delay">The user's specified <em>application delay</em>.</param>
    /// <param name="castTime">The user's specified <em>cast time</em> for the ability.</param>
    protected void QueueOGCD<P>(AID aid, Enemy? target, P priority, float delay = 0, float castTime = 0) where P : Enum => QueueOGCD(aid, target?.Actor, (int)(object)priority, delay, castTime);
    protected void QueueOGCD(AID aid, Enemy? target, int priority = 1, float delay = 0, float castTime = 0) => QueueOGCD(aid, target?.Actor, priority, delay, castTime);
    protected void QueueOGCD(AID aid, Actor? target, int priority = 1, float delay = 0, float castTime = 0)
    {
        if (priority == 0)
            return;

        QueueAction(aid, target, ActionQueue.Priority.Low + priority, delay, castTime);
    }
    #endregion

    protected bool QueueAction(AID aid, Actor? target, float priority, float delay, float castTime)
    {
        var def = ActionDefinitions.Instance.Spell(aid);
        Vector3 targetPos = default;

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

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority, delay: delay, castTime: castTime, targetPos: targetPos);
        return true;
    }
    #endregion

    #endregion

    #region HP/MP/Shield
    /// <summary>A quick and easy helper for retrieving the <em>current HP value</em> of the player.</summary>
    /// <returns>- A <em>value</em> representing the Player's <em>current HP</em></returns>
    protected uint HP { get; private set; }
    /// <summary>A quick and easy helper for retrieving the <em>MP value</em> of the player.</summary>
    /// <returns>- A <em>value</em> representing the Player's <em>current MP</em></returns>
    protected uint MP { get; private set; }

    /// <summary>A quick and easy helper for retrieving the <em>Shield value</em> of the player.</summary>
    /// <returns>- A <em>value</em> representing the Player's <em>current Shield</em></returns>
    protected uint Shield { get; private set; }

    /// <summary>A quick and easy helper for retrieving the <em>Current HP</em> of any specified actor, whether it is the player or any other target user desires.</summary>
    /// <param name="actor">Any specified player, ally, or target</param>
    /// <returns>- The current HP value of user's specified actor</returns>
    protected uint TargetCurrentHP(Actor actor) => actor.HPMP.CurHP;

    /// <summary>A quick and easy helper for retrieving the <em>Current Shield</em> of any specified actor, whether it is the player or any other target user desires.</summary>
    /// <param name="actor">Any specified player, ally, or target</param>
    /// <returns>- The current shield value of user's specified actor</returns>
    protected uint TargetCurrentShield(Actor actor) => actor.HPMP.Shield;

    /// <summary>A quick and easy helper for checking if any specified actor has any urrent shield value active.</summary>
    /// <param name="actor">Any specified player, ally, or target</param>
    /// <returns>- The current shield value of user's specified actor</returns>
    protected bool TargetHasShield(Actor actor) => actor.HPMP.Shield > 0.1f;
    #endregion

    #region Actions
    /// <summary>
    /// Checks if action is <em>Unlocked</em> based on <em>Level</em> and <em>Job Quest</em> (if required)
    /// <para><b><em>Example Given</em></b>: <em>Unlocked(AID.GnashingFang)</em></para>
    /// <para><b>Explanation</b>: <b><em>Unlocked</em></b> is the function. <b><em>AID.GnashingFang</em></b> is the ability being checked , called <em>using BossMod.GNB</em>.</para>
    /// </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <returns>- <em><b>TRUE</b></em> if the <em>specified Action is Unlocked</em> <para>- <em><b>FALSE</b></em> if <em>still locked</em> or <em>Job Quest unfinished</em></para></returns>
    protected bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid)); //Check if the desired ability is unlocked

    /// <summary>
    /// Checks if Trait is <em>Unlocked</em> based on Level and Job Quest (if required)
    /// <para><b><em>Example Given</em></b>: <em>Unlocked(TraitID.EnhancedBloodfest)</em></para>
    /// <para><b>Explanation</b>: <b><em>Unlocked</em></b> is the function. <b><em>TraitID.EnhancedBloodfest</em></b> is the trait being checked, called <em>using BossMod.GNB</em>.</para>
    /// </summary>
    /// <param name="tid"> The user's specified <em>Trait ID</em> being checked.</param>
    /// <returns>- <em><b>TRUE</b></em> if the <em>specified Trait is Unlocked</em> <para>- <em><b>FALSE</b></em> if <em>still locked</em> or <em>Job Quest unfinished</em></para></returns>
    protected bool Unlocked(TraitID tid) => TraitUnlocked((uint)(object)tid);

    /// <summary>
    /// <para>Checks if <em>last combo action</em> is what the user is specifying.</para>
    /// <para><b><em>NOTE</em></b>: This does <em><b>NOT</b></em> check all actions, only combo actions.</para>
    /// <para><b><em>Example Given</em></b>: <em>ComboLastMove == AID.BrutalShell</em></para>
    /// <para><b>Explanation</b>: <b><em>ComboLastMove</em></b> is the function. <b><em>AID.BrutalShell</em></b> is the specified combo action being checked, called <em>using BossMod.GNB</em>.</para>
    /// </summary>
    /// <returns>- <em><b>TRUE</b></em> if the last combo action is what the user is specifying <para>- <em><b>FALSE</b></em> if otherwise or last action was not a combo action</para></returns>
    protected AID ComboLastMove => (AID)(object)World.Client.ComboState.Action;

    /// <summary>
    /// Retrieves <em>actual</em> cast time of a specified action.
    /// <para><b><em>Example Given</em></b>: <em>ActualCastTime(AID.Fire3)</em></para>
    /// <para><b>Explanation</b>: <b><em>ActualCastTime</em></b> is the function. <b><em>AID.Fire3</em></b> is the specified action being checked, called <em>using BossMod.BLM</em>.</para>
    /// </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <returns>- A <em>value</em> representing the current <em>real-time Cast Time</em> of user's specified action</returns>
    protected virtual float ActualCastTime(AID aid) => ActionDefinitions.Instance.Spell(aid)!.CastTime;

    /// <summary>
    /// Retrieves <em>effective</em> cast time of a specified action by calculating the action's base cast time multiplied by the player's spell-speed factor, which accounts for haste buffs (like <em>Ley Lines</em>) and slow debuffs. It also accounts for <em>Swiftcast</em>.
    /// <para><b><em>Example Given</em></b>: <em>EffectiveCastTime(AID.Fire3)</em></para>
    /// <para><b>Explanation</b>: <b><em>EffectiveCastTime</em></b> is the function. <b><em>AID.Fire3</em></b> is the specified action being checked, called <em>using BossMod.BLM</em>.</para>
    /// </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <returns>- A <em>value</em> representing the current <em>effective Cast Time</em> of user's specified action</returns>
    protected virtual float EffectiveCastTime(AID aid) => PlayerHasEffect(ClassShared.SID.Swiftcast, 10) ? 0 : ActualCastTime(aid) * SpSGCDLength / 2.5f;

    /// <summary><para>Retrieves player's GCD length based on <em>Skill-Speed</em>.</para>
    /// <para><b><em>NOTE</em></b>: This function is only recommended for jobs with <em>Skill-Speed</em>. <em>Spell-Speed</em> users are <em>unaffected</em> by this function.</para></summary>
    /// <returns>- A <em>value</em> representing the player's current <em>GCD Length</em></returns>
    protected float SkSGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);

    /// <summary>Retrieves player's current <em>Skill-Speed</em> stat.</summary>
    /// <returns>- A <em>value</em> representing the player's current <em>Skill-Speed</em></returns>
    protected float SkS => ActionSpeed.Round(World.Client.PlayerStats.SkillSpeed);

    /// <summary><para>Retrieves player's GCD length based on <em>Spell-Speed</em>.</para>
    /// <para><b><em>NOTE</em></b>: This function is only recommended for jobs with <em>Spell-Speed</em>. <em>Skill-Speed</em> users are <em>unaffected</em> by this function.</para></summary>
    /// <returns>- A <em>value</em> representing the player's current <em>GCD Length</em></returns>
    protected float SpSGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level);

    /// <summary>Retrieves player's current <em>Spell-Speed</em> stat.</summary>
    /// <returns>- A <em>value</em> representing the player's current <em>Spell-Speed</em></returns>
    protected float SpS => ActionSpeed.Round(World.Client.PlayerStats.SpellSpeed);

    /// <summary>Checks if we can fit in a <em>skill-speed based</em> GCD.</summary>
    /// <param name="duration"> </param>
    /// <param name="extraGCDs"> How many extra GCDs the user can fit in.</param>
    /// <returns>- <em><b>TRUE</b></em> if we can fit in a <em>skill-speed based</em> GCD <para>- <em><b>FALSE</b></em> if otherwise</para></returns>
    protected bool CanFitSkSGCD(float duration, int extraGCDs = 0) => GCD + SkSGCDLength * extraGCDs < duration;

    /// <summary>Checks if we can fit in a <em>spell-speed based</em> GCD.</summary>
    /// <param name="duration"> </param>
    /// <param name="extraGCDs"> How many extra GCDs the user can fit in.</param>
    /// <returns>- <em><b>TRUE</b></em> if we can fit in a <em>spell-speed based</em> GCD <para>- <em><b>FALSE</b></em> if otherwise</para></returns>
    protected bool CanFitSpSGCD(float duration, int extraGCDs = 0) => GCD + SpSGCDLength * extraGCDs < duration;

    /// <summary><para>Checks if player is available to weave in any abilities.</para>
    /// <para><b><em>NOTE</em></b>: This function is only recommended for jobs with <em>Skill-Speed</em>. <em>Spell-Speed</em> users are <em>unaffected</em> by this.</para></summary>
    /// <param name="cooldown"> The cooldown time of the action specified.</param>
    /// <param name="actionLock"> The animation lock time of the action specified.</param>
    /// <param name="extraGCDs"> How many extra GCDs the user can fit in.</param>
    /// <param name="extraFixedDelay"> How much extra delay the user can add in, in seconds.</param>
    /// <returns>- <em><b>TRUE</b></em> if we can weave in a <em>skill-speed based</em> GCD <para>- <em><b>FALSE</b></em> if otherwise</para></returns>
    protected bool CanWeave(float cooldown, float actionLock, int extraGCDs = 0, float extraFixedDelay = 0)
        => MathF.Max(cooldown, World.Client.AnimationLock) + actionLock + AnimationLockDelay <= GCD + SkSGCDLength * extraGCDs + extraFixedDelay;

    /// <summary><para>Checks if player is available to weave in any spells.</para>
    /// <para><b><em>NOTE</em></b>: This function is only recommended for jobs with <em>Spell-Speed</em>. <em>Skill-Speed</em> users are <em>unaffected</em> by this.</para></summary>
    /// <param name="cooldown"> The cooldown time of the action specified.</param>
    /// <param name="actionLock"> The animation lock time of the action specified.</param>
    /// <param name="extraGCDs"> How many extra GCDs the user can fit in.</param>
    /// <param name="extraFixedDelay"> How much extra delay the user can add in, in seconds.</param>
    /// <returns>- <em><b>TRUE</b></em> if we can weave in a <em>spell-speed based</em> GCD <para>- <em><b>FALSE</b></em> if otherwise</para></returns>
    protected bool CanSpellWeave(float cooldown, float actionLock, int extraGCDs = 0, float extraFixedDelay = 0)
        => MathF.Max(cooldown, World.Client.AnimationLock) + actionLock + AnimationLockDelay <= GCD + SpSGCDLength * extraGCDs + extraFixedDelay;

    /// <summary><para>Checks if player is available to weave in any abilities.</para>
    /// <para><b><em>NOTE</em></b>: This function is only recommended for jobs with <em>Skill-Speed</em>. <em>Spell-Speed</em> users are <em>unaffected</em> by this.</para></summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <param name="extraGCDs"> How many extra GCDs the user can fit in.</param>
    /// <param name="extraFixedDelay"> How much extra delay the user can add in, in seconds.</param>
    /// <returns>- <em><b>TRUE</b></em> if we can weave in any abilities <para>- <em><b>FALSE</b></em> if otherwise</para></returns>
    protected bool CanWeave(AID aid, int extraGCDs = 0, float extraFixedDelay = 0)
    {
        if (!Unlocked(aid))
            return false;

        var res = ActionDefinitions.Instance[ActionID.MakeSpell(aid)]!;
        return SkS > 100
            ? CanSpellWeave(ChargeCD(aid), res.InstantAnimLock, extraGCDs, extraFixedDelay)
            : SpS > 100 && CanWeave(ChargeCD(aid), res.InstantAnimLock, extraGCDs, extraFixedDelay);
    }

    /// <summary>Checks if user is in pre-pull stage; useful for <em>First GCD</em> openings.</summary>
    /// <returns>- <em><b>TRUE</b></em> if user is in pre-pull stage or fully not in combat<para>- <em><b>FALSE</b></em> if otherwise</para></returns>
    protected bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f;

    /// <summary>Checks if user can <em>Weave in</em> any abilities.<b><em>NOTE</em></b>: This function is pretty sub-optimal, but gets the job done. <em>CanWeave() </em>is much more intricate if user really wants it.</para></summary>
    protected bool CanWeaveIn => GCD is <= 2.49f and >= 0.01f;

    /// <summary>Checks if user can <em>Early Weave in</em> any abilities.<para><b><em>NOTE</em></b>: This function is pretty sub-optimal, but gets the job done. <em>CanWeave() </em>is much more intricate if user really wants it.</para></summary>
    protected bool CanEarlyWeaveIn => GCD is <= 2.49f and >= 1.26f;

    /// <summary>Checks if user can <em>Late Weave in</em> any abilities.<para><b><em>NOTE</em></b>: This function is pretty sub-optimal, but gets the job done. <em>CanWeave() </em>is much more intricate if user really wants it.</para></summary>
    protected bool CanLateWeaveIn => GCD is <= 1.25f and >= 0.01f;

    /// <summary>Checks if user can <em>Quarter Weave in</em> any abilities.<para><b><em>NOTE</em></b>: This function is pretty sub-optimal, but gets the job done. <em>CanWeave() </em>is much more intricate if user really wants it.</para></summary>
    protected bool CanQuarterWeaveIn => GCD is < 0.9f and >= 0.01f;
    #endregion

    #region Cooldown
    /// <summary> Retrieves the total cooldown time left on the specified action. </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <returns>- A <em>value</em> representing the current <em>cooldown</em> on user's specified <em>Action ID</em></returns>
    protected float TotalCD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    /// <summary> Returns the charge cooldown time left on the specified action. </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <returns>- A <em>value</em> representing the current <em>charge cooldown</em> on user's specified <em>Action ID</em></returns>
    protected float ChargeCD(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) : float.MaxValue;

    /// <summary> Checks if action is ready to be used based on if it's <em>Unlocked</em> and its <em>charge cooldown timer</em>. </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <returns>- <em><b>TRUE</b></em> if the specified Action is <em>Unlocked</em> and <em>off cooldown.</em> <para>- <em><b>FALSE</b></em> if <em>locked</em> or still on <em>cooldown</em></para></returns>
    protected bool ActionReady(AID aid) => Unlocked(aid) && ChargeCD(aid) < 0.6f;

    /// <summary> Checks if action has any charges remaining. </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <returns>- <em><b>TRUE</b></em> if the specified Action has any <em>charges</em> available <para>- <em><b>FALSE</b></em> if locked or still on cooldown</para></returns>
    protected bool HasCharges(AID aid) => ChargeCD(aid) < 0.6f;

    /// <summary>Checks if action is on cooldown based on its <em>total cooldown timer</em>. </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <returns>- <em><b>TRUE</b></em> if the specified Action is <em>still on cooldown</em> <para>- <em><b>FALSE</b></em> if <em>off cooldown</em></para></returns>
    protected bool IsOnCooldown(AID aid) => TotalCD(aid) > 0.6f;

    /// <summary>Checks if action is off cooldown based on its <em>total cooldown timer</em>. </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <returns>- <em><b>TRUE</b></em> if the specified Action is <em>off cooldown</em> <para>- <em><b>FALSE</b></em> if <em>still on cooldown</em></para></returns>
    protected bool IsOffCooldown(AID aid) => !IsOnCooldown(aid);

    /// <summary>Checks if action is off cooldown based on its <em>charge cooldown timer</em>. </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <returns>- <em><b>TRUE</b></em> if the specified Action is <em>off cooldown</em> <para>- <em><b>FALSE</b></em> if <em>still on cooldown</em></para></returns>
    protected bool OnCooldown(AID aid) => MaxChargesIn(aid) > 0;

    /// <summary>Checks if last action used is what the user is specifying and within however long. </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    /// <returns>- <em><b>TRUE</b></em> if the specified Action was <em>just used</em> <para>- <em><b>FALSE</b></em> if was <em>not just used</em></para></returns>
    protected bool LastActionUsed(AID aid) => Manager.LastCast.Data?.IsSpell(aid) == true;

    /// <summary>Retrieves time remaining until specified action is at max charges. </summary>
    /// <param name="aid"> The user's specified <em>Action ID</em> being checked.</param>
    protected float MaxChargesIn(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.ChargeCapIn(World.Client.Cooldowns, World.Client.DutyActions, Player.Level) : float.MaxValue;

    #endregion

    #region Status
    /// <summary> Retrieves the amount of specified status effect's stacks remaining on any target.
    /// <para><b><em>NOTE</em></b>: The effect can be owned by anyone.</para>
    /// <para><em>Example Given:</em> "<em>StacksRemaining(Player, SID.Requiescat, 30) > 0</em>"</para></summary>
    /// <param name="target">The <em>specified Target</em> we're checking for specified status effect. (e.g. "<em>Player</em>")<para>(<b><em>NOTE</em></b>: can also be any target if called)</para> </param>
    /// <param name="sid">The <em>Status ID</em> of specified status effect. (e.g. "<em>SID.Requiescat</em>")</param>
    /// <param name="duration"> The <em>Total Effect Duration</em> of specified status effect. (e.g. since <em>Requiescat</em>'s buff is 30 seconds, we simply use "<em>30</em>")</param>
    /// <returns>- A value indicating how many stacks exist</returns>
    protected int StacksRemaining<SID>(Actor? target, SID sid, float duration = 1000f) where SID : Enum => StatusDetails(target, sid, Player.InstanceID, duration).Stacks;

    /// <summary> Retrieves the amount of specified status effect's time left remaining on any target.
    /// <para><b><em>NOTE</em></b>: The effect can be owned by anyone.</para>
    /// <para><em>Example Given:</em> "<em>StatusRemaining(Player, SID.Requiescat, 30) > 0f</em>"</para></summary>
    /// <param name="target">The <em>specified Target</em> we're checking for specified status effect. (e.g. "<em>Player</em>")<para>(<b><em>NOTE</em></b>: can also be any target if called)</para> </param>
    /// <param name="sid">The <em>Status ID</em> of specified status effect. (e.g. "<em>SID.Requiescat</em>")</param>
    /// <param name="duration"> The <em>Total Effect Duration</em> of specified status effect. (e.g. since <em>Requiescat</em>'s buff is 30 seconds, we simply use "<em>30</em>")</param>
    /// <returns>- A value indicating how much time left on existing effect</returns>
    protected float StatusRemaining<SID>(Actor? target, SID sid, float duration) where SID : Enum => StatusDetails(target, sid, Player.InstanceID, duration).Left;

    /// <summary> Checks if a specific status effect on the player exists.
    /// <para><b><em>NOTE</em></b>: The effect can be owned by anyone.</para>
    /// <para><em>Example Given:</em> "<em>PlayerHasEffect(SID.NoMercy, 20)</em>"</para></summary>
    /// <param name="sid">The <em>Status ID</em> of specified status effect. (e.g. "<em>SID.NoMercy</em>")</param>
    /// <param name="duration"> The <em>Total Effect Duration</em> of specified status effect. (e.g. since <em>No Mercy</em>'s buff is 20 seconds, we simply use "<em>20</em>")</param>
    /// <returns>- A value indicating if the effect exists</returns>
    protected bool PlayerHasEffect<SID>(SID sid, float duration) where SID : Enum => StatusRemaining(Player, sid, duration) > 0.1f;

    /// <summary> Checks if a specific status effect on any specified target exists.
    /// <para><b><em>NOTE</em></b>: The effect can be owned by anyone.</para>
    /// <para><em>Example Given:</em> "<em>TargetHasEffect(primaryTarget, SID.SonicBreak, 30)</em>"</para></summary>
    /// <param name="target">The <em>specified Target</em> we're checking for specified status effect. (e.g. "<em>primaryTarget</em>")<para>(<b><em>NOTE</em></b>: can even be "Player")</para> </param>
    /// <param name="sid">The <em>Status ID</em> of specified status effect. (e.g. "<em>SID.SonicBreak</em>")</param>
    /// <param name="duration"> The <em>Total Effect Duration</em> of specified status effect. (e.g. since <em>Sonic Break</em>'s debuff is 30 seconds, we simply use "<em>30</em>")</param>
    /// <returns>- A value indicating if the effect exists</returns>
    protected bool TargetHasEffect<SID>(Actor? target, SID sid, float duration = 1000f) where SID : Enum => StatusRemaining(target, sid, duration) > 0.1f;

    /// <summary> Checks if Player has any stacks of specific status effect.
    /// <para><b><em>NOTE</em></b>: The effect can be owned by anyone.</para>
    /// <para><em>Example Given:</em> "<em>PlayerHasStacks(SID.Requiescat)</em>"</para></summary>
    /// <param name="sid">The <em>Status ID</em> of specified status effect. (e.g. "<em>SID.Requiescat</em>")</param>
    /// <returns>- A value indicating if the effect exists</returns>
    protected bool PlayerHasStacks<SID>(SID sid) where SID : Enum => StacksRemaining(Player, sid) > 0;

    #endregion

    #region Targeting

    #region Position Checks
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
    protected PositionCheck IsSplashTarget => (primary, other) => Hints.TargetInAOECircle(other, primary.Position, 5);
    /// <summary>
    /// Position checker for determining the best target for an ability that deals damage in a <em>Cone</em> .
    /// </summary>
    protected PositionCheck IsConeTarget => (primary, other) => Hints.TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 45.Degrees());
    /// <summary>
    /// <para>Position checker for determining the best target for an ability that deals damage in a <em>Line</em> within <em>Ten (10) yalms</em>.</para>
    /// </summary>
    protected PositionCheck Is10yRectTarget => (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2);
    /// <summary>
    /// <para>Position checker for determining the best target for an ability that deals damage in a <em>Line</em> within <em>Fifteen (15) yalms</em>.</para>
    /// </summary>
    protected PositionCheck Is15yRectTarget => (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 15, 2);
    /// <summary>
    /// Position checker for determining the best target for an ability that deals damage in a <em>Line</em> within <em>Twenty-five (25) yalms</em>
    /// </summary>
    protected PositionCheck Is25yRectTarget => (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 25, 2);
    #endregion

    /// <summary>
    /// Checks if target is within <em>Zero (0) yalms</em> in distance, or if Player is inside hitbox.
    /// </summary>
    /// <param name="target">The user's specified <em>Target</em>.</param>
    /// <returns></returns>
    protected bool In0y(Actor? target) => Player.DistanceToHitbox(target) <= 0.00f;

    /// <summary>
    /// Checks if target is within <em>Three (3) yalms</em> in distance.
    /// </summary>
    /// <param name="target">The user's specified <em>Target</em>.</param>
    /// <returns></returns>
    protected bool In3y(Actor? target) => Player.DistanceToHitbox(target) <= 2.99f;

    /// <summary>
    /// Checks if target is within <em>Five (5) yalms</em> in distance.
    /// </summary>
    /// <param name="target">The user's specified <em>Target</em>.</param>
    /// <returns></returns>
    protected bool In5y(Actor? target) => Player.DistanceToHitbox(target) <= 4.99f;

    /// <summary>
    /// Checks if target is within <em>Ten (10) yalms</em> in distance.
    /// </summary>
    /// <param name="target">The user's specified <em>Target</em>.</param>
    /// <returns></returns>
    protected bool In10y(Actor? target) => Player.DistanceToHitbox(target) <= 9.99f;

    /// <summary>
    /// Checks if target is within <em>Fifteen (15) yalms</em> in distance.
    /// </summary>
    /// <param name="target">The user's specified <em>Target</em>.</param>
    /// <returns></returns>
    protected bool In15y(Actor? target) => Player.DistanceToHitbox(target) <= 14.99f;

    /// <summary>
    /// Checks if target is within <em>Twenty (20) yalms</em> in distance.
    /// </summary>
    /// <param name="target">The user's specified <em>Target</em>.</param>
    /// <returns></returns>
    protected bool In20y(Actor? target) => Player.DistanceToHitbox(target) <= 19.99f;

    /// <summary>
    /// Checks if target is within <em>Twenty-five (25) yalms</em> in distance.
    /// </summary>
    /// <param name="target">The user's specified <em>Target</em>.</param>
    /// <returns></returns>
    protected bool In25y(Actor? target) => Player.DistanceToHitbox(target) <= 24.99f;

    /// <summary>
    /// <para>A simpler smart-targeting helper for picking a <em>specific</em> target over your current target.</para>
    /// <para>Very useful for intricate planning of ability targeting in specific situations.</para>
    /// </summary>
    /// <param name="track">The user's picked strategy's option <em>Track</em>, retrieved from module's enums and definitions. (e.g. <em>strategy.Option(Track.NoMercy)</em>)</param>
    /// <returns></returns>
    protected Actor? TargetChoice(StrategyValues.OptionRef track) => ResolveTargetOverride(track.Value); //Resolves the target choice based on the strategy

    /// <summary>Targeting function for indicating when or not <em>AOE Circle</em> abilities should be used based on targets nearby.</summary>
    /// <param name="range">The range of the <em>AOE Circle</em> ability, or radius from center of Player; this should be adjusted accordingly to user's module specific to job's abilities.</param>
    /// <returns>- A tuple with the following booleans:
    /// <para><b>-- OnTwoOrMore</b>: A boolean indicating if there are two (2) or more targets inside Player's <em>AOE Circle</em>.</para>
    /// <para><b>-- OnThreeOrMore</b>: A boolean indicating if there are three (3) or more targets inside Player's <em>AOE Circle</em>.</para>
    /// <para><b>-- OnFourOrMore</b>: A boolean indicating if there are four (4) or more targets inside Player's <em>AOE Circle</em>.</para>
    /// <para><b>-- OnFiveOrMore</b>: A boolean indicating if there are five (5) or more targets inside Player's <em>AOE Circle</em>.</para></returns>
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
    /// <param name="strategy">The user's picked <em>Strategy</em></param>
    /// <param name="primaryTarget">The user's current <em>picked Target</em>.</param>
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
    /// <param name="strategy">The user's picked <em>Strategy</em></param>
    /// <param name="primaryTarget">The user's current <em>picked Target</em>.</param>
    /// <param name="range"></param>
    /// <param name="isInAOE"></param>
    /// <returns></returns>
    protected (Enemy? Best, int Targets) GetBestTarget(Enemy? primaryTarget, float range, PositionCheck isInAOE) => GetTarget(primaryTarget, range, isInAOE, (numTargets, _) => numTargets, a => a);

    /// <summary>
    /// This function picks the target based on HP, modified by how many targets are in the AOE.
    /// </summary>
    /// <param name="strategy"></param>
    /// <param name="primaryTarget">The user's current <em>picked Target</em>.</param>
    /// <param name="range"></param>
    /// <param name="isInAOE"></param>
    /// <returns></returns>
    protected (Enemy? Best, int Targets) GetTargetByHP(Enemy? primaryTarget, float range, PositionCheck isInAOE) => GetTarget(primaryTarget, range, isInAOE, (numTargets, enemy) => (numTargets, numTargets > 2 ? enemy.HPMP.CurHP : 0), args => args.numTargets);

    /// <summary>
    /// Main function for picking a target, generalized for any prioritization and simplification logic.
    /// </summary>
    /// <typeparam name="P"></typeparam>
    /// <param name="strategy"></param>
    /// <param name="primaryTarget">The user's current <em>picked Target</em>.</param>
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
    /// Identify an appropriate target for applying <em>DoT</em> effect. This has no impact if any <em>auto-targeting</em> is disabled.
    /// </summary>
    /// <typeparam name="P"></typeparam>
    /// <param name="strategy"></param>
    /// <param name="initial"></param>
    /// <param name="getTimer"></param>
    /// <param name="maxAllowedTargets"></param>
    /// <returns></returns>
    protected (Enemy? Target, P Timer) GetDOTTarget<P>(StrategyValues strategy, Enemy? initial, Func<Actor?, P> getTimer, int maxAllowedTargets) where P : struct, IComparable
    {
        var AOEStrat = strategy.Option(SharedTrack.AOE).As<AOEStrategy>();
        switch (AOEStrat)
        {
            case AOEStrategy.ForceST:
            case AOEStrategy.ForceAOE:
            case AOEStrategy.Automatic:
                return (initial, getTimer(initial?.Actor));
        }

        var newTarget = initial;
        var initialTimer = getTimer(initial?.Actor);
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

    /// <summary>
    /// Checks if target is valid. (e.g. not forbidden or a party member)
    /// </summary>
    protected static bool IsValidEnemy(Actor? actor) => actor != null && !actor.IsAlly;
    #endregion

    #region Positionals
    /// <summary>
    /// Retrieves the current positional of the target based on target's position and rotation.
    /// </summary>
    /// <param name="target">The user's specified <em>Target</em>.</param>
    /// <returns></returns>
    protected Positional GetCurrentPositional(Actor target) => (Player.Position - target.Position).Normalized().Dot(target.Rotation.ToDirection()) switch //Check current positional based on target
    {
        < -0.7071068f => Positional.Rear, //value for Rear positional
        < 0.7071068f => Positional.Flank, //value for Flank positional
        _ => Positional.Front //default to Front positional if not on Rear or Flank
    };

    /// <summary>
    /// Checks if player is on specified target's <em>Rear Positional</em>.
    /// </summary>
    /// <param name="target">The user's specified <em>Target</em>.</param>
    /// <returns></returns>
    protected bool IsOnRear(Actor target) => GetCurrentPositional(target) == Positional.Rear;

    /// <summary>
    /// Checks if player is on specified target's <em>Flank Positional</em>.
    /// </summary>
    /// <param name="target">The user's specified <em>Target</em>.</param>
    /// <returns></returns>
    protected bool IsOnFlank(Actor target) => GetCurrentPositional(target) == Positional.Flank;

    /// <summary>
    /// Finds the <em>best Positional</em> automatically.
    /// </summary>
    ///
    #endregion

    #region AI
    protected void GoalZoneSingle(float range)
    {
        if (PlayerTarget != null)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(PlayerTarget.Actor, range));
    }

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
    #endregion

    #region Misc
    /// <summary>Estimates the delay caused by <em>animation lock</em>.</summary>
    /// <returns>- A <em>value</em> representing the current <em>Animation Lock Delay</em></returns>
    protected float AnimationLockDelay { get; private set; }

    /// <summary>Estimates the time remaining until the <em>next Down-time phase</em>.</summary>
    protected float DowntimeIn { get; private set; }

    /// <summary>Elapsed time in <em>seconds</em> since the start of combat.</summary>
    protected float CombatTimer { get; private set; }

    /// <summary>Time remaining on pre-pull (or any) <em>Countdown Timer</em>.</summary>
    protected float? CountdownRemaining { get; private set; }

    protected bool IsMoving { get; private set; }
    #endregion

    #region Shared Abilities
    /// <summary>
    /// Checks if player is able to execute the melee-DPS shared ability: <em>True North</em>
    /// </summary>
    protected bool CanTrueNorth { get; private set; }

    /// <summary>
    /// Checks if player is under the effect of the melee-DPS shared ability: <em>True North</em>
    /// </summary>
    protected bool HasTrueNorth { get; private set; }

    /// <summary>
    /// Checks if player is able to execute the caster-DPS shared ability: <em>Swiftcast</em>
    /// </summary>
    protected bool CanSwiftcast { get; private set; }

    /// <summary>
    /// Checks if player is under the effect of the caster-DPS shared ability: <em>Swiftcast</em>
    /// </summary>
    protected bool HasSwiftcast { get; private set; }

    /// <summary>
    /// Checks if player is able to execute the ranged-DPS shared ability: <em>Peloton</em>
    /// </summary>
    protected bool CanPeloton { get; private set; }

    /// <summary>
    /// Checks if player is under the effect of the ranged-DPS shared ability: <em>Peloton</em>
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
        HasPeloton = StatusRemaining(Player, ClassShared.SID.Peloton, 30) > 0.1f;

        if (Player.MountId is not (103 or 117 or 128))
            Execution(strategy, PlayerTarget);
    }
    public abstract void Execution(StrategyValues strategy, Enemy? primaryTarget);
}

static class ModuleExtensions
{
    /// <summary>Defines our shared <em>AOE</em> (rotation) and <em>Hold</em> strategies.</summary>
    /// <param name="res">The definitions of our base module's strategies.</param>
    /// <returns>- Options for shared custom strategies to be used via <em>AutoRotation</em> or <em>Cooldown Planner</em></returns>
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
            .AddOption(HoldStrategy.HoldEverything, "HoldEverything", "Forbid use of  all cooldowns, buffs, and gauge abilities");
        return res;
    }

    /// <summary>A quick and easy helper for shortcutting how we define our <em>GCD</em> abilities.</summary>
    /// <param name="track">The <em>Track</em> for the ability that the user is specifying; ability tracked <em>must</em> be inside the module's <em>Track</em> enum for target selection.</param>
    /// <param name="internalName">The <em>Internal Name</em> for the ability that the user is specifying; we usually want to put the full name of the ability here, as this will show up as the main name representing this option. (e.g. "No Mercy")</param>
    /// <param name="displayName">The <em>Display Name</em> for the ability that the user is specifying; we usually want to put some sort of abbreviation here, as this will show up as the secondary name representing this option. (e.g. "NM" or "N.Mercy")</param>
    /// <param name="uiPriority">The priority for specified ability inside the UI. (e.g. Higher the number = More to the left (in CDPlanner) or top (in Autorotation) menus)</param>
    /// <param name="cooldown"><para>The <em>Cooldown</em> for the ability that the user is specifying; 0 if none.</para><para><b><em>NOTE</em></b>: For charge abilities, this will check for its Charge CD, not its Total CD.</para></param>
    /// <param name="effectDuration">The <em>Effect Duration</em> for the ability that the user is specifying; 0 if none.</param>
    /// <param name="supportedTargets">The <em>Targets Supported</em> for the ability that the user is specifying.</param>
    /// <param name="minLevel">The <em>Minimum Level</em> required for the ability that the user is specifying.</param>
    /// <param name="maxLevel">The <em>Maximum Level</em> required for the ability that the user is specifying.</param>
    /// <returns>- Basic GCD options for any specified ability to be used via <em>AutoRotation</em> or <em>Cooldown Planner</em></returns>
    public static RotationModuleDefinition.ConfigRef<GCDStrategy> DefineGCD<Index>(this RotationModuleDefinition res, Index track, string internalName, string displayName = "", int uiPriority = 100, float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100) where Index : Enum
    {
        return res.Define(track).As<GCDStrategy>(internalName, displayName: displayName, uiPriority: uiPriority)
            .AddOption(GCDStrategy.Automatic, "Auto", "Automatically uses when optimal", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(GCDStrategy.Force, "Force", "Force use ASAP", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(GCDStrategy.Delay, "Delay", "Do not use", 0, 0, ActionTargets.None, minLevel: minLevel, maxLevel);
    }

    /// <summary>A quick and easy helper for shortcutting how we define our <em>OGCD</em> abilities.</summary>
    /// <param name="track">The <em>Track</em> for the ability that the user is specifying; ability tracked <em>must</em> be inside the module's <em>Track</em> enum for target selection.</param>
    /// <param name="internalName">The <em>Internal Name</em> for the ability that the user is specifying; we usually want to put the full name of the ability here, as this will show up as the main name representing this option. (e.g. "No Mercy")</param>
    /// <param name="displayName">The <em>Display Name</em> for the ability that the user is specifying; we usually want to put some sort of abbreviation here, as this will show up as the secondary name representing this option. (e.g. "NM" or "N.Mercy")</param>
    /// <param name="uiPriority">The priority for specified ability inside the UI. (e.g. Higher the number = More to the left (in CDPlanner) or top (in Autorotation) menus)</param>
    /// <param name="cooldown"><para>The <em>Cooldown</em> for the ability that the user is specifying; 0 if none.</para><para><b><em>NOTE</em></b>: For charge abilities, this will check for its Charge CD, not its Total CD.</para></param>
    /// <param name="effectDuration">The <em>Effect Duration</em> for the ability that the user is specifying; 0 if none.</param>
    /// <param name="supportedTargets">The <em>Targets Supported</em> for the ability that the user is specifying.</param>
    /// <param name="minLevel">The <em>Minimum Level</em> required for the ability that the user is specifying.</param>
    /// <param name="maxLevel">The <em>Maximum Level</em> required for the ability that the user is specifying.</param>
    /// <returns>- Basic OGCD options for any specified ability to be used via <em>AutoRotation</em> or <em>Cooldown Planner</em></returns>
    public static RotationModuleDefinition.ConfigRef<OGCDStrategy> DefineOGCD<Index>(this RotationModuleDefinition res, Index track, string internalName, string displayName = "", int uiPriority = 100, float cooldown = 0, float effectDuration = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = 100) where Index : Enum
    {
        return res.Define(track).As<OGCDStrategy>(internalName, displayName: displayName, uiPriority: uiPriority)
            .AddOption(OGCDStrategy.Automatic, "Auto", "Automatically uses when optimal", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.Force, "Force", "Force use ASAP", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.AnyWeave, "AnyWeave", "Force use in next possible weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.EarlyWeave, "EarlyWeave", "Force use in next possible early-weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.LateWeave, "LateWeave", "Force use in next possible late-weave slot", cooldown, effectDuration, supportedTargets, minLevel: minLevel, maxLevel)
            .AddOption(OGCDStrategy.Delay, "Delay", "Do not use", 0, 0, ActionTargets.None, minLevel: minLevel, maxLevel);
    }

    #region Global Helpers
    /// <summary>A global helper for automatically executing the best optimal rotation. See <seealso cref="AOEStrategy"/> for more details.</summary>
    /// <returns>- <em><b>TRUE</b></em> if <seealso cref="AOEStrategy"/> is set to <seealso cref="AOEStrategy.Automatic"/>
    /// <para>- <em><b>FALSE</b></em> if set to any other option</para></returns>
    public static bool Automatic(this StrategyValues strategy) => strategy.Option(SharedTrack.AOE).As<AOEStrategy>() is AOEStrategy.Automatic;

    /// <summary>A global helper for force-executing the single-target rotation. See <seealso cref="AOEStrategy"/> for more details.</summary>
    /// <returns>- <em><b>TRUE</b></em> if <seealso cref="AOEStrategy"/> is set to <seealso cref="AOEStrategy.ForceST"/>
    /// <para>- <em><b>FALSE</b></em> if set to any other option</para></returns>
    public static bool ForceST(this StrategyValues strategy) => strategy.Option(SharedTrack.AOE).As<AOEStrategy>() is AOEStrategy.ForceST;

    /// <summary>A global helper for force-executing the AOE rotation. See <seealso cref="AOEStrategy"/> for more details.</summary>
    /// <returns>- <em><b>TRUE</b></em> if <seealso cref="AOEStrategy"/> is set to <seealso cref="AOEStrategy.ForceAOE"/>
    /// <para>- <em><b>FALSE</b></em> if set to any other option</para></returns>
    public static bool ForceAOE(this StrategyValues strategy) => strategy.Option(SharedTrack.AOE).As<AOEStrategy>() == AOEStrategy.ForceAOE;

    /// <summary>A global helper for forbidding ALL available abilities that are buff, gauge, or cooldown related. See <seealso cref="HoldStrategy"/> for more details.</summary>
    /// <returns>- <em><b>TRUE</b></em> if <seealso cref="HoldStrategy"/> is set to <seealso cref="HoldStrategy.HoldEverything"/>
    /// <para>- <em><b>FALSE</b></em> if set to any other option</para></returns>
    public static bool HoldAll(this StrategyValues strategy) => strategy.Option(SharedTrack.Hold).As<HoldStrategy>() == HoldStrategy.HoldEverything;

    /// <summary>A global helper for forbidding ALL available abilities that are related to raidbuffs. See <seealso cref="HoldStrategy"/> for more details.</summary>
    /// <returns>- <em><b>TRUE</b></em> if <seealso cref="HoldStrategy"/> is set to <seealso cref="HoldStrategy.HoldBuffs"/>
    /// <para>- <em><b>FALSE</b></em> if set to any other option</para></returns>
    public static bool HoldBuffs(this StrategyValues strategy) => strategy.Option(SharedTrack.Hold).As<HoldStrategy>() == HoldStrategy.HoldBuffs;

    /// <summary>A global helper for forbidding ALL available abilities that have any sort of cooldown attached to it. See <seealso cref="HoldStrategy"/> for more details.</summary>
    /// <returns>- <em><b>TRUE</b></em> if <seealso cref="HoldStrategy"/> is set to <seealso cref="HoldStrategy.HoldCooldowns"/>
    /// <para>- <em><b>FALSE</b></em> if set to any other option</para></returns>
    public static bool HoldCDs(this StrategyValues strategy) => strategy.Option(SharedTrack.Hold).As<HoldStrategy>() == HoldStrategy.HoldCooldowns;

    /// <summary>A global helper for forbidding ALL available abilities that are related to the job's gauge. See <seealso cref="HoldStrategy"/> for more details.</summary>
    /// <returns>- <em><b>TRUE</b></em> if <seealso cref="HoldStrategy"/> is set to <seealso cref="HoldStrategy.HoldGauge"/>
    /// <para>- <em><b>FALSE</b></em> if set to any other option</para></returns>
    public static bool HoldGauge(this StrategyValues strategy) => strategy.Option(SharedTrack.Hold).As<HoldStrategy>() == HoldStrategy.HoldGauge;

    /// <summary>A global helper for allowing ALL available abilities that are buff, gauge, or cooldown related. This is the default option for this strategy. See <seealso cref="HoldStrategy"/> for more details.</summary>
    /// <returns>- <em><b>TRUE</b></em> if <seealso cref="HoldStrategy"/> is set to <seealso cref="HoldStrategy.DontHold"/>
    /// <para>- <em><b>FALSE</b></em> if set to any other option</para></returns>
    public static bool DontHold(this StrategyValues strategy) => strategy.Option(SharedTrack.Hold).As<HoldStrategy>() == HoldStrategy.DontHold;
    #endregion
}
