using Dalamud.Game.ClientState.Conditions;

namespace BossMod.AI;

// utility for simulating user actions based on AI decisions:
// - navigation
// - using actions safely (without spamming, not in cutscenes, etc)
sealed class AIController(ActionManagerEx amex)
{
    public WPos? NaviTargetPos;
    public WDir? NaviTargetRot;
    public float? NaviTargetVertical;
    public bool AllowInterruptingCastByMovement;
    public bool ForceCancelCast;
    public bool ForceFacing;
    public bool WantJump;

    private readonly ActionManagerEx _amex = amex;
    private DateTime _nextJump;

    public bool InCutscene => Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78] || Service.Condition[ConditionFlag.Occupied33] || Service.Condition[ConditionFlag.BetweenAreas] || Service.Condition[ConditionFlag.OccupiedInQuestEvent];
    public bool IsMounted => Service.Condition[ConditionFlag.Mounted];
    public bool IsVerticalAllowed => Service.Condition[ConditionFlag.InFlight];
    public Angle CameraFacing => (Camera.Instance?.CameraAzimuth ?? 0).Radians() + 180.Degrees();
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
        var movement = AIMove.Instance!;

        if (player == null || player.IsDead || InCutscene)
        {
            movement.DesiredPosition = null;
            _amex.InputOverride.GamepadOverridesEnabled = false;
            return;
        }

        if (ForceFacing && NaviTargetRot != null && player.Rotation.ToDirection().Dot(NaviTargetRot.Value) < 0.996f)
        {
            _amex.FaceDirection(NaviTargetRot.Value);
        }

        // TODO this checks whether movement keys are pressed, we need a better solution
        bool moveRequested = _amex.InputOverride.IsMoveRequested();
        bool castInProgress = player.CastInfo != null && !player.CastInfo.EventHappened;
        bool forbidMovement = moveRequested || !AllowInterruptingCastByMovement && _amex.MoveMightInterruptCast;
        if (NaviTargetPos != null && !forbidMovement && (NaviTargetPos.Value - player.Position).LengthSq() > 0.01f)
        {
            movement.DesiredPosition = NaviTargetPos;
            if (WantJump)
                ExecuteJump();
        }
        else
        {
            movement.DesiredPosition = null;
            _amex.ForceCancelCastNextFrame |= ForceCancelCast && castInProgress;
        }

        if (NaviTargetVertical != null && IsVerticalAllowed && NaviTargetPos != null)
            movement.DesiredY = NaviTargetVertical;
        else
            movement.DesiredY = null;
    }

    private unsafe void ExecuteJump()
    {
        if (DateTime.Now >= _nextJump)
        {
            FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->UseAction(FFXIVClientStructs.FFXIV.Client.Game.ActionType.GeneralAction, 2);
            _nextJump = DateTime.Now.AddMilliseconds(100);
        }
    }
}
