using Dalamud.Game.ClientState.Keys;
using ImGuiNET;
using System;
using System.Numerics;
using System.Reflection;

namespace BossMod
{
    class DebugInput
    {
        private delegate byte ConvertVirtualKeyDelegate(int vkCode);
        private ConvertVirtualKeyDelegate _convertVirtualKey;

        private delegate ref int GetRefValueDelegate(int vkCode);
        private GetRefValueDelegate _getKeyRef;

        private WorldState _ws;
        private AI.AIController _navi;
        private Vector2 _dest;
        private WPos _prevPos;
        private DateTime _prevFrame;

        public DebugInput(InputOverride inputOverride, Autorotation autorotation)
        {
            _convertVirtualKey = Service.KeyState.GetType().GetMethod("ConvertVirtualKey", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<ConvertVirtualKeyDelegate>(Service.KeyState);
            _getKeyRef = Service.KeyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<GetRefValueDelegate>(Service.KeyState);
            _ws = autorotation.WorldState;
            _navi = new(inputOverride, autorotation);
        }

        public void Draw()
        {
            var curFrame = DateTime.Now;
            var dt = (curFrame - _prevFrame).TotalSeconds;
            _prevFrame = curFrame;

            var player = _ws.Party.Player();
            var curPos = player?.Position ?? new();
            ImGui.TextUnformatted($"Speed: {(curPos - _prevPos).Length() / dt:f2}");
            _prevPos = curPos;

            ImGui.InputFloat2("Destination", ref _dest);
            ImGui.SameLine();
            if (ImGui.Button("Move!"))
            {
                _navi.NaviTargetPos = new(_dest);

                var toTarget = _navi.NaviTargetPos.Value - curPos;
                _navi.NaviTargetRot = toTarget.Normalized();

                var cameraFacing = _navi.CameraFacing;
                var dot = cameraFacing.Dot(_navi.NaviTargetRot.Value);
                if (dot < -0.707107f)
                    _navi.NaviTargetRot = -_navi.NaviTargetRot.Value;
                else if (dot < 0.707107f)
                    _navi.NaviTargetRot = cameraFacing.OrthoL().Dot(_navi.NaviTargetRot.Value) > 0 ? _navi.NaviTargetRot.Value.OrthoR() : _navi.NaviTargetRot.Value.OrthoL();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel move"))
            {
                _navi.Clear();
            }
            _navi.Update(player);

            foreach (var vk in Service.KeyState.GetValidVirtualKeys())
            {
                ImGui.TextUnformatted($"{vk} ({(int)vk}): internal code={_convertVirtualKey((int)vk)}, state={_getKeyRef((int)vk)}");
            }
        }
    }
}
