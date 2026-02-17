using BossMod.Interfaces;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;

namespace BossMod.AI;

// utility for simulating user actions based on AI decisions:
// - navigation
// - using actions safely (without spamming, not in cutscenes, etc)
sealed class AIController(WorldState ws, IAmex amex, IMovementOverride movement, ITargetManager targetManager, IObjectTable objectTable, ICondition conditions)
{
    public WPos? NaviTargetPos;
    public float? NaviTargetVertical;
    public bool AllowInterruptingCastByMovement;
    public bool ForceCancelCast;

    private readonly IAmex _amex = amex;
    private readonly IMovementOverride _movement = movement;

    public bool IsVerticalAllowed => conditions[ConditionFlag.InFlight];
    public Angle CameraFacing => (Camera.Instance?.CameraAzimuth ?? 0).Radians() + 180.Degrees();
    public Angle CameraAltitude => (Camera.Instance?.CameraAltitude ?? 0).Radians();

    public void Clear()
    {
        NaviTargetPos = null;
        NaviTargetVertical = null;
        AllowInterruptingCastByMovement = false;
        ForceCancelCast = false;
    }

    public void SetFocusTarget(Actor? actor)
    {
        if (targetManager.FocusTarget?.EntityId != actor?.InstanceID)
            targetManager.FocusTarget = actor != null ? objectTable.SearchById((uint)actor.InstanceID) : null;
    }

    public void Update(Actor? player, AIHints hints, DateTime now)
    {
        if (player == null || player.IsDead || ws.Party.Members[PartyState.PlayerSlot].InCutscene)
        {
            return;
        }

        Vector3? desiredPosition = null;

        // TODO this checks whether movement keys are pressed, we need a better solution
        var moveRequested = _movement.IsMoveRequested();
        var castInProgress = player.CastInfo != null && !player.CastInfo.EventHappened;
        var forbidMovement = moveRequested || !AllowInterruptingCastByMovement && _amex.MoveMightInterruptCast;
        if (NaviTargetPos != null && !forbidMovement && (NaviTargetPos.Value - player.Position).LengthSq() > 0.01f)
        {
            desiredPosition = NaviTargetPos.Value.ToVec3(NaviTargetVertical != null && IsVerticalAllowed ? NaviTargetVertical.Value : player.PosRot.Y);
        }
        else
        {
            hints.ForceCancelCast |= ForceCancelCast && castInProgress;
        }

        if (hints.ForcedMovement == null && desiredPosition != null)
            hints.ForcedMovement = desiredPosition.Value - player.PosRot.XYZ();
    }
}
