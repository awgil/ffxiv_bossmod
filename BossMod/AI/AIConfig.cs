﻿namespace BossMod.AI;

[ConfigDisplay(Name = "AI settings (very experimental!!!)", Order = 6)]
sealed class AIConfig : ConfigNode
{
    public enum Slot { One, Two, Three, Four }

    [PropertyDisplay("Enable AI")]
    public bool Enabled = false;

    [PropertyDisplay("Draw UI")]
    public bool DrawUI = true;

    [PropertyDisplay("Show extra UI options")]
    public bool ShowExtraUIOptions = true;

    [PropertyDisplay("Show state in DTR Bar")]
    public bool ShowDTR = true;

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
    public float MaxDistanceToTarget = 3;

    [PropertyDisplay("Follow during active boss module")]
    public bool FollowDuringActiveBossModule = false;

    [PropertyDisplay("Follow during combat")]
    public bool FollowDuringCombat = false;

    [PropertyDisplay("Follow out of combat")]
    public bool FollowOutOfCombat = true;

    [PropertyDisplay("Forbid movement")]
    public bool ForbidMovement = false;

    [PropertyDisplay("Forbid actions")]
    public bool ForbidActions = false;

    [PropertyDisplay("Focus target master")]
    public bool FocusTargetMaster = true;

    [PropertyDisplay("Broadcast keypresses to other windows")]
    public bool BroadcastToSlaves = false;
}
