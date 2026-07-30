namespace BossMod.Endwalker.VariantCriterion.C03AAI.C031Ketuduke;

class Roar(BossModule module) : Components.GenericBaitAway(module)
{
    public bool Active;
    private BitMask _playerBubbles;
    private readonly List<(Actor actor, bool bubble)> _snakes = [];
    private bool _highlightSnakes;

    private static readonly AOEShapeCone _shape = new(60f, 90f.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        if (Active)
        {
            foreach (var s in _snakes)
            {
                var target = Raid.WithoutSlot(false, true, true).Closest(s.actor.Position);
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
            Arena.Actor(s.actor, Colors.Object, true);
            if (_highlightSnakes && s.bubble != _playerBubbles[pcSlot])
                Arena.ZoneCircleOutline(s.actor.Position, 1f, Colors.Safe);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.NZaratan or (uint)OID.SZaratan)
            _snakes.Add((actor, false));
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.Bubble:
                var index = _snakes.FindIndex(s => s.actor == actor);
                if (index >= 0)
                    _snakes[index] = (actor, true);
                _highlightSnakes = true;
                break;
            case (uint)SID.BubbleWeave:
                _playerBubbles.Set(Raid.FindSlot(actor.InstanceID));
                _highlightSnakes = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NHundredLashingsNormal or (uint)AID.NHundredLashingsBubble or (uint)AID.SHundredLashingsNormal or (uint)AID.SHundredLashingsBubble)
        {
            ++NumCasts;
            _snakes.Clear();
        }
    }
}
