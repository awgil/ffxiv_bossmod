namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class WARConfig : ConfigNode
    {
        [PropertyDisplay("Execute optimal single-target rotation on Heavy Swing and AOE rotation on Overpower")]
        public bool FullRotation = true;

        [PropertyDisplay("Execute preceeding actions for single-target combos (Maim, Storm Eye, Storm Path)")]
        public bool STCombos = true;

        [PropertyDisplay("Execute preceeding action for aoe combo (Mythril Tempest)")]
        public bool AOECombos = true;

        [PropertyDisplay("Smart targeting for Shirk and Nascent Flash (target if friendly, otherwise mouseover if friendly, otherwise offtank if available)")]
        public bool SmartNascentFlashShirkTarget = true;

        [PropertyDisplay("Use provoke on mouseover, if available and hostile")]
        public bool ProvokeMouseover = true;

        [PropertyDisplay("Use self for holmgang target")]
        public bool HolmgangSelf = true;

        [PropertyDisplay("Forbid tomahawk too early in prepull")]
        public bool ForbidEarlyTomahawk = true;
    }
}
