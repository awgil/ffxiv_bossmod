namespace BossMod
{
    class WARConfig : ConfigNode
    {
        public bool FullRotation = true;
        public bool STCombos = true;
        public bool AOECombos = true;
        public bool EnableMovement = true;
        public bool SmartShirkTarget = true;
        public bool SmartNascentFlashTarget = true;
        public bool HolmgangSelf = true;
        public WARRotation.Strategy.PotionUse PotionUse = WARRotation.Strategy.PotionUse.Manual;

        protected override void DrawContents()
        {
            DrawProperty(ref FullRotation, "Execute optimal single-target rotation on Heavy Swing and AOE rotation on Overpower");
            DrawProperty(ref STCombos, "Execute preceeding actions for single-target combos (Maim, Storm Eye, Storm Path)");
            DrawProperty(ref AOECombos, "Execute preceeding action for aoe combo (Mythril Tempest)");
            DrawProperty(ref EnableMovement, "Enable actions that affect position (Onslaught, Primal Rend)");
            DrawProperty(ref SmartShirkTarget, "Smart targeting for Shirk (target if friendly, otherwise mouseover if friendly, otherwise offtank if available)");
            DrawProperty(ref SmartNascentFlashTarget, "Smart targeting for Nascent Flash (target if friendly, otherwise mouseover if friendly, otherwise offtank if available)");
            DrawProperty(ref HolmgangSelf, "Use self for holmgang target");
            DrawProperty(ref PotionUse, "Potion use strategy");
        }
    }
}
