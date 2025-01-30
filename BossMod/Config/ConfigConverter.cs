using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BossMod;

public static class ConfigConverter
{
    public static readonly VersionedJSONSchema Schema = BuildSchema();

    private static VersionedJSONSchema BuildSchema()
    {
        var res = new VersionedJSONSchema();
        res.Converters.Add((j, _, _) => j); // v1: moved BossModuleConfig children to special encounter config node; use type names as keys - do nothing, next converter takes care of it
        res.Converters.Add((j, v, _) => // v2: flat structure (config root contains all nodes)
        {
            JsonObject newPayload = [];
            ConvertV1GatherChildren(newPayload, j.AsObject(), v == 0);
            return newPayload;
        });
        res.Converters.Add((j, _, _) => // v3: modified namespaces for old modules
        {
            j.AsObject().TryRenameNode("BossMod.Endwalker.P1S.P1SConfig", "BossMod.Endwalker.Savage.P1SErichthonios.P1SConfig");
            j.AsObject().TryRenameNode("BossMod.Endwalker.P4S2.P4S2Config", "BossMod.Endwalker.Savage.P4S2Hesperos.P4S2Config");
            return j;
        });
        res.Converters.Add((j, _, _) => // v4: cooldown plans moved to encounter configs
        {
            if (j["BossMod.CooldownPlanManager"]?["Plans"] is JsonObject plans)
            {
                foreach (var (k, planData) in plans)
                {
                    var oid = uint.Parse(k);
                    var info = BossModuleRegistry.FindByOID(oid);
                    var config = info?.PlanLevel > 0 ? info.ConfigType : null;
                    if (config?.FullName == null)
                        continue;

                    if (j[config.FullName] is not JsonObject node)
                        j[config.FullName] = node = [];
                    node["CooldownPlans"] = planData;
                }
            }
            j.AsObject().Remove("BossMod.CooldownPlanManager");
            return j;
        });
        res.Converters.Add((j, _, _) => // v5: bloodwhetting -> raw intuition in cd planner, to support low-level content
        {
            foreach (var (k, config) in j.AsObject())
                if (config?["CooldownPlans"]?["WAR"]?["Available"] is JsonArray plans)
                    foreach (var plan in plans)
                        if (plan!["PlanAbilities"] is JsonObject planAbilities)
                            planAbilities.TryRenameNode(ActionID.MakeSpell(WAR.AID.Bloodwhetting).Raw.ToString(), ActionID.MakeSpell(WAR.AID.RawIntuition).Raw.ToString());
            return j;
        });
        res.Converters.Add((j, _, _) => // v6: new cooldown planner
        {
            foreach (var (k, config) in j.AsObject())
            {
                if (config?["CooldownPlans"] is not JsonObject plans)
                    continue;
                bool isTEA = k == typeof(Shadowbringers.Ultimate.TEA.TEAConfig).FullName;
                foreach (var (cls, planList) in plans)
                {
                    if (planList?["Available"] is not JsonArray avail)
                        continue;
                    var c = Enum.Parse<Class>(cls);
                    foreach (var plan in avail)
                    {
                        if (plan?["PlanAbilities"] is not JsonObject abilities)
                            continue;

                        var actions = new JsonArray();
                        foreach (var (aidRaw, aidData) in abilities)
                        {
                            if (aidData is not JsonArray aidList)
                                continue;

                            var aid = new ActionID(uint.Parse(aidRaw));
                            // hack revert, out of config modules existing before v6 only TEA could use raw intuition instead of BW
                            if (!isTEA && aid.ID == (uint)WAR.AID.RawIntuition)
                                aid = ActionID.MakeSpell(WAR.AID.Bloodwhetting);

                            foreach (var abilUse in aidList)
                            {
                                abilUse!["ID"] = Utils.StringToIdentifier(aid.Name());
                                abilUse["StateID"] = $"0x{abilUse["StateID"]?.GetValue<uint>():X8}";
                                actions.Add(abilUse);
                            }
                        }
                        var jplan = (JsonObject)plan!;
                        jplan.Remove("PlanAbilities");
                        jplan["Actions"] = actions;
                    }
                }
            }
            return j;
        });
        res.Converters.Add((j, _, _) => // v7: action manager refactor
        {
            var amConfig = j["BossMod.ActionManagerConfig"] = new JsonObject();
            var autorotConfig = j["BossMod.AutorotationConfig"];
            amConfig["RemoveAnimationLockDelay"] = autorotConfig?["RemoveAnimationLockDelay"] ?? false;
            amConfig["PreventMovingWhileCasting"] = autorotConfig?["PreventMovingWhileCasting"] ?? false;
            amConfig["RestoreRotation"] = autorotConfig?["RestoreRotation"] ?? false;
            amConfig["GTMode"] = autorotConfig?["GTMode"] ?? "Manual";
            return j;
        });
        res.Converters.Add((j, _, _) => j); // v8: remove accidentally serializable Modified field
        res.Converters.Add((j, _, _) => // v9: and again the same thing...
        {
            foreach (var (_, config) in j.AsObject())
                if (config is JsonObject jconfig)
                    jconfig.Remove("Modified");
            return j;
        });
        res.Converters.Add((j, _, f) => // v10: autorotation v2: moved configs around and importantly moved cdplans outside
        {
            j.AsObject().TryRenameNode("BossMod.ActionManagerConfig", "BossMod.ActionTweaksConfig");
            j.AsObject().TryRenameNode("BossMod.AutorotationConfig", "BossMod.Autorotation.AutorotationConfig");
            ConvertV9Plans(j.AsObject(), f.Directory!);
            return j;
        });
        return res;
    }

    private static void ConvertV1GatherChildren(JsonObject result, JsonObject json, bool isV0)
    {
        if (json["__children__"] is not JsonObject children)
            return;
        foreach ((var childTypeName, var jChild) in children)
        {
            if (jChild is not JsonObject jChildObj)
                continue;

            string realTypeName = isV0 ? (jChildObj["__type__"]?.ToString() ?? childTypeName) : childTypeName;
            ConvertV1GatherChildren(result, jChildObj, isV0);
            result.Add(realTypeName, jChild);
        }
        json.Remove("__children__");
    }

    private static void ConvertV9Plans(JsonObject payload, DirectoryInfo dir)
    {
        var dbRoot = new DirectoryInfo(dir.FullName + "/BossMod/autorot/plans");
        if (!dbRoot.Exists)
            dbRoot.Create();
        using var manifestStream = new FileStream(dbRoot + ".manifest.json", FileMode.Create, FileAccess.Write, FileShare.Read);
        using var manifest = Serialization.WriteJson(manifestStream);
        manifest.WriteStartObject();
        manifest.WriteNumber("version", 0);
        manifest.WriteStartObject("payload");
        foreach (var (ct, cfg) in payload.AsObject())
        {
            if (!cfg!.AsObject().TryRemoveNode("CooldownPlans", out var cdplans))
                continue;
            var t = ct[..^6];
            var type = Type.GetType(t);
            manifest.WriteStartObject(t);
            foreach (var (cls, plans) in cdplans!.AsObject())
            {
                manifest.WriteStartObject(cls);
                if (plans!.AsObject().TryGetPropertyValue("SelectedIndex", out var jsel))
                    manifest.WriteNumber("SelectedIndex", jsel!.GetValue<int>());
                manifest.WriteStartArray("Plans");
                foreach (var plan in plans["Available"]!.AsArray())
                {
                    var guid = Guid.NewGuid().ToString();
                    manifest.WriteStringValue(guid);

                    var oplan = plan!.AsObject();
                    var utilityTracks = ConvertV9ActionsToUtilityTracks(oplan);
                    var rotationTracks = ConvertV9StrategiesToRotationTracks(oplan);
                    if (rotationTracks.Remove("Special", out var lb))
                        utilityTracks["LB"] = lb;

                    using var planStream = new FileStream($"{dbRoot}/{guid}.json", FileMode.Create, FileAccess.Write, FileShare.Read);
                    using var jplan = Serialization.WriteJson(planStream);
                    jplan.WriteStartObject();
                    jplan.WriteNumber("version", 0);
                    jplan.WriteStartObject("payload");
                    jplan.WriteString("Name", plan!["Name"]!.GetValue<string>());
                    jplan.WriteString("Encounter", t);
                    jplan.WriteString("Class", cls);
                    jplan.WriteNumber("Level", type != null ? BossModuleRegistry.FindByType(type)?.PlanLevel ?? 0 : 0);
                    jplan.WriteStartArray("PhaseDurations");
                    foreach (var d in plan["Timings"]!["PhaseDurations"]!.AsArray())
                        jplan.WriteNumberValue(d!.GetValue<float>());
                    jplan.WriteEndArray();
                    jplan.WriteStartObject("Modules");
                    ConvertV9WriteTrack(jplan, $"BossMod.Autorotation.Class{cls}Utility", utilityTracks);
                    ConvertV9WriteTrack(jplan, $"BossMod.Autorotation.Legacy.Legacy{cls}", rotationTracks);
                    jplan.WriteEndObject();
                    if (oplan.TryGetPropertyValue("Targets", out var jtargets))
                    {
                        jplan.WriteStartArray("Targeting");
                        foreach (var target in jtargets!.AsArray())
                        {
                            var jt = target!.AsObject();
                            if (jt.TryRemoveNode("OID", out var oid))
                            {
                                jt["Target"] = "EnemyByOID";
                                jt["TargetParam"] = int.Parse(oid!.GetValue<string>()[2..], System.Globalization.NumberStyles.HexNumber);
                            }
                            jt.WriteTo(jplan);
                        }
                        jplan.WriteEndArray();
                    }
                    jplan.WriteEndObject();
                    jplan.WriteEndObject();
                }
                manifest.WriteEndArray();
                manifest.WriteEndObject();
            }
            manifest.WriteEndObject();
        }
        manifest.WriteEndObject();
        manifest.WriteEndObject();
    }

    private static Dictionary<string, List<JsonObject>> ConvertV9ActionsToUtilityTracks(JsonObject plan)
    {
        Dictionary<string, List<JsonObject>> tracks = [];
        if (!plan.TryGetPropertyValue("Actions", out var actions))
            return tracks;
        foreach (var action in actions!.AsArray())
        {
            var aobj = action!.AsObject();
            aobj["Option"] = "Use";
            if (aobj.TryRemoveNode("LowPriority", out var jprio))
                aobj.Add("PriorityOverride", jprio!.GetValue<bool>() ? ActionQueue.Priority.Low : ActionQueue.Priority.High);
            if (aobj.TryRemoveNode("Target", out var jtarget))
            {
                switch (jtarget!["Type"]!.GetValue<string>()!)
                {
                    case "Self":
                        aobj["Target"] = "Self";
                        break;
                    case "EnemyByOID":
                        aobj["Target"] = "EnemyByOID";
                        aobj["TargetParam"] = jtarget["OID"]!.GetValue<int>();
                        break;
                    case "LowestHPPartyMember":
                        aobj["Target"] = "PartyWithLowestHP";
                        aobj["TargetParam"] = jtarget["AllowSelf"]!.GetValue<bool>() ? 1 : 0;
                        break;
                }
            }
            if (aobj.TryRemoveNode("ID", out var jid))
            {
                var id = jid!.GetValue<string>();
                switch (id)
                {
                    case "Bloodwhetting":
                        id = "BW";
                        aobj["Option"] = "BW";
                        break;
                    case "RawIntuition":
                        id = "BW";
                        aobj["Option"] = "RI";
                        break;
                    case "NascentFlash":
                        id = "BW";
                        aobj["Option"] = "NF";
                        break;
                }
                tracks.GetOrAdd(id).Add(aobj);
            }
        }
        return tracks;
    }

    private static Dictionary<string, List<JsonObject>> ConvertV9StrategiesToRotationTracks(JsonObject plan)
    {
        Dictionary<string, List<JsonObject>> tracks = [];
        if (!plan.TryGetPropertyValue("Strategies", out var strategies))
            return tracks;
        foreach (var (track, values) in strategies!.AsObject())
        {
            var t = tracks[track] = [.. values!.AsArray().Select(n => n!.AsObject())];
            foreach (var v in t)
                if (v.TryRemoveNode("Value", out var jv))
                    v["Option"] = jv;
        }
        return tracks;
    }

    private static void ConvertV9WriteTrack(Utf8JsonWriter writer, string module, Dictionary<string, List<JsonObject>> tracks)
    {
        writer.WriteStartObject(module);
        foreach (var (tn, td) in tracks)
        {
            writer.WriteStartArray(tn);
            foreach (var d in td)
            {
                d.WriteTo(writer);
            }
            writer.WriteEndArray();
        }
        writer.WriteEndObject();
    }
}
