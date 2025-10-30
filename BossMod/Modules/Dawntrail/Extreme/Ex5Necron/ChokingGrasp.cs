namespace BossMod.Dawntrail.Extreme.Ex5Necron;

class FearOfDeathRaidwide(BossModule module) : Components.RaidwideCast(module, AID.FearOfDeathRaidwide);
class FearOfDeathPuddle(BossModule module) : Components.StandardAOEs(module, AID.FearOfDeathPuddle, 3);

class ChokingGraspInstant(BossModule module) : Components.GenericBaitAway(module, AID.ChokingGraspInstant, damageType: AIHints.PredictedDamageType.Raidwide)
{
    private readonly List<Actor> _hands = [];

    private DateTime _activation;

    public override void Update()
    {
        CurrentBaits.Clear();

        if (_activation == default)
            return;

        foreach (var h in _hands)
        {
            var target = Raid.WithoutSlot().Closest(h.Position);
            if (target != null)
            {
                CurrentBaits.Add(new(h, target, new AOEShapeRect(24, 3), _activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _hands.Remove(caster);
            _activation = default;
        }

        if ((AID)spell.Action.ID == AID.FearOfDeathPuddle)
        {
            var hand = Module.Enemies(OID.IcyHandsP1).Closest(spell.TargetXZ);
            if (hand != null)
                _hands.Add(hand);

            if (_activation == default)
                _activation = WorldState.FutureTime(2.7f);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        Arena.Actors(_hands, ArenaColor.Object, true);
    }
}

class DelayedChokingGrasp(BossModule module) : Components.StandardAOEs(module, AID.ChokingGraspCast1, new AOEShapeRect(24, 3));
class EndsEmbrace(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.EndsEmbrace, AID.TheEndsEmbraceSpread, 3, 4.1f);

class EndsEmbraceBait(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly List<Actor> _hands = [];

    private DateTime _activation;

    public bool Baited;

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var h in _hands)
        {
            var closest = Raid.WithoutSlot().Closest(h.Position)!;
            CurrentBaits.Add(new(h, closest, new AOEShapeRect(24, 3), _activation));
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.IcyHandsP1 && id == 0x11D2)
        {
            _hands.Add(actor);
            _activation = WorldState.FutureTime(2.3f);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ChokingGraspCast1)
        {
            _hands.Remove(caster);
            Baited = true;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        Arena.Actors(_hands, ArenaColor.Object, true);
    }
}
