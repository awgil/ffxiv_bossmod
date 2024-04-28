namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke;

class SpringCrystalsRect(BossModule module, bool moveCasters, bool risky, float delay) : Components.GenericAOEs(module)
{
    public List<WPos> SafeZoneCenters = InitialSafeZoneCenters(module.Center);
    private readonly List<AOEInstance> _aoes = [];
    private readonly bool _moveCasters = moveCasters;
    private readonly bool _risky = risky;
    private readonly float _delay = delay;

    private static readonly AOEShapeRect _shape = new(38, 5, 38);

    private static List<WPos> InitialSafeZoneCenters(WPos origin)
    {
        List<WPos> res = [];
        for (int z = -15; z <= 15; z += 10)
            for (int x = -15; x <= 15; x += 10)
                res.Add(origin + new WDir(x, z));
        return res;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.NSpringCrystalRect or OID.SSpringCrystalRect)
        {
            var pos = actor.Position;
            if (_moveCasters)
            {
                // crystals are moved once or twice, but never outside arena bounds
                // orthogonal movement always happens, movement along direction happens only for half of them - but it doesn't actually affect aoe, so we can ignore it
                pos.X += pos.X < Module.Center.X ? 20 : -20;
                pos.Z += pos.Z < Module.Center.Z ? 20 : -20;
            }
            _aoes.Add(new(_shape, pos, actor.Rotation, WorldState.FutureTime(_delay), Risky: _risky));
            SafeZoneCenters.RemoveAll(c => _shape.Check(c, pos, actor.Rotation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NSaturateRect or AID.SSaturateRect)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
class SpringCrystalsRectMove(BossModule module) : SpringCrystalsRect(module, true, false, 40.3f);
class SpringCrystalsRectStay(BossModule module) : SpringCrystalsRect(module, false, true, 24.2f);

class SpringCrystalsSphere(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private bool _active;

    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _active ? _aoes : Enumerable.Empty<AOEInstance>();

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.NSpringCrystalSphere or OID.SSpringCrystalSphere)
        {
            _aoes.Add(new(_shape, actor.Position, default, WorldState.FutureTime(23.9f)));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID is OID.NSpringCrystalSphere or OID.SSpringCrystalSphere && (SID)status.ID == SID.Bubble)
        {
            _active = true;
            var index = _aoes.FindIndex(aoe => aoe.Origin.AlmostEqual(actor.Position, 1));
            if (index >= 0)
            {
                ref var aoe = ref _aoes.Ref(index);
                aoe.Origin += new WDir(aoe.Origin.X < Module.Center.X ? 20 : -20, 0);
            }
            else
            {
                ReportError($"Failed to find aoe for {actor}");
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NSaturateSphere or AID.SSaturateSphere)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
