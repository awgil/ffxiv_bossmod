namespace BossMod.Endwalker.Savage.P8S2
{
    class TyrantsFlare : Components.LocationTargetedAOEs
    {
        public TyrantsFlare() : base(ActionID.MakeSpell(AID.TyrantsFlare), 6) { }
    }

    // note: we can detect aoes ~2s before cast start by looking at PATE 0x11D3
    class EndOfDays : Components.SelfTargetedAOEs
    {
        public EndOfDays() : base(ActionID.MakeSpell(AID.EndOfDays), new AOEShapeRect(60, 5), 3) { }
    }

    // TODO: autoattack component
    // TODO: end-of-days early detect
    // TODO: HC components
    // TODO: limitless desolation component
    // TODO: final mechanic component
    public class P8S2 : BossModule
    {
        public P8S2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20)) { }
    }
}
