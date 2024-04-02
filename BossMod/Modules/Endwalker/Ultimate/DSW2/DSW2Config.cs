namespace BossMod.Endwalker.Ultimate.DSW2;

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

    public enum P2PreyCardinals
    {
        [PropertyDisplay("N/S always")]
        AlwaysNS,

        [PropertyDisplay("E/W always")]
        AlwaysEW,

        [PropertyDisplay("N/S, unless both preys start at E & W")]
        PreferNS,

        [PropertyDisplay("E/W, unless both preys start at N & S")]
        PreferEW,
    }

    [PropertyDisplay("P2 Sanctity of the Ward (meteors): preferred cardinals for prey targets")]
    public P2PreyCardinals P2Sanctity2PreyCardinals;

    [PropertyDisplay("P2 Sanctity of the Ward (meteors): force preferred cardinals even for 120-degree pattern (simpler swaps, but trickier movement)")]
    public bool P2Sanctity2ForcePreferredPrey = true;

    public enum P2PreySwapDirection
    {
        [PropertyDisplay("All prey roles rotate clockwise")]
        RotateCW,

        [PropertyDisplay("All prey roles rotate counterclockwise")]
        RotateCCW,

        [PropertyDisplay("Pairwise: N <-> E, S <-> W")]
        PairsNE,

        [PropertyDisplay("Pairwise: N <-> W, S <-> E")]
        PairsNW,
    }

    [PropertyDisplay("P2 Sanctity of the Ward (meteors): swap direction if both prey targets are on wrong cardinals")]
    public P2PreySwapDirection P2Sanctity2SwapDirection;

    [PropertyDisplay("P2 Sanctity of the Ward (meteors): preferred outer tower for prey role")]
    [PropertyCombo("CCW (leftmost, if facing outside)", "CW (rightmost, if facing outside)")]
    public bool P2Sanctity2PreferCWTowerAsPrey = true;

    public enum P2OuterTowers
    {
        [PropertyDisplay("Don't try to assign outer towers")]
        None,

        [PropertyDisplay("Always use preferred direction")]
        AlwaysPreferred,

        [PropertyDisplay("Prey targets both use common opposite direction, if it gives better angle; players in quadrants without prey targets still use preferred direction")]
        SynchronizedTargets,

        [PropertyDisplay("Prey targets both use common opposite direction, if it gives better angle; players in all quadrants use same direction")]
        SynchronizedRole,

        [PropertyDisplay("Prey targets use whatever direction gives the best angle; players in quadrants without prey targets still use preferred direction")]
        Individual
    }

    [PropertyDisplay("P2 Sanctity of the Ward (meteors): outer tower assignment strategy")]
    public P2OuterTowers P2Sanctity2OuterTowers = P2OuterTowers.Individual;

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

    [PropertyDisplay("P2 Sanctity of the Ward (meteors): intercardinal for non-prey role for second towers")]
    [PropertyCombo("CCW", "CW")]
    public bool P2Sanctity2NonPreyTowerCW = false;

    [PropertyDisplay("P3 Dive from Grace: look west for arrows rather than east (so forward arrow takes E spot, backward arrow takes W spot")]
    public bool P3DiveFromGraceLookWest = false;

    [PropertyDisplay("P3 enumeration towers: assignments")]
    [GroupDetails(new string[] { "NW Flex", "NE Flex", "SE Flex", "SW Flex", "NW Stay", "NE Stay", "SE Stay", "SW Stay" })]
    [GroupPreset("LPDU", new[] { 1, 3, 6, 0, 2, 4, 5, 7 })]
    [GroupPreset("LPDU but CCW", new[] { 0, 2, 5, 7, 1, 3, 4, 6 })]
    public GroupAssignmentUnique P3DarkdragonDiveCounterGroups = GroupAssignmentUnique.Default();

    [PropertyDisplay("P3 enumeration towers: prefer flexing to CCW tower (rather than to CW)")]
    public bool P3DarkdragonDiveCounterPreferCCWFlex = false;

    public enum P6MortalVow
    {
        [PropertyDisplay("Don't assume any order")]
        None,

        [PropertyDisplay("LPDU: MT->OT->M1 (M2 as fallback)->R1")]
        TanksMeleeR1,

        [PropertyDisplay("LPDU: MT->OT->M1 (M2 as fallback)->R2")]
        TanksMeleeR2,
    }

    [PropertyDisplay("P6 Mortal Vow pass order")]
    public P6MortalVow P6MortalVowOrder = P6MortalVow.None;

    public DSW2Config() : base(90) { }
}
