using BossMod.Interfaces;

namespace BossMod;

internal class MockMovementOverride : IMovementOverride
{
    public Vector3? DesiredDirection { get; set; }
    public Angle MisdirectionThreshold { get; set; }
    public Angle? DesiredSpinDirection { get; set; }
    public bool MovementBlocked { get; set; }

    public WDir UserMove => default;
    public WDir ActualMove => default;

    public void Dispose() { }

    public bool IsForceUnblocked() => false;
    public bool IsMoveRequested() => false;
    public bool IsMoving() => false;
    public bool FollowPathActive() => false;
}
