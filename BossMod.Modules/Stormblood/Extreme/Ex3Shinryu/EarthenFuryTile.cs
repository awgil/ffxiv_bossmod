namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class EarthenFuryTile(BossModule module) : Components.GenericAOEs(module, AID.EarthenFuryTile)
{
    private static readonly AOEShapeRect _shape = new(10, 10, 10);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _aoes.Add(new(_shape, Ex3Shinryu.TileCenter(caster.Position + (caster.Position - Arena.Center).Normalized() * 10), default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _aoes.Clear();
    }
}
