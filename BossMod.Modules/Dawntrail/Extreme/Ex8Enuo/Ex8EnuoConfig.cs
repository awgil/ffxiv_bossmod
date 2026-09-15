namespace BossMod.Dawntrail.Extreme.Ex8Enuo;

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
public class Ex8EnuoConfig : ConfigNode
{
    [PropertyDisplay("Soak assignments for Gaze of the Void (clockwise, starting from tank orbs)")]
    [GroupDetails(["Tank A", "Tank B", "2A", "2B", "3A", "3B", "4A", "4B"])]
    [GroupPreset("THMR (Hector)", [0, 1, 2, 3, 4, 5, 6, 7])]
    public GroupAssignmentUnique VoidGaze = new() { Assignments = [0, 1, 2, 3, 4, 5, 6, 7] };

    [PropertyDisplay("Intermission tower/bait assignments (clockwise, starting from north)")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("G1 W, G2 E", [6, 0, 4, 2, 5, 3, 7, 1])]
    public GroupAssignmentUnique IntermissionTower = new() { Assignments = [6, 0, 4, 2, 5, 3, 7, 1] };
}
