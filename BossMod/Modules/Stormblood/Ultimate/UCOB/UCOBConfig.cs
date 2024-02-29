namespace BossMod.Stormblood.Ultimate.UCOB
{
    [ConfigDisplay(Order = 0x200, Parent = typeof(StormbloodConfig))]
    public class UCOBConfig : CooldownPlanningConfigNode
    {
        public UCOBConfig() : base(70) { }

        [PropertyDisplay("P3 Quickmarch Trio: assignments (assuming bahamut is relative north/up, group L goes left; L1/R1 closest to bahamut)")]
        [GroupDetails(["L1", "L2", "L3", "L4", "R1", "R2", "R3", "R4" ])]
        [GroupPreset("Hector: THMR", [0, 4, 1, 5, 2, 6, 3, 7])]
        [GroupPreset("LPDU: HTTH/RMMR", [1, 2, 0, 3, 5, 6, 4, 7])]
        public GroupAssignmentUnique P3QuickmarchTrioAssignments = new() { Assignments = [0, 4, 1, 5, 2, 6, 3, 7] };
    }
}
