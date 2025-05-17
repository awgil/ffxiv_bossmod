using System.Text.Json.Nodes;

namespace BossMod.Autorotation;

public static class PlanPresetConverter
{
    public static readonly VersionedJSONSchema PlanSchema = BuildSchema(true);
    public static readonly VersionedJSONSchema PresetSchema = BuildSchema(false);

    private static VersionedJSONSchema BuildSchema(bool plan)
    {
        var res = new VersionedJSONSchema();
        res.Converters.Add((j, _, _) => // v1: StandardWAR -> VeynVAR rename
        {
            Dictionary<string, string> moduleRenames = new() { ["BossMod.Autorotation.StandardWAR"] = "BossMod.Autorotation.VeynWAR" };
            foreach (var m in EnumerateEntriesModules(j, plan))
                RenameKeys(m, moduleRenames);
            return j;
        });
        res.Converters.Add((j, _, _) =>
        {
            if (plan)
            {
                if ((string)j["Encounter"]!.AsValue()! == "BossMod.Dawntrail.Extreme.Ex4Zelenia.Zelenia")
                    j["Encounter"] = "BossMod.Dawntrail.Extreme.Ex4Zelenia.Ex4Zelenia";
            }

            return j;
        });
        return res;
    }

    // returns always 1 element for plans, or multiple (1 per preset) for preset database
    private static IEnumerable<JsonObject> EnumerateEntriesModules(JsonNode root, bool plan)
    {
        if (plan)
        {
            yield return root!["Modules"]!.AsObject();
        }
        else
        {
            foreach (var preset in root.AsArray())
            {
                yield return preset!["Modules"]!.AsObject();
            }
        }
    }

    private static void RenameKeys(JsonObject j, Dictionary<string, string> renames)
    {
        // TODO: net9 - use indexed access to simplify & speed up implementation
        for (int i = 0, cnt = j.Count; i < cnt; ++i)
        {
            var (k, v) = j.First();
            j.Remove(k);
            j.Add(renames.TryGetValue(k, out var renamed) ? renamed : k, v);
        }
    }
}
