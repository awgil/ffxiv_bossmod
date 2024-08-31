namespace BossMod.AI;

[ConfigDisplay(Name = "AI Configuration", Order = 6)]
sealed class AIConfig : ConfigNode
{
    public enum Slot { One, Two, Three, Four }

    // ui settings
    [PropertyDisplay("Enable AI", tooltip: "Disclaimer: AI is very experimental, use at your own risk!")]
    public bool Enabled = false;

    [PropertyDisplay("Show in-game UI")]
    public bool DrawUI = true;

    [PropertyDisplay("Show advanced options in the UI")]
    public bool ShowExtraUIOptions = true;

    [PropertyDisplay("Show AI status in the in-game UI's title bar")]
    public bool ShowStatusOnTitlebar = true;

    [PropertyDisplay("Show AI status in the server info bar")]
    public bool ShowDTR = true;

    // ai settings
    [PropertyDisplay($"Override positional")]
    public bool OverridePositional = false;

    [PropertyDisplay("Desired positional")]
    public Positional DesiredPositional = 0;

    [PropertyDisplay($"Follow slot")]
    public Slot FollowSlot = 0;

    [PropertyDisplay($"Override follow range")]
    public bool OverrideRange = false;

    [PropertyDisplay($"Follow slot range")]
    public float MaxDistanceToSlot = 1;

    [PropertyDisplay($"Follow target")]
    public bool FollowTarget = false;

    [PropertyDisplay($"Follow target range")]
    public float MaxDistanceToTarget = 2.6f;

    [PropertyDisplay("Follow during active boss module")]
    public bool FollowDuringActiveBossModule = false;

    [PropertyDisplay("Follow during combat")]
    public bool FollowDuringCombat = false;

    [PropertyDisplay("Follow out of combat")]
    public bool FollowOutOfCombat = false;

    [PropertyDisplay("Forbid movement")]
    public bool ForbidMovement = false;

    [PropertyDisplay("Forbid actions")]
    public bool ForbidActions = false;

    [PropertyDisplay("Focus target master")]
    public bool FocusTargetMaster = false;

    [PropertyDisplay("Broadcast keypresses to other windows")]
    public bool BroadcastToSlaves = false;
}
