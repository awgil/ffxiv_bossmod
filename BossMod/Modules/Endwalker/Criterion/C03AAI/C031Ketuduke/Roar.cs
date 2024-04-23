namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke;

class Roar(BossModule module) : Components.GenericBaitAway(module)
{
    public bool Active;
    private BitMask _playerBubbles;
    private readonly List<(Actor actor, bool bubble)> _snakes = [];
    private bool _highlightSnakes;

    private static readonly AOEShapeCone _shape = new(60, 90.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        if (Active)
        {
            foreach (var s in _snakes)
            {
                var target = Raid.WithoutSlot().Closest(s.actor.Position);
                if (target != null)
                    CurrentBaits.Add(new(s.actor, target, _shape));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var s in _snakes)
        {
            Arena.Actor(s.actor, ArenaColor.Object, true);
            if (_highlightSnakes && s.bubble != _playerBubbles[pcSlot])
                Arena.AddCircle(s.actor.Position, 1, ArenaColor.Safe);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.NZaratan or OID.SZaratan)
            _snakes.Add((actor, false));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.Bubble:
                var index = _snakes.FindIndex(s => s.actor == actor);
                if (index >= 0)
                    _snakes[index] = (actor, true);
                _highlightSnakes = true;
                break;
            case SID.BubbleWeave:
                _playerBubbles.Set(Raid.FindSlot(actor.InstanceID));
                _highlightSnakes = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NHundredLashingsNormal or AID.NHundredLashingsBubble or AID.SHundredLashingsNormal or AID.SHundredLashingsBubble)
        {
            ++NumCasts;
            _snakes.Clear();
        }
    }
}
