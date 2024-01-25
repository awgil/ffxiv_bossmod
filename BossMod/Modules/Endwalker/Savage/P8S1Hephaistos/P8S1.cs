namespace BossMod.Endwalker.Savage.P8S1Hephaistos
{
    class VolcanicTorches : Components.SelfTargetedAOEs
    {
        public VolcanicTorches() : base(ActionID.MakeSpell(AID.TorchFlame), new AOEShapeRect(5, 5, 5)) { }
    }

    class AbyssalFires : Components.LocationTargetedAOEs
    {
        public AbyssalFires() : base(ActionID.MakeSpell(AID.AbyssalFires), 15) { } // TODO: verify falloff
    }

    [ModuleInfo(CFCID = 884, NameID = 11399)]
    public class P8S1 : BossModule
    {
        public P8S1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20)) { }
    }
}
