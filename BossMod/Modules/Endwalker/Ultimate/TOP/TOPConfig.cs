namespace BossMod.Endwalker.Ultimate.TOP
{
    [ConfigDisplay(Order = 0x210, Parent = typeof(EndwalkerConfig))]
    public class TOPConfig : CooldownPlanningConfigNode
    {
        [PropertyDisplay("P1 Program Loop: assignments (G1 CW from NW, G2 CCW by default, in case of conflict 'lower' number flexes)")]
        [GroupDetails(new string[] { "G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4" })]
        [GroupPreset("LPDU (global): M1>M2>MT>OT>R1>R2>H1>H2", new[] { 5, 4, 1, 0, 7, 6, 3, 2 })]
        public GroupAssignmentUnique P1ProgramLoopAssignments = new() { Assignments = new[]{ 5, 4, 1, 0, 7, 6, 3, 2 } };

        [PropertyDisplay("P1 Program Loop: use global priority instead - consider G1 lower-numbered than G2 (so G1 more likely to flex)")]
        public bool P1ProgramLoopGlobalPriority = true;

        [PropertyDisplay("P1 Pantokrator: assignments (G1 N, G2 S, adjust CW by default, in case of conflict 'lower' number flexes)")]
        [GroupDetails(new string[] { "G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4" })]
        [GroupPreset("LPDU (light parties): flex T>M>R", new[] { 0, 4, 3, 7, 1, 5, 2, 6 })]
        public GroupAssignmentUnique P1PantokratorAssignments = new() { Assignments = new[] { 0, 4, 3, 7, 1, 5, 2, 6 } };

        [PropertyDisplay("P1 Pantokrator: use global priority instead - consider G1 lower-numbered than G2 (so G1 more likely to flex)")]
        public bool P1PantokratorGlobalPriority = false;

        [PropertyDisplay("P2 Party Synergy: assignments (G1 left, G2 right if looking at eye, in case of conflict 'lower' number flexes)")]
        [GroupDetails(new string[] { "G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4" })]
        [GroupPreset("LPDU (light parties): flex R>M>H", new[] { 3, 7, 2, 6, 1, 5, 0, 4 })]
        public GroupAssignmentUnique P2PartySynergyAssignments = new() { Assignments = new[] { 3, 7, 2, 6, 1, 5, 0, 4 } };

        [PropertyDisplay("P2 Party Synergy: use global priority instead - consider G1 lower-numbered than G2 (so G1 more likely to flex)")]
        public bool P2PartySynergyGlobalPriority = false;

        public TOPConfig() : base(90) { }
    }
}
