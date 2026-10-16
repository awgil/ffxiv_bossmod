namespace BossMod.AI;

[ConfigDisplay(Name = "AI Configuration", Order = 9, Group = "AI & Automation")]
public sealed class AIConfig : ConfigNode
{
    [PropertyDisplay("Enable")]
    public bool Enabled = false;

    // ui settings
    [PropertyDisplay("Show config window")]
    public bool DrawUI = true;

    [PropertyDisplay("Focus target master")]
    public bool FocusTargetMaster = false;

    [PropertyDisplay("Distance to master")]
    [PropertySlider(3, 25)]
    public float DistanceToMaster = 25f;

    [PropertyDisplay("Follow master even if a boss module is active")]
    public bool FollowDuringBoss = false;

    [PropertyDisplay("Disable movement")]
    public bool ForbidMovement = false;

    [PropertyDisplay("Disable auto-target")]
    public bool ForbidActions = false;

    [PropertyDisplay("Move towards current target", since: "7.5.1.21")]
    public bool GoalZoneFallback = true;

    [PropertyDisplay("Print AI status changes to chat", since: "7.5.1.36")]
    public bool ChatConfirmation = false;

    // defined here for DtrConfig migration
    public bool ShowDTR = true;
}
