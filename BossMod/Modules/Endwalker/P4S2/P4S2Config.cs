namespace BossMod.Endwalker.P4S2
{
    [ConfigDisplay(Order = 0x142, Parent = typeof(EndwalkerConfig))]
    public class P4S2Config : ConfigNode
    {
        [PropertyDisplay("Act 4: go 1/8 CCW to soak tower with dark debuff")]
        public bool Act4DarkSoakCCW = false;

        [PropertyDisplay("Act 4: go 3/8 CCW to break water tether")]
        public bool Act4WaterBreakCCW = false;

        [PropertyDisplay("Curtain call: DD break debuff first")]
        public bool CurtainCallDDFirst = false;
    }
}
