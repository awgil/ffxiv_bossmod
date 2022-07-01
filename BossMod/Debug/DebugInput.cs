using Dalamud.Game.ClientState.Keys;
using ImGuiNET;
using System;
using System.Numerics;
using System.Reflection;

namespace BossMod
{
    class DebugInput
    {
        private class NavigationAxis
        {
            private InputOverride _input;
            private VirtualKey _keyFwd;
            private VirtualKey _keyBack;
            private int _curDirection;

            public int CurDirection
            {
                get => _curDirection;
                set
                {
                    if (_curDirection == value)
                        return;
                    if (_curDirection > 0)
                        _input.ForceRelease(_keyFwd);
                    else if (_curDirection < 0)
                        _input.ForceRelease(_keyBack);
                    _curDirection = value;
                    if (value > 0)
                        _input.ForcePress(_keyFwd);
                    else if (value < 0)
                        _input.ForcePress(_keyBack);
                }
            }

            public NavigationAxis(InputOverride input, VirtualKey keyFwd, VirtualKey keyBack)
            {
                _input = input;
                _keyFwd = keyFwd;
                _keyBack = keyBack;
            }
        }

        private class Navigation
        {
            public WPos? TargetPos;
            public WDir? TargetRot;

            private NavigationAxis _axisForward;
            private NavigationAxis _axisStrafe;
            private NavigationAxis _axisRotate;

            public WDir CameraFacing => ((Camera.Instance?.CameraAzimuth ?? 0).Radians() + 180.Degrees()).ToDirection();

            public Navigation(InputOverride input)
            {
                _axisForward = new(input, VirtualKey.W, VirtualKey.S);
                _axisStrafe = new(input, VirtualKey.D, VirtualKey.A);
                _axisRotate = new(input, VirtualKey.LEFT, VirtualKey.RIGHT);
            }

            public void Update()
            {
                var player = Service.ClientState.LocalPlayer;
                if (player == null || TargetPos == null && TargetRot == null)
                {
                    _axisForward.CurDirection = 0;
                    _axisStrafe.CurDirection = 0;
                    _axisRotate.CurDirection = 0;
                    return;
                }
                var cameraFacing = CameraFacing;
                var playerPos = new WPos(player.Position.XZ());

                if (TargetRot != null && TargetRot.Value.Dot(cameraFacing) < 0.996f) // ~5 degrees
                {
                    _axisRotate.CurDirection = cameraFacing.OrthoL().Dot(TargetRot.Value) > 0 ? 1 : -1;
                }
                else
                {
                    _axisRotate.CurDirection = 0;
                }

                if (TargetPos != null && (TargetPos.Value - playerPos).LengthSq() > 0.04f)
                {
                    var delta = TargetPos.Value - playerPos;
                    var projFwd = delta.Dot(cameraFacing);
                    var projRight = delta.Dot(cameraFacing.OrthoR());
                    var fwdDir = projFwd > 0 ? 1 : -1;
                    var strafeDir = projRight > 0 ? 1 : -1;
                    if (_axisForward.CurDirection != 0 && _axisForward.CurDirection == fwdDir && Math.Abs(projFwd) > 0.5 * Math.Abs(projRight))
                    {
                        // continue moving forward/backward
                    }
                    else if (_axisStrafe.CurDirection != 0 && _axisStrafe.CurDirection == strafeDir && Math.Abs(projRight) > 0.5 * Math.Abs(projFwd))
                    {
                        // continue strafing
                    }
                    else if (Math.Abs(projFwd) > Math.Abs(projRight))
                    {
                        _axisForward.CurDirection = projFwd > 0 ? 1 : -1;
                        _axisStrafe.CurDirection = 0;
                    }
                    else
                    {
                        _axisForward.CurDirection = 0;
                        _axisStrafe.CurDirection = projRight > 0 ? 1 : -1;
                    }
                }
                else
                {
                    _axisForward.CurDirection = 0;
                    _axisStrafe.CurDirection = 0;
                }
            }
        }

        private delegate byte ConvertVirtualKeyDelegate(int vkCode);
        private ConvertVirtualKeyDelegate _convertVirtualKey;

        private delegate ref int GetRefValueDelegate(int vkCode);
        private GetRefValueDelegate _getKeyRef;

        private Navigation _navi;
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
