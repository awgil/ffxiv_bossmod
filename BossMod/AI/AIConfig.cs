namespace BossMod.AI;

[ConfigDisplay(Name = "AI Configuration", Order = 8)]
sealed class AIConfig : ConfigNode
{
    [PropertyDisplay("Enable")]
    public bool Enabled = false;

    // ui settings
    [PropertyDisplay("Show config window")]
    public bool DrawUI = true;

    [PropertyDisplay("Show status in the server info bar")]
    public bool ShowDTR = true;

    [PropertyDisplay("Focus target master")]
    public bool FocusTargetMaster = false;

    [PropertyDisplay("Distance to master")]
    [PropertySlider(3, 25, Speed = 0.1f)]
    public float DistanceToMaster = 25f;

    [PropertyDisplay("Follow master even if a boss module is active")]
    public bool FollowDuringBoss = false;

    [PropertyDisplay("Disable movement")]
    public bool ForbidMovement = false;

    [PropertyDisplay("Disable auto-target")]
    public bool ForbidActions = false;
}
