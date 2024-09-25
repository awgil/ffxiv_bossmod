using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Reflection;

namespace BossMod.Config;

public sealed class ChangelogProperties : ConfigNode
{
    public Version BossModVersion { get; set; } = new(0, 0, 0, 0);
}

public class ChangelogWindow(Version old, List<VersionedField> fields) : UIWindow("VBM Changelog", false, new(400, 300))
{
    private readonly List<string> ProcessedFields = [];

    private void SetOption(VersionedField field, bool value)
    {
        field.FieldInfo.SetValue(field.Node, value);
        ProcessedFields.Add(field.FieldKey);
        if (ProcessedFields.Count >= fields.Count)
        {
            IsOpen = false;
            Service.Config.Modified.Fire();
        }
    }

    public override void Draw()
    {
        ImGui.TextUnformatted($"The following config options have been added since version {old}:");

        ImGui.Separator();

        foreach (var group in fields.Where(f => !ProcessedFields.Contains(f.FieldKey)).GroupBy(f => f.Node.GetType()))
        {
            ImGui.TextUnformatted(group.Key.GetCustomAttribute<ConfigDisplayAttribute>()?.Name ?? "");
            foreach (var f in group)
            {
                using var id = ImRaii.PushId($"changelog{f.FieldKey}");

                var disp = f.FieldInfo.GetCustomAttribute<PropertyDisplayAttribute>();

                ImGui.Bullet();
                if (!string.IsNullOrEmpty(disp?.Tooltip))
                    UIMisc.HelpMarker(disp!.Tooltip);
                ImGui.SameLine();
                ImGui.TextUnformatted(disp?.Label ?? "unknown");
                ImGui.SameLine();
                if (ImGui.Button("Enable"))
                {
                    SetOption(f, true);
                    return;
                }
                ImGui.SameLine();
                if (ImGui.Button("Disable"))
                {
                    SetOption(f, false);
                    return;
                }
            }
        }
    }
}

public record VersionedField(ConfigNode Node, FieldInfo FieldInfo, Version AddedVersion)
{
    public string FieldKey => $"{Node}.{FieldInfo.Name}";
}

public static class ChangelogDisplay
{
    private static Version GetCurrentPluginVersion()
    {
#if DEBUG
        // version is always 0.0.0.0 in debug, making it useless for testing
        return new(0, 0, 0, 999);
#endif
        return Assembly.GetExecutingAssembly().GetName().Version!;
    }

    private static Version GetPreviousPluginVersion(ChangelogProperties props)
    {
#if DEBUG
        return new(0, 0, 0, 0);
#endif
        return props.BossModVersion;
    }

    private static IEnumerable<VersionedField> GetAllFields()
    {
        foreach (var n in Service.Config.Nodes)
        {
            var sinceNode = n.GetType().GetCustomAttribute<ConfigDisplayAttribute>()?.Since;

            foreach (var f in n.GetType().GetFields())
            {
                // i don't feel like supporting non bool fields
                if (f.FieldType != typeof(bool))
                    continue;

                if (sinceNode != null)
                    yield return new(n, f, Version.Parse(sinceNode));

                else if (f.GetCustomAttribute<PropertyDisplayAttribute>()?.Since is string sinceVersion)
                    yield return new(n, f, Version.Parse(sinceVersion));
            }
        }
    }

    public static void UpdateAndNotifyUser()
    {
        var props = Service.Config.Get<ChangelogProperties>();
        var previousVersion = GetPreviousPluginVersion(props);
        var currentVersion = GetCurrentPluginVersion();

        props.BossModVersion = currentVersion;

        var fields = GetAllFields().Where(f => f.AddedVersion > previousVersion).ToList();
        if (fields.Count > 0)
        {
            var win = new ChangelogWindow(previousVersion, fields)
            {
                IsOpen = true
            };
        }
    }
}
