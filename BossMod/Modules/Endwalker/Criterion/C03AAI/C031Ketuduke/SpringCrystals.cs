namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke;

class SpringCrystalsRect : Components.GenericAOEs
{
    public List<WPos> SafeZoneCenters = new();
    private List<AOEInstance> _aoes = new();
    private bool _moveCasters;
    private bool _risky;
    private float _delay;

    private static readonly AOEShapeRect _shape = new(38, 5, 38);

    public SpringCrystalsRect(bool moveCasters, bool risky, float delay)
    {
        _moveCasters = moveCasters;
        _risky = risky;
        _delay = delay;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void Init(BossModule module)
    {
        for (int z = -15; z <= 15; z += 10)
            for (int x = -15; x <= 15; x += 10)
                SafeZoneCenters.Add(module.Bounds.Center + new WDir(x, z));
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID is OID.NSpringCrystalRect or OID.SSpringCrystalRect)
        {
            var pos = actor.Position;
            if (_moveCasters)
            {
                // crystals are moved once or twice, but never outside arena bounds
                // orthogonal movement always happens, movement along direction happens only for half of them - but it doesn't actually affect aoe, so we can ignore it
                pos.X += pos.X < module.Bounds.Center.X ? 20 : -20;
                pos.Z += pos.Z < module.Bounds.Center.Z ? 20 : -20;
            }
            _aoes.Add(new(_shape, pos, actor.Rotation, module.WorldState.CurrentTime.AddSeconds(_delay), risky: _risky));
            SafeZoneCenters.RemoveAll(c => _shape.Check(c, pos, actor.Rotation));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NSaturateRect or AID.SSaturateRect)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
class SpringCrystalsRectMove : SpringCrystalsRect { public SpringCrystalsRectMove() : base(true, false, 40.3f) { } }
class SpringCrystalsRectStay : SpringCrystalsRect { public SpringCrystalsRectStay() : base(false, true, 24.2f) { } }

class SpringCrystalsSphere : Components.GenericAOEs
{
    private List<AOEInstance> _aoes = new();
    private bool _active;

    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _active ? _aoes : Enumerable.Empty<AOEInstance>();

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID is OID.NSpringCrystalSphere or OID.SSpringCrystalSphere)
        {
            _aoes.Add(new(_shape, actor.Position, default, module.WorldState.CurrentTime.AddSeconds(23.9f)));
        }
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID is OID.NSpringCrystalSphere or OID.SSpringCrystalSphere && (SID)status.ID == SID.Bubble)
        {
            _active = true;
            var index = _aoes.FindIndex(aoe => aoe.Origin.AlmostEqual(actor.Position, 1));
            if (index >= 0)
            {
                ref var pos = ref _aoes.AsSpan()[index].Origin;
                pos.X += pos.X < module.Bounds.Center.X ? 20 : -20;
            }
            else
            {
                module.ReportError(this, $"Failed to find aoe for {actor}");
            }
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NSaturateSphere or AID.SSaturateSphere)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
