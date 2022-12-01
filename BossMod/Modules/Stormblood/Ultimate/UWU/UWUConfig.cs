namespace BossMod.Stormblood.Ultimate.UWU
{
    [ConfigDisplay(Order = 0x200, Parent = typeof(StormbloodConfig))]
    public class UWUConfig : CooldownPlanningConfigNode
    {
        [PropertyDisplay("Titan gaols priorities (close < far)")]
        [GroupDetails(new string[] { "0", "1", "2", "3", "4", "5", "6", "7" })]
        public GroupAssignmentUnique P3GaolPriorities = GroupAssignmentUnique.Default();

        public UWUConfig() : base(70) { }
    }
}
