namespace BossMod.Dawntrail.Savage.RM09SVampFatale;

[ConfigDisplay(Parent = typeof(DawntrailConfig), Order = -4)]
public class RM09SVampFataleConfig : ConfigNode
{
    [PropertyDisplay("Hell in a Cell: Tower assignments (clockwise starting from true north)")]
    [GroupDetails(["1.1", "1.2", "1.3", "1.4", "2.1", "2.2", "2.3", "2.4"])]
    [GroupPreset("Default: THMR, LP1 then LP2", [0, 4, 1, 5, 2, 6, 3, 7])]
    public GroupAssignmentUnique HellCellAssignments = new() { Assignments = [0, 4, 1, 5, 2, 6, 3, 7] };
}
