using System.IO;
using System.Text.Json;

namespace BossMod.Autorotation;

// note: this also includes editor ui; it could be split off into a separate class, but then it would create headaches with multiple editors on same db...
public sealed class PresetDatabase
{
    // note: presets in the database are immutable (otherwise eg. preset manager won't see the changes in active preset)
    public readonly List<Preset> Presets = [];
    public Event<Preset?, Preset?> PresetModified = new(); // (old, new); old == null if preset is added, new == null if preset is removed

    private readonly FileInfo _dbPath;

    // transient editor created on demand
    private UIPresetDatabaseEditor? _editor;
    public UIPresetDatabaseEditor Editor => _editor ??= new(this);

    public PresetDatabase(FileInfo dbPath)
    {
        _dbPath = dbPath;
        if (dbPath.Exists)
        {
            try
            {
                using var fstream = File.OpenRead(dbPath.FullName);
                using var json = JsonDocument.Parse(fstream, new JsonDocumentOptions() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
                var version = json.RootElement.GetProperty("version").GetInt32();
                var payload = json.RootElement.GetProperty("payload");
                Presets = payload.Deserialize<List<Preset>>(Serialization.BuildSerializationOptions()) ?? [];
            }
            catch (Exception ex)
            {
                Service.Log($"Failed to parse preset database '{dbPath}': {ex}");
            }
        }
    }

    public void Save()
    {
        try
        {
            using var fstream = new FileStream(_dbPath.FullName, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var jwriter = new Utf8JsonWriter(fstream, new JsonWriterOptions() { Indented = true });
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
}
