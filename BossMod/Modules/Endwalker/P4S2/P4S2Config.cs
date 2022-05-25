namespace BossMod.Endwalker.P4S2
{
    [ConfigDisplay(Order = 0x142, Parent = typeof(EndwalkerConfig))]
    public class P4S2Config : ConfigNode
    {
        public bool Act4DarkSoakCCW = false;
        public bool Act4WaterBreakCCW = false;
        public bool CurtainCallDDFirst = false;

        public override void DrawContents(Tree tree)
        {
            DrawProperty(ref Act4DarkSoakCCW, "Act 4: go 1/8 CCW to soak tower with dark debuff");
            DrawProperty(ref Act4WaterBreakCCW, "Act 4: go 3/8 CCW to break water tether");
            DrawProperty(ref CurtainCallDDFirst, "Curtain call: DD break debuff first");
        }
    }
}
