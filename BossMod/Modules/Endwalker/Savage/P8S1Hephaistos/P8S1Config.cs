namespace BossMod.Endwalker.Savage.P8S1Hephaistos
{
    [ConfigDisplay(Order = 0x181, Parent = typeof(EndwalkerConfig))]
    public class P8S1Config : CooldownPlanningConfigNode
    {
        [PropertyDisplay("Snake assignments")]
        [GroupDetails(new string[] { "CCW from NW (flex)", "CCW from NW (fixed)", "CW from N (flex)", "CW from N (fixed)" })]
        public GroupAssignmentDDSupportPairs SnakeAssignments = new();

        public P8S1Config() : base(90) { }
    }
}
