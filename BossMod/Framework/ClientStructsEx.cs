using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System.Runtime.InteropServices;

namespace BossMod;

internal static class ClientStructsEx
{
    public static bool IsValidAllianceMember(this PartyMember member) => (member.Flags & 1) != 0;
}

[StructLayout(LayoutKind.Explicit, Size = 0x22E0)]
internal unsafe partial struct PlayerMove
{
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

    [FieldOffset(0x1C0)] public InterpolationState Interpolation;
}
