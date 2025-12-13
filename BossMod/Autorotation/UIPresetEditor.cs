using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.Autorotation;

public sealed class UIPresetEditor
{
    private class ModuleCategory
    {
        public ModuleCategory? Parent;
        public SortedDictionary<string, ModuleCategory> Subcategories = [];
        public List<(Type type, RotationModuleDefinition def, Func<RotationModuleManager, Actor, RotationModule> builder)> Leafs = [];
    }

    private readonly AutorotationConfig _autorotConfig = Service.Config.Get<AutorotationConfig>();
    private readonly PresetDatabase _db;
    private int _sourcePresetIndex;
    private bool _sourcePresetDefault;
    public readonly Preset Preset; // note: this is an edited copy, and as such it never has transient settings
    public bool Modified { get; private set; }
    public bool NameConflict { get; private set; }
    private int _selectedModuleIndex = -1;
    private readonly List<int> _orderedTrackList = []; // for current module, by UI order
    private readonly ModuleCategory _availableModules;
    private bool _showHiddenTracks;
    private bool _currentModuleHasHealerAI;

    private static readonly Type THealerAI = typeof(xan.HealerAI);
    private static readonly Type[] _misleadingHealerRotations = [
        typeof(xan.WHM),
        typeof(xan.AST),
        typeof(xan.SCH),
        typeof(xan.SGE),
        typeof(akechi.AkechiSCH)
    ];

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
        _currentModuleHasHealerAI = preset.Modules.Any(m => m.Type == THealerAI);
        SelectModule(FindModuleByType(initiallySelectedModuleType));
    }

    public void SelectModule(int index)
    {
        _selectedModuleIndex = index;
        _showHiddenTracks = false;
        _orderedTrackList.Clear();
        if (index >= 0)
        {
            var md = Preset.Modules[index].Definition;
            _orderedTrackList.AddRange(Enumerable.Range(0, md.Configs.Count));
            _orderedTrackList.SortByReverse(i => md.Configs[i].UIPriority);
        }
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

        using var table = ImRaii.Table("preset_details", 2);
        if (!table)
            return;
        ImGui.TableSetupColumn("Modules", ImGuiTableColumnFlags.WidthFixed, 200 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Strategies");
        ImGui.TableNextColumn();
        DrawModulesList();
        ImGui.TableNextColumn();
        DrawSettingsList();
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

        var size = ImGui.GetContentRegionAvail();
        var width = new Vector2(size.X, 0);
        using (var list = ImRaii.ListBox("###modules", new(size.X, size.Y - 100)))
        {
            if (list)
            {
                for (int i = 0; i < Preset.Modules.Count; ++i)
                {
                    var m = Preset.Modules[i];
                    if (i != 0 && Preset.Modules[i - 1].Definition.Order != m.Definition.Order)
                        ImGui.Separator();

                    if (DrawModule(m.Type, m.Definition, i == _selectedModuleIndex))
                    {
                        SelectModule(i);
                    }

                    if (ImGui.IsItemActive() && !ImGui.IsItemHovered())
                    {
                        var j = ImGui.GetMouseDragDelta().Y < 0 ? i - 1 : i + 1;
                        if (j >= 0 && j < Preset.Modules.Count && Preset.Modules[j].Definition.Order == m.Definition.Order)
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
            var m = Preset.Modules[_selectedModuleIndex];
            AddAvailableModule(m.Type, m.Definition, m.Builder, _availableModules);
            Preset.Modules.RemoveAt(_selectedModuleIndex);
            Modified = true;
            _currentModuleHasHealerAI &= m.Type != THealerAI;
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
                var index = Preset.AddModule(leaf.type, leaf.def, leaf.builder);
                Modified = true;
                SelectModule(index);
                _currentModuleHasHealerAI |= leaf.type == THealerAI;
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

        ImGui.Checkbox("Show hidden tracks", ref _showHiddenTracks);

        using var _ = ImRaii.PushStyle(ImGuiStyleVar.CellPadding, new Vector2(5, 5));
        using var table = ImRaii.Table("preset_options", 2, ImGuiTableFlags.BordersInnerH);
        if (!table)
            return;
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 250 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("");

        if (!_sourcePresetDefault && SuggestHealerAI(ms))
            return;

        foreach (var track in _orderedTrackList)
        {
            var cfg = ms.Definition.Configs[track];
            var active = ms.SerializedSettings.FindIndex(s => s.Track == track && s.Mod == default);
            if (active < 0 && cfg.UIPriority < 0 && !_showHiddenTracks)
                break;

            using (ImRaii.PushId($"{cfg.InternalName}_default"))
            {
                if (active >= 0)
                {
                    var v2 = ms.SerializedSettings[active].Value with { }; // make clone
                    if (RendererFactory.Draw(StrategyContext.Preset, cfg, ref v2))
                    {
                        ms.SerializedSettings.RemoveAt(active);
                        if (!cfg.IsDefault(v2))
                            ms.SerializedSettings.Insert(active, new(default, track, v2));
                        Modified = true;
                    }
                }
                else
                {
                    var v1 = cfg.CreateEmpty();
                    if (RendererFactory.Draw(StrategyContext.Preset, cfg, ref v1))
                    {
                        if (!cfg.IsDefault(v1))
                            ms.SerializedSettings.Add(new(default, track, v1));
                        Modified = true;
                    }
                }
            }
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Dummy(new(1, 27));
        ImGui.Text("Hotkey overrides");
        ImGui.TableNextColumn();
        ImGui.Dummy(new(1, 24));
        ImGui.SetNextItemWidth(200 * ImGuiHelpers.GlobalScale);
        using (var combo = ImRaii.Combo("###selectover", "Add new..."))
        {
            if (combo)
            {
                for (var i = 0; i < ms.Definition.Configs.Count; i++)
                    if (ImGui.Selectable(ms.Definition.Configs[i].UIName))
                    {
                        ms.SerializedSettings.Add(new(Preset.Modifier.Shift, i, ms.Definition.Configs[i].CreateForEditor()));
                    }
            }
        }

        for (var i = 0; i < ms.SerializedSettings.Count; i++)
        {
            ref var val = ref ms.SerializedSettings.Ref(i);
            if (val.Mod == default)
                continue;

            var cfg = ms.Definition.Configs[val.Track];

            using var _i2 = ImRaii.PushId($"{cfg.InternalName}_override_{i}");

            Modified |= RendererFactory.Draw(StrategyContext.Preset, cfg, ref val.Value);

            Modified |= DrawModifier(ref val.Mod, Preset.Modifier.Shift, "Shift");
            ImGui.SameLine();
            Modified |= DrawModifier(ref val.Mod, Preset.Modifier.Ctrl, "Ctrl");
            ImGui.SameLine();
            Modified |= DrawModifier(ref val.Mod, Preset.Modifier.Alt, "Alt");
            ImGui.SameLine();
            if (ImGui.Button("Delete override"))
                ms.SerializedSettings.RemoveAt(i);
        }
    }

    private bool SuggestHealerAI(Preset.ModuleSettings ms)
    {
        if (!_currentModuleHasHealerAI && _autorotConfig.SuggestHealerAI && _misleadingHealerRotations.Contains(ms.Type))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Auto-heal/auto-raise");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Not part of the standard rotation. Use the Healer AI module instead.");
            if (ImGui.Button("Add Healer AI"))
            {
                var rot = RotationModuleRegistry.Modules[THealerAI];
                var index = Preset.AddModule(THealerAI, rot.Definition, rot.Builder);
                Modified = true;
                SelectModule(index);
                _currentModuleHasHealerAI = true;
                return true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Don't show this suggestion again"))
            {
                _autorotConfig.SuggestHealerAI = false;
                _autorotConfig.Modified.Fire();
            }
        }
        return false;
    }

    private bool DrawModifier(ref Preset.Modifier mod, Preset.Modifier flag, string label)
    {
        var value = mod.HasFlag(flag);
        using var _ = ImRaii.Disabled(mod == flag);

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
        if (_db.DefaultPresets.Any(p => p.Name == Preset.Name))
            return true;

        for (var i = 0; i < _db.UserPresets.Count; ++i)
            if (i != _sourcePresetIndex && _db.UserPresets[i].Name == Preset.Name)
                return true;
        return false;
    }

    private int FindModuleByType(Type? type) => type != null ? Preset.Modules.FindIndex(m => m.Type == type) : -1;

    private ModuleCategory BuildAvailableModules()
    {
        ModuleCategory res = new();
        foreach (var m in RotationModuleRegistry.Modules)
        {
            if (m.Value.Definition.DevMode && !Service.IsDev)
                continue; // skip dev-mode-only module in "production"
            if (m.Value.Definition.RelatedBossModule != null)
                continue; // don't care about boss-specific modules for presets
            if (FindModuleByType(m.Key) >= 0)
                continue; // module is already added to preset
            AddAvailableModule(m.Key, m.Value.Definition, m.Value.Builder, res);
        }
        return res;
    }

    private void AddAvailableModule(Type type, RotationModuleDefinition def, Func<RotationModuleManager, Actor, RotationModule> builder, ModuleCategory root)
    {
        var cat = root;
        foreach (var part in def.Category.Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!cat.Subcategories.TryGetValue(part, out var sub))
                sub = cat.Subcategories[part] = new() { Parent = cat };
            cat = sub;
        }
        cat.Leafs.Add((type, def, builder));
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
