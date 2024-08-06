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

    [PropertyDisplay($"Override Follow range")]
    public bool FollowRangeOverride = false;

    [PropertyDisplay($"Follow range")]
    public int FollowRange = 3;

    [PropertyDisplay($"Follow target")]
    public bool FollowTarget = false;

    [PropertyDisplay("Follow during active module")]
    public bool FollowDuringActiveBossModule = false;

    [PropertyDisplay("Follow in combat")]
    public bool FollowDuringCombat = false;

    [PropertyDisplay("Follow out of combat")]
    public bool FollowOutOfCombat = true;

    [PropertyDisplay("Focus target master")]
    public bool FocusTargetLeader = true;

    [PropertyDisplay("Broadcast keypresses to other windows")]
    public bool BroadcastToSlaves = false;
}
