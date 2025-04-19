using System.IO;
using System.Text.Json;

namespace BossMod.Autorotation;

// note: plans in the database are immutable (otherwise eg. manager won't see the changes in active plan)
public sealed class PlanDatabase
{
    public record class PlanList
    {
        public int SelectedIndex = -1;
        public List<Plan> Plans = [];
    }

    public readonly Dictionary<Type, Dictionary<Class, PlanList>> Plans = [];
    public Event<Type, Class> ListModifed = new();
    public Event<Plan?, Plan?> PlanModified = new(); // (old, new); old == null if plan is added, new == null if plan is removed

    private readonly FileInfo _manifestPath;
    private readonly DirectoryInfo _planStore;

    public PlanDatabase(string rootPath)
    {
        _manifestPath = new(rootPath + ".manifest.json");
        _planStore = new(rootPath);
        if (!_planStore.Exists)
            _planStore.Create();

        // first load all actually available plans
        Dictionary<string, Plan> foundPlans = [];
        var serOptions = Serialization.BuildSerializationOptions();
        foreach (var f in _planStore.EnumerateFiles("*.json"))
        {
            try
            {
                var data = PlanPresetConverter.PlanSchema.Load(f);
                using var json = data.document;
                var plan = data.payload.Deserialize<Plan>(serOptions);
                if (plan != null)
                {
                    plan.Guid = f.Name[..^5];
                    foundPlans[plan.Guid] = plan;
                }
            }
            catch (Exception ex)
            {
                Service.Log($"Failed to parse plan '{f.FullName}': {ex}");
            }
        }

        // now load the manifest, and add referenced plans to the database
        if (_manifestPath.Exists)
        {
            try
            {
                using var json = Serialization.ReadJson(_manifestPath.FullName);
                var version = json.RootElement.GetProperty("version").GetInt32();
                var payload = json.RootElement.GetProperty("payload");
                foreach (var enc in payload.EnumerateObject())
                {
                    var encType = Type.GetType(enc.Name);
                    var encInfo = encType != null ? BossModuleRegistry.FindByType(encType) : null;
                    if (encInfo == null)
                    {
                        Service.Log($"Error while deserializing plan database: failed to find encounter {enc.Name}");
                        continue;
                    }

                    var encData = Plans[encType!] = [];
                    foreach (var cls in enc.Value.EnumerateObject())
                    {
                        var job = Enum.Parse<Class>(cls.Name);
                        var planList = encData[job] = new();
                        planList.SelectedIndex = cls.Value.GetProperty(nameof(PlanList.SelectedIndex)).GetInt32();
                        foreach (var planRef in cls.Value.GetProperty(nameof(PlanList.Plans)).EnumerateArray())
                        {
                            var planGuid = planRef.GetString() ?? "";
                            if (!foundPlans.Remove(planGuid, out var plan))
                            {
                                Service.Log($"Error while deserializing plan database: failed to find plan '{planGuid}' for {job} {enc.Name}");
                                if (planList.SelectedIndex == planList.Plans.Count)
                                    planList.SelectedIndex = -1;
                                else if (planList.SelectedIndex > planList.Plans.Count)
                                    --planList.SelectedIndex;
                            }
                            else if (plan.Encounter != encType || plan.Class != job)
                            {
                                Service.Log($"Error while deserializing plan database: plan '{planGuid}' expected for {job} {enc.Name}, but is actually for {plan.Class} {plan.Encounter.FullName}");
                                foundPlans[planGuid] = plan; // add back, so that it's added to proper bucket later
                                if (planList.SelectedIndex == planList.Plans.Count)
                                    planList.SelectedIndex = -1;
                                else if (planList.SelectedIndex > planList.Plans.Count)
                                    --planList.SelectedIndex;
                            }
                            else
                            {
                                planList.Plans.Add(plan);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Service.Log($"Failed to parse plan database '{_manifestPath}': {ex}");
            }
        }

        // finally, add unreferenced plans to the database
        foreach (var plan in foundPlans.Values)
        {
            Service.Log($"Found plan unreferenced by database: '{plan.Guid}' for {plan.Class} {plan.Encounter.FullName}");
            GetPlans(plan.Encounter, plan.Class).Plans.Add(plan);
        }
    }

    public PlanList GetPlans(Type module, Class cls) => Plans.GetOrAdd(module).GetOrAdd(cls); // note: adding is fine, we don't serialize empty lists anyway

    // should be called when plans are added/removed/reordered/selection changed
    public void ModifyManifest(Type encounter, Class cls)
    {
        ListModifed.Fire(encounter, cls);
        try
        {
            using var fstream = new FileStream(_manifestPath.FullName, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var jwriter = Serialization.WriteJson(fstream);
            jwriter.WriteStartObject();
            jwriter.WriteNumber("version", 0);
            jwriter.WriteStartObject("payload");
            foreach (var (enc, encData) in Plans)
            {
                if (encData.All(kv => kv.Value.Plans.Count == 0))
                    continue; // no plans for this encounter

                jwriter.WriteStartObject(enc.FullName!);
                foreach (var (job, planList) in encData)
                {
                    if (planList.Plans.Count == 0)
                        continue; // no plans for this class

                    jwriter.WriteStartObject(job.ToString());
                    jwriter.WriteNumber(nameof(PlanList.SelectedIndex), planList.SelectedIndex);
                    jwriter.WriteStartArray(nameof(PlanList.Plans));
                    foreach (var p in planList.Plans)
                        jwriter.WriteStringValue(p.Guid);
                    jwriter.WriteEndArray();
                    jwriter.WriteEndObject();
                }
                jwriter.WriteEndObject();
            }
            jwriter.WriteEndObject();
            jwriter.WriteEndObject();
            Service.Log($"Manifest saved successfully to '{_manifestPath.FullName}'");
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to write manifest to '{_manifestPath.FullName}': {ex}");
        }
    }

    // either existing or modified should be non-null
    public void ModifyPlan(Plan? existing, Plan? modified)
    {
        if (existing == null)
        {
            // add new plan
            SavePlan(modified!);
            GetPlans(modified!.Encounter, modified.Class).Plans.Add(modified);
            ModifyManifest(modified.Encounter, modified.Class);
        }
        else if (modified == null)
        {
            // delete existing plan
            DeletePlan(existing);
            var plans = Plans[existing.Encounter][existing.Class];
            var index = plans.Plans.IndexOf(existing);
            plans.Plans.RemoveAt(index);
            if (plans.SelectedIndex == index)
                plans.SelectedIndex = -1;
            else if (plans.SelectedIndex > index)
                --plans.SelectedIndex;
            ModifyManifest(existing.Encounter, existing.Class);
        }
        else if (existing.Guid == modified.Guid && existing.Encounter == modified.Encounter && existing.Class == modified.Class)
        {
            // just modify the plan
            SavePlan(modified);
            var plans = Plans[existing.Encounter][existing.Class].Plans;
            var index = plans.IndexOf(existing);
            plans[index] = modified;
        }
        else
        {
            Service.Log("Trying to change guid/encounter/class of a plan, which is not supported");
            return;
        }

        PlanModified.Fire(existing, modified);
    }

    private void SavePlan(Plan plan)
    {
        var filename = $"{_planStore.FullName}/{plan.Guid}.json";
        try
        {
            PlanPresetConverter.PlanSchema.Save(new(filename), jwriter => JsonSerializer.Serialize(jwriter, plan, Serialization.BuildSerializationOptions()));
            Service.Log($"Plan saved successfully to '{filename}'");
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to write database to '{filename}': {ex}");
        }
    }

    private void DeletePlan(Plan plan)
    {
        var filename = $"{_planStore.FullName}/{plan.Guid}.json";
        try
        {
            new FileInfo(filename).Delete();
            Service.Log($"Plan '{filename}' deleted successfully");
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to delete plan '{filename}': {ex}");
        }
    }
}
