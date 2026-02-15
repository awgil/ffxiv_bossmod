using BossMod.Interfaces;
using System.Runtime.InteropServices;

namespace BossMod;

internal unsafe class MockMovementOverride : IMovementOverride
{
    public MockMovementOverride()
    {
        ForcedMovementDirection = (float*)Marshal.AllocHGlobal(sizeof(float));
        *ForcedMovementDirection = 1f;
    }

    public Vector3? DesiredDirection { get; set; }
    public Angle MisdirectionThreshold { get; set; }
    public Angle? DesiredSpinDirection { get; set; }
    public bool MovementBlocked { get; set; }

    public WDir UserMove => default;
    public WDir ActualMove => default;
    public float* ForcedMovementDirection { get; }

    public void Dispose()
    {
        Marshal.FreeHGlobal((nint)ForcedMovementDirection);
    }

    public bool IsForceUnblocked() => false;
    public bool IsMoveRequested() => false;
    public bool IsMoving() => false;
    public bool FollowPathActive() => false;
}
