namespace BossMod.Shadowbringers.Alliance.A36FalseIdol;

class MagicalInterference(BossModule module) : Components.GenericAOEs(module, AID.MagicalInterference)
{
    private readonly List<(WPos Source, DateTime Activation)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_casters.Count == 0)
            yield break;

        var start = _casters[0].Activation.AddSeconds(0.5f);
        using var en = _casters.GetEnumerator();
        while (en.MoveNext())
        {
            var cur = en.Current;
            yield return new AOEInstance(new AOEShapeRect(50, 5), en.Current.Source, default, en.Current.Activation, Color: cur.Activation > start ? ArenaColor.AOE : ArenaColor.Danger, Risky: cur.Activation <= start);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.MagicalInterference && state == 0x00010002)
            _casters.Add((actor.Position, WorldState.FutureTime(8.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _casters.RemoveAll(c => c.Source.AlmostEqual(caster.Position, 1));
        }
    }
}
