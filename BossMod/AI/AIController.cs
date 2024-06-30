using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Keys;

namespace BossMod.AI;

// utility for simulating user actions based on AI decisions:
// - navigation
// - using actions safely (without spamming, not in cutscenes, etc)
sealed class AIController(ActionManagerEx amex)
{
    private sealed class NaviAxis(InputOverride io, VirtualKey keyFwd, VirtualKey keyBack)
    {
        private int _curDirection;

        public int CurDirection
        {
            get => _curDirection;
            set
            {
                if (_curDirection == value)
                {
                    if (value != 0 && !Service.KeyState[value > 0 ? keyFwd : keyBack])
                        io.SimulatePress(value > 0 ? keyFwd : keyBack);
                    return;
                }

                if (_curDirection > 0)
                    io.SimulateRelease(keyFwd);
                else if (_curDirection < 0)
                    io.SimulateRelease(keyBack);
                _curDirection = value;
                if (value > 0)
                    io.SimulatePress(keyFwd);
                else if (value < 0)
                    io.SimulatePress(keyBack);
            }
        }
    }

    private sealed class NaviInput(InputOverride io, VirtualKey key)
    {
        private bool _held;

        public bool Held
        {
            get => _held;
            set
            {
                if (_held == value)
                {
                    if (value && !Service.KeyState[key])
                        io.SimulatePress(key);
                    return;
                }

                if (_held)
                    io.SimulateRelease(key);
                _held = value;
                if (value)
                    io.SimulatePress(key);
            }
        }
    }

    public WPos? NaviTargetPos;
    public WDir? NaviTargetRot;
    public float? NaviTargetVertical;
    public bool AllowInterruptingCastByMovement;
    public bool ForceCancelCast;
    public bool ForceFacing;
    public bool WantJump;

    private readonly ActionManagerEx _amex = amex;
    private readonly NaviAxis _axisForward = new(amex.InputOverride, VirtualKey.W, VirtualKey.S);
    private readonly NaviAxis _axisStrafe = new(amex.InputOverride, VirtualKey.D, VirtualKey.A);
    private readonly NaviAxis _axisRotate = new(amex.InputOverride, VirtualKey.LEFT, VirtualKey.RIGHT);
    private readonly NaviAxis _axisVertical = new(amex.InputOverride, VirtualKey.UP, VirtualKey.DOWN);
    private readonly NaviInput _keyJump = new(amex.InputOverride, VirtualKey.SPACE);

    public bool InCutscene => Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78] || Service.Condition[ConditionFlag.Occupied33] || Service.Condition[ConditionFlag.BetweenAreas] || Service.Condition[ConditionFlag.OccupiedInQuestEvent];
    public bool IsMounted => Service.Condition[ConditionFlag.Mounted];
    public bool IsVerticalAllowed => Service.Condition[ConditionFlag.InFlight];
    public Angle CameraFacing => ((Camera.Instance?.CameraAzimuth ?? 0).Radians() + 180.Degrees());
    public Angle CameraAltitude => (Camera.Instance?.CameraAltitude ?? 0).Radians();

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

    public void SetFocusTarget(Actor? actor)
    {
        if (Service.TargetManager.FocusTarget?.EntityId != actor?.InstanceID)
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
            _amex.InputOverride.GamepadOverridesEnabled = false;
            return;
        }

        if (ForceFacing && NaviTargetRot != null && player.Rotation.ToDirection().Dot(NaviTargetRot.Value) < 0.996f)
        {
            _amex.FaceDirection(NaviTargetRot.Value);
        }

        bool moveRequested = _amex.InputOverride.IsMoveRequested();
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
        bool forbidMovement = moveRequested || !AllowInterruptingCastByMovement && _amex.MoveMightInterruptCast;
        if (NaviTargetPos != null && !forbidMovement && (NaviTargetPos.Value - player.Position).LengthSq() > 0.01f)
        {
            var dir = cameraFacing - Angle.FromDirection(NaviTargetPos.Value - player.Position);
            _amex.InputOverride.GamepadOverridesEnabled = true;
            _amex.InputOverride.GamepadOverrides[3] = (int)(100 * dir.Sin());
            _amex.InputOverride.GamepadOverrides[4] = (int)(100 * dir.Cos());
            _axisForward.CurDirection = _amex.InputOverride.GamepadOverrides[4] > 10 ? 1 : _amex.InputOverride.GamepadOverrides[4] < 10 ? -1 : 0; // this is a hack, needed to prevent afk :( this will be ignored anyway due to gamepad inputs
            _keyJump.Held = !_keyJump.Held && WantJump;
        }
        else
        {
            _amex.InputOverride.GamepadOverridesEnabled = false;
            _axisForward.CurDirection = ForceCancelCast && castInProgress ? 1 : 0; // this is a hack to cancel any cast...
            _keyJump.Held = false;
        }

        if (NaviTargetVertical != null && IsVerticalAllowed && NaviTargetPos != null)
        {
            var deltaY = NaviTargetVertical.Value - player.PosRot.Y;
            var deltaXZ = (NaviTargetPos.Value - player.Position).Length();
            var deltaAltitude = (CameraAltitude - Angle.FromDirection(new(_amex.InputOverride.GamepadOverrides[4] < 0 ? deltaY : -deltaY, deltaXZ))).Deg;
            _amex.InputOverride.GamepadOverrides[6] = Math.Clamp((int)(deltaAltitude * 5), -100, 100);
        }
        else
        {
            _amex.InputOverride.GamepadOverrides[6] = 0;
        }
    }
}
