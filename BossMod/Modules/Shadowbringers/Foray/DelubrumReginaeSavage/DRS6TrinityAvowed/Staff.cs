namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

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
        if ((AID)spell.Action.ID is AID.ChillBlast1 or AID.FreezingBlast1 or AID.HeatedBlast1 or AID.SearingBlast1 or AID.ChillBlast2 or AID.FreezingBlast2 or AID.HeatedBlast2 or AID.SearingBlast2)
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

class QuickMarchStaff1(BossModule module) : QuickMarch(module)
{
    private readonly FreedomOfBozja1? _freedom = module.FindComponent<FreedomOfBozja1>();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (_freedom?.ActorUnsafeAt(actor, pos) ?? false);
}

class FreedomOfBozja2(BossModule module) : FreedomOfBozja(module, true);
