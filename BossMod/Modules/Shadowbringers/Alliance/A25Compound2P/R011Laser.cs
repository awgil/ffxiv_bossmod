namespace BossMod.Shadowbringers.Alliance.A25Compound2P;

class R011Laser(BossModule module) : Components.GenericAOEs(module, AID.R011LaserAOE)
{
    private readonly List<Actor> _casters = [];
    private readonly List<(Actor from, Actor to)> _tethers = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.R011LaserCast)
            _casters.Add(caster);

        if (spell.Action == WatchedAction)
        {
            _casters.Clear();
            _tethers.Clear();
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Transfer && _casters.Count > 0 && WorldState.Actors.Find(tether.Target) is { } t)
            _tethers.Add((source, t));
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_tethers.Count == 0)
            yield break;

        foreach (var c in _casters)
        {
            var t = _tethers.FindIndex(t => t.to.Position.AlmostEqual(c.Position, 1));
            var src = t >= 0 ? _tethers[t].from : c;
            yield return new AOEInstance(new AOEShapeRect(70, 7.5f), src.Position, src.Rotation, Module.CastFinishAt(c.CastInfo));
        }
    }
}

class R011LaserAOE(BossModule module) : Components.StandardAOEs(module, AID.R011LaserAOE, new AOEShapeRect(70, 7.5f));
