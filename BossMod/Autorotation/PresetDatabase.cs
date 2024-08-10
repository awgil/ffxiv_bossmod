using System.IO;
using System.Text.Json;

namespace BossMod.Autorotation;

// note: presets in the database are immutable (otherwise eg. manager won't see the changes in active preset)
public sealed class PresetDatabase
{
    public readonly List<Preset> Presets = [];
    public Event<Preset?, Preset?> PresetModified = new(); // (old, new); old == null if preset is added, new == null if preset is removed

    private readonly FileInfo _dbPath;

    public PresetDatabase(string rootPath)
    {
        _dbPath = new(rootPath + ".db.json");
        if (_dbPath.Exists)
        {
            try
            {
                using var json = Serialization.ReadJson(_dbPath.FullName);
                var version = json.RootElement.GetProperty("version").GetInt32();
                var payload = json.RootElement.GetProperty("payload");
                Presets = payload.Deserialize<List<Preset>>(Serialization.BuildSerializationOptions()) ?? [];
            }
            catch (Exception ex)
            {
                Service.Log($"Failed to parse preset database '{_dbPath}': {ex}");
            }
        }
    }

    // if index >= 0: replace or delete
    // if index == -1: add (if replacement is non-null) or notify about reordering (otherwise)
    public void Modify(int index, Preset? replacement)
    {
        var previous = index >= 0 ? Presets[index] : null;

        if (index < 0 && replacement != null)
            Presets.Add(replacement);
        else if (index >= 0 && replacement == null)
            Presets.RemoveAt(index);
        else if (index >= 0 && replacement != null)
            Presets[index] = replacement;

        if (previous != null || replacement != null)
            PresetModified.Fire(previous, replacement);

        Save();
    }

    public void Save()
    {
        try
        {
            using var fstream = new FileStream(_dbPath.FullName, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var jwriter = Serialization.WriteJson(fstream);
            jwriter.WriteStartObject();
            jwriter.WriteNumber("version", 0);
            jwriter.WritePropertyName("payload");
            JsonSerializer.Serialize(jwriter, Presets, Serialization.BuildSerializationOptions());
            jwriter.WriteEndObject();
            Service.Log($"Database saved successfully to '{_dbPath.FullName}'");
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to write database to '{_dbPath.FullName}': {ex}");
        }
    }

    public IEnumerable<Preset> PresetsForClass(Class c) => Presets.Where(p => p.Modules.Any(m => RotationModuleRegistry.Modules[m.Key].Definition.Classes[(int)c]));
}
