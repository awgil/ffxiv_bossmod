using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Keys;
using System;

namespace BossMod.AI
{
    // utility for simulating user actions based on AI decisions:
    // - navigation
    // - using actions safely (without spamming, not in cutscenes, etc)
    class AIController
    {
        private class NaviAxis
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
                            _input.SimulatePress(value > 0 ? _keyFwd : _keyBack);
                        return;
                    }

                    if (_curDirection > 0)
                        _input.SimulateRelease(_keyFwd);
                    else if (_curDirection < 0)
                        _input.SimulateRelease(_keyBack);
                    _curDirection = value;
                    if (value > 0)
                        _input.SimulatePress(_keyFwd);
                    else if (value < 0)
                        _input.SimulatePress(_keyBack);
                }
            }

            public NaviAxis(InputOverride input, VirtualKey keyFwd, VirtualKey keyBack)
            {
                _input = input;
                _keyFwd = keyFwd;
                _keyBack = keyBack;
            }
        }

        public WPos? NaviTargetPos;
        public WDir? NaviTargetRot;
        public float? NaviTargetVertical;
        public bool AllowInterruptingCastByMovement;
        public bool ForceFacing;

        private NaviAxis _axisForward;
        private NaviAxis _axisStrafe;
        private NaviAxis _axisRotate;
        private NaviAxis _axisVertical;
        private InputOverride _input;
        private Autorotation _autorot;

        public bool InCutscene => Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78] || Service.Condition[ConditionFlag.Occupied33] || Service.Condition[ConditionFlag.BetweenAreas];
        public bool IsMounted => Service.Condition[ConditionFlag.Mounted];
        public bool IsVerticalAllowed => Service.Condition[ConditionFlag.InFlight];
        public WDir CameraFacing => ((Camera.Instance?.CameraAzimuth ?? 0).Radians() + 180.Degrees()).ToDirection();
        public Angle CameraAltitude => (Camera.Instance?.CameraAltitude ?? 0).Radians();

        public unsafe AIController(InputOverride input, Autorotation autorot)
        {
            _axisForward = new(input, VirtualKey.W, VirtualKey.S);
            _axisStrafe = new(input, VirtualKey.D, VirtualKey.A);
            _axisRotate = new(input, VirtualKey.LEFT, VirtualKey.RIGHT);
            _axisVertical = new(input, VirtualKey.UP, VirtualKey.DOWN);
            _input = input;
            _autorot = autorot;
        }

        public void Clear()
        {
            NaviTargetPos = null;
            NaviTargetRot = null;
            NaviTargetVertical = null;
            AllowInterruptingCastByMovement = false;
            ForceFacing = false;
        }

        public void SetPrimaryTarget(Actor? actor)
        {
            if (Service.TargetManager.Target?.ObjectId != actor?.InstanceID)
                Service.TargetManager.SetTarget(actor != null ? Service.ObjectTable.SearchById((uint)actor.InstanceID) : null);
        }

        public void SetFocusTarget(Actor? actor)
        {
            if (Service.TargetManager.FocusTarget?.ObjectId != actor?.InstanceID)
                Service.TargetManager.SetFocusTarget(actor != null ? Service.ObjectTable.SearchById((uint)actor.InstanceID) : null);
        }

        public unsafe void Update(Actor? player)
        {
            if (player == null || InCutscene)
            {
                _axisForward.CurDirection = 0;
                _axisStrafe.CurDirection = 0;
                _axisRotate.CurDirection = 0;
                _axisVertical.CurDirection = 0;
                return;
            }

            if (ForceFacing && NaviTargetRot != null && player.Rotation.ToDirection().Dot(NaviTargetRot.Value) < 0.996f)
            {
                var am = ActionManagerEx.Instance!;
                am.FaceDirection(NaviTargetRot.Value);
                _axisForward.CurDirection = am.CastTimeRemaining > 0 ? 1 : 0; // this is a hack to cancel any cast...
            }

            bool moveRequested = _input.IsMoveRequested();
            var cameraFacing = CameraFacing;
            if (!moveRequested && NaviTargetRot != null && NaviTargetRot.Value.Dot(cameraFacing) < 0.996f) // ~5 degrees
            {
                _axisRotate.CurDirection = cameraFacing.OrthoL().Dot(NaviTargetRot.Value) > 0 ? 1 : -1;
            }
            else
            {
                _axisRotate.CurDirection = 0;
            }

            bool forbidMovement = moveRequested || !AllowInterruptingCastByMovement && (player.CastInfo != null && !player.CastInfo.EventHappened || _autorot.AboutToStartCast);
            if (NaviTargetPos != null && !forbidMovement && (NaviTargetPos.Value - player.Position).LengthSq() > 0.04f)
            {
                var delta = NaviTargetPos.Value - player.Position;
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

            if (NaviTargetVertical != null && IsVerticalAllowed && NaviTargetPos != null)
            {
                var deltaY = NaviTargetVertical.Value - player.PosRot.Y;
                var deltaXZ = (NaviTargetPos.Value - player.Position).Length();
                var deltaAltitude = Angle.FromDirection(new(-deltaY, deltaXZ)) - CameraAltitude;
                _axisVertical.CurDirection = deltaAltitude.Rad > 0.088f ? -1 : deltaAltitude.Rad < -0.088f ? +1 : 0;
            }
            else
            {
                _axisVertical.CurDirection = 0;
            }
        }
    }
}
