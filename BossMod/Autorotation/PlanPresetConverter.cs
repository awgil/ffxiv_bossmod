using System.Text.Json.Nodes;

namespace BossMod.Autorotation;

public static class PlanPresetConverter
{
    public static readonly VersionedJSONSchema PlanSchema = BuildSchema(true);
    public static readonly VersionedJSONSchema PresetSchema = BuildSchema(false);

    private static VersionedJSONSchema BuildSchema(bool plan)
    {
        var res = new VersionedJSONSchema();

        // v1: StandardWAR -> VeynVAR rename
        res.Converters.Add((j, _, _) =>
        {
            Dictionary<string, string> moduleRenames = new() { ["BossMod.Autorotation.StandardWAR"] = "BossMod.Autorotation.VeynWAR" };
            foreach (var m in EnumerateEntriesModules(j, plan))
                RenameKeys(m, moduleRenames);
            return j;
        });

        // fix module name
        res.Converters.Add((j, _, _) =>
        {
            if (plan)
            {
                if ((string)j["Encounter"]!.AsValue()! == "BossMod.Dawntrail.Extreme.Ex4Zelenia.Zelenia")
                    j["Encounter"] = "BossMod.Dawntrail.Extreme.Ex4Zelenia.Ex4Zelenia";
            }

            return j;
        });

        // AutoFarm rework
        res.Converters.Add((j, _, _) =>
        {
            foreach (var m in EnumerateEntriesModules(j, plan))
            {
                if (m.TryGetPropertyValue("BossMod.Autorotation.MiscAI.AutoFarm", out var node))
                {
                    List<int> tracksToRemove = [];
                    List<JsonNode> tracksToAdd = [];
                    var i = 0;

                    var arr = node!.AsArray();

                    foreach (var obj in arr)
                    {
                        var cfg = obj!.AsObject();
                        if (cfg["Track"]?.AsValue().TryGetValue<string>(out var track) == true)
                        {
                            if (cfg["Option"]?.AsValue().TryGetValue<string>(out var opt) == true)
                            {
                                if (track == "General" && opt == "FightBack")
                                {
                                    tracksToRemove.Add(i);
                                    tracksToAdd.Add(JsonNode.Parse("""{"Track": "General", "Option": "Passive"}""")!);
                                }

                                if (track == "Target")
                                {
                                    tracksToRemove.Add(i);

                                    if (opt == "FATE")
                                        tracksToAdd.Add(JsonNode.Parse("""{"Track": "FATE", "Option": "Enabled"}""")!);

                                    if (opt == "All")
                                        tracksToAdd.Add(JsonNode.Parse("""{"Track": "Everything", "Option": "Enabled"}""")!);
                                }
                            }
                        }
                        i++;
                    }

                    foreach (var key in tracksToRemove)
                        arr.RemoveAt(key);

                    foreach (var ta in tracksToAdd)
                        arr.Add(ta);

                    m.Remove("BossMod.Autorotation.MiscAI.AutoFarm");
                    m["BossMod.Autorotation.MiscAI.AutoTarget"] = arr;
                }
            }

            return j;
        });

        res.Converters.Add((j, _, _) =>
        {
            foreach (var m in EnumerateEntriesModules(j, plan))
                // replaced by AutoTarget, only user (questionable) has an updated preset now
                m.Remove("BossMod.Autorotation.MiscAI.AutoPull");

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
