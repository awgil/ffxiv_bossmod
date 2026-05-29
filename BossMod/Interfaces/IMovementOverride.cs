namespace BossMod.Services;

public interface IMovementOverride : IDisposable
{
    bool IsMoving();
    bool IsMoveRequested();
    bool IsForceUnblocked();

    Vector3? DesiredDirection { get; set; }
    Angle MisdirectionThreshold { get; set; }
    Angle? DesiredSpinDirection { get; set; }
}

internal sealed class MockMovementOverride : IMovementOverride
{
    public bool IsMoving() => false;
    public bool IsMoveRequested() => false;
    public bool IsForceUnblocked() => false;

    public Vector3? DesiredDirection { get; set; }
    public Angle MisdirectionThreshold { get; set; }
    public Angle? DesiredSpinDirection { get; set; }

    public void Dispose() { }
}
