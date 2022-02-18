using System.Numerics;

namespace BossMod.P4S
{
    using static BossModule;

    // component showing where to drag boss for max pinax uptime
    class PinaxUptime : Component
    {
        private enum Corner { NE, SE, SW, NW }

        private P4S _module;
        private Corner[] _assignments = new Corner[4];

        public PinaxUptime(P4S module)
        {
            _module = module;
        }

        public override void DrawArenaForeground(MiniArena arena)
        {
            var pc = _module.Player();
            if (pc?.Role != Role.Tank)
                return;

            // draw position between lighting and fire squares
            var offset = (_assignments[0], _assignments[1]) switch
            {
                (Corner.NE, Corner.SE) or (Corner.SE, Corner.NE) => Vector3.UnitX,
                (Corner.SE, Corner.SW) or (Corner.SW, Corner.SE) => Vector3.UnitZ,
                (Corner.SW, Corner.NW) or (Corner.NW, Corner.SW) => -Vector3.UnitX,
                (Corner.NW, Corner.NE) or (Corner.NE, Corner.NW) => -Vector3.UnitZ,
                _ => Vector3.Zero
            };
            if (offset == Vector3.Zero)
                return;

            arena.AddCircle(arena.WorldCenter + 18 * offset, 2, arena.ColorSafe);
        }

        public override void OnEventEnvControl(uint featureID, byte index, uint state)
        {
            if (featureID == 0x8003759C && state == 0x00020001 && index >= 5 && index <= 20)
            {
                int i = index - 5;
                _assignments[i >> 2] = (Corner)(i & 3);
            }
        }
    }
}
