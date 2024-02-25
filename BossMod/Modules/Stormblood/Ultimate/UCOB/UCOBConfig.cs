namespace BossMod.Stormblood.Ultimate.UCOB
{
    [ConfigDisplay(Order = 0x200, Parent = typeof(StormbloodConfig))]
    public class UCOBConfig : CooldownPlanningConfigNode
    {
        public UCOBConfig() : base(70) { }
    }
}
