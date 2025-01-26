using System.Text.Json.Nodes;

namespace BossMod;

public static class JsonExtensions
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
