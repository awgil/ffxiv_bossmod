using System;

namespace BossMod.Endwalker.Extreme.Ex3Endsigner
{
    class DespairUnforgotten : BossComponent
    {
        private enum State { None, Donut, Spread, Flare, Stack }

        public bool Done { get; private set; }
        private State[] _states = new State[PartyState.MaxPartySize * 4];
        private int[] _doneCasts = new int[PartyState.MaxPartySize];

        public DespairUnforgotten()
        {
            PartyStatusUpdate(SID.RewindDespair, UpdateRewindStatus);
            PartyStatusUpdate(SID.EchoesOfNausea, (module, slot, _, _, _, _) => UpdateEchoesStatus(module, slot, State.Donut));
            PartyStatusUpdate(SID.EchoesOfBefoulment, (module, slot, _, _, _, _) => UpdateEchoesStatus(module, slot, State.Spread));
            PartyStatusUpdate(SID.EchoesOfFuture, (module, slot, _, _, _, _) => UpdateEchoesStatus(module, slot, State.Flare));
            PartyStatusUpdate(SID.EchoesOfBenevolence, (module, slot, _, _, _, _) => UpdateEchoesStatus(module, slot, State.Stack));
            PartyStatusLose(SID.EchoesOfNausea, LoseEchoesStatus);
            PartyStatusLose(SID.EchoesOfBefoulment, LoseEchoesStatus);
            PartyStatusLose(SID.EchoesOfFuture, LoseEchoesStatus);
            PartyStatusLose(SID.EchoesOfBenevolence, LoseEchoesStatus);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
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

        private void UpdateRewindStatus(BossModule module, int slot, Actor actor, ulong sourceID, ushort extra, DateTime expireAt)
        {
            int rings = extra switch
            {
                0x17C => 1,
                0x17D => 2,
                0x17E => 3,
                _ => 0,
            };
            if (rings == 0)
                module.ReportError(this, $"Unexpected extra {extra:X} for rewind status");
            else
                _states[slot * 4 + 3] = _states[slot * 4 + 3 - rings];
        }

        private void UpdateEchoesStatus(BossModule module, int slot, State state)
        {
            if (_doneCasts[slot] > 3)
                module.ReportError(this, $"Unexpected state change after {_doneCasts[slot]} casts");
            else
                _states[slot * 4 + _doneCasts[slot]] = state;
        }

        private void LoseEchoesStatus(BossModule module, int slot, Actor actor)
        {
            Done |= ++_doneCasts[slot] > 3;
        }
    }
}
