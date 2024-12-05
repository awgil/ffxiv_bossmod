namespace BossMod.Dawntrail.Ultimate.FRU;

[ConfigDisplay(Order = 0x200, Parent = typeof(DawntrailConfig))]
public class FRUConfig() : ConfigNode()
{
    // TODO: fixed tethers option
    [PropertyDisplay("P1 Bound of Faith (light party tethers): group assignments & flex priority (lower number flexes)")]
    [GroupDetails(["N prio 1", "N prio 2", "N prio 3", "N prio 4", "S prio 1", "S prio 2", "S prio 3", "S prio 4"])]
    [GroupPreset("Supports N, DD S", [0, 1, 2, 3, 4, 5, 6, 7])]
    public GroupAssignmentUnique P1BoundOfFaithAssignment = GroupAssignmentUnique.DefaultRoles();

    [PropertyDisplay("P1 Fall of Faith (cone tethers) : conga priority (two people without tethers with lower priorities join odd group)")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("TTHHMMRR", [0, 1, 2, 3, 4, 5, 6, 7])]
    [GroupPreset("RHMTTMHR", [3, 4, 1, 6, 2, 5, 0, 7])]
    public GroupAssignmentUnique P1FallOfFaithAssignment = GroupAssignmentUnique.DefaultRoles();

    [PropertyDisplay("P1 Fall of Faith (cone tethers): odd groups go W (rather than N)")]
    public bool P1FallOfFaithEW = false;

    [PropertyDisplay("P1 Explosions: tower fill priority (lower number goes north)")]
    [GroupDetails(["MT (ignore)", "OT (ignore)", "Fixed N", "Fixed Center", "Fixed S", "Flex 1", "Flex 2", "Flex 3"])]
    [GroupPreset("H1-R2-H2 fixed, M1-M2-R1 flex", [0, 1, 2, 4, 5, 6, 7, 3])]
    public GroupAssignmentUnique P1ExplosionsAssignment = GroupAssignmentUnique.DefaultRoles();

    [PropertyDisplay("P2 Diamond Dust: cardinal assignments")]
    [GroupDetails(["Support N", "Support E", "Support S", "Support W", "DD N", "DD E", "DD S", "DD W"])]
    public GroupAssignmentUnique P2DiamondDustCardinals = GroupAssignmentUnique.DefaultRoles();

    [PropertyDisplay("P2 Diamond Dust: supports go to CCW intercardinal")]
    public bool P2DiamondDustSupportsCCW;

    [PropertyDisplay("P2 Diamond Dust: DD go to CCW intercardinal")]
    public bool P2DiamondDustDDCCW;

    [PropertyDisplay("P2 Light Rampant: conga spots (lower numbers to the west, assume CW rotation)")]
    [GroupDetails(["N1", "N2", "N3", "N4", "S1", "S2", "S3", "S4"])]
    [GroupPreset("HHTT/RRMM", [2, 3, 0, 1, 6, 7, 4, 5])]
    public GroupAssignmentUnique P2LightRampantAssignment = GroupAssignmentUnique.DefaultRoles();

    [PropertyDisplay("P3 Ultimate Relativity: spot assignments (lower number goes NW/SW)")]
    [GroupDetails(["1", "2", "3", "4"])]
    [GroupPreset("HTTH/RMMR", [1, 2, 0, 3, 1, 2, 0, 3])]
    public GroupAssignmentDDSupportPairs P3UltimateRelativityAssignment = GroupAssignmentDDSupportPairs.DefaultMeleeTogether();
}
