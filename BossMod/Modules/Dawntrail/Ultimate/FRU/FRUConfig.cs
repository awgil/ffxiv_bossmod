namespace BossMod.Dawntrail.Ultimate.FRU;

[ConfigDisplay(Order = 0x200, Parent = typeof(DawntrailConfig))]
public class FRUConfig() : ConfigNode()
{
    // TODO: fixed tethers option
    [PropertyDisplay("P1 Bound of Faith (light party tethers): group assignments & flex priority (lower number flexes)")]
    [GroupDetails(["N prio 1", "N prio 2", "N prio 3", "N prio 4", "S prio 1", "S prio 2", "S prio 3", "S prio 4"])]
    [GroupPreset("Supports N, DD S", [0, 1, 2, 3, 4, 5, 6, 7])]
    [GroupPreset("G1 N, G2 S, TMRH", [0, 4, 3, 7, 1, 5, 2, 6])]
    public GroupAssignmentUnique P1BoundOfFaithAssignment = GroupAssignmentUnique.DefaultRoles();

    [PropertyDisplay("P1 Fall of Faith (cone tethers): conga priority (two people without tethers with lower priorities join odd group)")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("TTHHMMRR", [0, 1, 2, 3, 4, 5, 6, 7])]
    [GroupPreset("RHMTTMHR", [3, 4, 1, 6, 2, 5, 0, 7])]
    public GroupAssignmentUnique P1FallOfFaithAssignment = GroupAssignmentUnique.DefaultRoles();

    [PropertyDisplay("P1 Fall of Faith (cone tethers): odd groups go W (rather than N)")]
    public bool P1FallOfFaithEW = false;

    [PropertyDisplay("P1 Explosions: tower fill priority (lower number goes north)")]
    [GroupDetails(["Tank N", "Tank S", "Fixed N", "Fixed Center", "Fixed S", "Flex 1", "Flex 2", "Flex 3"])]
    [GroupPreset("H1-R2-H2 fixed, M1-M2-R1 flex", [0, 1, 2, 4, 5, 6, 7, 3])]
    public GroupAssignmentUnique P1ExplosionsAssignment = new() { Assignments = [0, 1, 2, 4, 5, 6, 7, 3] };

    [PropertyDisplay("P1 Explosions: flex roles only fill 3/4 if their natural tower is 1 (instead of doing conga)")]
    public bool P1ExplosionsPriorityFill;

    [PropertyDisplay("P1 Explosions: have tanks stack on tankbuster (survivable with saves, simplifies uptime)")]
    public bool P1ExplosionsTankbusterCheese;

    [PropertyDisplay("P2 Diamond Dust: cardinal assignments")]
    [GroupDetails(["Support N", "Support E", "Support S", "Support W", "DD N", "DD E", "DD S", "DD W"])]
    [GroupPreset("Default", [0, 2, 3, 1, 7, 6, 4, 5])]
    public GroupAssignmentUnique P2DiamondDustCardinals = new() { Assignments = [0, 2, 3, 1, 7, 6, 4, 5] };

    [PropertyDisplay("P2 Diamond Dust: supports go to CCW intercardinal")]
    public bool P2DiamondDustSupportsCCW;

    [PropertyDisplay("P2 Diamond Dust: DD go to CCW intercardinal")]
    public bool P2DiamondDustDDCCW;

    [PropertyDisplay("P2 Diamond Dust: knockback groups")]
    [GroupDetails(["G1 (CCW from N)", "G2 (CW from NE)"])]
    public GroupAssignmentLightParties P2DiamondDustKnockbacks = GroupAssignmentLightParties.DefaultLightParties();

    [PropertyDisplay("P2 Light Rampant: conga spots (lower numbers to the west, assume CW rotation)")]
    [GroupDetails(["N1", "N2", "N3", "N4", "S1", "S2", "S3", "S4"])]
    [GroupPreset("HHTT/RRMM", [2, 3, 0, 1, 6, 7, 4, 5])]
    public GroupAssignmentUnique P2LightRampantAssignment = GroupAssignmentUnique.DefaultRoles();

    [PropertyDisplay("P3 Ultimate Relativity: spot assignments (lower number goes NW/SW)")]
    [GroupDetails(["1", "2", "3", "4"])]
    [GroupPreset("HTTH/RMMR", [1, 2, 0, 3, 1, 2, 0, 3])]
    public GroupAssignmentDDSupportPairs P3UltimateRelativityAssignment = GroupAssignmentDDSupportPairs.DefaultMeleeTogether();

    [PropertyDisplay("P3 Apocalypse: assignments (G1 CCW from N, G2 CW from NE, in case of conflict 'lower' number flexes)")]
    [GroupDetails(["G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4"])]
    [GroupPreset("TTHH/MMRR", [0, 1, 2, 3, 4, 5, 6, 7])]
    [GroupPreset("TMRH/TMRH", [0, 4, 3, 7, 1, 5, 2, 6])]
    public GroupAssignmentUnique P3ApocalypseAssignments = GroupAssignmentUnique.DefaultRoles();

    [PropertyDisplay("P3 Apocalypse: uptime swaps (only consider swaps within prio 1/2 and 3/4, assuming these are melee and ranged)")]
    public bool P3ApocalypseUptime;

    [PropertyDisplay("P4 Darklit Dragonsong: assignments (lower prio stays more clockwise, lowest prio support takes N tower)")]
    [GroupDetails(["Support prio1", "Support prio2", "Support prio3", "Support prio4", "DD prio1", "DD prio2", "DD prio3", "DD prio4"])]
    [GroupPreset("Default (healer N)", [2, 3, 0, 1, 4, 5, 6, 7])]
    public GroupAssignmentUnique P4DarklitDragonsongAssignments = new() { Assignments = [2, 3, 0, 1, 4, 5, 6, 7] };

    [PropertyDisplay("P4 Crystallize Time: assignments for claws (lower prio goes west)", separator: true)]
    [GroupDetails(["Prio 1", "Prio 2", "Prio 3", "Prio 4", "Prio 5", "Prio 6", "Prio 7", "Prio 8"])]
    [GroupPreset("Default HTMR", [3, 2, 1, 0, 4, 5, 6, 7])]
    public GroupAssignmentUnique P4CrystallizeTimeAssignments = new() { Assignments = [3, 2, 1, 0, 4, 5, 6, 7] };

    // ai-only settings
    [PropertyDisplay("P1 Cyclonic Break (proteans): bait clock spots (supports should be near dd to resolve pairs)", tooltip: "Only used by AI")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    [GroupPreset("Default", [0, 4, 6, 2, 5, 3, 7, 1])]
    public GroupAssignmentUnique P1CyclonicBreakSpots = new() { Assignments = [0, 4, 6, 2, 5, 3, 7, 1] };

    [PropertyDisplay("P1 Cyclonic Break (proteans): pairs dodge direction", tooltip: "Only used by AI")]
    [PropertyCombo("Supports CW, DD CCW", "Supports CCW, DD CW")]
    public bool P1CyclonicBreakStackSupportsCCW = true;

    [PropertyDisplay("P1 Cyclonic Break (proteans): spread dodge direction for supports", tooltip: "Only used by AI")]
    [PropertyCombo("CW", "CCW")]
    public bool P1CyclonicBreakSpreadSupportsCCW;

    [PropertyDisplay("P1 Cyclonic Break (proteans): spread dodge direction for DD", tooltip: "Only used by AI")]
    [PropertyCombo("CW", "CCW")]
    public bool P1CyclonicBreakSpreadDDCCW;

    [PropertyDisplay("P1 Utopian Sky: initial clock spots (MT should be near OT to resolve tankbuster)", tooltip: "Only used by AI")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    [GroupPreset("Default", [0, 1, 6, 2, 5, 3, 7, 4])]
    public GroupAssignmentUnique P1UtopianSkyInitialSpots = new() { Assignments = [0, 1, 6, 2, 5, 3, 7, 4] };

    [PropertyDisplay("P1 Utopian Sky: spread spots (G1 CCW from N, G2 CW from NE)", tooltip: "Only used by AI")]
    [GroupDetails(["G1 Close", "G1 Far Center", "G1 Far Left", "G1 Far Right", "G2 Close", "G2 Far Center", "G2 Far Left", "G2 Far Right"])]
    [GroupPreset("Default", [1, 5, 0, 4, 2, 6, 3, 7])]
    public GroupAssignmentUnique P1UtopianSkySpreadSpots = new() { Assignments = [1, 5, 0, 4, 2, 6, 3, 7] };

    [PropertyDisplay("P2 Mirror Mirror: spread spots for first proteans (looking from boss toward blue mirror)", tooltip: "Only used by AI")]
    [GroupDetails(["Boss opposite right", "Boss opposite left", "Boss side right", "Boss side left", "Mirror diagonal right", "Mirror diagonal left", "Mirror wall right", "Mirror wall left"])]
    [GroupPreset("Default", [0, 1, 4, 5, 2, 3, 6, 7])]
    public GroupAssignmentUnique P2MirrorMirror1SpreadSpots = new() { Assignments = [0, 1, 4, 5, 2, 3, 6, 7] };

    [PropertyDisplay("P2 Mirror Mirror: spread spots for second proteans (looking toward red mirror, if both red mirrors are symmetrical assume CW rotation)", tooltip: "Only used by AI")]
    [GroupDetails(["Boss wall right", "Boss wall left", "Boss center", "Boss diagonal", "Mirror wall right", "Mirror wall left", "Mirror center", "Mirror diagonal"])]
    [GroupPreset("Default", [1, 0, 6, 7, 2, 3, 4, 5])]
    public GroupAssignmentUnique P2MirrorMirror2SpreadSpots = new() { Assignments = [1, 0, 6, 7, 2, 3, 4, 5] };

    [PropertyDisplay("P2 Banish after Light Rampant: spread clock spots (supports should be near dd to resolve pairs)", tooltip: "Only used by AI")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    [GroupPreset("Default", [0, 4, 6, 2, 5, 3, 7, 1])]
    public GroupAssignmentUnique P2Banish2SpreadSpots = new() { Assignments = [0, 4, 6, 2, 5, 3, 7, 1] };

    [PropertyDisplay("P2 Banish after Light Rampant: role that moves from their default spread spot to resolve pairs", tooltip: "Only used by AI")]
    [PropertyCombo("DD", "Supports")]
    public bool P2Banish2SupportsMoveToStack = true;

    [PropertyDisplay("P2 Banish after Light Rampant: direction to move to resolve pairs", tooltip: "Only used by AI")]
    [PropertyCombo("CW", "CCW")]
    public bool P2Banish2MoveCCWToStack = true;
}
