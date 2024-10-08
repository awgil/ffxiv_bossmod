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

    // ai settings
    [PropertyDisplay($"Follow slot")]
    public Slot FollowSlot = 0;

    [PropertyDisplay("Disable movement")]
    public bool ForbidMovement = false;

    [PropertyDisplay("Disable auto-target")]
    public bool ForbidActions = false;

    [PropertyDisplay("Automatically engage FATE mobs", since: "0.0.0.253")]
    public bool AutoFate = true;

    [PropertyDisplay("Focus target master")]
    public bool FocusTargetMaster = false;

    [PropertyDisplay("Broadcast keypresses to other windows")]
    public bool BroadcastToSlaves = false;
}
