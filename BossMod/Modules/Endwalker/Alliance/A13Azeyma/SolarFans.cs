namespace BossMod.Endwalker.Alliance.A13Azeyma;

class SolarFans : Components.ChargeAOEs
{
    public SolarFans() : base(ActionID.MakeSpell(AID.SolarFansAOE), 5) { } // TODO: or SolarFansCharge? not sure which one deals damage...
}

class RadiantRhythm : Components.GenericAOEs
{
    private IReadOnlyList<Actor> _flames = ActorEnumeration.EmptyList;

    private static readonly AOEShapeDonutSector _shape = new(20, 30, 45.Degrees());

    public RadiantRhythm() : base(ActionID.MakeSpell(AID.RadiantFlight)) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => NextCenterDirections(module.Bounds.Center).Select(d => new AOEInstance(_shape, module.Bounds.Center, d));

    public override void Init(BossModule module)
    {
        _flames = module.Enemies(OID.WardensFlame);
    }

    private IEnumerable<Angle> NextCenterDirections(WPos center) => _flames.Where(f => (f.Position - center).LengthSq() > 25).Select(f => Angle.FromDirection(f.Position - center) + 45.Degrees());
}

class RadiantFinish : Components.SelfTargetedAOEs
{
    public RadiantFinish() : base(ActionID.MakeSpell(AID.RadiantFlourish), new AOEShapeCircle(25)) { }
}
