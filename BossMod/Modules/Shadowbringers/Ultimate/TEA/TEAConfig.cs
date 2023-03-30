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

        public TEAConfig() : base(80) { }
    }
}
