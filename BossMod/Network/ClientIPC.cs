using System.Runtime.InteropServices;

namespace BossMod.Network.ClientIPC;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ActionRequest
{
    public byte ActionProcState; // see ActionManager.GetAdjustedCastTime implementation, last optional arg
    public ActionType Type;
    public ushort u1;
    public uint ActionID;
    public ushort Sequence;
    public ushort IntCasterRot; // 0 = N, increases CCW, 0xFFFF = 2pi
    public ushort IntDirToTarget; // 0 = N, increases CCW, 0xFFFF = 2pi
    public ushort u3;
    public ulong TargetID;
    public ushort ItemSourceSlot;
    public ushort ItemSourceContainer;
    public uint u4;
    public ulong u5;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ActionRequestGroundTargeted
{
    public byte ActionProcState; // see ActionManager.GetAdjustedCastTime implementation, last optional arg
    public ActionType Type;
    public ushort u1;
    public uint ActionID;
    public ushort Sequence;
    public ushort IntCasterRot; // 0 = N, increases CCW, 0xFFFF = 2pi
    public ushort IntDirToTarget; // 0 = N, increases CCW, 0xFFFF = 2pi
    public ushort u3;
    public float LocX;
    public float LocY;
    public float LocZ;
    public uint u4;
    public ulong u5;
}
