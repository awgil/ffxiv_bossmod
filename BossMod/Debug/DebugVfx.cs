using ImGuiNET;
using System.Runtime.InteropServices;

namespace BossMod;

[StructLayout(LayoutKind.Explicit, Size = 0x1A0)]
public unsafe struct VfxInitData
{
}

[StructLayout(LayoutKind.Explicit, Size = 0x1D0)]
public unsafe struct VfxInstance
{
}

public unsafe class DebugVfx : IDisposable
{
    private delegate VfxInitData* VfxInitDataCtorDelegate(VfxInitData* self);
    private VfxInitDataCtorDelegate VfxInitDataCtor;

    // StartOmen: a3=2, a4=0, a13=-1
    private delegate VfxInstance* CreateVfxDelegate(byte* path, VfxInitData* init, byte a3, byte a4, float originX, float originY, float originZ, float sizeX, float sizeY, float sizeZ, float angle, float duration, int a13);
    private CreateVfxDelegate CreateVfx;

    public DebugVfx()
    {
        VfxInitDataCtor = Marshal.GetDelegateForFunctionPointer<VfxInitDataCtorDelegate>(Service.SigScanner.ScanText("0F 28 05 ?? ?? ?? ?? 49 83 CA FF"));
        CreateVfx = Marshal.GetDelegateForFunctionPointer<CreateVfxDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B D8 48 8D 95"));
    }

    public void Dispose()
    {
    }

    public void Draw()
    {
        if (ImGui.Button("Create!"))
        {
            CreateTestVfx();
        }
    }

    private void CreateTestVfx()
    {
        var pos = Service.ClientState.LocalPlayer?.Position ?? new();
        var path = "vfx/omen/eff/general_1bf.avfx";
        var pathBytes = System.Text.Encoding.UTF8.GetBytes(path);

        var init = new VfxInitData();
        VfxInitDataCtor(&init);

        fixed (byte* pathPtr = pathBytes)
        {
            var vfx = CreateVfx(pathPtr, &init, 2, 0, pos.X, pos.Y, pos.Z, 10, 5, 10, 0, 20, -1);
            Service.Log($"vfx: {(nint)vfx:X}");
        }
    }
}
