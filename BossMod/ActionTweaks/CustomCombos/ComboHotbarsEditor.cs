using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace BossMod.ActionTweaks.CustomCombos;

public sealed class ComboHotbarsEditor
{
    private const uint ColorCovered = 0xff50c050;
    private const uint ColorUncovered = 0xff606060;
    private const uint ColorEmpty = 0x80303030;

    public unsafe void Draw(CustomComboConfig root)
    {
        var config = root.Hotbars;
        UIMisc.SectionHeader("Combo hotbars");
        if (ImGui.Checkbox("Enable combo hotbars", ref config.Enabled))
            root.Modified.Fire();
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Saves your hotbar layout on top of the normal ones while vbm is loaded.");
        using (ImRaii.Disabled(!config.Enabled))
        using (ImRaii.PushIndent())
        {
            if (ImGui.Checkbox("Show vanilla layout", ref config.ShowVanilla))
                root.Modified.Fire();
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                ImGui.SetTooltip("Hide the combo layout. Use this to edit your vanilla hotbars.");
        }

        ImGui.TextDisabled("Click a slot to make it a combo slot. Combo slots are saved internally to vbm while keeping your vanilla layouts.");

        var svc = Service.ComboHotbars;
        var m = RaptureHotbarModule.Instance();
        if (svc == null || m == null || !m->ModuleReady || (m->ActiveHotbarClassJobId & 0x80) != 0)
        {
            ImGui.TextDisabled("n/a: not logged in");
            return;
        }

        DrawPending(root, svc);

        var iconSize = new Vector2(32 * ImGuiHelpers.GlobalScale);
        for (var bar = 0u; bar < ComboHotbarService.NumBars; ++bar)
        {
            using var id = ImRaii.PushId((int)bar);
            var job = ComboHotbarService.EffectiveJob(m, bar);
            var covered = config.BarSlots(job, bar).Count();

            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(bar < ComboHotbarService.NumNormalBars ? $"Hotbar {bar + 1}" : $"Cross hotbar {bar - ComboHotbarService.NumNormalBars + 1}");
            if (job == 0)
            {
                ImGui.SameLine();
                ImGui.TextDisabled("(shared)");
            }
            ImGui.SameLine(150 * ImGuiHelpers.GlobalScale);
            if (covered < ComboHotbarService.NumSlots)
            {
                if (ImGui.Button("Cover all"))
                    svc.CoverBar(bar);
                ImGui.SameLine();
            }
            if (covered > 0)
            {
                using (ImRaii.Disabled(covered > 1))
                    if (ImGui.Button("Uncover"))
                        svc.UncoverBar(bar);
                ImGui.SameLine();
            }
            ImGui.NewLine();

            for (var slot = 0u; slot < ComboHotbarService.NumSlots; ++slot)
            {
                if (slot > 0)
                    ImGui.SameLine(0, slot % 4 == 0 ? 8 * ImGuiHelpers.GlobalScale : 2 * ImGuiHelpers.GlobalScale);
                DrawSlot(m, config, svc, job, bar, slot, iconSize);
            }
        }
    }

    private static void DrawPending(CustomComboConfig root, ComboHotbarService svc)
    {
        var config = root.Hotbars;
        if (config.Pending.Count > 0)
        {
            ImGui.TextColored(0xff40a0ff, $"{config.Pending.Count} action(s) waiting to be added to your combo hotbars when you unlock them:");
            using (ImRaii.PushIndent())
                foreach (var p in config.Pending)
                    ImGui.TextUnformatted(ComboHotbarService.SlotName(p.Type, p.Id));
            if (ImGui.Button("Discard waiting actions"))
            {
                config.Pending.Clear();
                root.Modified.Fire();
            }
        }
        if (svc.Messages.Count > 0)
        {
            foreach (var msg in svc.Messages)
                ImGui.TextColored(0xff4c4cff, msg);
            if (ImGui.Button("Dismiss"))
                svc.Messages.Clear();
        }
    }

    private static unsafe void DrawSlot(RaptureHotbarModule* m, ComboHotbarsConfig config, ComboHotbarService svc, uint job, uint bar, uint slot, Vector2 size)
    {
        var s = m->Hotbars[(int)bar].GetHotbarSlot(slot);
        var entry = config.Find(job, bar, slot);
        var pos = ImGui.GetCursorScreenPos();

        // working slot shows the combo content if overlay is active, normal content otherwise
        var icon = s->IconId != 0 ? s->IconId : s->CommandType == RaptureHotbarModule.HotbarSlotType.Action ? Service.LuminaRow<Lumina.Excel.Sheets.Action>(s->CommandId)?.Icon ?? 0u : 0u;
        var dl = ImGui.GetWindowDrawList();
        dl.AddRectFilled(pos, pos + size, ColorEmpty);
        UIMisc.Image(icon != 0 ? Service.Texture?.GetFromGameIcon(icon) : null, size);
        var clicked = ImGui.IsItemClicked();
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(entry != null ? $"Combo slot: {ComboHotbarService.SlotName(entry.Type, entry.Id)}" : $"Normal slot: {ComboHotbarService.SlotName(s->CommandType, s->CommandId)}");
        }
        dl.AddRect(pos, pos + size, entry != null ? ColorCovered : ColorUncovered, 0, ImDrawFlags.None, entry != null ? 3 : 1);

        if (clicked)
        {
            if (entry != null)
                svc.UncoverSlot(bar, slot);
            else
                svc.CoverSlot(bar, slot);
        }
    }
}
