namespace BossMod;

[ConfigDisplay(Name = "Action Tweaks", Order = 4, Group = "Autorotation")]
public sealed class ActionTweaksConfig : ConfigNode
{
    // TODO: consider exposing max-delay to config; 0 would mean 'remove all delay', max-value would mean 'disable'
    [PageStart("Animation lock")]
    [PropertyDisplay("Remove extra lag-induced animation lock delay from instant casts (read tooltip!)", tooltip: "Do NOT use with XivAlexander or NoClippy - this should automatically disable itself if they are detected, but double check first!")]
    public bool RemoveAnimationLockDelay = false;

    [PropertyDisplay("Animation lock max. simulated delay (read tooltip!)", tooltip: "Configures the maximum simulated delay in milliseconds when using animation lock removal - this is required and cannot be reduced to zero. Setting this to 20ms will enable triple-weaving when using autorotation. The minimum setting to remove triple-weaving is 26ms. The minimum of 20ms has been accepted by FFLogs and should not cause issues with your logs.")]
    [PropertySlider(20, 50)]
    public int AnimationLockDelayMax = 26;

    [PropertyDisplay("Remove extra framerate-induced cooldown delay", tooltip: "Dynamically adjusts cooldown and animation locks to ensure queued actions resolve immediately regardless of framerate limitations")]
    public bool RemoveCooldownDelay = false;

    [PageStart("Movement & casting")]
    [PropertyDisplay("Prevent movement while casting", tags: ["slidecast"])]
    public bool PreventMovingWhileCasting = false;

    public enum ModifierKey
    {
        [PropertyDisplay("Nothing")]
        None,
        [PropertyDisplay("CTRL")]
        Ctrl,
        [PropertyDisplay("ALT")]
        Alt,
        [PropertyDisplay("SHIFT")]
        Shift,
        [PropertyDisplay("LMB + RMB")]
        M12
    }

    [PropertyDisplay("Hold to disable", depends: nameof(PreventMovingWhileCasting), tags: ["slidecast"])]
    public ModifierKey MoveEscapeHatch = ModifierKey.None;

    [PropertyDisplay("Automatically cancel cast if target dies")]
    public bool CancelCastOnDeadTarget = false;

    [PropertyDisplay("Prevent movement and actions during pyretic/stillness")]
    public bool AutoPyretic = true;

    [PropertyDisplay("Anticipation window (seconds)", depends: nameof(AutoPyretic))]
    [PropertySlider(0, 10)]
    public float PyreticThreshold = 1.0f;

    [PropertyDisplay("Prevent undesired movement during Temporary Misdirection")]
    public bool AutoMisdirection = false;

    [PropertyDisplay("Maximum facing deviation (degrees)", depends: nameof(AutoMisdirection))]
    [PropertySlider(0, 180)]
    public float MisdirectionThreshold = 180;

    [PageStart("Targeting & queueing")]
    [PropertyDisplay("Use actions on mouseover target")]
    public bool PreferMouseover = false;

    [PropertyDisplay("Use custom queueing for manually pressed actions", tooltip: "This setting allows better integration with autorotations and will prevent you from triple-weaving or drifting GCDs if you press a healing ability while autorotation is going on")]
    public bool UseManualQueue = false;

    [PropertyDisplay("Smart ability targeting", tooltip: "If the usual (mouseover/primary) target is not valid for an action, select the next best target automatically (e.g. co-tank for Shirk)", depends: nameof(UseManualQueue))]
    public bool SmartTargets = true;

    [PropertyDisplay("Delay actions during countdown", tooltip: "If enabled, manually pressed actions will be delayed during countdown to ensure that they hit at 0 seconds.", depends: nameof(UseManualQueue), since: "7.5.7.0")]
    public bool CountdownHold = false;

    [PropertyDisplay("Try to prevent dashing into AOEs", tooltip: "Prevent automatic use of targeted dashes (like WAR Onslaught) if they would move you into a dangerous area. May not work as expected in instances that do not have modules.\n\nThis option will also apply to manually pressed dashes if you have \"Use custom queueing for manually pressed actions\" enabled.", since: "0.0.0.290")]
    public bool DashSafety = true;

    [PropertyDisplay("Apply to all dashes, not just gap closers", tooltip: "Includes backdashes (e.g. SAM Yaten), teleports (e.g. NIN Shukuchi), and fixed-length dashes (e.g. DRG Elusive Jump)", depends: nameof(DashSafety))]
    public bool DashSafetyExtra = true;

    [PropertyDisplay("Automatically manage auto attacks", tooltip: "This setting prevents starting autos early during countdown, starts them automatically at pull, when switching targets and when using any actions that don't explicitly cancel autos.")]
    public bool AutoAutos2 = true;

    [PropertyDisplay("Automatically dismount to execute actions")]
    public bool AutoDismount = true;

    public enum GroundTargetingMode
    {
        [PropertyDisplay("Manually select position by extra click (normal game behaviour)")]
        Manual,

        [PropertyDisplay("Cast at current mouse position")]
        AtCursor,

        [PropertyDisplay("Cast at selected target's position")]
        AtTarget
    }
    [PropertyDisplay("Automatic target selection for ground-targeted abilities:")]
    public GroundTargetingMode GTMode = GroundTargetingMode.Manual;
}
