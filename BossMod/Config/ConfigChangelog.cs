using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Reflection;

namespace BossMod;

public sealed record class VersionedField(ConfigNode Node, FieldInfo FieldInfo, Version AddedVersion)
{
    public string FieldKey => $"{Node}.{FieldInfo.Name}";
}

abstract class ChangelogNotice
{
    public abstract Version Since { get; }
    public abstract void Draw();
}

class AINotice1 : ChangelogNotice
{
    public override Version Since => new(0, 0, 0, 253);

    private void Bullet(string txt)
    {
        ImGui.Bullet();
        ImGui.SameLine();
        ImGui.TextWrapped(txt);
    }

    public override void Draw()
    {
        ImGui.TextUnformatted("The following AI configuration options have been removed:");
        Bullet("AI Preset: AI mode will no longer attempt to control which preset is active, and the presence or absence of a preset will not affect targeting behavior.");
        Bullet("Override positional");
        Bullet("Override follow range");
        Bullet("Follow target: This option had no effect.");
        Bullet("Follow during active boss module");
        Bullet("Follow during combat");
        Bullet("Follow out of combat");
        Bullet("Show advanced options in the UI");
        ImGui.Spacing();
        ImGui.TextUnformatted("The following AI configuration options have been renamed:");
        Bullet("Forbid movement: renamed to Disable movement. Behavior is unchanged.");
        Bullet("Forbid actions: renamed to Disable auto-target. Behavior is unchanged.");
    }
}

public class ConfigChangelogWindow : UIWindow
{
    private readonly Version PreviousVersion;
    private readonly List<VersionedField> Fields;
    private readonly List<ChangelogNotice> Notices;

    private int StuffCount => Fields.Count + Notices.Count;

    public ConfigChangelogWindow() : base("VBM Changelog", true, new(400, 300))
    {
        PreviousVersion = GetPreviousPluginVersion();
        Service.Config.AssemblyVersion = GetCurrentPluginVersion();
        if (Service.Config.AssemblyVersion != PreviousVersion)
        {
            Service.Config.Modified.Fire();
            Fields = GetAllFields().Where(f => f.AddedVersion > PreviousVersion).ToList();
            Notices = GetNotices().Where(f => f.Since > PreviousVersion).ToList();
        }
        else
        {
            Fields = [];
            Notices = [];
        }

        if (StuffCount == 0)
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

        foreach (var n in Notices)
        {
            using var id = ImRaii.PushId($"notice{n.GetType()}");
            n.Draw();
            if (ImGui.Button("OK"))
                postIteration += () => Acknowledge(n);

            ImGui.Separator();
        }

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

    private void Acknowledge(ChangelogNotice n)
    {
        Notices.Remove(n);
        if (StuffCount == 0)
            IsOpen = false;
    }

    private void SetOption(VersionedField field, bool value)
    {
        field.FieldInfo.SetValue(field.Node, value);
        Service.Config.Modified.Fire();

        Fields.Remove(field);
        if (StuffCount == 0)
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

    private static IEnumerable<ChangelogNotice> GetNotices()
    {
        foreach (var t in Utils.GetDerivedTypes<ChangelogNotice>(Assembly.GetExecutingAssembly()).Where(t => !t.IsAbstract))
        {
            var inst = Activator.CreateInstance(t);
            if (inst != null)
                yield return (ChangelogNotice)inst;
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
