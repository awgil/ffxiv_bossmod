namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

class GetDownAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GetDownAOEFirst)
        {
            var donut = true;
            _aoes.Add(new(new AOEShapeCircle(7), caster.Position, default, Module.CastFinishAt(spell)));
            for (var i = 0; i < 7; i++)
            {
                _aoes.Add(new(donut ? new AOEShapeDonut(5, 60) : new AOEShapeCircle(7), caster.Position, default, _aoes[^1].Activation.AddSeconds(2.5f)));
                donut = !donut;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GetDownAOEFirst or AID.GetDownAOEDonut or AID.GetDownAOECircle)
        {
            NumCasts++;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}

class GetDownProtean(BossModule module) : Components.GenericBaitAway(module, AID.GetDownProtean)
{
    private BitMask targetedPlayers;

    private DateTime NextActivation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArcadyNightFever or AID.ArcadyNightEncore)
        {
            NextActivation = WorldState.FutureTime(5.3f);
            AddBaits(Raid.WithoutSlot());
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            var closest = Raid.WithoutSlot().MinBy(c => (Module.PrimaryActor.AngleTo(c) - spell.Rotation).Abs().Rad);
            if (closest != null)
            {
                targetedPlayers.Set(Raid.FindSlot(closest.InstanceID));
                var lastRole = closest.Class.GetRole3();
                NextActivation = WorldState.FutureTime(2.5f);

                CurrentBaits.Clear();
                AddBaits(Raid.WithSlot().ExcludedFromMask(targetedPlayers).Where(r => r.Item2.Class.GetRole3() != lastRole).Select(r => r.Item2));
            }
        }
    }

    private void AddBaits(IEnumerable<Actor> players) => CurrentBaits.AddRange(players.Select(r => new Bait(Module.PrimaryActor, r, new AOEShapeCone(60, 22.5f.Degrees()), NextActivation)));
}
class GetDownRepeat(BossModule module) : Components.StandardAOEs(module, AID.GetDownRepeat, new AOEShapeCone(40, 22.5f.Degrees()));
