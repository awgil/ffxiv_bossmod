namespace BossMod.Shadowbringers.Ultimate.TEA
{
    [ConfigDisplay(Order = 0x200, Parent = typeof(ShadowbringersConfig))]
    public class TEAConfig : CooldownPlanningConfigNode
    {
        public TEAConfig() : base(80) { }
    }
}
