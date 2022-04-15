namespace BossMod.Endwalker.EndsingerEx
{
    class DespairUnforgotten : BossModule.Component
    {
        private enum State { None, Donut, Spread, Flare, Stack }

        public bool Done { get; private set; }
        private State[] _states = new State[PartyState.MaxSize * 4];
        private int[] _doneCasts = new int[PartyState.MaxSize];

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
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

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            // TODO: think what to draw here...
        }

        public override void OnStatusGain(BossModule module, Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.RewindDespair:
                    int rings = actor.Statuses[index].Extra switch
                    {
                        0x17C => 1,
                        0x17D => 2,
                        0x17E => 3,
                        _ => 0,
                    };
                    if (rings == 0)
                    {
                        module.ReportError(this, $"Unexpected extra {actor.Statuses[index].Extra:X} for rewind status");
                        break;
                    }

                    int slot = module.WorldState.Party.FindSlot(actor.InstanceID);
                    if (slot >= 0)
                        _states[slot * 4 + 3] = _states[slot * 4 + 3 - rings];
                    break;
                case SID.EchoesOfNausea:
                    ModifyState(module, actor, State.Donut);
                    break;
                case SID.EchoesOfBefoulment:
                    ModifyState(module, actor, State.Spread);
                    break;
                case SID.EchoesOfFuture:
                    ModifyState(module, actor, State.Flare);
                    break;
                case SID.EchoesOfBenevolence:
                    ModifyState(module, actor, State.Stack);
                    break;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.EchoesOfNausea:
                case SID.EchoesOfBefoulment:
                case SID.EchoesOfFuture:
                case SID.EchoesOfBenevolence:
                    int slot = module.WorldState.Party.FindSlot(actor.InstanceID);
                    if (slot >= 0)
                        Done |= ++_doneCasts[slot] > 3;
                    break;
            }
        }

        private void ModifyState(BossModule module, Actor actor, State state)
        {
            int slot = module.WorldState.Party.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                if (_doneCasts[slot] > 2)
                {
                    module.ReportError(this, $"Unexpected state change after {_doneCasts[slot]} casts");
                    return;
                }
                _states[slot * 4 + _doneCasts[slot]] = state;
            }
        }
    }
}
