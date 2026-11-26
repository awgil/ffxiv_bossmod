using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.Text.Json;

namespace BossMod.ActionTweaks.CustomCombos;

public sealed class CustomComboTab(WorldState ws)
{
    public const string Name = "Custom Combos";

    private readonly CustomComboConfig _config = Service.Config.Get<CustomComboConfig>();
    private CustomCombo? _selected;
    private CustomComboPreset? _selectedPreset;
    private bool _revealSelected;
    private string _importError = "";
    private bool? _pvp;

    public bool Visible => _config.Enabled;

    public void Draw()
    {
        _pvp ??= ws.IsPvPArea;
        if (_selected != null && (_selected.PvP != _pvp || _selectedPreset == null && !_config.Combos.Contains(_selected)))
            _selected = null;

        var modified = DrawToolbar();
        using (var table = ImRaii.Table("combos", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV))
        {
            if (table)
            {
                ImGui.TableSetupColumn("List", ImGuiTableColumnFlags.WidthFixed, 300 * ImGuiHelpers.GlobalScale);
                ImGui.TableSetupColumn("Details");
                ImGui.TableNextColumn();
                modified |= DrawList();
                ImGui.TableNextColumn();
                modified |= DrawDetails();
            }
        }

        if (modified)
            _config.Modified.Fire();
    }

    private bool DrawToolbar()
    {
        var modified = false;
        UIMisc.HelpMarker("A combo replaces an action on your hotbar: its conditional actions are checked first, then its combo chain.\nPvP combos are separate, they only work in PvP.");
        ImGui.SameLine();
        if (ImGui.RadioButton("PvE", _pvp == false))
            _pvp = false;
        ImGui.SameLine();
        if (ImGui.RadioButton("PvP", _pvp == true))
            _pvp = true;
        ImGui.SameLine();
        if (ImGui.Button("New"))
        {
            Add(new() { Name = "New combo", Job = ws.Party.Player()?.Class ?? Class.None, PvP = _pvp == true });
            modified = true;
        }
        ImGui.SameLine();
        if (UIMisc.Button("Copy", _selected == null, "No combo is selected"))
        {
            var preset = _selectedPreset;
            var copy = Clone(_selected!);
            if (preset != null)
                _config.Presets.Remove(preset.Key);
            else
                copy.Name += " (copy)";
            Add(copy);
            modified = true;
        }
        ImGui.SameLine();
        if (UIMisc.Button("Delete", 0, (_selected == null, "No combo is selected"), (_selectedPreset != null, "Presets can't be deleted, only turned off"), (!ImGui.GetIO().KeyShift, "Hold shift to delete")))
        {
            _config.Combos.Remove(_selected!);
            _selected = null;
            modified = true;
        }
        ImGui.SameLine();
        if (UIMisc.Button("Export", _selected == null, "No combo is selected"))
            ImGui.SetClipboardText(JsonSerializer.Serialize(_selected, Serialization.BuildSerializationOptions()));
        ImGui.SameLine();
        if (ImGui.Button("Import"))
            modified |= Import();
        if (_importError.Length > 0)
        {
            ImGui.SameLine();
            ImGui.TextColored(0xff4c4cff, _importError);
        }
        return modified;
    }

    private bool DrawList()
    {
        using var child = ImRaii.Child("##list", new(0, ImGui.GetContentRegionAvail().Y));
        if (!child)
            return false;

        var modified = false;
        var player = ws.Party.Player();
        var byJob = _config.Combos.Select((c, i) => (Combo: c, Index: i)).Where(e => e.Combo.PvP == _pvp).ToLookup(e => e.Combo.Job);
        if (byJob[Class.None].Any())
            modified |= DrawJob(Class.None, [.. byJob[Class.None]], player);
        foreach (var (label, classes) in CustomComboEditor.GetJobGroups(_pvp == true))
        {
            if (classes.Count == 0)
                continue;
            ImGui.Spacing();
            ImGui.TextDisabled(label);
            foreach (var job in classes)
                modified |= DrawJob(job, [.. byJob[job]], player);
        }
        _revealSelected = false;
        return modified;
    }

    private bool DrawJob(Class job, List<(CustomCombo Combo, int Index)> combos, Actor? player)
    {
        using var id = ImRaii.PushId((int)job);
        var pvp = _pvp == true;
        var prefix = CustomComboPresets.KeyPrefix(job, pvp);
        var count = combos.Count + _config.Presets.Count(k => k.StartsWith(prefix, StringComparison.Ordinal));
        CustomComboEditor.DrawJobIcon(job);
        ImGui.SameLine();
        if (_revealSelected && _selected?.Job == job)
            ImGui.SetNextItemOpen(true);
        var flags = job == Class.None || job == player?.Class ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
        using var node = ImRaii.TreeNode($"{CustomComboEditor.JobName(job)}{(count > 0 ? $" ({count})" : "")}###job", flags);
        if (!node)
            return false;

        var modified = false;
        (int A, int B) swap = (-1, -1);
        for (var k = 0; k < combos.Count; ++k)
        {
            var (combo, index) = combos[k];
            using var rowId = ImRaii.PushId(index);
            modified |= ImGui.Checkbox("##enabled", ref combo.Enabled);
            if (DrawEntry(combo, combo.Name.Length > 0 ? combo.Name : "<unnamed>", combo.Enabled))
                Select(combo, null);
            if (ImGui.IsItemActive() && !ImGui.IsItemHovered())
            {
                var j = ImGui.GetMouseDragDelta().Y < 0 ? k - 1 : k + 1;
                if (j >= 0 && j < combos.Count)
                {
                    swap = (index, combos[j].Index);
                    ImGui.ResetMouseDragDelta();
                }
            }
        }
        if (swap.A >= 0)
        {
            (_config.Combos[swap.A], _config.Combos[swap.B]) = (_config.Combos[swap.B], _config.Combos[swap.A]);
            modified = true;
        }

        if (job == Class.None)
            return modified;
        var presets = CustomComboPresets.ForClass(job, pvp);
        if (presets.Count > 0)
            ImGui.TextDisabled("Presets");
        foreach (var preset in presets)
        {
            using var rowId = ImRaii.PushId(preset.Key);
            var enabled = _config.Presets.Contains(preset.Key);
            if (ImGui.Checkbox("##enabled", ref enabled))
            {
                if (enabled)
                    _config.Presets.Add(preset.Key);
                else
                    _config.Presets.Remove(preset.Key);
                modified = true;
            }
            if (DrawEntry(preset.Combo, preset.Combo.Name, enabled))
                Select(preset.Combo, preset);
            if (ImGui.IsItemHovered() && preset.Description != null)
                ImGui.SetTooltip(preset.Description);
            if (enabled && ShadowedBy(preset) is { } other)
            {
                ImGui.SameLine();
                using (ImRaii.PushColor(ImGuiCol.Text, 0xff40c0ff))
                    UIMisc.IconText(FontAwesomeIcon.ExclamationTriangle);
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip($"{other} replaces the same action and comes first, so this does nothing");
            }
        }
        return modified;
    }

    private bool DrawEntry(CustomCombo combo, string name, bool enabled)
    {
        ImGui.SameLine();
        UIMisc.Image(combo.Trigger != 0 ? Service.Texture?.GetFromGameIcon(CustomComboEditor.TriggerIcon(combo, ws)) : null, new(ImGui.GetFrameHeight()));
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        using var dim = ImRaii.PushColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled), !enabled);
        var clicked = ImGui.Selectable($"{name}###entry", combo == _selected);
        if (_revealSelected && combo == _selected)
            ImGui.SetScrollHereY();
        return clicked;
    }

    private bool DrawDetails()
    {
        using var child = ImRaii.Child("##details", new(0, ImGui.GetContentRegionAvail().Y));
        if (!child)
            return false;
        if (_selected == null)
        {
            ImGui.TextUnformatted("Select a combo to edit, or create a new one.");
            return false;
        }

        if (_selectedPreset != null)
        {
            if (_selectedPreset.Description != null)
                ImGui.TextWrapped(_selectedPreset.Description);
            ImGui.TextDisabled("Presets can't be edited. Copy it to make your own version.");
        }
        CustomComboEditor.DrawPreviewIcon(_selected);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled("Preview");
        var modified = CustomComboEditor.DrawCombo(_selected, ws, _selectedPreset != null);
        return modified && _selectedPreset == null;
    }

    private void Select(CustomCombo combo, CustomComboPreset? preset)
    {
        _selected = combo;
        _selectedPreset = preset;
    }

    private void Add(CustomCombo combo)
    {
        _config.Combos.Add(combo);
        _pvp = combo.PvP;
        Select(combo, null);
        _revealSelected = true;
    }

    private bool Import()
    {
        try
        {
            var combo = JsonSerializer.Deserialize<CustomCombo>(ImGui.GetClipboardText(), Serialization.BuildSerializationOptions());
            if (combo == null)
            {
                _importError = "Clipboard does not contain a combo";
                return false;
            }
            Add(combo);
            _importError = "";
            return true;
        }
        catch (Exception ex)
        {
            _importError = $"Failed to import: {ex.Message}";
            return false;
        }
    }

    private static CustomCombo Clone(CustomCombo combo)
    {
        var options = Serialization.BuildSerializationOptions();
        return JsonSerializer.Deserialize<CustomCombo>(JsonSerializer.Serialize(combo, options), options)!;
    }

    // name of the active combo that's checked before the preset for the same action
    private string? ShadowedBy(CustomComboPreset preset)
    {
        var trigger = CustomComboData.Canonical(preset.Combo.Trigger);
        foreach (var combo in CustomComboPresets.ActiveCombos(_config, preset.Combo.Job, preset.Combo.PvP))
        {
            if (combo == preset.Combo)
                return null;
            if (CustomComboData.Canonical(combo.Trigger) == trigger)
                return combo.Name;
        }
        return null;
    }
}
