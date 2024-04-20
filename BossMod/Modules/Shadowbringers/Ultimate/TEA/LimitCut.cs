namespace BossMod.Shadowbringers.Ultimate.TEA;

class LimitCut(BossModule module, float alphaDelay) : Components.GenericBaitAway(module)
{
    private enum State { Teleport, Alpha, Blasty }

    public int[] PlayerOrder = new int[PartyState.MaxPartySize];
    private readonly float _alphaDelay = alphaDelay;
    private State _nextState;
    private Actor? _chaser;
    private WPos _prevPos;
    private DateTime _nextHit;

    private static readonly AOEShapeCone _shapeAlpha = new(30, 45.Degrees());
    private static readonly AOEShapeRect _shapeBlasty = new(55, 5);

    public override void Update()
    {
        if (_nextState == State.Teleport && _chaser != null && _chaser.Position != _prevPos)
        {
            _nextState = State.Alpha;
            _prevPos = _chaser.Position;
            SetNextBaiter(NumCasts + 1, _shapeAlpha);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (PlayerOrder[slot] > 0)
            hints.Add($"Order: {PlayerOrder[slot]}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (PlayerOrder[slot] > NumCasts)
        {
            var hitIn = Math.Max(0, (float)(_nextHit - WorldState.CurrentTime).TotalSeconds);
            var hitIndex = NumCasts + 1;
            while (PlayerOrder[slot] > hitIndex)
            {
                hitIn += (hitIndex & 1) != 0 ? 1.5f : 3.2f;
                ++hitIndex;
            }
            if (hitIn < 5)
            {
                var action = actor.Class.GetClassCategory() is ClassCategory.Healer or ClassCategory.Caster ? ActionID.MakeSpell(WHM.AID.Surecast) : ActionID.MakeSpell(WAR.AID.ArmsLength);
                hints.PlannedActions.Add((action, actor, hitIn, false));
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID is >= 79 and <= 86)
        {
            int slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                PlayerOrder[slot] = (int)iconID - 78;

            if (_chaser == null)
            {
                // initialize baits on first icon; note that icons appear over ~300ms
                _chaser = ((TEA)Module).CruiseChaser();
                _prevPos = _chaser?.Position ?? default;
                _nextHit = WorldState.FutureTime(9.5f);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AlphaSwordP2:
                ++NumCasts;
                _nextState = State.Blasty;
                SetNextBaiter(NumCasts + 1, _shapeBlasty);
                _nextHit = WorldState.FutureTime(1.5f);
                break;
            case AID.SuperBlasstyChargeP2:
            case AID.SuperBlasstyChargeP3:
                ++NumCasts;
                _nextState = State.Teleport;
                CurrentBaits.Clear();
                _nextHit = WorldState.FutureTime(_alphaDelay);
                break;

        }
    }

    private void SetNextBaiter(int order, AOEShape shape)
    {
        CurrentBaits.Clear();
        var target = Raid[Array.IndexOf(PlayerOrder, order)];
        if (_chaser != null && target != null)
            CurrentBaits.Add(new(_chaser, target, shape));
    }
}
