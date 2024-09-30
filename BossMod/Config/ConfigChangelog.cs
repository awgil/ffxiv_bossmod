using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Reflection;

namespace BossMod;

public sealed record class VersionedField(ConfigNode Node, FieldInfo FieldInfo, Version AddedVersion)
{
    public string FieldKey => $"{Node}.{FieldInfo.Name}";
}

public class ConfigChangelogWindow : UIWindow
{
    private readonly Version PreviousVersion;
    private readonly List<VersionedField> Fields;

    public ConfigChangelogWindow() : base("VBM Changelog", true, new(400, 300))
    {
        PreviousVersion = GetPreviousPluginVersion();
        Service.Config.AssemblyVersion = GetCurrentPluginVersion();
        if (Service.Config.AssemblyVersion != PreviousVersion)
        {
            Service.Config.Modified.Fire();
            Fields = GetAllFields().Where(f => f.AddedVersion > PreviousVersion).ToList();
        }
        else
        {
            Fields = [];
        }

        if (Fields.Count == 0)
        {
            // nothing interesting to show...
            IsOpen = false;
            Dispose();
        }
    }

    public override void Draw()
    {
        ImGui.TextUnformatted($"The following config options have been added since version {PreviousVersion}:");

        ImGui.Separator();

        Action? postIteration = null;
        foreach (var group in Fields.GroupBy(f => f.Node.GetType()))
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
                    postIteration += () => SetOption(f, true);
                ImGui.SameLine();
                if (ImGui.Button("Disable"))
                    postIteration += () => SetOption(f, false);
            }
        }
        postIteration?.Invoke();
    }

    private void SetOption(VersionedField field, bool value)
    {
        field.FieldInfo.SetValue(field.Node, value);
        Service.Config.Modified.Fire();

        Fields.Remove(field);
        if (Fields.Count == 0)
            IsOpen = false;
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

    private static Version GetCurrentPluginVersion()
    {
#if DEBUG
        // version is always 0.0.0.0 in debug, making it useless for testing
        return new(0, 0, 0, 999);
#else
        return Assembly.GetExecutingAssembly().GetName().Version!;
#endif
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate")]
    private static Version GetPreviousPluginVersion()
    {
#if DEBUG
        // change value to something sensible if you want to test the changelog stuff
        return new(0, 0, 0, 999);
#else
        return Service.Config.AssemblyVersion;
#endif
    }
}
