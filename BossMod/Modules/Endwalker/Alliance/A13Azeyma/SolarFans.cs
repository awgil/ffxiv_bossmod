namespace BossMod.Endwalker.Alliance.A13Azeyma;

class SolarFans(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.SolarFansAOE), 5); // TODO: or SolarFansCharge? not sure which one deals damage...

class RadiantRhythm : Components.GenericAOEs
{
    private IReadOnlyList<Actor> _flames;

    private static readonly AOEShapeDonutSector _shape = new(20, 30, 45.Degrees());

    public RadiantRhythm(BossModule module) : base(module, ActionID.MakeSpell(AID.RadiantFlight))
    {
        _flames = module.Enemies(OID.WardensFlame);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => NextCenterDirections(Module.Bounds.Center).Select(d => new AOEInstance(_shape, Module.Bounds.Center, d));

    private IEnumerable<Angle> NextCenterDirections(WPos center) => _flames.Where(f => (f.Position - center).LengthSq() > 25).Select(f => Angle.FromDirection(f.Position - center) + 45.Degrees());
}

class RadiantFinish(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RadiantFlourish), new AOEShapeCircle(25));
