namespace BossMod;

// attribute that specifies group count and names for group assignment property
[AttributeUsage(AttributeTargets.Field)]
public sealed class GroupDetailsAttribute(string[] names) : Attribute
{
    public string[] Names { get; } = names;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class GroupPresetAttribute(string name, int[] preset) : Attribute
{
    public string Name { get; } = name;
    public int[] Preset { get; } = preset;
}

// config node property that allows assigning party roles to arbitrary named groups
// typically you would use derived classes that provide validation
public class GroupAssignment
{
    public int[] Assignments = Utils.MakeArray((int)PartyRolesConfig.Assignment.Unassigned, -1); // assignment -> group id

    public int this[PartyRolesConfig.Assignment r]
    {
        get => Assignments[(int)r];
        set => Assignments[(int)r] = value;
    }

    public virtual bool Validate() => true;

    // if these role->group assignments are valid and passed actor->role assignments are valid for passed raid, enumerate slot/group pairs
    // if anything is invalid, enumerable is empty
    public IEnumerable<(int slot, int group)> Resolve(PartyState party, PartyRolesConfig actorAssignments)
    {
        if (Validate())
        {
            var roleToSlot = actorAssignments.SlotsPerAssignment(party);
            if (roleToSlot.Length == Assignments.Length)
            {
                for (int role = 0; role < Assignments.Length; ++role)
                {
                    yield return (roleToSlot[role], Assignments[role]);
                }
            }
        }
    }

    // build slot mask for members of specified group; returns 0 if resolve fails
    public BitMask BuildGroupMask(int group, PartyState party, PartyRolesConfig actorAssignments)
    {
        BitMask mask = new();
        foreach (var (slot, g) in Resolve(party, actorAssignments))
            if (g == group)
                mask.Set(slot);
        return mask;
    }

    // shortcuts using global config
    public IEnumerable<(int slot, int group)> Resolve(PartyState party) => Resolve(party, Service.Config.Get<PartyRolesConfig>());
    public BitMask BuildGroupMask(int group, PartyState party) => BuildGroupMask(group, party, Service.Config.Get<PartyRolesConfig>());
}

// assignments to two light parties with THMR split
public class GroupAssignmentLightParties : GroupAssignment
{
    public static GroupAssignmentLightParties DefaultLightParties()
    {
        var r = new GroupAssignmentLightParties();
        r[PartyRolesConfig.Assignment.MT] = r[PartyRolesConfig.Assignment.H1] = r[PartyRolesConfig.Assignment.M1] = r[PartyRolesConfig.Assignment.R1] = 0;
        r[PartyRolesConfig.Assignment.OT] = r[PartyRolesConfig.Assignment.H2] = r[PartyRolesConfig.Assignment.M2] = r[PartyRolesConfig.Assignment.R2] = 1;
        return r;
    }

    public override bool Validate()
    {
        for (int i = 0; i < (int)PartyRolesConfig.Assignment.Unassigned; i += 2)
            if (Assignments[i] < 0 || Assignments[i] >= 2 || Assignments[i + 1] < 0 || Assignments[i + 1] >= 2 || Assignments[i] == Assignments[i + 1])
                return false;
        return true;
    }
}

// assignments to four tank/healer+DD pairs
public class GroupAssignmentDDSupportPairs : GroupAssignment
{
    public static GroupAssignmentDDSupportPairs DefaultOneMeleePerPair()
    {
        GroupAssignmentDDSupportPairs r = new();
        r[PartyRolesConfig.Assignment.MT] = r[PartyRolesConfig.Assignment.R1] = 0;
        r[PartyRolesConfig.Assignment.H1] = r[PartyRolesConfig.Assignment.M1] = 1;
        r[PartyRolesConfig.Assignment.OT] = r[PartyRolesConfig.Assignment.R2] = 2;
        r[PartyRolesConfig.Assignment.H2] = r[PartyRolesConfig.Assignment.M2] = 3;
        return r;
    }

    public static GroupAssignmentDDSupportPairs DefaultMeleeTogether()
    {
        GroupAssignmentDDSupportPairs r = new();
        r[PartyRolesConfig.Assignment.MT] = r[PartyRolesConfig.Assignment.M1] = 0;
        r[PartyRolesConfig.Assignment.OT] = r[PartyRolesConfig.Assignment.M2] = 1;
        r[PartyRolesConfig.Assignment.H1] = r[PartyRolesConfig.Assignment.R1] = 2;
        r[PartyRolesConfig.Assignment.H2] = r[PartyRolesConfig.Assignment.R2] = 3;
        return r;
    }

    public override bool Validate()
    {
        BitMask mask = new(); // bits 0-3 - support for group N, bits 4-7 - dd for group (N-4)
        void addToMask(int group, int offset)
        {
            if (group is >= 0 and < 4)
                mask.Set(group + offset);
        }
        for (int i = 0; i < 4; ++i)
            addToMask(Assignments[i], 0);
        for (int i = 4; i < 8; ++i)
            addToMask(Assignments[i], 4);
        return mask.Raw == 0xff;
    }
}

// assignments to 8 unique roles
public class GroupAssignmentUnique : GroupAssignment
{
    public static GroupAssignmentUnique Default()
    {
        GroupAssignmentUnique r = new();
        r[PartyRolesConfig.Assignment.MT] = 0;
        r[PartyRolesConfig.Assignment.M1] = 1;
        r[PartyRolesConfig.Assignment.OT] = 2;
        r[PartyRolesConfig.Assignment.M2] = 3;
        r[PartyRolesConfig.Assignment.R1] = 4;
        r[PartyRolesConfig.Assignment.H1] = 5;
        r[PartyRolesConfig.Assignment.R2] = 6;
        r[PartyRolesConfig.Assignment.H2] = 7;
        return r;
    }

    public override bool Validate()
    {
        BitMask mask = new();
        for (int i = 0; i < 8; ++i)
            mask.Set(Assignments[i]);
        return mask.Raw == 0xff;
    }
}
