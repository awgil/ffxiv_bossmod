namespace BossMod.Endwalker.Ultimate.DSW2
{
    [ConfigDisplay(Order = 0x201, Parent = typeof(EndwalkerConfig))]
    public class DSW2Config : ConfigNode
    {
        [PropertyDisplay("P2 Sanctity of the Ward: group assignments")]
        [GroupDetails(new string[]{ "West", "East" })]
        public GroupAssignmentLightParties P2SanctityGroups = DefaultGroupAssignment.LightParties<GroupAssignmentLightParties>();

        [PropertyDisplay("P2 Sanctity of the Ward: role responsible for balancing groups (if not set, swap with role partner instead)")]
        public Role P2SanctitySwapRole;
    }
}
