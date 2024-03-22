namespace BossMod.Endwalker.NormalTrials.Trial7Zeromus
{
    class AbyssalEchoes : Components.SelfTargetedAOEs
    {
        public AbyssalEchoes() : base(ActionID.MakeSpell(AID.AbyssalEchoes), new AOEShapeCircle(12), 5) { }
    }
    
    class BigBangPuddle : Components.LocationTargetedAOEs
    {
        public BigBangPuddle() : base(ActionID.MakeSpell(AID.BigBangAOE), 5) { }
    }
    
    class BigBangSpread : Components.SpreadFromCastTargets// Extreme only
    {
        public BigBangSpread() : base(ActionID.MakeSpell(AID.BigBangSpread), 5) { }
    }    
    class BigCrunchPuddle : Components.LocationTargetedAOEs
    {
        public BigCrunchPuddle() : base(ActionID.MakeSpell(AID.BigCrunchAOE), 5) { }
    }
    
    class BigCrunchSpread : Components.SpreadFromCastTargets// Extreme only
    {
        public BigCrunchSpread() : base(ActionID.MakeSpell(AID.BigCrunchSpread), 5) { }
    }

    class UnknownBlackHole: Components.SelfTargetedAOEs
    {
        public UnknownBlackHole() : base(ActionID.MakeSpell(AID.UnknownBlackHole), new AOEShapeCircle(4)) { }
    }

    [ConfigDisplay(Order = 0x070, Parent = typeof(EndwalkerConfig))]
    public class Trial7ZeromusConfig : CooldownPlanningConfigNode
    {
        public Trial7ZeromusConfig() : base(90) { }
    }

    [ModuleInfo(CFCID = 964, PrimaryActorOID = 0x404E)]
    public class Trial7Zeromus : BossModule
    {
        public Trial7Zeromus(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20)) { }
    }
}
