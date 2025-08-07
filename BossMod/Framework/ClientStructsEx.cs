using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System.Runtime.InteropServices;

namespace BossMod;

internal static class ClientStructsEx
{
    public static bool IsValidAllianceMember(this PartyMember member) => (member.Flags & 1) != 0;
}

// TODO i might have adjusted the wrong offset for the 7.3 fix but it doesn't really matter if all we care about is interpolation
[StructLayout(LayoutKind.Explicit, Size = 0x22E0)]
internal unsafe partial struct PlayerMove
{
    // this was 0x1E0 in 7.25
    [FieldOffset(0x1E0)] public MoveContainer Move;
}

[StructLayout(LayoutKind.Explicit, Size = 0x430)]
internal unsafe partial struct MoveContainer
{
    [StructLayout(LayoutKind.Explicit, Size = 0x88)]
    public unsafe partial struct InterpolationState
    {
        [FieldOffset(0x10)] public float DesiredRotation;
        [FieldOffset(0x14)] public float OriginalRotation;
        [FieldOffset(0x40)] public bool RotationInterpolationInProgress;
    }

    // this was 0x1C0 in 7.25
    [FieldOffset(0x1D0)] public InterpolationState Interpolation;
}

[StructLayout(LayoutKind.Explicit, Size = 0x76F0)]
internal unsafe partial struct ControlEx
{
    [FieldOffset(0x7118)] public float BaseMoveSpeed;

    public static ControlEx* Instance() => (ControlEx*)Control.Instance();
}
