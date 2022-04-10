using BossMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIDev
{
    // utility for applying replay operations to world state, accurate scrolling, etc.
    class ReplayPlayer
    {
        public Replay Replay;
        public WorldState WorldState = new();
        private int _nextOp = 0; // first unexecuted operation; note that it corresponds to first operation with timestamp > that worldstate's current

        public ReplayPlayer(Replay r)
        {
            Replay = r;
        }

        // reset to empty state; note that world state is recreated
        public void Reset()
        {
            WorldState = new();
            _nextOp = 0;
        }

        // execute next group of operations that share timestamp; returns false if we've reached the end
        public bool TickForward()
        {
            if (_nextOp == Replay.Ops.Count)
                return false;

            WorldState.CurrentTime = Replay.Ops[_nextOp].Timestamp;
            while (_nextOp < Replay.Ops.Count && Replay.Ops[_nextOp].Timestamp == WorldState.CurrentTime)
                Replay.Ops[_nextOp++].Redo(WorldState);
            return true;
        }

        // undo last group of operations that share timestamp; returns false if we've reached the beginning
        public bool TickBackward()
        {
            if (_nextOp == 0)
                return false;

            do
            {
                Replay.Ops[--_nextOp].Undo(WorldState);
            } while (Replay.Ops[_nextOp].Timestamp == WorldState.CurrentTime && _nextOp > 0);
            WorldState.CurrentTime = _nextOp > 0 ? Replay.Ops[_nextOp - 1].Timestamp : new();
            return true;
        }

        // execute actions to advance current time to specific timestamp
        public bool AdvanceTo(DateTime timestamp, Action update)
        {
            if (timestamp <= WorldState.CurrentTime)
                return false;

            while (_nextOp < Replay.Ops.Count && Replay.Ops[_nextOp].Timestamp <= timestamp)
            {
                TickForward();
                update();
            }

            if (WorldState.CurrentTime < timestamp)
            {
                WorldState.CurrentTime = timestamp;
                update();
            }

            return true;
        }

        // undo actions to specific timestamp
        public bool ReverseTo(DateTime timestamp, Action update)
        {
            if (timestamp >= WorldState.CurrentTime)
                return false;

            while (_nextOp > 0 && Replay.Ops[_nextOp - 1].Timestamp > timestamp)
            {
                TickBackward();
                update();
            }

            if (WorldState.CurrentTime > timestamp)
            {
                WorldState.CurrentTime = timestamp;
                update();
            }

            return true;
        }
    }
}
