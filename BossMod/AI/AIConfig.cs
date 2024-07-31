namespace BossMod.AI;

[ConfigDisplay(Name = "AI settings (very experimental!!!)", Order = 6)]
sealed class AIConfig : ConfigNode
{
    [PropertyDisplay("Enable AI")]
    public bool Enabled = false;

    [PropertyDisplay("Draw UI")]
    public bool DrawUI = true;

    [PropertyDisplay($"Follow slot\n(-1 is target)")]
    public int FollowSlot = 0;

    [PropertyDisplay($"Follow range")]
    public int FollowRange = 3;

    [PropertyDisplay("Follow during active module")]
    public bool FollowActiveBM = false;

    [PropertyDisplay("Follow in combat")]
    public bool FollowInCombat = false;

    [PropertyDisplay("Follow out of combat")]
    public bool FollowOOC = true;

    [PropertyDisplay("Prefered positional")]
    public int PreferedPositional = 2;

    [PropertyDisplay("Focus target master")]
    public bool FocusTargetLeader = true;

    [PropertyDisplay("Broadcast keypresses to other windows")]
    public bool BroadcastToSlaves = false;
}
