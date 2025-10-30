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

        res.Converters.Add((j, _, _) =>
        {
            foreach (var m in EnumerateEntriesModules(j, plan))
            {
                if (m.TryGetPropertyValue("BossMod.Autorotation.MiscAI.AutoTarget", out var node))
                {
                    foreach (var obj in node!.AsArray())
                    {
                        if (obj?["Track"]?.AsValue().GetValue<string>() == "General" && obj?["Option"]?.AsValue().GetValue<string>() == "Conservative")
                            obj["Option"] = "Aggressive";
                    }
                }
            }

            return j;
        });

        // fix module name
        res.Converters.Add((j, _, _) =>
        {
            if (plan)
            {
                if (j["Encounter"]?.AsValue().GetValue<string>() == "BossMod.Dawntrail.Savage.M08SHowlingBlade.M08SHowlingBlade")
                    j["Encounter"] = "BossMod.Dawntrail.Savage.RM08SHowlingBlade.RM08SHowlingBlade";
            }

            return j;
        });

        res.Converters.Add((j, _, _) =>
        {
            var optionRenames = Utils.LoadFromAssembly<List<OptionRename>>("BossMod.Autorotation.OptionRenames.json");

            foreach (var m in EnumerateEntriesModules(j, plan))
            {
                foreach (var (modName, opts) in m)
                {
                    if (plan)
                    {
                        foreach (var (trackName, entries) in opts!.AsObject())
                        {
                            if (trackName == "_defaults")
                            {
                                foreach (var (defName, defNode) in entries!.AsObject())
                                {
                                    var defVal = defNode!.GetValue<string>()!;
                                    if (optionRenames.FirstOrNull(r => r.Module == modName && r.Option == defName && r.Before == defVal) is { } rename)
                                        defNode.ReplaceWith(rename.After);
                                }
                                continue;
                            }

                            foreach (var entry in entries!.AsArray())
                            {
                                var optName = entry!["Option"]!.GetValue<string>()!;
                                if (optionRenames.FirstOrNull(r => r.Module == modName && r.Option == trackName && r.Before == optName) is { } rename)
                                    entry["Option"] = rename.After;
                            }
                        }
                    }
                    else
                    {
                        var optsArray = opts!.AsArray();
                        foreach (var optsNode in optsArray)
                        {
                            var optsObject = optsNode!.AsObject()!;

                            if (optsObject.TryGetPropertyValue("Track", out var trackName))
                            {
                                var tn = trackName!.GetValue<string>()!;
                                if (optsObject.TryGetPropertyValue("Option", out var optName))
                                {
                                    var on = optName!.GetValue<string>()!;

                                    if (optionRenames.FirstOrNull(r => r.Module == modName && r.Option == tn && r.Before == on) is { } rename)
                                        optsObject["Option"] = rename.After;
                                }
                            }
                        }
                    }
                }
            }

            return j;
        });

        return res;
    }

    record struct OptionRename(string Module, string Option, string Before, string After);

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
