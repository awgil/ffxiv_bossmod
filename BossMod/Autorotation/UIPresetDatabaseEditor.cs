using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Text.Json;

namespace BossMod.Autorotation;

// note: the editor assumes it's the only thing that modifies the database instance; having multiple editors or editing database externally will break things
public sealed class UIPresetDatabaseEditor(PresetDatabase db)
{
    private int _selectedPresetIndex = -1;
    private int _pendingSelectPresetIndex = -1; // if >= 0, we want to select different preset, but current one has modifications
    private Type? _selectedModuleType; // we want module selection to be persistent when changing presets
    private UIPresetEditor? _selectedPreset;

    private bool HaveUnsavedModifications => _selectedPreset?.Modified ?? false;

    public void Draw()
    {
        if (_pendingSelectPresetIndex >= 0)
            DrawPendingSwitch();
        DrawPresetSelector();
        if (_selectedPreset != null)
        {
            _selectedPreset.Draw();
            _selectedModuleType = _selectedPreset.SelectedModuleType;
        }
        else
        {
            ImGui.TextUnformatted("Select preset to edit or create a new one.");
        }
    }

    private void DrawPendingSwitch()
    {
        if (_pendingSelectPresetIndex < 0)
            return;
        if (!HaveUnsavedModifications)
        {
            CompleteChangeCurrentPreset();
            return;
        }

        ImGui.OpenPopup("Unsaved modifications"); // TODO: why do i have to do it every frame???
        bool modalOpen = true;
        using var modal = ImRaii.PopupModal("Unsaved modifications", ref modalOpen, ImGuiWindowFlags.AlwaysAutoResize);
        if (!modal)
            return;
        ImGui.TextUnformatted($"Currently opened preset {_selectedPreset?.Preset.Name} has unsaved modifications.");
        ImGui.TextUnformatted("To select a new preset, you need to either save or discard them.");
        ImGui.TextUnformatted("How do you want to proceed?");
        if (DrawSaveCurrentPresetButton())
        {
            SaveCurrentPreset();
            CompleteChangeCurrentPreset();
        }
        ImGui.SameLine();
        if (UIMisc.Button("Save as copy", _selectedPresetIndex < 0, "Can't save new preset as copy"))
        {
            SaveCurrentPresetAsCopy();
            CompleteChangeCurrentPreset();
        }
        ImGui.SameLine();
        if (ImGui.Button("Discard"))
        {
            CompleteChangeCurrentPreset();
        }
        ImGui.SameLine();
        if (ImGui.Button("Cancel") || !modalOpen)
        {
            _pendingSelectPresetIndex = -1;
        }
        if (_pendingSelectPresetIndex < 0)
            ImGui.CloseCurrentPopup();
    }

    private void DrawPresetSelector()
    {
        UIMisc.HelpMarker("""
            To start using autorotation, create a *preset*.
            Preset configures rotation *modules* and their *strategies*.
            Module is a piece of code that evaluates game state and fills prioritized list of candidate actions.
            The autorotation framework selects the highest priority action from the list to execute on next opportunity.
            Each module can be further configured by a set of *strategies*, which customize different aspects of its behaviour.
            For example, you might want to create a 'single target' and 'aoe' presets, which would use the same modules, but would configure their strategies differently.
            You could optionally assign keyboard modifiers to each strategy value; such value would only be applied if modifier is held.
            This allows you, for example, to set up preset so that it delays 2-minute burst if shift is held.
            """);
        ImGui.SameLine();

        ImGui.SetNextItemWidth(200);
        using (var combo = ImRaii.Combo("Preset", _selectedPreset == null ? "" : _selectedPresetIndex < 0 ? "<new>" : db.Presets[_selectedPresetIndex].Name))
        {
            if (combo)
            {
                for (int i = 0; i < db.Presets.Count; ++i)
                {
                    var preset = db.Presets[i];
                    if (ImGui.Selectable(preset.Name, _selectedPresetIndex == i))
                    {
                        _pendingSelectPresetIndex = i;
                    }

                    if (ImGui.IsItemActive() && !ImGui.IsItemHovered())
                    {
                        var j = ImGui.GetMouseDragDelta().Y < 0 ? i - 1 : i + 1;
                        if (j >= 0 && j < db.Presets.Count)
                        {
                            (db.Presets[i], db.Presets[j]) = (db.Presets[j], db.Presets[i]);
                            if (_selectedPresetIndex == i)
                                _selectedPresetIndex = j;
                            else if (_selectedPresetIndex == j)
                                _selectedPresetIndex = i;
                            db.Modify(-1, null);
                            ImGui.ResetMouseDragDelta();
                        }
                    }
                }
            }
        }

        ImGui.SameLine();
        if (DrawSaveCurrentPresetButton())
            SaveCurrentPreset();
        ImGui.SameLine();
        if (UIMisc.Button("Save as copy", _selectedPresetIndex < 0, "Can't save new preset as copy"))
            SaveCurrentPresetAsCopy();
        ImGui.SameLine();
        if (UIMisc.Button("Revert", 0, (!HaveUnsavedModifications, "Current preset is not modified"), (_selectedPresetIndex < 0, "No preset is selected")))
            RevertCurrentPreset();
        ImGui.SameLine();
        if (UIMisc.Button("New", HaveUnsavedModifications, "Current preset is modified, save or discard changes"))
            CreateNewPreset(-1);
        ImGui.SameLine();
        if (UIMisc.Button("Copy", 0, (HaveUnsavedModifications, "Current preset is modified, save or discard changes"), (_selectedPresetIndex < 0, "No preset is selected")))
            CreateNewPreset(_selectedPresetIndex);
        ImGui.SameLine();
        if (UIMisc.Button("Delete", 0, (!ImGui.GetIO().KeyShift, "Hold shift to delete"), (_selectedPresetIndex < 0, "No preset is selected")))
            DeleteCurrentPreset();
        ImGui.SameLine();
        if (UIMisc.Button("Export", _selectedPreset == null, "No preset is selected"))
            ExportToClipboard();
        ImGui.SameLine();
        if (UIMisc.Button("Import", HaveUnsavedModifications, "Current preset is modified, save or discard changes"))
            ImportNewPresetFromClipboard();
    }

    private bool DrawSaveCurrentPresetButton() => UIMisc.Button("Save", 0, (!HaveUnsavedModifications, "Current preset is not modified"), (_selectedPreset?.NameConflict ?? false, "Current preset name is empty or duplicates name of other existing preset"));

    private void RevertCurrentPreset() => _selectedPreset = new(db, _selectedPresetIndex, _selectedModuleType);

    private void SaveCurrentPreset()
    {
        if (_selectedPreset != null && _selectedPreset.Modified && !_selectedPreset.NameConflict)
        {
            db.Modify(_selectedPresetIndex, _selectedPreset.Preset);
            if (_selectedPresetIndex < 0)
                _selectedPresetIndex = db.Presets.Count - 1;
            RevertCurrentPreset();
        }
        else
        {
            Service.Log($"[PD] Save called when current preset #{_selectedPresetIndex} is not modified or has bad name '{_selectedPreset?.Preset.Name}'");
        }
    }

    private void SaveCurrentPresetAsCopy()
    {
        if (_selectedPresetIndex >= 0 && _selectedPreset != null)
        {
            _selectedPreset.DetachFromSource();
            _selectedPreset.MakeNameUnique();
            _selectedPresetIndex = db.Presets.Count;
            db.Modify(-1, _selectedPreset.Preset);
            RevertCurrentPreset();
        }
        else
        {
            Service.Log($"[PD] Save-as called when no preset is selected");
        }
    }

    private void CreateNewPreset(int referenceIndex)
    {
        _selectedPresetIndex = -1;
        _selectedPreset = new(db, referenceIndex, _selectedModuleType);
        _selectedPreset.DetachFromSource();
        _selectedPreset.MakeNameUnique();
    }

    private void DeleteCurrentPreset()
    {
        if (_selectedPresetIndex >= 0)
        {
            db.Modify(_selectedPresetIndex, null);
            _selectedPresetIndex = -1;
            _selectedPreset = null;
        }
        else
        {
            Service.Log($"[PD] Delete called no preset is selected");
        }
    }

    private void CompleteChangeCurrentPreset()
    {
        _selectedPresetIndex = _pendingSelectPresetIndex;
        _pendingSelectPresetIndex = -1;
        RevertCurrentPreset();
    }

    private void ExportToClipboard()
    {
        if (_selectedPreset != null)
        {
            ImGui.SetClipboardText(JsonSerializer.Serialize(_selectedPreset.Preset, Serialization.BuildSerializationOptions()));
        }
        else
        {
            Service.Log($"[PD] Export called no preset is selected");
        }
    }

    private void ImportNewPresetFromClipboard()
    {
        try
        {
            var preset = JsonSerializer.Deserialize<Preset>(ImGui.GetClipboardText(), Serialization.BuildSerializationOptions())!;
            _selectedPresetIndex = -1;
            _selectedPreset = new(db, preset, _selectedModuleType);
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to parse preset: {ex}");
        }
    }
}
