namespace BossMod.Endwalker.Ultimate.DSW2
{
    [ConfigDisplay(Order = 0x201, Parent = typeof(EndwalkerConfig))]
    public class DSW2Config : ConfigNode
    {
        [PropertyDisplay("P2 Sanctity of the Ward (charges): group assignments")]
        [GroupDetails(new string[]{ "West/Across", "East/Behind" })]
        public GroupAssignmentLightParties P2SanctityGroups = new();

        [PropertyDisplay("P2 Sanctity of the Ward (charges): groups relative to DRK (across/behind) rather than absolute (west/east)")]
        public bool P2SanctityRelative = false;

        [PropertyDisplay("P2 Sanctity of the Ward (charges): role responsible for balancing groups (if not set, swap with role partner instead)")]
        public Role P2SanctitySwapRole;

        [PropertyDisplay("P2 Sanctity of the Ward (meteors): pair assignments")]
        [GroupDetails(new string[] { "North", "West", "South", "East" })]
        public GroupAssignmentDDSupportPairs P2Sanctity2Pairs = new();

        [PropertyDisplay("P2 Sanctity of the Ward (meteors): prefer E/W for prey rather than N/S")]
        public bool P2Sanctity2PreferEWPrey = false;

        // TODO: swap E/W <-> N/S if preferred position has bad towers?

        [PropertyDisplay("P2 Sanctity of the Ward (meteors): swap CCW (rather than CW) when both soakers are on wrong cardinals")]
        public bool P2Sanctity2SwapCCW = false;
    }
}
