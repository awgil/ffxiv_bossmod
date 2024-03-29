namespace BossMod.Stormblood.Ultimate.UCOB;

[ConfigDisplay(Order = 0x200, Parent = typeof(StormbloodConfig))]
public class UCOBConfig : CooldownPlanningConfigNode
{
    public UCOBConfig() : base(70) { }

    [PropertyDisplay("P3 Quickmarch/Heavensfall Trio: safespot assignments (assuming bahamut is relative north/up, group L goes left; L1/R1 closest to bosses)")]
    [GroupDetails(["L1", "L2", "L3", "L4", "R1", "R2", "R3", "R4"])]
    [GroupPreset("Hector: THMR", [0, 4, 1, 5, 2, 6, 3, 7])]
    [GroupPreset("LPDU: HTTH/RMMR", [1, 2, 0, 3, 5, 6, 4, 7])]
    public GroupAssignmentUnique P3QuickmarchTrioAssignments = new() { Assignments = [0, 4, 1, 5, 2, 6, 3, 7] };

    [PropertyDisplay("P3 Heavensfall Trio: tower priority, CW starting from nael")]
    [GroupDetails(["0", "1", "2", "3", "4", "5", "6", "7"])]
    [GroupPreset("Hector: THMR, G1 CCW, G2 CW", [7, 0, 6, 1, 5, 2, 4, 3])]
    public GroupAssignmentUnique P3HeavensfallTrioTowers = new() { Assignments = [7, 0, 6, 1, 5, 2, 4, 3] };
}
