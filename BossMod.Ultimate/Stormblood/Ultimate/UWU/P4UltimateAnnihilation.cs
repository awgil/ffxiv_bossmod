namespace BossMod.Stormblood.Ultimate.UWU;

class P4UltimateAnnihilation(BossModule module) : BossComponent(module)
{
    private readonly IReadOnlyList<Actor> _orbs = module.Enemies(OID.Aetheroplasm);

    private const float _radius = 6;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var orb in _orbs.Where(o => !o.IsDead))
        {
            Arena.Actor(orb, ArenaColor.Object, true);
            Arena.AddCircle(orb.Position, _radius, ArenaColor.Object);
        }
    }
}

class P4Bury(BossModule module) : Components.GenericAOEs(module, AID.Bury)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RadiantPlumeUltima)
        {
            var activation = Module.CastFinishAt(spell, 1);
            _predicted.Add(new(new AOEShapeCircle(4.3f), new(103, 105), default, activation));
            _predicted.Add(new(new AOEShapeCircle(4.3f), new(97, 105), default, activation));
            _predicted.Add(new(new AOEShapeCircle(4.3f), new(94, 100), default, activation));
            _predicted.Add(new(new AOEShapeCircle(4.3f), new(97, 95), default, activation));
            _predicted.Add(new(new AOEShapeCircle(4.3f), new(103, 95), default, activation));
            _predicted.Add(new(new AOEShapeCircle(4.3f), new(106, 100), default, activation));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_predicted.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 5), _predicted[0].Activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 1));
            foreach (ref var p in _predicted.AsSpan())
                p.Activation = WorldState.FutureTime(2.1f);

        }
    }
}
