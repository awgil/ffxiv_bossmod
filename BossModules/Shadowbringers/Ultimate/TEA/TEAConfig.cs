namespace BossMod.Shadowbringers.Ultimate.TEA;

public class GroupAssignmentFourUnique : GroupAssignment
{
    public static GroupAssignmentFourUnique Default()
    {
        var r = new GroupAssignmentFourUnique();
        r[PartyRolesConfig.Assignment.M1] = 3;
        r[PartyRolesConfig.Assignment.M2] = 2;
        r[PartyRolesConfig.Assignment.R1] = 1;
        r[PartyRolesConfig.Assignment.R2] = 0;
        r[PartyRolesConfig.Assignment.MT] = r[PartyRolesConfig.Assignment.OT] = r[PartyRolesConfig.Assignment.H1] = r[PartyRolesConfig.Assignment.H2] = 4;
        return r;
    }

    public override bool Validate()
    {
        var assigned = new int[5];

        for (var i = 0; i < (int)PartyRolesConfig.Assignment.Unassigned; i++)
        {
            if (Assignments[i] >= 0)
                assigned[Assignments[i]]++;
        }

        // TODO: implement doll skip, i don't know what people do with dolls there
        //if (assigned.Sum() == 0)
        //    return true;

        return assigned[0] == 1 && assigned[1] == 1 && assigned[2] == 1 && assigned[3] == 1;
    }
}

[ConfigDisplay(Order = 0x200, Parent = typeof(ShadowbringersConfig))]
public class TEAConfig() : ConfigNode()
{
    [PropertyDisplay("P1: Doll assignments (middle tornado = relative south)")]
    [GroupDetails(["NW", "NE", "SE", "SW", "Ignore"])]
    [GroupPreset("NA standard", [4, 4, 4, 4, 3, 2, 1, 0])]
    //[GroupPreset("Ignore dolls", [4, 4, 4, 4, 4, 4, 4, 4])]
    public GroupAssignmentFourUnique P1DollAssignments = GroupAssignmentFourUnique.Default();

    [PropertyDisplay("P1: Mark other players' dolls as forbidden", tooltip: "Requires a valid configuration for 'Doll assignments' (the text must not be yellow).\n\nVBM autorot will not use actions on forbidden targets, and will also prevent you from doing so manually if you have 'Use custom queueing' enabled in the Action Tweaks settings.\n\nIf necessary, you can use actions on forbidden targets by pressing the action twice quickly.")]
    public bool P1DollPullSafety = true;

    public enum P2Intermission
    {
        [PropertyDisplay("Don't show any hints")]
        None,

        [PropertyDisplay("Always show W->NE hints")]
        AlwaysFirst,

        [PropertyDisplay("W->NE for 1/2/5/6, E->SW for 3/4/7/8")]
        FirstForOddPairs,
    }

    [PropertyDisplay("Intermission: safespot hints")]
    public P2Intermission P2IntermissionHints = P2Intermission.FirstForOddPairs;

    [PropertyDisplay("P2: nisi pair assignments")]
    [GroupDetails(["Pair 1", "Pair 2", "Pair 3", "Pair 4"])]
    [GroupPreset("Melee together", [0, 1, 2, 3, 0, 1, 2, 3])]
    [GroupPreset("DD CCW", [0, 2, 1, 3, 1, 0, 2, 3])]
    public GroupAssignmentDDSupportPairs P2NisiPairs = GroupAssignmentDDSupportPairs.DefaultMeleeTogether();
}
