using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;
using FFXIVClientStructs.Interop;
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
    private delegate void SetIconDelegate(VfxContainer* self, uint iconId, ulong targetId);

    // to untether, an inlined version of this function is called with 0s in the last two arguments
    private delegate void SetTetherDelegate(VfxContainer* self, byte tetherIndex, ushort unk1, ulong targetId, byte tetherProgress);

    // StartOmen: a3=2, a4=0, a13=-1
    private delegate VfxData* CreateVfxDelegate(byte* path, VfxInitData* init, byte a3, byte a4, float originX, float originY, float originZ, float sizeX, float sizeY, float sizeZ, float angle, float duration, int a13);
    private readonly CreateVfxDelegate CreateVfx;
    private readonly ClearVfxDataDelegate ClearVfx;
    private readonly SetIconDelegate SetIcon;
    private readonly SetTetherDelegate SetTether;

    private readonly List<Pointer<VfxData>> _spawnedVfx = [];

    public DebugVfx()
    {
        VfxInitDataCtor = Marshal.GetDelegateForFunctionPointer<VfxInitDataCtorDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 8D 57 06 48 8D 4C 24 ??"));
        CreateVfx = Marshal.GetDelegateForFunctionPointer<CreateVfxDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B D8 48 8D 95"));
        ClearVfx = Marshal.GetDelegateForFunctionPointer<ClearVfxDataDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 4D 89 A4 DE ?? ?? ?? ??"));
        SetIcon = Marshal.GetDelegateForFunctionPointer<SetIconDelegate>(Service.SigScanner.ScanText("85 D2 0F 84 ?? ?? ?? ?? 48 89 6C 24 ?? 57 48 83 EC 30"));
        SetTether = Marshal.GetDelegateForFunctionPointer<SetTetherDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 0F B6 54 24 ?? 45 33 C0"));
    }

    public void Dispose()
    {
    }

    private string _icon = "";
    private string _tether = "";

    public void Draw()
    {
        var player = Utils.CharacterInternal(Service.ObjectTable.LocalPlayer);
        if (player == null)
            return;

        ImGui.InputText("Icon ID", ref _icon, 20);
        if (ImGui.Button("Spawn icon on self"))
        {
            if (uint.TryParse(_icon, out var iconId))
            {
                SetIcon.Invoke(&player->Vfx, iconId, player->GetGameObjectId());
            }
        }

        ImGui.InputText("Tether ID", ref _tether, 20);
        if (ImGui.Button("Add tether (self + targeted object)"))
        {
            var target = Service.TargetManager.Target;
            if (ushort.TryParse(_tether, out var tetherId) && target != null)
            {
                SetTether.Invoke(&player->Vfx, 0, tetherId, target.GameObjectId, 1);
            }
        }
        ImGui.SameLine();
        if (ImGui.Button("Clear tether"))
            SetTether.Invoke(&player->Vfx, 0, 0, 0, 0);

        if (ImGui.Button("Create!"))
        {
            CreateTestVfx();
        }

        Dalamud.Utility.Util.ShowStruct(&player->Vfx);

        using var v = ImRaii.ListBox("VFX (click item to remove)");
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
        var pos = Service.ObjectTable.LocalPlayer?.Position ?? new();
        var path = "vfx/omen/eff/gl_circle_5007_x1.avfx";
        var pathBytes = System.Text.Encoding.UTF8.GetBytes(path);

        var init = new VfxInitData();
        VfxInitDataCtor(&init);

        fixed (byte* pathPtr = pathBytes)
        {
            _spawnedVfx.Add(CreateVfx(pathPtr, &init, 2, 0, pos.X, pos.Y, pos.Z, 10, 5, 10, 0, 1, -1));
        }
    }
}
