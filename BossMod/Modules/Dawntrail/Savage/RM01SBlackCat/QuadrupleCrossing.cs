namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

// same component covers normal and leaping version
class QuadrupleCrossingProtean(BossModule module) : Components.GenericBaitAway(module)
{
    private Actor? _origin;
    private DateTime _activation;

    private static readonly AOEShapeCone _shape = new(100, 22.5f.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_origin != null && _activation != default)
            foreach (var p in Module.Raid.WithoutSlot().SortedByRange(_origin.Position).Take(4))
                CurrentBaits.Add(new(_origin, p, _shape, _activation));
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.LeapTarget)
            _origin = actor;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.QuadrupleCrossingFirst:
                _origin = caster;
                _activation = Module.CastFinishAt(spell, 0.8f);
                break;
            case AID.LeapingQuadrupleCrossingBossL:
            case AID.LeapingQuadrupleCrossingBossR:
                // origin will be set to leap target when it's created
                _activation = Module.CastFinishAt(spell, 1.8f);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.QuadrupleCrossingProtean or AID.LeapingQuadrupleCrossingBossProtean)
        {
            ++NumCasts;
            _activation = Module.WorldState.FutureTime(3);
            foreach (var t in spell.Targets)
                ForbiddenPlayers.Set(Module.Raid.FindSlot(t.ID));
        }
    }
}

class QuadrupleCrossingAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(100, 22.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count >= 8 ? _aoes.Skip(NumCasts).Take(4) : [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.QuadrupleCrossingProtean:
            case AID.LeapingQuadrupleCrossingBossProtean:
                _aoes.Add(new(_shape, caster.Position, caster.Rotation, Module.WorldState.FutureTime(5.9f)));
                break;
            case AID.QuadrupleCrossingAOE:
            case AID.LeapingQuadrupleCrossingBossAOE:
                ++NumCasts;
                break;
        }
    }
}
