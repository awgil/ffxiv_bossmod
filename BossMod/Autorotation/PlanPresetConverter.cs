using System.Text.Json.Nodes;

namespace BossMod.Autorotation;

// TODO: waiting for .net9 to complete implementation, since it adds proper API for renaming keys, it's a pita to maintain order otherwise
public static class PlanPresetConverter
{
    // note: we always apply renames _after_ changes - this allows converting old modules to new ones without affecting plans/presets that already use new ones
    private record class TrackChanges(Dictionary<string, string> OptionRenames);
    private record class ModuleChanges(Dictionary<string, TrackChanges> TrackChanges, Dictionary<string, string> TrackRenames);
    private record class ModuleConverter(Dictionary<string, ModuleChanges> ModuleChanges, Dictionary<string, string> ModuleRenames);

    public static readonly VersionedJSONSchema PlanSchema = BuildSchema(true);
    public static readonly VersionedJSONSchema PresetSchema = BuildSchema(false);

    private static VersionedJSONSchema BuildSchema(bool plan)
    {
        var res = new VersionedJSONSchema();
        //AddModuleConverter(res, plan, BuildModuleConverterV1()); // v1: StandardWAR -> VeynVAR rename
        return res;
    }

    //private static void AddModuleConverter(VersionedJSONSchema schema, bool plan, ModuleConverter cvt)
    //{
    //    schema.Converters.Add((j, _, _) =>
    //    {
    //        if (plan)
    //        {
    //            var modules = j!["Modules"]!.AsObject();
    //            foreach (var (moduleName, moduleData) in modules)
    //            {
    //                if (cvt.ModuleChanges.TryGetValue(moduleName, out var moduleChanges))
    //                {
    //                    var tracks = moduleData!.AsObject();
    //                    foreach (var (trackName, trackData) in tracks)
    //                    {
    //                        if (moduleChanges.TrackChanges.TryGetValue(trackName, out var trackChanges))
    //                        {
    //                            foreach (var entry in trackData!.AsArray())
    //                            {
    //                                var optionName = entry!["Option"]!.GetValue<string>();
    //                                if (trackChanges.OptionRenames.TryGetValue(optionName, out var optionNewName))
    //                                    entry["Option"] = optionNewName;
    //                            }
    //                        }
    //                    }
    //                    ApplyRenames(tracks, moduleChanges.TrackRenames);
    //                }
    //            }
    //            ApplyRenames(modules, cvt.ModuleRenames);
    //        }
    //        else
    //        {
    //            foreach (var preset in j.AsArray())
    //            {
    //                var modules = preset!["Modules"]!.AsObject();
    //                foreach (var (moduleName, moduleData) in modules)
    //                {
    //                    if (cvt.ModuleChanges.TryGetValue(moduleName, out var moduleChanges))
    //                    {
    //                        foreach (var entry in moduleData!.AsArray())
    //                        {
    //                            var trackName = entry!["Track"]!.GetValue<string>();
    //                            if (moduleChanges.TrackChanges.TryGetValue(trackName, out var trackChanges))
    //                            {
    //                                var optionName = entry!["Option"]!.GetValue<string>();
    //                                if (trackChanges.OptionRenames.TryGetValue(optionName, out var optionNewName))
    //                                    entry["Option"] = optionNewName;
    //                            }

    //                            if (moduleChanges.TrackRenames.TryGetValue(trackName, out var trackNewName))
    //                                entry["Track"] = trackNewName;
    //                        }
    //                    }
    //                }
    //                ApplyRenames(modules, cvt.ModuleRenames);
    //            }
    //        }
    //        return j;
    //    });
    //}

    //private static void ApplyRenames(JsonObject j, Dictionary<string, string> renames)
    //{
    //    if (renames.Count == 0)
    //        return;

    //    for (int i = 0; i < j.Count; ++i)
    //    {
    //        // TODO: implement...
    //    }
    //}

    //private static ModuleConverter BuildModuleConverterV1()
    //{
    //    Dictionary<string, string> moduleRenames = new()
    //    {
    //        ["BossMod.Autorotation.StandardWAR"] = "BossMod.Autorotation.VeynWAR",
    //    };
    //    return new([], moduleRenames);
    //}
}
