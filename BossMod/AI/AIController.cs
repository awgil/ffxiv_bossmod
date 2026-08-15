using Dalamud.Game.ClientState.Conditions;

namespace BossMod.AI;

// utility for simulating user actions based on AI decisions:
// - navigation
// - using actions safely (without spamming, not in cutscenes, etc)
sealed class AIController(WorldState ws, ActionManagerEx amex, MovementOverride movement)
{
    public WPos? NaviTargetPos;
    public float? NaviTargetVertical;
    public bool AllowInterruptingCastByMovement;
    public bool ForceCancelCastOtherAI;
    public bool ForceCancelCastMechanicAI;

    private readonly ActionManagerEx _amex = amex;
    private readonly MovementOverride _movement = movement;

    public bool IsVerticalAllowed => Service.Condition[ConditionFlag.InFlight];
    public Angle CameraFacing => (Camera.Instance?.CameraAzimuth ?? 0f).Radians() + 180f.Degrees();
    public Angle CameraAltitude => (Camera.Instance?.CameraAltitude ?? 0f).Radians();

    public void Clear()
    {
        NaviTargetPos = null;
        NaviTargetVertical = null;
        AllowInterruptingCastByMovement = false;
        ForceCancelCastOtherAI = false;
        ForceCancelCastMechanicAI = false;
    }

    public void SetFocusTarget(Actor? actor)
    {
        if (Service.TargetManager.FocusTarget?.EntityId != actor?.InstanceID)
        {
            Service.TargetManager.FocusTarget = actor != null ? Service.ObjectTable.SearchById((uint)actor.InstanceID) : null;
        }
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
        var castInProgress = player.CastInfo != null;
        var forbidMovement = moveRequested || !AllowInterruptingCastByMovement && _amex.MoveMightInterruptCast;
        if (NaviTargetPos != null && !forbidMovement && (NaviTargetPos.Value - player.Position).LengthSq() > 0.001f)
        {
            desiredPosition = NaviTargetPos.Value.ToVec3(NaviTargetVertical != null && IsVerticalAllowed ? NaviTargetVertical.Value : player.PosRot.Y);
        }
        else
        {
            hints.ForceCancelCastMechanic |= ForceCancelCastMechanicAI && castInProgress;
            hints.ForceCancelCastOther |= ForceCancelCastOtherAI && castInProgress;
        }

        if (hints.ForcedMovement == null && desiredPosition != null)
        {
            hints.ForcedMovement = desiredPosition.Value - player.PosRot.XYZ();
        }
    }
}
