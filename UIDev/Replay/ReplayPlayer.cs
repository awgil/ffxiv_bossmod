using BossMod;
using System;

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

            var ts = Replay.Ops[_nextOp].Timestamp;
            while (_nextOp < Replay.Ops.Count && Replay.Ops[_nextOp].Timestamp == ts)
                WorldState.Execute(Replay.Ops[_nextOp++]);
            return true;
        }

        // execute actions to advance current time to specific timestamp
        // after this call, next-op points to first operation with timestamp *greater* than what was passed, and world's current time is *less than or equal* to what was passed
        public void AdvanceTo(DateTime timestamp, Action update)
        {
            while (_nextOp < Replay.Ops.Count && Replay.Ops[_nextOp].Timestamp <= timestamp)
            {
                TickForward();
                update();
            }
        }
    }
}
