namespace BossMod.Endwalker.Savage.P11SThemis;

class Lightstream(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(50, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LightstreamAOEFirst or AID.LightstreamAOERest)
        {
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
            ++NumCasts;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        var rotation = (IconID)iconID switch
        {
            IconID.RotateCW => -10.Degrees(),
            IconID.RotateCCW => 10.Degrees(),
            _ => default
        };
        if (rotation != default)
        {
            for (int i = 0; i < 7; ++i)
                _aoes.Add(new(_shape, actor.Position, actor.Rotation + i * rotation, WorldState.FutureTime(8 + i * 1.1f)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }
}
