namespace BossMod.Endwalker.Ultimate.DSW2
{
    [ConfigDisplay(Order = 0x201, Parent = typeof(EndwalkerConfig))]
    public class DSW2Config : CooldownPlanningConfigNode
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

        [PropertyDisplay("P2 Sanctity of the Ward (meteors): swap N/E and S/W (rather than N/W and S/E) when both soakers are on wrong cardinals")]
        public bool P2Sanctity2SwapBothNE = false;

        [PropertyDisplay("P2 Sanctity of the Ward (meteors): take CW (rather than CCW) intercardinal as non-prey role for second towers")]
        public bool P2Sanctity2NonPreyTowerCW = false;

        [PropertyDisplay("P3 four towers with counters: assignments")]
        [GroupDetails(new string[] { "NW Flex", "NE Flex", "SE Flex", "SW Flex", "NW Stay", "NE Stay", "SE Stay", "SW Stay" })]
        public GroupAssignmentUnique P3DarkdragonDiveCounterGroups = new();

        [PropertyDisplay("P3 four towers with counters: prefer flexing to CCW tower (rather than to CW)")]
        public bool P3DarkdragonDiveCounterPreferCCWFlex = false;

        public DSW2Config() : base(90) { }
    }
}
