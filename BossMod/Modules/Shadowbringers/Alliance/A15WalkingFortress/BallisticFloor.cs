namespace BossMod.Shadowbringers.Alliance.A15WalkingFortress;

class BallisticFloor(BossModule module) : Components.GenericAOEs(module, AID.BallisticImpactFloor)
{
    record struct Pattern(List<WPos> Tiles, DateTime Appear, int Order, DateTime Activate);

    private readonly List<Pattern> _patterns = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.BallisticImpactGroundSquare)
        {
            if (_patterns.Count > 0 && _patterns[^1].Appear.AddSeconds(2) > WorldState.CurrentTime)
                _patterns[^1].Tiles.Add(actor.Position);
            else
                _patterns.Add(new([actor.Position], WorldState.CurrentTime, _patterns.Count, WorldState.FutureTime(6.1f)));

            var startDelay = _patterns.Count switch
            {
                2 => 6.1f,
                3 => 9.2f,
                _ => 0
            };

            if (startDelay > 0)
            {
                var appear1 = _patterns[0].Appear.AddSeconds(startDelay);
                for (var i = 0; i < _patterns.Count; i++)
                {
                    _patterns.Ref(i).Activate = appear1;
                    appear1 = appear1.AddSeconds(2.1f);
                }
            }
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _patterns.Take(1).SelectMany(p => p.Tiles.Select(t => new AOEInstance(new AOEShapeRect(7.5f, 7.5f, 7.5f), t, default, p.Activate)));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_patterns.Count > 0)
            {
                _patterns[0].Tiles.RemoveAll(t => t.AlmostEqual(caster.Position, 1));
                if (_patterns[0].Tiles.Count == 0)
                    _patterns.RemoveAt(0);
            }
        }
    }
}
