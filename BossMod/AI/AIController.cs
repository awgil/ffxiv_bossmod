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
                            ActionManagerEx.Instance!.InputOverride.SimulatePress(value > 0 ? _keyFwd : _keyBack);
                        return;
                    }

                    if (_curDirection > 0)
                        ActionManagerEx.Instance!.InputOverride.SimulateRelease(_keyFwd);
                    else if (_curDirection < 0)
                        ActionManagerEx.Instance!.InputOverride.SimulateRelease(_keyBack);
                    _curDirection = value;
                    if (value > 0)
                        ActionManagerEx.Instance!.InputOverride.SimulatePress(_keyFwd);
                    else if (value < 0)
                        ActionManagerEx.Instance!.InputOverride.SimulatePress(_keyBack);
                }
            }

            public NaviAxis(VirtualKey keyFwd, VirtualKey keyBack)
            {
                _keyFwd = keyFwd;
                _keyBack = keyBack;
            }
        }

        private class NaviInput
        {
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
                            ActionManagerEx.Instance!.InputOverride.SimulatePress(_key);
                        return;
                    }

                    if (_held)
                        ActionManagerEx.Instance!.InputOverride.SimulateRelease(_key);
                    _held = value;
                    if (value)
                        ActionManagerEx.Instance!.InputOverride.SimulatePress(_key);
                }
            }

            public NaviInput(VirtualKey key)
            {
                _key = key;
            }
        }

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

        public bool InCutscene => Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78] || Service.Condition[ConditionFlag.Occupied33] || Service.Condition[ConditionFlag.BetweenAreas] || Service.Condition[ConditionFlag.OccupiedInQuestEvent];
        public bool IsMounted => Service.Condition[ConditionFlag.Mounted];
        public bool IsVerticalAllowed => Service.Condition[ConditionFlag.InFlight];
        public Angle CameraFacing => ((Camera.Instance?.CameraAzimuth ?? 0).Radians() + 180.Degrees());
        public Angle CameraAltitude => (Camera.Instance?.CameraAltitude ?? 0).Radians();

        public AIController()
        {
            _axisForward = new(VirtualKey.W, VirtualKey.S);
            _axisStrafe = new(VirtualKey.D, VirtualKey.A);
            _axisRotate = new(VirtualKey.LEFT, VirtualKey.RIGHT);
            _axisVertical = new(VirtualKey.UP, VirtualKey.DOWN);
            _keyJump = new(VirtualKey.SPACE);
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
                Service.TargetManager.Target = actor != null ? Service.ObjectTable.SearchById((uint)actor.InstanceID) : null;
        }

        public void SetFocusTarget(Actor? actor)
        {
            if (Service.TargetManager.FocusTarget?.ObjectId != actor?.InstanceID)
                Service.TargetManager.FocusTarget = actor != null ? Service.ObjectTable.SearchById((uint)actor.InstanceID) : null;
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
                ActionManagerEx.Instance!.InputOverride.GamepadOverridesEnabled = false;
                return;
            }

            if (ForceFacing && NaviTargetRot != null && player.Rotation.ToDirection().Dot(NaviTargetRot.Value) < 0.996f)
            {
                ActionManagerEx.Instance!.FaceDirection(NaviTargetRot.Value);
            }

            bool moveRequested = ActionManagerEx.Instance!.InputOverride.IsMoveRequested();
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
            bool forbidMovement = moveRequested || !AllowInterruptingCastByMovement && ActionManagerEx.Instance!.MoveMightInterruptCast;
            if (NaviTargetPos != null && !forbidMovement && (NaviTargetPos.Value - player.Position).LengthSq() > 0.01f)
            {
                var dir = cameraFacing - Angle.FromDirection(NaviTargetPos.Value - player.Position);
                ActionManagerEx.Instance!.InputOverride.GamepadOverridesEnabled = true;
                ActionManagerEx.Instance!.InputOverride.GamepadOverrides[3] = (int)(100 * dir.Sin());
                ActionManagerEx.Instance!.InputOverride.GamepadOverrides[4] = (int)(100 * dir.Cos());
                _axisForward.CurDirection = ActionManagerEx.Instance!.InputOverride.GamepadOverrides[4] > 10 ? 1 : ActionManagerEx.Instance!.InputOverride.GamepadOverrides[4] < 10 ? -1 : 0; // this is a hack, needed to prevent afk :( this will be ignored anyway due to gamepad inputs
                _keyJump.Held = !_keyJump.Held && WantJump;
            }
            else
            {
                ActionManagerEx.Instance!.InputOverride.GamepadOverridesEnabled = false;
                _axisForward.CurDirection = ForceCancelCast && castInProgress ? 1 : 0; // this is a hack to cancel any cast...
                _keyJump.Held = false;
            }

            if (NaviTargetVertical != null && IsVerticalAllowed && NaviTargetPos != null)
            {
                var deltaY = NaviTargetVertical.Value - player.PosRot.Y;
                var deltaXZ = (NaviTargetPos.Value - player.Position).Length();
                var deltaAltitude = (CameraAltitude - Angle.FromDirection(new(ActionManagerEx.Instance!.InputOverride.GamepadOverrides[4] < 0 ? deltaY : -deltaY, deltaXZ))).Deg;
                ActionManagerEx.Instance!.InputOverride.GamepadOverrides[6] = Math.Clamp((int)(deltaAltitude * 5), -100, 100);
            }
            else
            {
                ActionManagerEx.Instance!.InputOverride.GamepadOverrides[6] = 0;
            }
        }
    }
}
