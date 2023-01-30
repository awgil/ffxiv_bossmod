namespace BossMod.Endwalker.Ultimate.TOP
{
    [ConfigDisplay(Order = 0x210, Parent = typeof(EndwalkerConfig))]
    public class TOPConfig : CooldownPlanningConfigNode
    {
        [PropertyDisplay("P1 Program Loop: assignments (lower-numbered priority flexes on conflict - so prio1 most likely to flex, G1 CW from NW, G2 CCW)")]
        [GroupDetails(new string[] { "G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4" })]
        [GroupPreset("Role", new[] { 0, 1, 2, 3, 4, 5, 6, 7 })]
        [GroupPreset("Role inverted", new[] { 4, 5, 6, 7, 0, 1, 2, 3 })]
        [GroupPreset("Light Party TMRH", new[] { 0, 4, 3, 7, 1, 5, 2, 6 })]
        [GroupPreset("Light Party TMRH inverted", new[] { 4, 0, 7, 3, 5, 1, 6, 2 })]
        [GroupPreset("Light Party RTMH", new[] { 1, 5, 3, 7, 2, 6, 0, 4 })]
        [GroupPreset("Light Party RTMH inverted", new[] { 5, 1, 7, 3, 6, 2, 4, 0 })]
        [GroupPreset("Global THMR", new[] { 0, 1, 2, 3, 4, 5, 6, 7 })]
        [GroupPreset("Global THMR reverse", new[] { 7, 6, 5, 4, 3, 2, 1, 0 })]
        [GroupPreset("Global RMTH", new[] { 4, 5, 6, 7, 2, 3, 0, 1 })]
        [GroupPreset("Global RMTH reverse", new[] { 3, 2, 1, 0, 5, 4, 7, 6 })]
        public GroupAssignmentUnique P1ProgramLoopAssignments = GroupAssignmentUnique.Default();

        [PropertyDisplay("P1 Program Loop: use global priority instead - consider G1 lower-numbered than G2, so G1 more likely to flex")]
        public bool P1ProgramLoopGlobalPriority = false;

        [PropertyDisplay("P1 Pantokrator: assignments (lower-numbered priority flexes on conflict - so prio1 most likely to flex, G1 CW from NW, G2 CCW)")]
        [GroupDetails(new string[] { "G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4" })]
        [GroupPreset("Role", new[] { 0, 1, 2, 3, 4, 5, 6, 7 })]
        [GroupPreset("Role inverted", new[] { 4, 5, 6, 7, 0, 1, 2, 3 })]
        [GroupPreset("Light Party TMRH", new[] { 0, 4, 3, 7, 1, 5, 2, 6 })]
        [GroupPreset("Light Party TMRH inverted", new[] { 4, 0, 7, 3, 5, 1, 6, 2 })]
        [GroupPreset("Light Party RTMH", new[] { 1, 5, 3, 7, 2, 6, 0, 4 })]
        [GroupPreset("Light Party RTMH inverted", new[] { 5, 1, 7, 3, 6, 2, 4, 0 })]
        [GroupPreset("Global THMR", new[] { 0, 1, 2, 3, 4, 5, 6, 7 })]
        [GroupPreset("Global THMR reverse", new[] { 7, 6, 5, 4, 3, 2, 1, 0 })]
        [GroupPreset("Global RMTH", new[] { 4, 5, 6, 7, 2, 3, 0, 1 })]
        [GroupPreset("Global RMTH reverse", new[] { 3, 2, 1, 0, 5, 4, 7, 6 })]
        public GroupAssignmentUnique P1PantokratorAssignments = GroupAssignmentUnique.Default();

        [PropertyDisplay("P1 Pantokrator: use global priority instead - consider G1 lower-numbered than G2, so G1 more likely to flex")]
        public bool P1PantokratorGlobalPriority = false;

        [PropertyDisplay("P2 Party Synergy: assignments (lower-numbered priority flexes on conflict - so prio1 most likely to flex, G1 left, G2 right if looking at eye)")]
        [GroupDetails(new string[] { "G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4" })]
        [GroupPreset("Role", new[] { 0, 1, 2, 3, 4, 5, 6, 7 })]
        [GroupPreset("Role inverted", new[] { 4, 5, 6, 7, 0, 1, 2, 3 })]
        [GroupPreset("Light Party TMRH", new[] { 0, 4, 3, 7, 1, 5, 2, 6 })]
        [GroupPreset("Light Party TMRH inverted", new[] { 4, 0, 7, 3, 5, 1, 6, 2 })]
        [GroupPreset("Light Party RTMH", new[] { 1, 5, 3, 7, 2, 6, 0, 4 })]
        [GroupPreset("Light Party RTMH inverted", new[] { 5, 1, 7, 3, 6, 2, 4, 0 })]
        [GroupPreset("Global THMR", new[] { 0, 1, 2, 3, 4, 5, 6, 7 })]
        [GroupPreset("Global THMR reverse", new[] { 7, 6, 5, 4, 3, 2, 1, 0 })]
        [GroupPreset("Global RMTH", new[] { 4, 5, 6, 7, 2, 3, 0, 1 })]
        [GroupPreset("Global RMTH reverse", new[] { 3, 2, 1, 0, 5, 4, 7, 6 })]
        public GroupAssignmentUnique P2PartySynergyAssignments = GroupAssignmentUnique.Default();

        [PropertyDisplay("P2 Party Synergy: use global priority instead - consider G1 lower-numbered than G2, so G1 more likely to flex")]
        public bool P2PartySynergyGlobalPriority = false;

        public TOPConfig() : base(90) { }
    }
}
