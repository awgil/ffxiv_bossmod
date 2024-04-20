namespace BossMod.Stormblood.Ultimate.UWU;

// predict puddles under all players until actual casts start
class P1FeatherRain(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.FeatherRain), "GTFO from puddle!")
{
    private readonly List<WPos> _predicted = [];
    private readonly List<Actor> _casters = [];
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(3);

    public bool CastsPredicted => _predicted.Count > 0;
    public bool CastsActive => _casters.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var p in _predicted)
            yield return new(_shape, p, new(), _activation);
        foreach (var c in _casters)
            yield return new(_shape, c.CastInfo!.LocXZ, new(), c.CastInfo!.NPCFinishAt);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E3A && (OID)actor.OID is OID.Garuda or OID.GarudaSister)
        {
            _predicted.Clear();
            _predicted.AddRange(Raid.WithoutSlot().Select(p => p.Position));
            _activation = WorldState.FutureTime(2.5f);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _predicted.Clear();
            _casters.Add(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _casters.Remove(caster);
        }
    }
}
