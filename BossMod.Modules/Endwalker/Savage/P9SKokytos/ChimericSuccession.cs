namespace BossMod.Endwalker.Savage.P9SKokytos;

class ChimericSuccession(BossModule module) : Components.UniformStackSpread(module, 6, 20, 4, alwaysShowSpreads: true)
{
    public int NumCasts { get; private set; }
    private readonly Actor?[] _baitOrder = [null, null, null, null];
    private BitMask _forbiddenStack;
    private DateTime _jumpActivation;

    public bool JumpActive => _jumpActivation != default;

    public override void Update()
    {
        Stacks.Clear();
        var target = JumpActive ? Raid.WithSlot().ExcludedFromMask(_forbiddenStack).Actors().Farthest(Module.PrimaryActor.Position) : null;
        if (target != null)
            AddStack(target, _jumpActivation, _forbiddenStack);
        base.Update();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FrontFirestrikes or AID.RearFirestrikes)
            _jumpActivation = Module.CastFinishAt(spell, 0.4f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Icemeld1:
            case AID.Icemeld2:
            case AID.Icemeld3:
            case AID.Icemeld4:
                ++NumCasts;
                InitBaits();
                break;
            case AID.PyremeldFront:
            case AID.PyremeldRear:
                _jumpActivation = default;
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        int order = (IconID)iconID switch
        {
            IconID.Icon1 => 0,
            IconID.Icon2 => 1,
            IconID.Icon3 => 2,
            IconID.Icon4 => 3,
            _ => -1
        };
        if (order < 0)
            return;
        _baitOrder[order] = actor;
        _forbiddenStack.Set(Raid.FindSlot(actor.InstanceID));
        if (order == 0)
            InitBaits();
    }

    private void InitBaits()
    {
        Spreads.Clear();
        var target = NumCasts < _baitOrder.Length ? _baitOrder[NumCasts] : null;
        if (target != null)
            AddSpread(target, WorldState.FutureTime(NumCasts == 0 ? 10.1f : 3));
    }
}

// TODO: think of a way to show baits before cast start to help aiming outside...
class SwingingKickFront(BossModule module) : Components.StandardAOEs(module, AID.SwingingKickFront, new AOEShapeCone(40, 90.Degrees()));
class SwingingKickRear(BossModule module) : Components.StandardAOEs(module, AID.SwingingKickRear, new AOEShapeCone(40, 90.Degrees()));
