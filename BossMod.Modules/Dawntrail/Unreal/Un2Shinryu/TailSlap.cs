namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

class TailSlap(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _shape = new(20, 10, 20);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Tail)
        {
            _aoes.Clear();
            _aoes.Add(new(_shape, actor.Position, actor.Rotation, WorldState.FutureTime(5.7f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TailDiveNorth:
            case AID.TailDiveSouth:
                if (Module.FindComponent<TailMarker>()?.Target is { } target)
                    _aoes.Add(new(_shape, new(Un2Shinryu.TileCenter(target.Position).X, (AID)spell.Action.ID == AID.TailDiveNorth ? -10 : 10), default, WorldState.FutureTime(10.1f)));
                break;
            case AID.TailSlap:
            case AID.TailSlapAlt:
                _aoes.Clear();
                ++NumCasts;
                break;
        }
    }
}
