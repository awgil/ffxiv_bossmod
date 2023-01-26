namespace BossMod.Endwalker.Ultimate.TOP
{
    [ConfigDisplay(Order = 0x210, Parent = typeof(EndwalkerConfig))]
    public class TOPConfig : CooldownPlanningConfigNode
    {
        [PropertyDisplay("Program Loop/Pantokrator: assignments (lower priority flexes on conflict, G1 CW from NW, G2 CCW)")]
        [GroupDetails(new string[] { "G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4" })]
        [GroupPreset("Role", new[] { 0, 1, 2, 3, 4, 5, 6, 7 })]
        [GroupPreset("Role inverted", new[] { 4, 5, 6, 7, 0, 1, 2, 3 })]
        [GroupPreset("Light Party TMRH", new[] { 0, 4, 3, 7, 1, 5, 2, 6 })]
        [GroupPreset("Light Party TMRH inverted", new[] { 4, 0, 7, 3, 5, 1, 6, 2 })]
        public GroupAssignmentUnique ProgramLoopPantokratorAssignments = GroupAssignmentUnique.Default();

        public TOPConfig() : base(90) { }
    }
}
