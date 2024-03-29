namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

[ConfigDisplay(Order = 0x181, Parent = typeof(EndwalkerConfig))]
public class P8S1Config : CooldownPlanningConfigNode
{
    [PropertyDisplay("Snake 1: assignments")]
    [GroupDetails(new string[] { "Prio 1 (always CW from N)", "Prio 2 (flex CW from N)", "Prio 3 (flex CCW from NW)", "Prio 4 (always CCW from NW)" })]
    [GroupPreset("PF", new[] { 0, 1, 2, 3, 0, 1, 2, 3 })]
    [GroupPreset("Hector", new[] { 2, 1, 3, 0, 2, 1, 3, 0 })]
    public GroupAssignmentDDSupportPairs Snake1Assignments = GroupAssignmentDDSupportPairs.DefaultMeleeTogether();

    [PropertyDisplay("Snake 2: assignments")]
    [GroupDetails(new[] { "G1 (NW/CCW) flex", "G2 (SE/CW) flex", "G1 (NW/CCW) fixed", "G2 (SE/CW) fixed" })]
    [GroupPreset("PF/Hector", new[] { 0, 1, 2, 3, 0, 1, 2, 3 })]
    public GroupAssignmentDDSupportPairs Snake2Assignments = GroupAssignmentDDSupportPairs.DefaultMeleeTogether();

    [PropertyDisplay("Snake 2: use cardinal priorities (G1 N/W, PF strat) instead of ordering (G1 first safe CCW from NW, Hector strat)")]
    public bool Snake2CardinalPriorities = true;

    public P8S1Config() : base(90) { }
}
