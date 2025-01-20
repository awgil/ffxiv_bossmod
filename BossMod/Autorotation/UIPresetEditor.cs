using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod.Autorotation;

public sealed class UIPresetEditor
{
    private class ModuleCategory
    {
        public ModuleCategory? Parent;
        public SortedDictionary<string, ModuleCategory> Subcategories = [];
        public List<(Type type, RotationModuleDefinition def)> Leafs = [];
    }

    private readonly PresetDatabase _db;
    private int _sourcePresetIndex;
    private bool _sourcePresetDefault;
    public readonly Preset Preset;
    public bool Modified { get; private set; }
    public bool NameConflict { get; private set; }
    private int _selectedModuleIndex = -1;
    private int _selectedSettingIndex = -1;
    private readonly List<int> _orderedTrackList = []; // for current module, by UI order
    private readonly List<int> _settingGuids = []; // a hack to keep track of held item during drag-n-drop
    private readonly ModuleCategory _availableModules;

    public Type? SelectedModuleType => Preset.Modules.BoundSafeAt(_selectedModuleIndex)?.Type;

    public UIPresetEditor(PresetDatabase db, int index, bool isDefaultPreset, Type? initiallySelectedModuleType)
    {
        _db = db;
        _sourcePresetIndex = index;
        _sourcePresetDefault = isDefaultPreset;
        if (index >= 0)
        {
            Preset = (isDefaultPreset ? db.DefaultPresets : db.UserPresets)[index].MakeClone(false);
        }
        else
        {
            Preset = new("New");
            NameConflict = CheckNameConflict();
            MakeNameUnique();
            Modified = false; // don't bother...
        }
        _availableModules = BuildAvailableModules();
        SelectModule(FindModuleByType(initiallySelectedModuleType));
    }

    public UIPresetEditor(PresetDatabase db, Preset preset, Type? initiallySelectedModuleType)
    {
        _db = db;
        _sourcePresetIndex = -1;
        Preset = preset;
        NameConflict = CheckNameConflict();
        Modified = true;
        _availableModules = BuildAvailableModules();
        SelectModule(FindModuleByType(initiallySelectedModuleType));
    }

    public void SelectModule(int index)
    {
        _selectedModuleIndex = index;
        _selectedSettingIndex = -1;
        _orderedTrackList.Clear();
        if (index >= 0)
        {
            var md = RotationModuleRegistry.Modules[Preset.Modules[index].Type].Definition;
            _orderedTrackList.AddRange(Enumerable.Range(0, md.Configs.Count));
            _orderedTrackList.SortByReverse(i => md.Configs[i].UIPriority);
        }
        RebuildSettingGuids();
    }

    public void Draw()
    {
        using (ImRaii.Disabled(_sourcePresetDefault))
        {
            if (ImGui.InputText("Name", ref Preset.Name, 256))
            {
                Modified = true;
                NameConflict = CheckNameConflict();
            }
        }

        using var table = ImRaii.Table("preset_details", 3);
        if (!table)
            return;
        ImGui.TableSetupColumn("Modules");
        ImGui.TableSetupColumn("Strategies");
        ImGui.TableSetupColumn("Details");
        ImGui.TableHeadersRow();
        ImGui.TableNextColumn();
        DrawModulesList();
        ImGui.TableNextColumn();
        DrawSettingsList();
        ImGui.TableNextColumn();
        DrawDetails();
    }

    public void DetachFromSource()
    {
        _sourcePresetIndex = -1;
        _sourcePresetDefault = false;
        NameConflict = CheckNameConflict();
    }

    public void MakeNameUnique()
    {
        var baseName = Preset.Name;
        var i = 1;
        while (NameConflict)
        {
            Preset.Name = $"{baseName} ({i++})";
            Modified = true;
            NameConflict = CheckNameConflict();
        }
    }

    public void DrawModulesList()
    {
        Action? post = null;
        using (var popup = ImRaii.Popup("add_module"))
            if (popup)
                DrawModuleAddPopup(_availableModules, ref post);
        post?.Invoke();

        var width = new Vector2(ImGui.GetContentRegionAvail().X, 0);
        using (var list = ImRaii.ListBox("###modules", width))
        {
            if (list)
            {
                for (int i = 0; i < Preset.Modules.Count; ++i)
                {
                    var m = Preset.Modules[i];
                    if (DrawModule(m.Type, RotationModuleRegistry.Modules[m.Type].Definition, i == _selectedModuleIndex))
                    {
                        SelectModule(i);
                    }

                    if (ImGui.IsItemActive() && !ImGui.IsItemHovered())
                    {
                        var j = ImGui.GetMouseDragDelta().Y < 0 ? i - 1 : i + 1;
                        if (j >= 0 && j < Preset.Modules.Count)
                        {
                            (Preset.Modules[i], Preset.Modules[j]) = (Preset.Modules[j], Preset.Modules[i]);
                            Modified = true;
                            if (_selectedModuleIndex == i)
                                _selectedModuleIndex = j;
                            else if (_selectedModuleIndex == j)
                                _selectedModuleIndex = i;
                            ImGui.ResetMouseDragDelta();
                        }
                    }
                }
            }
        }

        using var d = ImRaii.Disabled(_sourcePresetDefault);

        if (ImGui.Button("Add##module", width))
            ImGui.OpenPopup("add_module");

        if (UIMisc.Button("Remove##module", width.X, (_selectedModuleIndex < 0, "Select any module to remove"), (!ImGui.GetIO().KeyShift, "Hold shift")))
        {
            var m = Preset.Modules[_selectedModuleIndex].Type;
            AddAvailableModule(m, RotationModuleRegistry.Modules[m].Definition, _availableModules);
            Preset.Modules.RemoveAt(_selectedModuleIndex);
            Modified = true;
            SelectModule(-1);
        }
    }

    private void DrawModuleAddPopup(ModuleCategory cat, ref Action? actions)
    {
        foreach (var sub in cat.Subcategories)
        {
            if (ImGui.BeginMenu(sub.Key))
            {
                DrawModuleAddPopup(sub.Value, ref actions);
                ImGui.EndMenu();
            }
        }

        foreach (var leaf in cat.Leafs)
        {
            if (DrawModule(leaf.type, leaf.def))
            {
                Preset.Modules.Add(new(leaf.type));
                Modified = true;
                SelectModule(Preset.Modules.Count - 1);
                actions += () => RemoveAvailableModule(cat, leaf.type);
            }
        }
    }

    private bool DrawModule(Type type, RotationModuleDefinition definition, bool selected = false)
    {
        var res = ImGui.Selectable(definition.DisplayName, selected); // note: this assumes display names are unique
        if (ImGui.IsItemHovered())
        {
            using var tooltip = ImRaii.Tooltip();
            if (tooltip)
            {
                UIRotationModule.DescribeModule(type, definition);
            }
        }
        return res;
    }

    private void DrawSettingsList()
    {
        if (_selectedModuleIndex < 0)
        {
            ImGui.TextUnformatted("Add or select rotation module to configure its strategies");
            return;
        }

        using var d = ImRaii.Disabled(_sourcePresetDefault);

        var width = new Vector2(ImGui.GetContentRegionAvail().X, 0);
        var ms = Preset.Modules[_selectedModuleIndex];
        var md = RotationModuleRegistry.Modules[ms.Type].Definition;
        DrawAddSettingPopup(ms, md);

        using (var list = ImRaii.ListBox("###settings", width))
        {
            if (list)
            {
                for (int i = 0; i < ms.Settings.Count; ++i)
                {
                    var persistent = i < ms.NumSerialized;
                    ref var m = ref ms.Settings.Ref(i);
                    var cfg = md.Configs[m.Track];
                    if (ImGui.Selectable($"[{i + 1}]{(persistent ? "" : " (transient)")} {cfg.UIName} [{m.Mod}] = {cfg.Options[m.Value.Option].UIName}###setting{_settingGuids[i]}", i == _selectedSettingIndex))
                    {
                        _selectedSettingIndex = i;
                    }

                    if (ImGui.IsItemActive() && !ImGui.IsItemHovered())
                    {
                        var j = ImGui.GetMouseDragDelta().Y < 0 ? i - 1 : i + 1;
                        if (j >= 0 && j < ms.Settings.Count && persistent == (j < ms.NumSerialized))
                        {
                            (ms.Settings[i], ms.Settings[j]) = (ms.Settings[j], ms.Settings[i]);
                            (_settingGuids[i], _settingGuids[j]) = (_settingGuids[j], _settingGuids[i]);
                            Modified |= persistent;
                            if (_selectedSettingIndex == i)
                                _selectedSettingIndex = j;
                            else if (_selectedSettingIndex == j)
                                _selectedSettingIndex = i;
                            ImGui.ResetMouseDragDelta();
                        }
                    }
                }
            }
        }
        if (UIMisc.Button("Add##setting", md.Configs.Count == 0, "Module does not support configuration", width.X))
            ImGui.OpenPopup("add_setting");

        if (UIMisc.Button("Remove##setting", width.X, (_selectedSettingIndex < 0, "Select any strategy to remove"), (!ImGui.GetIO().KeyShift, "Hold shift")))
        {
            ms.Settings.RemoveAt(_selectedSettingIndex);
            if (_selectedSettingIndex < ms.NumSerialized)
            {
                --ms.NumSerialized;
                Modified = true;
            }
            _selectedSettingIndex = -1;
            RebuildSettingGuids();
        }
    }

    private void DrawAddSettingPopup(Preset.ModuleSettings ms, RotationModuleDefinition def)
    {
        using var popup = ImRaii.Popup("add_setting");
        if (!popup)
            return;

        foreach (var i in _orderedTrackList)
        {
            var cfg = def.Configs[i];
            if (ImGui.Selectable(cfg.UIName))
            {
                _selectedSettingIndex = ms.NumSerialized++;
                ms.Settings.Insert(_selectedSettingIndex, new(Preset.Modifier.None, i, new() { Option = cfg.Options.Count > 1 ? 1 : 0 }));
                Modified = true;
                RebuildSettingGuids();
            }
        }
    }

    private void DrawDetails()
    {
        if (_selectedModuleIndex < 0)
        {
            ImGui.TextUnformatted("Select module to preview resulting strategies with different modifiers");
            ImGui.TextUnformatted("Select strategy to edit its preferences");
        }
        else if (_selectedSettingIndex < 0)
        {
            DrawModulePreview();
        }
        else
        {
            DrawSettingDetails();
        }
    }

    private void DrawModulePreview()
    {
        ImGui.TextUnformatted($"Current modifiers: {Preset.CurrentModifiers()}");
        var ms = Preset.Modules[_selectedModuleIndex];
        var md = RotationModuleRegistry.Modules[ms.Type].Definition;
        var values = Preset.ActiveStrategyOverrides(_selectedModuleIndex);
        foreach (var i in _orderedTrackList)
        {
            ref var val = ref values.Values[i];
            ImGui.TextUnformatted($"{md.Configs[i].UIName} = {md.Configs[i].Options[val.Option].UIName}");
        }
    }

    private void DrawSettingDetails()
    {
        using var d = ImRaii.Disabled(_sourcePresetDefault);
        var ms = Preset.Modules[_selectedModuleIndex];
        var md = RotationModuleRegistry.Modules[ms.Type].Definition;
        ref var s = ref ms.Settings.Ref(_selectedSettingIndex);
        var cfg = md.Configs[s.Track];
        ImGui.TextUnformatted($"Setting: {cfg.UIName}");
        var persistent = _selectedSettingIndex < ms.NumSerialized;
        Modified |= DrawModifier(ref s.Mod, Preset.Modifier.Shift, "Shift") && persistent;
        Modified |= DrawModifier(ref s.Mod, Preset.Modifier.Ctrl, "Ctrl") && persistent;
        Modified |= DrawModifier(ref s.Mod, Preset.Modifier.Alt, "Alt") && persistent;
        Modified |= UIStrategyValue.DrawEditor(ref s.Value, cfg, null, null) && persistent;
    }

    private bool DrawModifier(ref Preset.Modifier mod, Preset.Modifier flag, string label)
    {
        bool value = mod.HasFlag(flag);
        if (ImGui.Checkbox(label, ref value))
        {
            if (value)
                mod |= flag;
            else
                mod &= ~flag;
            return true;
        }
        return false;
    }

    private bool CheckNameConflict()
    {
        for (int i = 0; i < _db.UserPresets.Count; ++i)
            if (i != _sourcePresetIndex && _db.UserPresets[i].Name == Preset.Name)
                return true;
        return false;
    }

    private void RebuildSettingGuids()
    {
        _settingGuids.Clear();
        if (_selectedModuleIndex >= 0)
            _settingGuids.AddRange(Enumerable.Range(0, Preset.Modules[_selectedModuleIndex].Settings.Count));
    }

    private int FindModuleByType(Type? type) => type != null ? Preset.Modules.FindIndex(m => m.Type == type) : -1;

    private ModuleCategory BuildAvailableModules()
    {
        ModuleCategory res = new();
        foreach (var m in RotationModuleRegistry.Modules)
        {
            if (m.Value.Definition.RelatedBossModule != null)
                continue; // don't care about boss-specific modules for presets
            if (FindModuleByType(m.Key) >= 0)
                continue; // module is already added to preset
            AddAvailableModule(m.Key, m.Value.Definition, res);
        }
        return res;
    }

    private void AddAvailableModule(Type type, RotationModuleDefinition def, ModuleCategory root)
    {
        var cat = root;
        foreach (var part in def.Category.Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!cat.Subcategories.TryGetValue(part, out var sub))
                sub = cat.Subcategories[part] = new() { Parent = cat };
            cat = sub;
        }
        cat.Leafs.Add((type, def));
        cat.Leafs.SortBy(e => e.def.DisplayName);
    }

    private void RemoveAvailableModule(ModuleCategory cat, Type type)
    {
        cat.Leafs.RemoveAll(e => e.type == type);
        while (cat.Leafs.Count == 0 && cat.Subcategories.Count == 0 && cat.Parent != null)
        {
            foreach (var kv in cat.Parent.Subcategories)
            {
                if (kv.Value == cat)
                {
                    cat.Parent.Subcategories.Remove(kv.Key);
                    break;
                }
            }
            cat = cat.Parent;
        }
    }
}
