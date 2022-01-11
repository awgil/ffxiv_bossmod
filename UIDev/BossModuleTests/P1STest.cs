using ImGuiNET;
using BossMod;
using System;

namespace UIDev
{
    class P1STest : ITest
    {
        private float _azimuth;
        private WorldState _ws;
        private P1S _o;

        public P1STest()
        {
            _ws = new();
            _ws.AddActor(1, (uint)P1S.OID.Boss, WorldState.ActorType.Enemy, new(100, 0, 100), 0, 1);
            _o = new P1S(_ws);
        }

        public void Dispose()
        {
            _o.Dispose();
        }

        public void Draw()
        {
            _o.Draw(_azimuth / 180 * MathF.PI);

            ImGui.DragFloat("Camera azimuth", ref _azimuth, 1, -180, 180);

            if (ImGui.Button(!_ws.PlayerInCombat ? "Pull" : "Wipe"))
            {
                _ws.UpdateCastInfo(_ws.FindActor(1)!, null);
                _ws.PlayerInCombat = !_ws.PlayerInCombat;
            }

            var boss = _ws.FindActor(1)!;
            int cnt = 0;
            foreach (var e in Enum.GetValues<P1S.AID>())
            {
                if (ImGui.Button(e.ToString()))
                    _ws.UpdateCastInfo(boss, boss.CastInfo == null ? new WorldState.CastInfo { ActionID = (uint)e } : null);
                if (++cnt % 5 != 0)
                    ImGui.SameLine();
            }
        }
    }
}
