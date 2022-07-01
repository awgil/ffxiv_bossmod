namespace BossMod.Endwalker.Savage.P3SPhoinix
{
    [ConfigDisplay(Order = 0x130, Parent = typeof(EndwalkerConfig))]
    public class P3SConfig : CooldownPlanningConfigNode { }

    public class P3S : BossModule
    {
        public P3S(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }
    }
}
