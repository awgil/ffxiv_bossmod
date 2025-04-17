using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;
using FFXIVClientStructs.Interop;
using ImGuiNET;
using System.Runtime.InteropServices;

namespace BossMod;

[StructLayout(LayoutKind.Explicit, Size = 0x1A0)]
public unsafe struct VfxInitData
{
}

public sealed unsafe class DebugVfx : IDisposable
{
    private delegate VfxInitData* VfxInitDataCtorDelegate(VfxInitData* self);
    private readonly VfxInitDataCtorDelegate VfxInitDataCtor;
    private delegate void ClearVfxDataDelegate(VfxData* self);

    // StartOmen: a3=2, a4=0, a13=-1
    private delegate VfxData* CreateVfxDelegate(byte* path, VfxInitData* init, byte a3, byte a4, float originX, float originY, float originZ, float sizeX, float sizeY, float sizeZ, float angle, float duration, int a13);
    private readonly CreateVfxDelegate CreateVfx;
    private readonly ClearVfxDataDelegate ClearVfx;

    private readonly List<Pointer<VfxData>> _spawnedVfx = [];

    public DebugVfx()
    {
        VfxInitDataCtor = Marshal.GetDelegateForFunctionPointer<VfxInitDataCtorDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 8D 57 06 48 8D 4C 24 ??"));
        CreateVfx = Marshal.GetDelegateForFunctionPointer<CreateVfxDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B D8 48 8D 95"));
        ClearVfx = Marshal.GetDelegateForFunctionPointer<ClearVfxDataDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 4D 89 A4 DE ?? ?? ?? ??"));
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

        using var v = ImRaii.ListBox("VFX");
        if (v)
            for (var i = 0; i < _spawnedVfx.Count; i++)
            {
                var f = _spawnedVfx[i];
                if (ImGui.Selectable($"{(nint)f.Value:X}"))
                {
                    ClearVfx.Invoke(f.Value);
                    _spawnedVfx.RemoveAt(i);
                    break;
                }
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
            _spawnedVfx.Add(CreateVfx(pathPtr, &init, 2, 0, pos.X, pos.Y, pos.Z, 10, 5, 10, 0, 1, -1));
        }
    }
}
