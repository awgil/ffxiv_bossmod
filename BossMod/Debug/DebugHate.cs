using Dalamud.Memory;
using ImGuiNET;
using System.Runtime.InteropServices;

namespace BossMod;

[StructLayout(LayoutKind.Explicit, Size = 0x108)]
public unsafe struct Hate
{
    [FieldOffset(0x00)] public fixed byte HateArray[0x08 * 32];
    [FieldOffset(0x100)] public int HateArrayLength;
    [FieldOffset(0x104)] public uint HateTargetId;

    public ReadOnlySpan<HateInfo> HateSpan
    {
        get
        {
            fixed (byte* ptr = HateArray)
            {
                return new ReadOnlySpan<HateInfo>(ptr, HateArrayLength);
            }
        }
    }
}

[StructLayout(LayoutKind.Explicit, Size = 0x08)]
public struct HateInfo
{
    [FieldOffset(0x00)] public uint ObjectId;
    [FieldOffset(0x04)] public int Enmity;
}

[StructLayout(LayoutKind.Explicit, Size = 0x908)]
public unsafe struct Hater
{
    [FieldOffset(0x00)] public fixed byte HaterArray[0x48 * 32];
    [FieldOffset(0x900)] public int HaterArrayLength;

    public ReadOnlySpan<HaterInfo> HaterSpan
    {
        get
        {
            fixed (byte* ptr = HaterArray)
            {
                return new ReadOnlySpan<HaterInfo>(ptr, HaterArrayLength);
            }
        }
    }
}

[StructLayout(LayoutKind.Explicit, Size = 0x48)]
public unsafe struct HaterInfo
{
    [FieldOffset(0x00)] public fixed byte Name[64];
    [FieldOffset(0x40)] public uint ObjectId;
    [FieldOffset(0x44)] public int Enmity;
}

class DebugHate
{
    public unsafe void Draw()
    {
        var uistate = FFXIVClientStructs.FFXIV.Client.Game.UI.UIState.Instance();

        // aggro list of current target
        var hate = (Hate*)((IntPtr)uistate + 0x08);
        ImGui.BeginTable("hate", 3);
        ImGui.TableSetupColumn("ObjectID");
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Enmity");
        ImGui.TableHeadersRow();
        foreach (var h in hate->HateSpan)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn(); ImGui.TextUnformatted($"{h.ObjectId:X}");
            ImGui.TableNextColumn(); ImGui.TextUnformatted(Utils.ObjectString(h.ObjectId));
            ImGui.TableNextColumn(); ImGui.TextUnformatted($"{h.Enmity}");
        }
        ImGui.EndTable();

        // list of actors aggroed to player
        var hater = (Hater*)((IntPtr)uistate + 0x110);
        ImGui.BeginTable("hater", 3);
        ImGui.TableSetupColumn("ObjectID");
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Enmity");
        ImGui.TableHeadersRow();
        for (var i = 0; i < hater->HaterArrayLength; ++i)
        {
            var h = ((HaterInfo*)hater->HaterArray) + i;
            ImGui.TableNextRow();
            ImGui.TableNextColumn(); ImGui.TextUnformatted($"{h->ObjectId:X}");
            ImGui.TableNextColumn(); ImGui.TextUnformatted(MemoryHelper.ReadSeString((IntPtr)h, 64).ToString());
            ImGui.TableNextColumn(); ImGui.TextUnformatted($"{h->Enmity}");
        }
        ImGui.EndTable();
    }
}
