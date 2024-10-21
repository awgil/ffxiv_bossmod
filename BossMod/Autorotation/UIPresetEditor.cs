using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod.Autorotation;

public sealed class UIPresetEditor
{
    private class ModuleCategory
    {
        public SortedDictionary<string, ModuleCategory> Subcategories = [];
        public List<Type> Leafs = [];
    }

    private readonly PresetDatabase _db;
    private int _sourcePresetIndex;
    private bool _sourcePresetDefault;
    public readonly Preset Preset;
    public bool Modified { get; private set; }
    public bool NameConflict { get; private set; }
    public Type? SelectedModuleType { get; private set; }
    private int _selectedSettingIndex = -1;
    private readonly List<int> _orderedTrackList = []; // for current module, by UI order
    private readonly List<int> _settingGuids = []; // a hack to keep track of held item during drag-n-drop
    private readonly ModuleCategory _modulesByCategory;

    public UIPresetEditor(PresetDatabase db, int index, bool isDefaultPreset, Type? initiallySelectedModuleType)
    {
        _db = db;
        _sourcePresetIndex = index;
        _sourcePresetDefault = isDefaultPreset;
        _modulesByCategory = SplitModulesByCategory();
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
        SelectModule(initiallySelectedModuleType);
    }

    public UIPresetEditor(PresetDatabase db, Preset preset, Type? initiallySelectedModuleType)
    {
        _db = db;
        _sourcePresetIndex = -1;
        _modulesByCategory = SplitModulesByCategory();
        Preset = preset;
        NameConflict = CheckNameConflict();
        Modified = true;
        SelectModule(initiallySelectedModuleType);
    }

    public void SelectModule(Type? type)
    {
        SelectedModuleType = type;
        _selectedSettingIndex = -1;
        _orderedTrackList.Clear();
        if (type != null && Preset.Modules.ContainsKey(type))
        {
            var md = RotationModuleRegistry.Modules[type].Definition;
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
        using (var popup = ImRaii.Popup("add_module"))
            if (popup)
                DrawModuleAddPopup(_modulesByCategory);

        var width = new Vector2(ImGui.GetContentRegionAvail().X, 0);
        using (var list = ImRaii.ListBox("###modules", width))
        {
            if (list)
            {
                foreach (var m in Preset.Modules.Keys)
                {
                    if (DrawModule(m, RotationModuleRegistry.Modules[m].Definition, m == SelectedModuleType))
                    {
                        SelectModule(m);
                    }
                }
            }
        }

        using var d = ImRaii.Disabled(_sourcePresetDefault);

        if (ImGui.Button("Add##module", width))
            ImGui.OpenPopup("add_module");

        if (UIMisc.Button("Remove##module", width.X, (SelectedModuleType == null || !Preset.Modules.ContainsKey(SelectedModuleType), "Select any module to remove"), (!ImGui.GetIO().KeyShift, "Hold shift")))
        {
            Preset.Modules.Remove(SelectedModuleType!);
            Modified = true;
            SelectModule(null);
        }
    }

    private void DrawModuleAddPopup(ModuleCategory cat)
    {
        foreach (var sub in cat.Subcategories)
        {
            if (HaveModulesToAddInCategory(sub.Value))
            {
                if (ImGui.BeginMenu(sub.Key))
                {
                    DrawModuleAddPopup(sub.Value);
                    ImGui.EndMenu();
                }
            }
        }

        foreach (var leaf in cat.Leafs)
        {
            if (!Preset.Modules.ContainsKey(leaf))
            {
                if (DrawModule(leaf, RotationModuleRegistry.Modules[leaf].Definition))
                {
                    Preset.Modules[leaf] = new();
                    Modified = true;
                    SelectModule(leaf);
                }
            }
        }
    }

    private bool DrawModule(Type type, RotationModuleDefinition definition, bool selected = false)
    {
        var res = ImGui.Selectable(definition.DisplayName, selected);
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
        if (SelectedModuleType == null || !Preset.Modules.TryGetValue(SelectedModuleType, out var ms))
        {
            ImGui.TextUnformatted("Add or select rotation module to configure its strategies");
            return;
        }

        using var d = ImRaii.Disabled(_sourcePresetDefault);

        var width = new Vector2(ImGui.GetContentRegionAvail().X, 0);
        var md = RotationModuleRegistry.Modules[SelectedModuleType].Definition;
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
        if (SelectedModuleType == null || !Preset.Modules.TryGetValue(SelectedModuleType, out var ms))
        {
            ImGui.TextUnformatted("Select module to preview resulting strategies with different modifiers");
            ImGui.TextUnformatted("Select strategy to edit its preferences");
        }
        else if (_selectedSettingIndex < 0)
        {
            DrawModulePreview(SelectedModuleType);
        }
        else
        {
            DrawSettingDetails(SelectedModuleType, ms);
        }
    }

    private void DrawModulePreview(Type moduleType)
    {
        ImGui.TextUnformatted($"Current modifiers: {Preset.CurrentModifiers()}");
        var md = RotationModuleRegistry.Modules[moduleType].Definition;
        var values = Preset.ActiveStrategyOverrides(moduleType);
        foreach (var i in _orderedTrackList)
        {
            ref var val = ref values.Values[i];
            ImGui.TextUnformatted($"{md.Configs[i].UIName} = {md.Configs[i].Options[val.Option].UIName}");
        }
    }

    private void DrawSettingDetails(Type moduleType, Preset.ModuleSettings ms)
    {
        using var d = ImRaii.Disabled(_sourcePresetDefault);
        var md = RotationModuleRegistry.Modules[moduleType].Definition;
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
        if (SelectedModuleType != null && Preset.Modules.TryGetValue(SelectedModuleType, out var ms))
            _settingGuids.AddRange(Enumerable.Range(0, ms.Settings.Count));
    }

    private ModuleCategory SplitModulesByCategory()
    {
        ModuleCategory res = new();
        foreach (var m in RotationModuleRegistry.Modules)
        {
            if (m.Value.Definition.RelatedBossModule != null)
                continue; // don't care about boss-specific modules for presets

            var cat = res;
            foreach (var part in m.Value.Definition.Category.Split('|', StringSplitOptions.RemoveEmptyEntries))
                cat = cat.Subcategories.GetOrAdd(part);
            cat.Leafs.Add(m.Key);
        }
        return res;
    }

    private bool HaveModulesToAddInCategory(ModuleCategory cat) => cat.Leafs.Any(leaf => !Preset.Modules.ContainsKey(leaf)) || cat.Subcategories.Values.Any(HaveModulesToAddInCategory);
}
