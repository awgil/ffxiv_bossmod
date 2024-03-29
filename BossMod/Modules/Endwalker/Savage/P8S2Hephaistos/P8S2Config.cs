namespace BossMod.Endwalker.Savage.P8S2;

[ConfigDisplay(Order = 0x182, Parent = typeof(EndwalkerConfig))]
public class P8S2Config : CooldownPlanningConfigNode
{
    [PropertyDisplay("Limitless desolation: tanks/healers use right side")]
    public bool LimitlessDesolationTHRight = false;

    [PropertyDisplay("High concept 1: long debuffs take S towers")]
    public bool HC1LongGoS = true;

    public P8S2Config() : base(90) { }
}
