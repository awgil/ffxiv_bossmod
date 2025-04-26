namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

[ConfigDisplay(Order = 0x300, Parent = typeof(DawntrailConfig))]
public class RM08SHowlingBladeConfig : ConfigNode
{
    [PropertyDisplay("Windfang/Stonefang clock spots", tooltip: "Only used by AI")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    [GroupPreset("Default", [0, 4, 6, 2, 5, 3, 7, 1])]
    public GroupAssignmentUnique WindfangStonefangSpots = new() { Assignments = [0, 4, 6, 2, 5, 3, 7, 1] };
}
