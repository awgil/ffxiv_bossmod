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

        private AI.Navigation _navi;
        private Vector2 _dest;
        private WPos _prevPos;
        private DateTime _prevFrame;

        public DebugInput(InputOverride inputOverride)
        {
            _convertVirtualKey = Service.KeyState.GetType().GetMethod("ConvertVirtualKey", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<ConvertVirtualKeyDelegate>(Service.KeyState);
            _getKeyRef = Service.KeyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<GetRefValueDelegate>(Service.KeyState);
            _navi = new(inputOverride);
        }

        public void Draw()
        {
            var curFrame = DateTime.Now;
            var dt = (curFrame - _prevFrame).TotalSeconds;
            _prevFrame = curFrame;

            var player = Service.ClientState.LocalPlayer;
            var curPos = new WPos(player?.Position.XZ() ?? new());
            ImGui.TextUnformatted($"Speed: {(curPos - _prevPos).Length() / dt:f2}");
            _prevPos = curPos;

            ImGui.InputFloat2("Destination", ref _dest);
            ImGui.SameLine();
            if (ImGui.Button("Move!"))
            {
                _navi.TargetPos = new(_dest);

                var toTarget = _navi.TargetPos.Value - curPos;
                _navi.TargetRot = toTarget.Normalized();

                var cameraFacing = _navi.CameraFacing;
                var dot = cameraFacing.Dot(_navi.TargetRot.Value);
                if (dot < -0.707107f)
                    _navi.TargetRot = -_navi.TargetRot.Value;
                else if (dot < 0.707107f)
                    _navi.TargetRot = cameraFacing.OrthoL().Dot(_navi.TargetRot.Value) > 0 ? _navi.TargetRot.Value.OrthoR() : _navi.TargetRot.Value.OrthoL();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel move"))
            {
                _navi.TargetPos = null;
                _navi.TargetRot = null;
            }
            _navi.Update();

            foreach (var vk in Service.KeyState.GetValidVirtualKeys())
            {
                ImGui.TextUnformatted($"{vk} ({(int)vk}): internal code={_convertVirtualKey((int)vk)}, state={_getKeyRef((int)vk)}");
            }
        }
    }
}
