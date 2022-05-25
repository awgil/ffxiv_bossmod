namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class WARConfig : ConfigNode
    {
        public bool FullRotation = true;
        public bool STCombos = true;
        public bool AOECombos = true;
        public bool SmartNascentFlashShirkTarget = true;
        public bool HolmgangSelf = true;

        public override void DrawContents(Tree tree)
        {
            DrawProperty(ref FullRotation, "Execute optimal single-target rotation on Heavy Swing and AOE rotation on Overpower");
            DrawProperty(ref STCombos, "Execute preceeding actions for single-target combos (Maim, Storm Eye, Storm Path)");
            DrawProperty(ref AOECombos, "Execute preceeding action for aoe combo (Mythril Tempest)");
            DrawProperty(ref SmartNascentFlashShirkTarget, "Smart targeting for Shirk and Nascent Flash (target if friendly, otherwise mouseover if friendly, otherwise offtank if available)");
            DrawProperty(ref HolmgangSelf, "Use self for holmgang target");
        }
    }
}
