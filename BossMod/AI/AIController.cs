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

        private class NaviInput
        {
            private InputOverride _input;
            private VirtualKey _key;
            private bool _held;

            public bool Held
            {
                get => _held;
                set
                {
                    if (_held == value)
                    {
                        if (value && !Service.KeyState[_key])
                            _input.SimulatePress(_key);
                        return;
                    }

                    if (_held)
                        _input.SimulateRelease(_key);
                    _held = value;
                    if (value)
                        _input.SimulatePress(_key);
                }
            }

            public NaviInput(InputOverride input, VirtualKey key)
            {
                _input = input;
                _key = key;
            }
        }

        public InputOverride Input { get; private init; }
        public WPos? NaviTargetPos;
        public WDir? NaviTargetRot;
        public float? NaviTargetVertical;
        public bool AllowInterruptingCastByMovement;
        public bool ForceCancelCast;
        public bool ForceFacing;
        public bool WantJump;

        private NaviAxis _axisForward;
        private NaviAxis _axisStrafe;
        private NaviAxis _axisRotate;
        private NaviAxis _axisVertical;
        private NaviInput _keyJump;
        private Autorotation _autorot;

        public bool InCutscene => Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78] || Service.Condition[ConditionFlag.Occupied33] || Service.Condition[ConditionFlag.BetweenAreas] || Service.Condition[ConditionFlag.OccupiedInQuestEvent];
        public bool IsMounted => Service.Condition[ConditionFlag.Mounted];
        public bool IsVerticalAllowed => Service.Condition[ConditionFlag.InFlight];
        public Angle CameraFacing => ((Camera.Instance?.CameraAzimuth ?? 0).Radians() + 180.Degrees());
        public Angle CameraAltitude => (Camera.Instance?.CameraAltitude ?? 0).Radians();

        public AIController(InputOverride input, Autorotation autorot)
        {
            Input = input;
            _axisForward = new(input, VirtualKey.W, VirtualKey.S);
            _axisStrafe = new(input, VirtualKey.D, VirtualKey.A);
            _axisRotate = new(input, VirtualKey.LEFT, VirtualKey.RIGHT);
            _axisVertical = new(input, VirtualKey.UP, VirtualKey.DOWN);
            _keyJump = new(input, VirtualKey.SPACE);
            _autorot = autorot;
        }

        public void Clear()
        {
            NaviTargetPos = null;
            NaviTargetRot = null;
            NaviTargetVertical = null;
            AllowInterruptingCastByMovement = false;
            ForceCancelCast = false;
            ForceFacing = false;
            WantJump = false;
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

        public void Update(Actor? player)
        {
            if (player == null || player.IsDead || InCutscene)
            {
                _axisForward.CurDirection = 0;
                _axisStrafe.CurDirection = 0;
                _axisRotate.CurDirection = 0;
                _axisVertical.CurDirection = 0;
                _keyJump.Held = false;
                Input.GamepadOverridesEnabled = false;
                return;
            }

            if (ForceFacing && NaviTargetRot != null && player.Rotation.ToDirection().Dot(NaviTargetRot.Value) < 0.996f)
            {
                ActionManagerEx.Instance!.FaceDirection(NaviTargetRot.Value);
            }

            bool moveRequested = Input.IsMoveRequested();
            var cameraFacing = CameraFacing;
            var cameraFacingDir = cameraFacing.ToDirection();
            if (!moveRequested && NaviTargetRot != null && NaviTargetRot.Value.Dot(cameraFacingDir) < 0.996f) // ~5 degrees
            {
                _axisRotate.CurDirection = cameraFacingDir.OrthoL().Dot(NaviTargetRot.Value) > 0 ? 1 : -1;
            }
            else
            {
                _axisRotate.CurDirection = 0;
            }

            bool castInProgress = player.CastInfo != null && !player.CastInfo.EventHappened;
            bool forbidMovement = moveRequested || !AllowInterruptingCastByMovement && (castInProgress || _autorot.AboutToStartCast);
            if (NaviTargetPos != null && !forbidMovement && (NaviTargetPos.Value - player.Position).LengthSq() > 0.01f)
            {
                var dir = cameraFacing - Angle.FromDirection(NaviTargetPos.Value - player.Position);
                Input.GamepadOverridesEnabled = true;
                Input.GamepadOverrides[3] = (int)(100 * dir.Sin());
                Input.GamepadOverrides[4] = (int)(100 * dir.Cos());
                _axisForward.CurDirection = Input.GamepadOverrides[4] > 10 ? 1 : Input.GamepadOverrides[4] < 10 ? -1 : 0; // this is a hack, needed to prevent afk :( this will be ignored anyway due to gamepad inputs
                _keyJump.Held = !_keyJump.Held && WantJump;
            }
            else
            {
                Input.GamepadOverridesEnabled = false;
                _axisForward.CurDirection = ForceCancelCast && castInProgress ? 1 : 0; // this is a hack to cancel any cast...
                _keyJump.Held = false;
            }

            if (NaviTargetVertical != null && IsVerticalAllowed && NaviTargetPos != null)
            {
                var deltaY = NaviTargetVertical.Value - player.PosRot.Y;
                var deltaXZ = (NaviTargetPos.Value - player.Position).Length();
                var deltaAltitude = (CameraAltitude - Angle.FromDirection(new(Input.GamepadOverrides[4] < 0 ? deltaY : -deltaY, deltaXZ))).Deg;
                Input.GamepadOverrides[6] = Math.Clamp((int)(deltaAltitude * 5), -100, 100);
            }
            else
            {
                Input.GamepadOverrides[6] = 0;
            }
        }
    }
}
