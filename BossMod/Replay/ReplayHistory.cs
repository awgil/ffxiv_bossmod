using System.IO;
using System.Text.Json;

namespace BossMod;

public record struct ReplayMemory(string Path, bool IsOpen, DateTime PlaybackPosition);

public class ReplayHistory
{
    public List<ReplayMemory> History = [];

    public static ReplayHistory Load()
    {
        var file = Path.Combine(Plugin.GetStorageDir(), "replay-history.json");
        try
        {
            using var stream = Utils.OpenShareable(file);
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
        var file = Path.Combine(Plugin.GetStorageDir(), "replay-history.json");
        using var stream = File.Create(file);
        JsonSerializer.Serialize(stream, History);
    }
}
