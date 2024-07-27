namespace BossMod;

[ConfigDisplay(Name = "Action tweaks settings", Order = 4)]
public sealed class ActionTweaksConfig : ConfigNode
{
    // TODO: consider exposing max-delay to config; 0 would mean 'remove all delay', max-value would mean 'disable'
    [PropertyDisplay("Remove extra lag-induced animation lock delay from instant casts (a-la xivalex)")]
    public bool RemoveAnimationLockDelay = false;

    [PropertyDisplay("Remove extra framerate-induced cooldown delay")]
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

    [PropertyDisplay("Hold to allow movement while casting")]
    public ModifierKey MoveEscapeHatch = ModifierKey.None;

    [PropertyDisplay("Automatically cancel a cast when target is dead")]
    public bool CancelCastOnDeadTarget = false;

    [PropertyDisplay("Restore character orientation after action use (no effect if 'auto face target' in game settings is disabled)")]
    public bool RestoreRotation = false;

    [PropertyDisplay("Use actions on mouseover target, if possible")]
    public bool PreferMouseover = false;

    [PropertyDisplay("Smart targeting: if usual (mouseover/primary) target is not valid for action, select best target automatically (e.g. co-tank for Shirk)")]
    public bool SmartTargets = true;

    [PropertyDisplay("Use custom queueing for manually pressed actions, to better integrate with autorotation")]
    public bool UseManualQueue = false;

    public enum GroundTargetingMode
    {
        [PropertyDisplay("Manually select position by extra click (normal game behaviour)")]
        Manual,

        [PropertyDisplay("Cast at current mouse position")]
        AtCursor,

        [PropertyDisplay("Cast at selected target's position")]
        AtTarget
    }
    [PropertyDisplay("Target selection for ground-targeted abilities")]
    public GroundTargetingMode GTMode = GroundTargetingMode.Manual;
}
