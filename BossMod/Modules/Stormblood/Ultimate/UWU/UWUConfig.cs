namespace BossMod.Stormblood.Ultimate.UWU;

[ConfigDisplay(Order = 0x210, Parent = typeof(StormbloodConfig))]
public class UWUConfig() : CooldownPlanningConfigNode(70)
{
    [PropertyDisplay("Titan gaols priorities (close < far)")]
    [GroupDetails(["0", "1", "2", "3", "4", "5", "6", "7"])]
    public GroupAssignmentUnique P3GaolPriorities = GroupAssignmentUnique.Default();
}
