namespace BossMod.Interfaces;

public interface IMovementOverride : IDisposable
{
    public bool IsMoving();
    public bool IsMoveRequested();
    public bool IsForceUnblocked();
    public bool FollowPathActive();

    public Vector3? DesiredDirection { get; set; }
    public Angle MisdirectionThreshold { get; set; }
    public Angle? DesiredSpinDirection { get; set; }
    public bool MovementBlocked { get; set; }

    public WDir UserMove { get; }
    public WDir ActualMove { get; }
}
