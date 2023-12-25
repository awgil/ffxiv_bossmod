namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class GNBConfig : ConfigNode
    {
        [PropertyDisplay("Execute optimal rotations on Keen Edge (ST) or Demon Slice (AOE)")]
        public bool FullRotation = true;

        [PropertyDisplay("Execute preceeding actions for single-target combos")]
        public bool STCombos = true;

        [PropertyDisplay("Execute preceeding action for aoe combo")]
        public bool AOECombos = true;

        [PropertyDisplay("Smart targeting for Shirk and Heart of Corundum (target if friendly, otherwise mouseover if friendly, otherwise offtank if available)")]
        public bool SmartHeartofCorundumShirkTarget = true;

        [PropertyDisplay("Use provoke on mouseover, if available and hostile")]
        public bool ProvokeMouseover = true;

        [PropertyDisplay("Forbid Lightning Shot too early in prepull")]
        public bool ForbidEarlyLightningShot = true;
    }
}
