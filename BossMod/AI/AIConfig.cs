namespace BossMod.AI;

[ConfigDisplay(Name = "AI Configuration", Order = 7)]
sealed class AIConfig : ConfigNode
{
    public enum Slot { One, Two, Three, Four }

    // ui settings
    [PropertyDisplay("Enable AI", tooltip: "Disclaimer: AI is very experimental, use at your own risk!")]
    public bool Enabled = false;

    [PropertyDisplay("Show in-game UI")]
    public bool DrawUI = true;

    [PropertyDisplay("Show AI status in the in-game UI's title bar")]
    public bool ShowStatusOnTitlebar = true;

    [PropertyDisplay("Show AI status in the server info bar")]
    public bool ShowDTR = true;

    [PropertyDisplay("Delay on Movement (seconds)", tooltip: "Keep this value low! Too high and it won't move in time for some mechanics")]
    public float MoveDelay = 0f;

    // ai settings
    [PropertyDisplay($"Follow slot")]
    public Slot FollowSlot = 0;

    [PropertyDisplay("Disable movement")]
    public bool ForbidMovement = false;

    [PropertyDisplay("Disable auto-target")]
    public bool ForbidActions = false;

    [PropertyDisplay("Focus target master")]
    public bool FocusTargetMaster = false;

    [PropertyDisplay("Broadcast keypresses to other windows")]
    public bool BroadcastToSlaves = false;

    // hybrid mode settings
    [PropertyDisplay("Respect manual targeting", tooltip: "When enabled, the AI will not override your manually selected targets")]
    public bool RespectManualTargeting = false;

    public enum ManualOverrideKey { None, Shift, Ctrl, Alt }

    [PropertyDisplay("Manual movement key", tooltip: "Hold this key to temporarily take manual movement control")]
    public ManualOverrideKey MovementOverrideKey = ManualOverrideKey.Shift;
}
