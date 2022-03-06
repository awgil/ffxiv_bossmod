using System;
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

        public event EventHandler<(Waymark, Vector3?)>? Changed;

        public Vector3? this[Waymark wm]
        {
            get => _positions[(int)wm];
            set
            {
                if (_positions[(int)wm] != value)
                {
                    _positions[(int)wm] = value;
                    Changed?.Invoke(this, (wm, value));
                }
            }
        }
    }
}
