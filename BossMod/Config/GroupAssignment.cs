using System;

namespace BossMod
{
    // attribute that specifies group count and names for group assignment property
    [AttributeUsage(AttributeTargets.Field)]
    public class GroupDetailsAttribute : Attribute
    {
        public string[] Names;

        public GroupDetailsAttribute(string[] names)
        {
            Names = names;
        }
    }

    // config node property that allows assigning party roles to arbitrary named groups
    // typically you would use derived classes that provide validation
    public class GroupAssignment
    {
        public int[] Assignments = new int[(int)PartyRolesConfig.Role.Unassigned]; // role -> group id

        public virtual bool Validate() => true;
    }

    // assignments to two light parties with THMR split
    public class GroupAssignmentLightParties : GroupAssignment
    {
        public override bool Validate()
        {
            for (int i = 0; i < (int)PartyRolesConfig.Role.Unassigned; i += 2)
                if (Assignments[i] < 0 || Assignments[i] >= 2 || Assignments[i + 1] < 0 || Assignments[i + 1] >= 2 || Assignments[i] == Assignments[i + 1])
                    return false;
            return true;
        }
    }

    // utilities for building default group assignments
    public static class DefaultGroupAssignment
    {
        public static T LightParties<T>() where T : GroupAssignment, new()
        {
            T res = new();
            res.Assignments[(int)PartyRolesConfig.Role.MT] = res.Assignments[(int)PartyRolesConfig.Role.H1] = res.Assignments[(int)PartyRolesConfig.Role.M1] = res.Assignments[(int)PartyRolesConfig.Role.R1] = 0;
            res.Assignments[(int)PartyRolesConfig.Role.OT] = res.Assignments[(int)PartyRolesConfig.Role.H2] = res.Assignments[(int)PartyRolesConfig.Role.M2] = res.Assignments[(int)PartyRolesConfig.Role.R2] = 1;
            return res;
        }
    }
}
