namespace BossMod.Stormblood.Ultimate.UWU;

class P3Upheaval(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Upheaval, 24, true)
{
    private ArcList SafeAngles = new(default, 0);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
        {
            Service.Log($"setting center");
            SafeAngles = new(spell.LocXZ, 29);

            SafeAngles.ForbidInverseCircle(Arena.Center, 16);

            foreach (var boulder in Module.Enemies(OID.BombBoulder))
                SafeAngles.ForbidCircle(boulder.Position, 6.3f);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BombBoulder)
            SafeAngles.ForbidCircle(actor.Position, 6.3f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var source in Sources(slot, actor))
            foreach (var (min, max) in SafeAngles.Allowed(1.Degrees()))
                hints.AddForbiddenZone(ShapeDistance.PrecisePosition(SafeAngles.Center + ((max + min) * 0.5f).ToDirection() * 4, new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f), source.Activation);
    }
}

class P3Burst(BossModule module) : Components.GenericAOEs(module, AID.Burst)
{
    private readonly IReadOnlyList<Actor> _bombs = module.Enemies(OID.BombBoulder);
    private readonly Dictionary<ulong, DateTime?> _bombActivation = [];

    private static readonly AOEShape _shape = new AOEShapeCircle(6.3f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var b in _bombs)
        {
            var activation = _bombActivation.GetValueOrDefault(b.InstanceID);
            if (activation != null)
                yield return new(_shape, b.Position, b.Rotation, Module.CastFinishAt(b.CastInfo, 0, activation.Value));
        }
    }

    public override void Update()
    {
        foreach (var b in _bombs.Where(b => !_bombActivation.ContainsKey(b.InstanceID)))
            _bombActivation[b.InstanceID] = WorldState.FutureTime(6.5f);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _bombActivation[caster.InstanceID] = null;
    }
}
