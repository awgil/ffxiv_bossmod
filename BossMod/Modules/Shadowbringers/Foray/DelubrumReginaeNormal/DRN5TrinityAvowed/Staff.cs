namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN5TrinityAvowed;

class FreedomOfBozja : TemperatureAOE
{
    private readonly List<(Actor orb, int temperature)> _orbs = [];
    private readonly DateTime _activation;
    private readonly bool _risky;

    private static readonly AOEShapeCircle _shape = new(22);

    public FreedomOfBozja(BossModule module, bool risky) : base(module)
    {
        _risky = risky;
        _activation = WorldState.FutureTime(10);
        InitOrb(OID.SwirlingOrb, -1);
        InitOrb(OID.TempestuousOrb, -2);
        InitOrb(OID.BlazingOrb, +1);
        InitOrb(OID.RoaringOrb, +2);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var playerTemp = Temperature(actor);
        foreach (var o in _orbs)
            yield return new(_shape, o.orb.Position, o.orb.Rotation, _activation, o.temperature == -playerTemp ? ArenaColor.SafeFromAOE : ArenaColor.AOE, _risky);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ChillBlast or AID.FreezingBlast or AID.HeatedBlast or AID.SearingBlast)
            ++NumCasts;
    }

    public bool ActorUnsafeAt(Actor actor, WPos pos)
    {
        var playerTemp = Temperature(actor);
        return _orbs.Any(o => _shape.Check(pos, o.orb.Position) != (o.temperature == -playerTemp));
    }

    private void InitOrb(OID oid, int temp)
    {
        var orb = Module.Enemies(oid).FirstOrDefault();
        if (orb != null)
            _orbs.Add((orb, temp));
    }
}

class FreedomOfBozja1(BossModule module) : FreedomOfBozja(module, false);
class FreedomOfBozja2(BossModule module) : FreedomOfBozja(module, true);