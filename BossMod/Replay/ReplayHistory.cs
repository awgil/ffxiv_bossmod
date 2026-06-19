using System.IO;
using System.Text.Json;

namespace BossMod;

public record struct ReplayMemory(string Path, bool IsOpen, DateTime PlaybackPosition);

public class ReplayHistory
{
    public List<ReplayMemory> History = [];

    private static DirectoryInfo GetStorageDir()
    {
        var dir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vbm-replay"));
        if (!dir.Exists)
            dir.Create();

        return dir;
    }

    public static ReplayHistory Load()
    {
        var file = Path.Combine(GetStorageDir().FullName, "history.json");
        try
        {
            using var stream = File.OpenRead(file);
            var m = JsonSerializer.Deserialize<List<ReplayMemory>>(stream);
            return new() { History = m! };
        }
        catch (Exception ex)
        {
            if (ex is JsonException or IOException)
            {
                Service.PluginLog.Warning(ex, "Unable to load replay history");
                return new() { History = [] };
            }

            throw;
        }
    }

    public void Save()
    {
        var file = Path.Combine(GetStorageDir().FullName, "history.json");
        using var stream = File.Create(file);
        JsonSerializer.Serialize(stream, History);
    }
}
