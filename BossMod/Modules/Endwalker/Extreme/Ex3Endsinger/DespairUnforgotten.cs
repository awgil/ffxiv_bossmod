namespace BossMod.Endwalker.Extreme.Ex3Endsigner;

class DespairUnforgotten(BossModule module) : BossComponent(module)
{
    private enum State { None, Donut, Spread, Flare, Stack }

    public bool Done { get; private set; }
    private readonly State[] _states = new State[PartyState.MaxPartySize * 4];
    private readonly int[] _doneCasts = new int[PartyState.MaxPartySize];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        // TODO: improve
        if (_doneCasts[slot] > 3)
            return;
        switch (_states[slot * 4 + _doneCasts[slot]])
        {
            case State.Donut:
            case State.Stack:
                hints.Add("Stay in center", false);
                break;
            case State.Spread:
            case State.Flare:
                hints.Add("Go to edge", false);
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // TODO: think what to draw here...
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.RewindDespair:
                int rings = status.Extra switch
                {
                    0x17C => 1,
                    0x17D => 2,
                    0x17E => 3,
                    _ => 0,
                };
                if (rings == 0)
                {
                    ReportError($"Unexpected extra {status.Extra:X} for rewind status");
                    break;
                }

                int slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    _states[slot * 4 + 3] = _states[slot * 4 + 3 - rings];
                break;
            case SID.EchoesOfNausea:
                ModifyState(actor, State.Donut);
                break;
            case SID.EchoesOfBefoulment:
                ModifyState(actor, State.Spread);
                break;
            case SID.EchoesOfFuture:
                ModifyState(actor, State.Flare);
                break;
            case SID.EchoesOfBenevolence:
                ModifyState(actor, State.Stack);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.EchoesOfNausea:
            case SID.EchoesOfBefoulment:
            case SID.EchoesOfFuture:
            case SID.EchoesOfBenevolence:
                int slot = WorldState.Party.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    Done |= ++_doneCasts[slot] > 3;
                break;
        }
    }

    private void ModifyState(Actor actor, State state)
    {
        int slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
        {
            if (_doneCasts[slot] > 3)
                ReportError($"Unexpected state change after {_doneCasts[slot]} casts");
            else
                _states[slot * 4 + _doneCasts[slot]] = state;
        }
    }
}
