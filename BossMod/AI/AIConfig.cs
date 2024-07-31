namespace BossMod.AI;

[ConfigDisplay(Name = "AI settings (very experimental!!!)", Order = 6)]
sealed class AIConfig : ConfigNode
{
    public enum Slot { One, Two, Three, Four }

    [PropertyDisplay("Enable AI")]
    public bool Enabled = false;

    [PropertyDisplay("Draw UI")]
    public bool DrawUI = true;

    [PropertyDisplay("PreferedPositional")]
    public Positional PreferedPositional = 0;

    [PropertyDisplay($"Follow slot")]
    public Slot FollowSlot = 0;

    [PropertyDisplay($"Follow range")]
    public int FollowRange = 3;

    [PropertyDisplay($"Follow target")]
    public bool FollowTarget = false;

    [PropertyDisplay("Follow during active module")]
    public bool FollowActiveBM = false;

    [PropertyDisplay("Follow in combat")]
    public bool FollowInCombat = false;

    [PropertyDisplay("Follow out of combat")]
    public bool FollowOOC = true;

    [PropertyDisplay("Focus target master")]
    public bool FocusTargetLeader = true;

    [PropertyDisplay("Broadcast keypresses to other windows")]
    public bool BroadcastToSlaves = false;
}
