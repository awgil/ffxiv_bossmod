using Dalamud.Game.ClientState.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.AI
{
    class Navigation
    {
        private class Axis
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
                    {
                        if (value != 0 && !Service.KeyState[value > 0 ? _keyFwd : _keyBack])
                            _input.ForcePress(value > 0 ? _keyFwd : _keyBack);
                        return;
                    }

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

            public Axis(InputOverride input, VirtualKey keyFwd, VirtualKey keyBack)
            {
                _input = input;
                _keyFwd = keyFwd;
                _keyBack = keyBack;
            }
        }

        public WPos? TargetPos;
        public WDir? TargetRot;

        private Axis _axisForward;
        private Axis _axisStrafe;
        private Axis _axisRotate;

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
                    _axisForward.CurDirection = fwdDir;
                    _axisStrafe.CurDirection = 0;
                }
                else if (_axisStrafe.CurDirection != 0 && _axisStrafe.CurDirection == strafeDir && Math.Abs(projRight) > 0.5 * Math.Abs(projFwd))
                {
                    // continue strafing
                    _axisForward.CurDirection = 0;
                    _axisStrafe.CurDirection = strafeDir;
                }
                else if (Math.Abs(projFwd) > Math.Abs(projRight))
                {
                    _axisForward.CurDirection = fwdDir;
                    _axisStrafe.CurDirection = 0;
                }
                else
                {
                    _axisForward.CurDirection = 0;
                    _axisStrafe.CurDirection = strafeDir;
                }
            }
            else
            {
                _axisForward.CurDirection = 0;
                _axisStrafe.CurDirection = 0;
            }
        }
    }
}
