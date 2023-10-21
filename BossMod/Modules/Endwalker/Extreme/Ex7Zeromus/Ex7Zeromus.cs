namespace BossMod.Endwalker.Extreme.Ex7Zeromus
{
    class AbyssalEchoes : Components.SelfTargetedAOEs
    {
        public AbyssalEchoes() : base(ActionID.MakeSpell(AID.AbyssalEchoes), new AOEShapeCircle(12), 5) { }
    }

    class BigBangPuddle : Components.LocationTargetedAOEs
    {
        public BigBangPuddle() : base(ActionID.MakeSpell(AID.BigBangAOE), 5) { }
    }

    class BigBangSpread : Components.SpreadFromCastTargets
    {
        public BigBangSpread() : base(ActionID.MakeSpell(AID.BigBangSpread), 5) { }
    }

    class BigCrunchPuddle : Components.LocationTargetedAOEs
    {
        public BigCrunchPuddle() : base(ActionID.MakeSpell(AID.BigCrunchAOE), 5) { }
    }

    class BigCrunchSpread : Components.SpreadFromCastTargets
    {
        public BigCrunchSpread() : base(ActionID.MakeSpell(AID.BigCrunchSpread), 5) { }
    }

    [ConfigDisplay(Order = 0x070, Parent = typeof(EndwalkerConfig))]
    public class Ex7ZeromusConfig : CooldownPlanningConfigNode
    {
        public Ex7ZeromusConfig() : base(90) { }
    }

    public class Ex7Zeromus : BossModule
    {
        public Ex7Zeromus(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20)) { }
    }
}
