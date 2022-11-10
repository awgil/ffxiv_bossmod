namespace BossMod.Endwalker.Savage.P8S2
{
    [ConfigDisplay(Order = 0x182, Parent = typeof(EndwalkerConfig))]
    public class P8S2Config : CooldownPlanningConfigNode
    {
        [PropertyDisplay("Limitless desolation: tanks/healers use right side")]
        public bool LimitlessDesolationTHRight = false;

        public P8S2Config() : base(90) { }
    }
}
