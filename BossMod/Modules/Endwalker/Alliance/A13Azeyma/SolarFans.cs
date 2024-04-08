namespace BossMod.Endwalker.Alliance.A13Azeyma;

class SolarFans : Components.ChargeAOEs
{
    public SolarFans() : base(ActionID.MakeSpell(AID.SolarFansAOE), 5) { }
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

class RadiantFlourish : Components.GenericAOEs
{
    private int teleportcounter;
    private static readonly AOEShapeCircle circle = new(25);
    private readonly List<AOEInstance> _aoes = [];
    private const float RadianConversion = MathF.PI / 180;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SolarFansAOE)
            _aoes.Add(new(circle, spell.LocXZ, activation: module.WorldState.CurrentTime.AddSeconds(16.6f)));
        if ((AID)spell.Action.ID == AID.RadiantFlourish)
            _aoes.Clear();
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TeleportFlame) //correct circle location if Flight happens 10 times instead of 8 times, ugly hack but i couldn't find a better difference in logs
        {
            if (++teleportcounter > 8)
            {
                teleportcounter = 0;
                _aoes.Add(new(circle, RotateAroundOrigin(90, module.Bounds.Center, _aoes[0].Origin), activation: _aoes[0].Activation.AddSeconds(1.4f)));
                _aoes.Add(new(circle, RotateAroundOrigin(90, module.Bounds.Center, _aoes[1].Origin), activation: _aoes[1].Activation.AddSeconds(1.4f)));
                _aoes.RemoveAt(0);
                _aoes.RemoveAt(0);
            }
        }
    }

    private static WPos RotateAroundOrigin(float rotatebydegrees, WPos origin, WPos caster) //TODO: consider moving to utils for future use
    {
        float x = MathF.Cos(rotatebydegrees * RadianConversion) * (caster.X - origin.X) - MathF.Sin(rotatebydegrees * RadianConversion) * (caster.Z - origin.Z);
        float z = MathF.Sin(rotatebydegrees * RadianConversion) * (caster.X - origin.X) + MathF.Cos(rotatebydegrees * RadianConversion) * (caster.Z - origin.Z);
        return new WPos(origin.X + x, origin.Z + z);
    }
}
