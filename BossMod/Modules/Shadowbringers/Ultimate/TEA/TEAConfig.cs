namespace BossMod.Shadowbringers.Ultimate.TEA
{
    [ConfigDisplay(Order = 0x200, Parent = typeof(ShadowbringersConfig))]
    public class TEAConfig : CooldownPlanningConfigNode
    {
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
        [GroupDetails(new string[] { "Pair 1", "Pair 2", "Pair 3", "Pair 4" })]
        [GroupPreset("Melee together", new[] { 0, 1, 2, 3, 0, 1, 2, 3 })]
        [GroupPreset("DD CCW", new[] { 0, 2, 1, 3, 1, 0, 2, 3 })]
        public GroupAssignmentDDSupportPairs P2NisiPairs = GroupAssignmentDDSupportPairs.DefaultMeleeTogether();

        public TEAConfig() : base(80) { }
    }
}
