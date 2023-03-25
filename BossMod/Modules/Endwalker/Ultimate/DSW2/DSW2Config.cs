namespace BossMod.Endwalker.Ultimate.DSW2
{
    [ConfigDisplay(Order = 0x201, Parent = typeof(EndwalkerConfig))]
    public class DSW2Config : CooldownPlanningConfigNode
    {
        [PropertyDisplay("P2 Sanctity of the Ward (charges): group assignments")]
        [GroupDetails(new string[] { "West/Across", "East/Behind" })]
        [GroupPreset("Default light parties", new[] { 0, 1, 0, 1, 0, 1, 0, 1 })]
        [GroupPreset("Inverted light parties", new[] { 1, 0, 1, 0, 1, 0, 1, 0 })]
        public GroupAssignmentLightParties P2SanctityGroups = GroupAssignmentLightParties.DefaultLightParties();

        [PropertyDisplay("P2 Sanctity of the Ward (charges): groups relative to DRK (across/behind) rather than absolute (west/east)")]
        public bool P2SanctityRelative = false;

        [PropertyDisplay("P2 Sanctity of the Ward (charges): role responsible for balancing groups (if not set, swap with role partner instead)")]
        public Role P2SanctitySwapRole;

        [PropertyDisplay("P2 Sanctity of the Ward (meteors): pair assignments")]
        [GroupDetails(new string[] { "North", "East", "South", "West" })]
        [GroupPreset("MT/R1 N, OT/R2 S, H1/M1 E, H2/M2 W", new[] { 0, 2, 1, 3, 1, 3, 0, 2 })]
        [GroupPreset("MT/R1 N, OT/R2 S, H1/M1 W, H2/M2 E", new[] { 0, 2, 3, 1, 3, 1, 0, 2 })]
        public GroupAssignmentDDSupportPairs P2Sanctity2Pairs = GroupAssignmentDDSupportPairs.DefaultOneMeleePerPair();

        [PropertyDisplay("P2 Sanctity of the Ward (meteors): prefer E/W for prey rather than N/S")]
        public bool P2Sanctity2PreferEWPrey = false;

        [PropertyDisplay("P2 Sanctity of the Ward (meteors): force preferred cardinals even for 120-degree best distance (simpler swaps, but trickier movement)")]
        public bool P2Sanctity2ForcePreferredPrey = false;

        [PropertyDisplay("P2 Sanctity of the Ward (meteors): swap N/E and S/W (rather than N/W and S/E) when both soakers are on wrong cardinals")]
        public bool P2Sanctity2SwapBothNE = false;

        [PropertyDisplay("P2 Sanctity of the Ward (meteors): prefer CW (rather than CCW) outer tower as prey role")]
        public bool P2Sanctity2PreferCWTowerAsPrey = true;

        [PropertyDisplay("P2 Sanctity of the Ward (meteors): use non-preferred outer tower as prey role if it increases distance")]
        public bool P2Sanctity2AllowNonPreferredTowerAsPrey = true;

        [PropertyDisplay("P2 Sanctity of the Ward (meteors): if selecting non-preferred outer towers for prey, continue moving in non-preferred direction (rather than always moving in preferred direction)")]
        public bool P2Sanctity2PreyFollowsSelectedDirection = false;

        public enum P2InnerTowers
        {
            [PropertyDisplay("Don't try to assign inner towers")]
            None,

            [PropertyDisplay("Assign closest unambiguous inner tower")]
            Closest,

            [PropertyDisplay("Assign first CW tower that is not assigned to someone closer")]
            CW,
        }

        [PropertyDisplay("P2 Sanctity of the Ward (meteors): inner tower assignment strategy")]
        public P2InnerTowers P2Sanctity2InnerTowers = P2InnerTowers.CW;

        [PropertyDisplay("P2 Sanctity of the Ward (meteors): take CW (rather than CCW) intercardinal as non-prey role for second towers")]
        public bool P2Sanctity2NonPreyTowerCW = false;

        [PropertyDisplay("P3 four towers with counters: assignments")]
        [GroupDetails(new string[] { "NW Flex", "NE Flex", "SE Flex", "SW Flex", "NW Stay", "NE Stay", "SE Stay", "SW Stay" })]
        public GroupAssignmentUnique P3DarkdragonDiveCounterGroups = GroupAssignmentUnique.Default();

        [PropertyDisplay("P3 four towers with counters: prefer flexing to CCW tower (rather than to CW)")]
        public bool P3DarkdragonDiveCounterPreferCCWFlex = false;

        public DSW2Config() : base(90) { }
    }
}
