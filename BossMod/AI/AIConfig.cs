namespace BossMod.AI;

[ConfigDisplay(Name = "AI settings (very experimental!!!)", Order = 6)]
sealed class AIConfig : ConfigNode
{
    [PropertyDisplay("Enable AI")]
    public bool Enabled = false;

    [PropertyDisplay("Show status in DTR bar")]
    public bool ShowDTR = false;

    [PropertyDisplay("Draw UI")]
    public bool DrawUI = true;

    [PropertyDisplay("Focus target leader")]
    public bool FocusTargetLeader = true;

    [PropertyDisplay("Broadcast keypresses to other windows")]
    public bool BroadcastToSlaves = false;

    [PropertyDisplay("Follow party slot")]
    [PropertyCombo(["Slot 1", "Slot 2", "Slot 3", "Slot 4", "Slot 5", "Slot 6", "Slot 7", "Slot 8"])]
    public int FollowSlot = 0;

    [PropertyDisplay("Forbid actions")]
    public bool ForbidActions = false;

    [PropertyDisplay("Forbid movement")]
    public bool ForbidMovement = false;

    [PropertyDisplay("Follow during combat")]
    public bool FollowDuringCombat = false;

    [PropertyDisplay("Follow during active boss module")]
    public bool FollowDuringActiveBossModule = false;

    [PropertyDisplay("Follow out of combat")]
    public bool FollowOutOfCombat = true;

    [PropertyDisplay("Follow target")]
    public bool FollowTarget = false;

    [PropertyDisplay("Desired positional when following target")]
    [PropertyCombo(["Any", "Flank", "Rear", "Front"])]
    public Positional DesiredPositional = Positional.Any;
}
