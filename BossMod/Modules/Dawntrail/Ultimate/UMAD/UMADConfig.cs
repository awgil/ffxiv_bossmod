namespace BossMod.Dawntrail.Ultimate.UMAD;

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
public class UMADConfig : ConfigNode
{
    [PropertyDisplay("P1 Graven 1: conga line order for Wave Cannon (W -> E)")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("HHTTMMRR", [3, 2, 1, 0, 4, 5, 6, 7])]
    public GroupAssignmentUnique P1WaveCannonConga = new() { Assignments = [3, 2, 1, 0, 4, 5, 6, 7] };

    public enum P1ArrowShape
    {
        None,
        [PropertyDisplay("Big box (CW)")]
        BigBox,
    }

    [PropertyDisplay("P1 Tele-Portent: arrow placement hints")]
    public P1ArrowShape P1Arrows = P1ArrowShape.BigBox;

    public enum P2ForsakenStrategyType
    {
        None,
        [PropertyDisplay("Kroxy/Rinon: AAABBBBA, role pairs")]
        KroxyRinon
    }

    [PropertyDisplay("P2 Forsaken strategy")]
    public P2ForsakenStrategyType P2ForsakenStrategy = P2ForsakenStrategyType.None;

    [PropertyDisplay("P2 Forsaken: pair assignments")]
    [GroupDetails(["1", "2", "3", "4"])]
    [GroupPreset("HTMR, role pairs", [0, 1, 0, 1, 2, 3, 2, 3])]
    public GroupAssignmentRolePairs P2ForsakenPairs = GroupAssignmentRolePairs.MeleeRanged();

    [PropertyDisplay("P2 Forsaken: tower priority", tooltip: "Ordered left to right, looking at boss; 1 = always in left tower, 8 = always in right tower")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("HHTTMMRR", [3, 2, 1, 0, 4, 5, 6, 7])]
    public GroupAssignmentUnique P2ForsakenTiebreaker = new() { Assignments = [3, 2, 1, 0, 4, 5, 6, 7] };

    public enum P3BlackholeStrategyType
    {
        None,
        [PropertyDisplay("DPS -> Support -> Accretion")]
        DSA,
        [PropertyDisplay("Support -> DPS -> Accretion")]
        SDA,
        [PropertyDisplay("DSA with double tethers")]
        DoubleTether
    }

    [PropertyDisplay("P3 Blackhole strategy")]
    public P3BlackholeStrategyType P3BlackholeStrategy = P3BlackholeStrategyType.None;

    public enum P1GravityPuddlePlacement
    {
        None,
        [PropertyDisplay("Whole party stack on A/C marker")]
        StackAll
    }

    [SectionStart("AI-only settings")]
    [PropertyDisplay("P1 Gravitas: Puddle drop strategy")]
    public P1GravityPuddlePlacement P1GravityPuddleStrategy = P1GravityPuddlePlacement.None;

    [PropertyDisplay("P1 Gravitas: Spread destinations")]
    [GroupDetails(["G1 (left)", "G2 (right)"])]
    public GroupAssignmentLightParties P1GravityPuddleSpread = GroupAssignmentLightParties.DefaultLightParties();
}

public class GroupAssignmentRolePairs : GroupAssignment
{
    public static GroupAssignmentRolePairs MeleeRanged()
    {
        var t = new GroupAssignmentRolePairs();
        t[PartyRolesConfig.Assignment.MT] = t[PartyRolesConfig.Assignment.H1] = 0;
        t[PartyRolesConfig.Assignment.OT] = t[PartyRolesConfig.Assignment.H2] = 1;
        t[PartyRolesConfig.Assignment.M1] = t[PartyRolesConfig.Assignment.R1] = 2;
        t[PartyRolesConfig.Assignment.M2] = t[PartyRolesConfig.Assignment.R2] = 3;
        return t;
    }

    public override bool Validate()
    {
        var counts = new int[4];
        for (var i = 0; i < Assignments.Length; i++)
            if (Assignments[i] is >= 0 and < 4)
                counts[Assignments[i]]++;
        return counts.All(c => c == 2);
    }
}

public static class BlackholeOrder
{
    public static readonly (char Role, int Order)[][] DSA = [
        [('D', 1)],
        [('D', 1), ('S', 1)],

        [('D', 1), ('S', 1), ('A', 1)],
        [('D', 2), ('S', 1), ('A', 1)],
        [('D', 2), ('S', 2), ('A', 1)],

        [('D', 2), ('S', 2), ('A', 2)],
        [('D', 3), ('S', 2), ('A', 2)],
        [('D', 3), ('S', 3), ('A', 2)],

        [('D', 3), ('S', 3)],
        [('S', 3)],
    ];

    public static readonly (char Role, int Order)[][] SDA = [
        [('S', 1)],
        [('S', 1), ('D', 1)],

        [('S', 1), ('D', 1), ('A', 1)],
        [('S', 2), ('D', 1), ('A', 1)],
        [('S', 2), ('D', 2), ('A', 1)],

        [('S', 2), ('D', 2), ('A', 2)],
        [('S', 3), ('D', 2), ('A', 2)],
        [('S', 3), ('D', 3), ('A', 2)],

        [('S', 3), ('D', 3)],
        [('D', 3)],
    ];

    public static readonly (char Role, int Order)[][] DoubleTether = [
        [('S', 1)],
        [('D', 1), ('D', 1)],

        [('D', 1), ('S', 1), ('A', 1)],
        [('D', 2), ('S', 1), ('A', 1)],
        [('D', 2), ('S', 2), ('A', 1)],

        [('D', 2), ('S', 2), ('A', 2)],
        [('D', 3), ('S', 2), ('A', 2)],
        [('D', 3), ('S', 3), ('A', 2)],

        [('S', 3), ('S', 3)],
        [('D', 3)]
    ];
}
