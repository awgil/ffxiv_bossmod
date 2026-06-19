namespace BossMod.Dawntrail.Ultimate.UMAD;

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
public class UMADConfig : ConfigNode
{
    [PropertyDisplay("P1 Gravitas 1: conga line order for Wave Cannon (W -> E)")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("HHTTMMRR", [3, 2, 1, 0, 4, 5, 6, 7])]
    public GroupAssignmentUnique P1WaveCannonConga = new() { Assignments = [3, 2, 1, 0, 4, 5, 6, 7] };

    public enum P1ArrowShape
    {
        None,
        [PropertyDisplay("Big box (CW)")]
        BigBox,
        [PropertyDisplay("Big box 'freaky' (cardinal arrows are slightly closer to the boss)")]
        Freaky
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

    //[SectionStart("AI-only settings")]
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
