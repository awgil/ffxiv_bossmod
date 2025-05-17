namespace BossMod;

[ConfigDisplay(Name = "Action Tweaks", Order = 4)]
public sealed class ActionTweaksConfig : ConfigNode
{
    // TODO: consider exposing max-delay to config; 0 would mean 'remove all delay', max-value would mean 'disable'
    [PropertyDisplay("Remove extra lag-induced animation lock delay from instant casts (read tooltip!)", tooltip: "Do NOT use with XivAlexander or NoClippy - this should automatically disable itself if they are detected, but double check first!")]
    public bool RemoveAnimationLockDelay = false;

    [PropertyDisplay("Animation lock max. simulated delay (read tooltip!)", tooltip: "Configures the maximum simulated delay in milliseconds when using animation lock removal - this is required and cannot be reduced to zero. Setting this to 20ms will enable triple-weaving when using autorotation. The minimum setting to remove triple-weaving is 26ms. The minimum of 20ms has been accepted by FFLogs and should not cause issues with your logs.")]
    [PropertySlider(20, 50, Speed = 0.1f)]
    public int AnimationLockDelayMax = 26;

    [PropertyDisplay("Remove extra framerate-induced cooldown delay", tooltip: "Dynamically adjusts cooldown and animation locks to ensure queued actions resolve immediately regardless of framerate limitations")]
    public bool RemoveCooldownDelay = false;

    [PropertyDisplay("Prevent movement while casting")]
    public bool PreventMovingWhileCasting = false;

    public enum ModifierKey
    {
        [PropertyDisplay("None")]
        None,
        [PropertyDisplay("Control")]
        Ctrl,
        [PropertyDisplay("Alt")]
        Alt,
        [PropertyDisplay("Shift")]
        Shift,
        [PropertyDisplay("LMB + RMB")]
        M12
    }

    [PropertyDisplay("Key to hold to allow movement while casting", depends: nameof(PreventMovingWhileCasting))]
    public ModifierKey MoveEscapeHatch = ModifierKey.None;

    [PropertyDisplay("Automatically cancel a cast when target is dead")]
    public bool CancelCastOnDeadTarget = false;

    [PropertyDisplay("Prevent movement and action execution when pyretic-like mechanics are imminent (set to 0 to disable, otherwise increase threshold depending on your ping).")]
    [PropertySlider(0, 10, Speed = 0.01f)]
    public float PyreticThreshold = 1.0f;

    [PropertyDisplay("Auto misdirection: prevent movement under misdirection if angle between normal movement and misdirected is greater than this threshold (set to 180 to disable).")]
    [PropertySlider(0, 180)]
    public float MisdirectionThreshold = 180;

    [PropertyDisplay("Use actions on mouseover target")]
    public bool PreferMouseover = false;

    [PropertyDisplay("Smart ability targeting", tooltip: "If the usual (mouseover/primary) target is not valid for an action, select the next best target automatically (e.g. co-tank for Shirk)")]
    public bool SmartTargets = true;

    [PropertyDisplay("Use custom queueing for manually pressed actions", tooltip: "This setting allows better integration with autorotations and will prevent you from triple-weaving or drifting GCDs if you press a healing ability while autorotation is going on")]
    public bool UseManualQueue = false;

    [PropertyDisplay("Try to prevent dashing into AOEs", tooltip: "Prevent automatic use of targeted dashes (like WAR Onslaught) if they would move you into a dangerous area. May not work as expected in instances that do not have modules.\n\nThis option will also apply to manually pressed dashes if you have \"Use custom queueing for manually pressed actions\" enabled.", since: "0.0.0.290")]
    public bool DashSafety = true;

    [PropertyDisplay("Apply the previous option to all dashes, not just gap closers", tooltip: "Includes backdashes (e.g. SAM Yaten), teleports (e.g. NIN Shukuchi), and fixed-length dashes (e.g. DRG Elusive Jump)", depends: nameof(DashSafety))]
    public bool DashSafetyExtra = true;

    [PropertyDisplay("Automatically manage auto attacks", tooltip: "This setting prevents starting autos early during countdown, starts them automatically at pull, when switching targets and when using any actions that don't explicitly cancel autos.")]
    public bool AutoAutos = false;

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
    [PropertyDisplay("Automatic target selection for ground-targeted abilities")]
    public GroundTargetingMode GTMode = GroundTargetingMode.Manual;
}
