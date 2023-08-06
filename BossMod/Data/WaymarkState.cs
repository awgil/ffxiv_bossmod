using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    public enum Waymark : byte
    {
        A, B, C, D, N1, N2, N3, N4, Count
    }

    // waymark positions in world; part of the world state structure
    public class WaymarkState
    {
        private Vector3?[] _positions = new Vector3?[8]; // null if unset

        public Vector3? this[Waymark wm]
        {
            get => _positions[(int)wm];
            private set => _positions[(int)wm] = value;
        }

        public IEnumerable<WorldState.Operation> CompareToInitial()
        {
            for (int i = 0; i < _positions.Length; ++i)
                if (_positions[i] != null)
                    yield return new OpWaymarkChange() { ID = (Waymark)i, Pos = _positions[i] };
        }

        // implementation of operations
        public event EventHandler<OpWaymarkChange>? Changed;
        public class OpWaymarkChange : WorldState.Operation
        {
            public Waymark ID;
            public Vector3? Pos;

            protected override void Exec(WorldState ws)
            {
                ws.Waymarks[ID] = Pos;
                ws.Waymarks.Changed?.Invoke(ws, this);
            }

            public override void Write(ReplayRecorder.Output output)
            {
                if (Pos != null)
                    WriteTag(output, "WAY+").Emit((byte)ID).Emit(Pos.Value);
                else
                    WriteTag(output, "WAY-").Emit((byte)ID);
            }
        }
    }
}
