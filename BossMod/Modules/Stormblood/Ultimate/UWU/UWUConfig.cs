namespace BossMod.Stormblood.Ultimate.UWU
{
    [ConfigDisplay(Order = 0x200, Parent = typeof(StormbloodConfig))]
    public class UWUConfig : CooldownPlanningConfigNode
    {
        public UWUConfig() : base(70) { }
    }
}
