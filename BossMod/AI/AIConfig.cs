namespace BossMod.AI;

// AIConfig Class
[ConfigDisplay(Name = "AI settings (very experimental!!!)", Order = 6)]
sealed class AIConfig : ConfigNode
{
    [PropertyDisplay("Enable AI")]
    public bool Enabled = false;

    [PropertyDisplay("Draw UI")]
    public bool DrawUI = true;

    [PropertyDisplay("Follow Leader")]
    public bool FollowLeader = true;

    [PropertyDisplay("Focus Target Leader")]
    public bool FocusTargetLeader = true;

    [PropertyDisplay("Broadcast keypresses to other windows")]
    public bool BroadcastToSlaves = false;

    [PropertyDisplay("Follow Party Slot")]
    [PropertyCombo(["Slot 1", "Slot 2", "Slot 3", "Slot 4", "Slot 5", "Slot 6", "Slot 7", "Slot 8"])]
    public int FollowSlot = 0; // Default to Slot 1
}
