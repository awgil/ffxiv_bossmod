namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

class TurretsTour : Components.GenericAOEs
{
    private readonly List<(Actor turret, AOEShapeRect shape)> _turrets = [];
    private readonly List<(Actor caster, AOEShapeRect shape, Angle rotation)> _casters = [];
    private DateTime _activation;

    private static readonly AOEShapeRect _defaultShape = new(50, 3);

    public TurretsTour(BossModule module) : base(module)
    {
        var turrets = module.Enemies(OID.AutomaticTurret);
        foreach (var t in turrets)
        {
            var target = turrets.Exclude(t).InShape(_defaultShape, t).Closest(t.Position);
            var shape = target != null ? _defaultShape with { LengthFront = (target.Position - t.Position).Length() } : _defaultShape;
            _turrets.Add((t, shape));
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var t in _turrets)
            yield return new(t.shape, t.turret.Position, t.turret.Rotation, _activation);
        foreach (var c in _casters)
            yield return new(c.shape, c.caster.Position, c.rotation, Module.CastFinishAt(c.caster.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TurretsTourNormalAOE1)
        {
            var toTarget = spell.LocXZ - caster.Position;
            _casters.Add((caster, new AOEShapeRect(toTarget.Length(), _defaultShape.HalfWidth), Angle.FromDirection(toTarget)));
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TurretsTourNormalAOE1)
            _casters.RemoveAll(c => c.caster == caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TurretsTourNormalAOE2 or AID.TurretsTourNormalAOE3)
        {
            _turrets.RemoveAll(t => t.turret == caster);
            ++NumCasts;
        }
    }
}
