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

            public NaviAxis(InputOverride input, VirtualKey keyFwd, VirtualKey keyBack)
            {
                _input = input;
                _keyFwd = keyFwd;
                _keyBack = keyBack;
            }
        }

        public WPos? NaviTargetPos;
        public WDir? NaviTargetRot;
        public ActionID PlannedAction;
        public ulong PlannedActionTarget;
        public bool AllowInterruptingCastByMovement;
        public DateTime LastUsedActionTimestamp { get; private set; }

        private Autorotation _autorot;
        private NaviAxis _axisForward;
        private NaviAxis _axisStrafe;
        private NaviAxis _axisRotate;
        private unsafe FFXIVClientStructs.FFXIV.Client.Game.ActionManager* _actionManager = null;

        public bool InCutscene => Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78];
        public WDir CameraFacing => ((Camera.Instance?.CameraAzimuth ?? 0).Radians() + 180.Degrees()).ToDirection();

        public unsafe AIController(InputOverride input, Autorotation autorot)
        {
            _autorot = autorot;
            _axisForward = new(input, VirtualKey.W, VirtualKey.S);
            _axisStrafe = new(input, VirtualKey.D, VirtualKey.A);
            _axisRotate = new(input, VirtualKey.LEFT, VirtualKey.RIGHT);
            _actionManager = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        }

        public unsafe float Cooldown(ActionID action)
        {
            var recastGroup = _actionManager->GetRecastGroup((int)action.Type, action.ID);
            var recast = _actionManager->GetRecastGroupDetail(recastGroup);
            return recast != null ? recast->Total - recast->Elapsed : 0;
        }

        public unsafe float Range(ActionID action)
        {
            return action.Type == ActionType.Spell ? FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionRange(action.ID) : 0;
        }

        public void Clear()
        {
            NaviTargetPos = null;
            NaviTargetRot = null;
            PlannedAction = new();
            PlannedActionTarget = 0;
            AllowInterruptingCastByMovement = false;
        }

        public void SetTarget(ulong actorID)
        {
            Service.TargetManager.SetTarget(Service.ObjectTable.SearchById((uint)actorID));
        }

        public unsafe void Update(Actor? player)
        {
            if (player == null || InCutscene)
            {
                _axisForward.CurDirection = 0;
                _axisStrafe.CurDirection = 0;
                _axisRotate.CurDirection = 0;
                return;
            }

            bool actionReady = PlannedAction && Math.Max(Cooldown(PlannedAction), _autorot.AnimLock) < 0.1f;
            UpdateNavigation(player, actionReady);

            var now = DateTime.Now;
            if (actionReady && (now - LastUsedActionTimestamp).TotalMilliseconds > 100)
            {
                _autorot.DisableReplacement = true;
                _actionManager->UseAction((FFXIVClientStructs.FFXIV.Client.Game.ActionType)PlannedAction.Type, PlannedAction.ID, (long)PlannedActionTarget);
                _autorot.DisableReplacement = false;
                LastUsedActionTimestamp = now;
            }
        }

        private void UpdateNavigation(Actor player, bool actionReady)
        {
            var cameraFacing = CameraFacing;
            if (NaviTargetRot != null && NaviTargetRot.Value.Dot(cameraFacing) < 0.996f) // ~5 degrees
            {
                _axisRotate.CurDirection = cameraFacing.OrthoL().Dot(NaviTargetRot.Value) > 0 ? 1 : -1;
            }
            else
            {
                _axisRotate.CurDirection = 0;
            }

            bool forbidMovement = !AllowInterruptingCastByMovement && (actionReady && PlannedAction.IsCasted() || player.CastInfo != null && !player.CastInfo.EventHappened);
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
        }
    }
}
