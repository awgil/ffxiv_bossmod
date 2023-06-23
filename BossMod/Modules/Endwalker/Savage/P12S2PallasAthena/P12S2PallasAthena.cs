namespace BossMod.Endwalker.Savage.P12S2PallasAthena
{
    [ConfigDisplay(Order = 0x1C2, Parent = typeof(EndwalkerConfig))]
    public class P12S2PallasAthenaConfig : CooldownPlanningConfigNode
    {
        public P12S2PallasAthenaConfig() : base(90) { }
    }

    public class P12S2PallasAthena : BossModule
    {
        public static ArenaBoundsRect DefaultBounds = new ArenaBoundsRect(new(100, 95), 20, 15);
        public static ArenaBoundsCircle SmallBounds = new ArenaBoundsCircle(new(100, 90), 7);

        public P12S2PallasAthena(WorldState ws, Actor primary) : base(ws, primary, DefaultBounds) { }
    }
}
