using System.IO;
using System.Text.Json;

namespace BossMod.Autorotation;

// note: presets in the database are immutable (otherwise eg. manager won't see the changes in active preset)
public sealed class PresetDatabase
{
    private readonly AutorotationConfig _cfg = Service.Config.Get<AutorotationConfig>();

    public readonly List<Preset> DefaultPresets; // default presets, distributed as part of the plugin
    public readonly List<Preset> UserPresets; // user-defined presets, stored in user's preset db
    public Event<Preset?, Preset?> PresetModified = new(); // (old, new); old == null if preset is added, new == null if preset is removed

    private readonly FileInfo _dbPath;

    public IEnumerable<Preset> AllPresets => DefaultPresets.Select(p => p with { HiddenByDefault = _cfg.HideDefaultPreset }).Concat(UserPresets);

    public PresetDatabase(string rootPath, FileInfo defaultPresets)
    {
        _dbPath = new(rootPath + ".db.json");
        DefaultPresets = LoadPresetsFromFile(defaultPresets);
        UserPresets = LoadPresetsFromFile(_dbPath);
    }

    private List<Preset> LoadPresetsFromFile(FileInfo file)
    {
        try
        {
            var data = PlanPresetConverter.PresetSchema.Load(file);
            using var json = data.document;
            return data.payload.Deserialize<List<Preset>>(Serialization.BuildSerializationOptions()) ?? [];
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to parse preset database '{file.FullName}': {ex}");
            return [];
        }
    }

    // if index >= 0: replace or delete
    // if index == -1: add (if replacement is non-null) or notify about reordering (otherwise)
    public void Modify(int index, Preset? replacement)
    {
        var previous = index >= 0 ? UserPresets[index] : null;

        if (index < 0 && replacement != null)
            UserPresets.Add(replacement);
        else if (index >= 0 && replacement == null)
            UserPresets.RemoveAt(index);
        else if (index >= 0 && replacement != null)
            UserPresets[index] = replacement;

        if (previous != null || replacement != null)
            PresetModified.Fire(previous, replacement);

        Save();
    }

    public void Save()
    {
        try
        {
            PlanPresetConverter.PresetSchema.Save(_dbPath, jwriter => JsonSerializer.Serialize(jwriter, UserPresets, Serialization.BuildSerializationOptions()));
            Service.Log($"Database saved successfully to '{_dbPath.FullName}'");
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to write database to '{_dbPath.FullName}': {ex}");
        }
    }

    public IEnumerable<Preset> PresetsForClass(Class c) => AllPresets.Where(p => p.Modules.Any(m => m.Definition.Classes[(int)c]));

    public Preset? FindPresetByName(ReadOnlySpan<char> name, StringComparison cmp = StringComparison.CurrentCultureIgnoreCase)
    {
        foreach (var p in AllPresets)
            if (name.Equals(p.Name, cmp))
                return p;
        return null;
    }
}
