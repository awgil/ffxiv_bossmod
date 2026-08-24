namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

class IronSplitter(BossModule module) : Components.GenericAOEs(module, AID.IronSplitter)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var distance = (caster.Position - Module.Center).Length();
            if (distance is < 3 or > 9 and < 11 or > 17 and < 19) // tiles
            {
                _aoes.Add(new(new AOEShapeCircle(4), Module.Center, new(), Module.CastFinishAt(spell)));
                _aoes.Add(new(new AOEShapeDonut(8, 12), Module.Center, new(), Module.CastFinishAt(spell)));
                _aoes.Add(new(new AOEShapeDonut(16, 20), Module.Center, new(), Module.CastFinishAt(spell)));
            }
            else
            {
                _aoes.Add(new(new AOEShapeDonut(4, 8), Module.Center, new(), Module.CastFinishAt(spell)));
                _aoes.Add(new(new AOEShapeDonut(12, 16), Module.Center, new(), Module.CastFinishAt(spell)));
                _aoes.Add(new(new AOEShapeDonut(20, 25), Module.Center, new(), Module.CastFinishAt(spell)));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _aoes.Clear();
        }
    }
}
