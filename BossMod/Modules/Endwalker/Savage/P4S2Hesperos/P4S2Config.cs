namespace BossMod.Endwalker.Savage.P4S2Hesperos;

[ConfigDisplay(Order = 0x142, Parent = typeof(EndwalkerConfig))]
public class P4S2Config() : CooldownPlanningConfigNode(90)
{
    [PropertyDisplay("Act 4: go 1/8 CCW to soak tower with dark debuff")]
    public bool Act4DarkSoakCCW = false;

    [PropertyDisplay("Act 4: go 3/8 CCW to break water tether")]
    public bool Act4WaterBreakCCW = false;

    [PropertyDisplay("Curtain call: DD break debuff first")]
    public bool CurtainCallDDFirst = false;
}
