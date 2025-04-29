namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

[ConfigDisplay(Order = 0x300, Parent = typeof(DawntrailConfig))]
public class RM08SHowlingBladeConfig : ConfigNode
{
    [PropertyDisplay("Beckon Moonlight: highlight first and third safe quadrants on minimap (for \"quad moonlight\")")]
    public bool QuadMoonlightHints = false;

    [PropertyDisplay("Windfang/Stonefang clock spots", tooltip: "Only used by AI")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    [GroupPreset("Default", [0, 4, 6, 2, 5, 3, 7, 1])]
    public GroupAssignmentUnique WindfangStonefangSpots = new() { Assignments = [0, 4, 6, 2, 5, 3, 7, 1] };
}
