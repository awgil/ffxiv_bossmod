namespace BossMod.AI;

[ConfigDisplay(Name = "Multibox support", Order = 8)]
sealed class AIConfig : ConfigNode
{
    // ui settings
    [PropertyDisplay("Show config window")]
    public bool DrawUI = false;

    [PropertyDisplay("Focus target master")]
    public bool FocusTargetMaster = false;

    [PropertyDisplay("Distance to master")]
    public float DistanceToMaster = 25f;

    [PropertyDisplay("Follow master even if a boss module is active")]
    public bool FollowDuringBoss = false;
}
