namespace BossMod.Dawntrail.Ultimate.UMAD;

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
public class UMADConfig : ConfigNode
{
    public enum P1ArrowShape
    {
        None,
        [PropertyDisplay("Big box (CW)")]
        BigBox
    }

    [PropertyDisplay("P1 Tele-Portent: arrow placement hints")]
    public P1ArrowShape P1Arrows = P1ArrowShape.BigBox;

    [PropertyDisplay("P2 Forsaken: pair assignments")]
    public GroupAssignmentRolePairs P2ForsakenPairs = GroupAssignmentRolePairs.MeleeRanged();
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
        for (int i = 0; i < Assignments.Length; i++)
            counts[Assignments[i]]++;
        return counts.All(x => x == 2);
    }
}
