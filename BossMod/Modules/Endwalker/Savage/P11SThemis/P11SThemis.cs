namespace BossMod.Endwalker.Savage.P11SThemis
{
    [ConfigDisplay(Order = 0x1B0, Parent = typeof(EndwalkerConfig))]
    public class P11SThemisConfig : CooldownPlanningConfigNode
    {
        public P11SThemisConfig() : base(90) { }
    }

    public class P11SThemis : BossModule
    {
        public P11SThemis(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }
    }
}
