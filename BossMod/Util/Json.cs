using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Text.Json.Nodes;

namespace BossMod;

public static class Json
{
    public static bool TryRemoveNode(this JsonObject parent, string key, out JsonNode? node) => parent.TryGetPropertyValue(key, out node) && parent.Remove(key);

    public static bool TryRenameNode(this JsonObject parent, string oldKey, string newKey)
    {
        if (!TryRemoveNode(parent, oldKey, out JsonNode? node))
            return false;
        parent.Add(newKey, node);
        return true;
    }
}

public static partial class Utils
{
    public static T LoadFromAssembly<T>(string resourceName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName) ?? throw new InvalidDataException($"unable to locate resource {resourceName}");
        using var reader = new StreamReader(stream);
        using var jreader = new JsonTextReader(reader);
        return new JsonSerializer().Deserialize<T>(jreader) ?? throw new InvalidDataException($"unable to load json from file");
    }
}
