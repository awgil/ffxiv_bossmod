using ImGuiNET;
using System;
using System.Numerics;
using System.Reflection;

namespace BossMod
{
    class DebugInput : IDisposable
    {
        private delegate byte ConvertVirtualKeyDelegate(int vkCode);
        private ConvertVirtualKeyDelegate _convertVirtualKey;

        private delegate ref int GetRefValueDelegate(int vkCode);
        private GetRefValueDelegate _getKeyRef;

        private WorldState _ws;
        private AI.AIController _navi;
        private Vector2 _dest;
        private WPos _prevPos;
        private bool _jump;
        private DateTime _prevFrame;
        private bool _gamepadAxisOverrideEnable;
        private float _gamepadAxisOverrideAngle;
        private int _gamepadButtonOverride = -1;
        private int _gamepadButtonValue;
        private bool _gamepadNavigate;

        public DebugInput(InputOverride inputOverride, Autorotation autorot)
        {
            _convertVirtualKey = Service.KeyState.GetType().GetMethod("ConvertVirtualKey", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<ConvertVirtualKeyDelegate>(Service.KeyState);
            _getKeyRef = Service.KeyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<GetRefValueDelegate>(Service.KeyState);
            _ws = autorot.WorldState;
            _navi = new(inputOverride, autorot);
        }

        public void Dispose()
        {
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

            ImGui.Checkbox("Jump!", ref _jump);
            ImGui.InputFloat2("Destination", ref _dest);
            ImGui.SameLine();
            if (ImGui.Button("Move!"))
            {
                _navi.NaviTargetPos = new(_dest);

                //var toTarget = _navi.NaviTargetPos.Value - curPos;
                //_navi.NaviTargetRot = toTarget.Normalized();

                //var cameraFacing = _navi.CameraFacing;
                //var dot = cameraFacing.Dot(_navi.NaviTargetRot.Value);
                //if (dot < -0.707107f)
                //    _navi.NaviTargetRot = -_navi.NaviTargetRot.Value;
                //else if (dot < 0.707107f)
                //    _navi.NaviTargetRot = cameraFacing.OrthoL().Dot(_navi.NaviTargetRot.Value) > 0 ? _navi.NaviTargetRot.Value.OrthoR() : _navi.NaviTargetRot.Value.OrthoL();

                _navi.WantJump = _jump;
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel move"))
            {
                _navi.Clear();
            }
            _navi.Update(player);

            DrawGamepad();

            foreach (var vk in Service.KeyState.GetValidVirtualKeys())
            {
                ImGui.TextUnformatted($"{vk} ({(int)vk}): internal code={_convertVirtualKey((int)vk)}, state={_getKeyRef((int)vk)}");
            }
        }

        private void DrawGamepad()
        {
            ImGui.SliderFloat("Gamepad angle", ref _gamepadAxisOverrideAngle, -180, 180);
            ImGui.SameLine();
            ImGui.Checkbox("Gamepad move", ref _gamepadAxisOverrideEnable);

            ImGui.SliderInt("Gamepad value", ref _gamepadButtonValue, -100, 100);
            ImGui.SameLine();
            ImGui.InputInt("Gamepad button", ref _gamepadButtonOverride);

            ImGui.Checkbox("Gamepad navigate", ref _gamepadNavigate);

            Array.Fill(_navi.Input.GamepadOverrides, 0);
            if (_gamepadButtonOverride >= 0 && _gamepadButtonOverride < _navi.Input.GamepadOverrides.Length)
            {
                _navi.Input.GamepadOverridesEnabled = true;
                _navi.Input.GamepadOverrides[_gamepadButtonOverride] = _gamepadButtonValue;
            }
            else if (_gamepadAxisOverrideEnable)
            {
                _navi.Input.GamepadOverridesEnabled = true;
                _navi.Input.GamepadOverrides[3] = (int)(100 * _gamepadAxisOverrideAngle.Degrees().Sin());
                _navi.Input.GamepadOverrides[4] = (int)(100 * _gamepadAxisOverrideAngle.Degrees().Cos());
            }
            else if (_gamepadNavigate)
            {
                var dest = new WPos(_dest);
                var dir = (Camera.Instance?.CameraAzimuth ?? 0).Radians() - Angle.FromDirection(dest - (_ws.Party.Player()?.Position ?? dest)) + 180.Degrees();
                _navi.Input.GamepadOverridesEnabled = true;
                _navi.Input.GamepadOverrides[3] = (int)(100 * dir.Sin());
                _navi.Input.GamepadOverrides[4] = (int)(100 * dir.Cos());
            }
            else
            {
                _navi.Input.GamepadOverridesEnabled = false;
            }
        }
    }
}
