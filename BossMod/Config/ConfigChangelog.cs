using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using System.Diagnostics.CodeAnalysis;
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

    protected void Bullet(string txt)
    {
        ImGui.Bullet();
        ImGui.SameLine();
        ImGui.TextWrapped(txt);
    }
}

class AIMigrationNotice : ChangelogNotice
{
    public override Version Since => new(0, 0, 0, 289);

    public override void Draw()
    {
        var link = "https://github.com/awgil/ffxiv_bossmod/wiki/AI-Migration-guide";
        ImGui.TextUnformatted("AI is dead, long live AI!");
        Bullet("AI feature is now deprecated and will be removed in one of the future versions.");
        Bullet("The replacement is simple and much more flexible and powerful.");
        Bullet($"See wiki ({link}) for details.");
        if (ImGui.Button("Open wiki"))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(link) { UseShellExecute = true });
            }
            catch (Exception e)
            {
                Service.Log($"Error opening link: {e}");
            }
        }
    }
}

class MultiPresetNotice : ChangelogNotice
{
    public override Version Since => new(0, 2, 3, 0);

    public override void Draw()
    {
        ImGui.TextWrapped("You can now enable multiple presets at once.");
        Bullet("A new built-in preset has been added - VBM AI. This provides the same functionality as the legacy AI feature. It will try to dodge AOEs and automatically target enemies.");
        Bullet("The existing /vbm ar commands have unchanged behavior. For example, /vbm ar set <preset> will enable the given preset and disable all others. To use multi-preset functionality, you can use the new subcommands 'activate', 'deactivate', and 'togglemulti'.");
    }
}

class DashSafetyNotice : ChangelogNotice
{
    public override Version Since => new(0, 2, 5, 1);

    public override void Draw()
    {
        ImGui.TextWrapped($"The option \"Try to prevent dashing into AOEs\" is now enabled by default. You can disable it in Settings -> Action Tweaks.");
    }
}

public class ConfigChangelogWindow : UIWindow
{
    private readonly Version PreviousVersion;
    private readonly List<VersionedField> Fields = [];
    private readonly List<ChangelogNotice> Notices = [];

    private int StuffCount => Fields.Count + Notices.Count;

    public ConfigChangelogWindow() : base("VBM Changelog", true, new(400, 300))
    {
        PreviousVersion = GetPreviousPluginVersion();
        Service.Config.AssemblyVersion = GetCurrentPluginVersion();
        if (Service.Config.AssemblyVersion != PreviousVersion)
        {
            Service.Config.Modified.Fire();
            Fields = [.. GetAllFields().Where(f => f.AddedVersion > PreviousVersion)];
            Notices = [.. GetNotices().Where(f => f.Since > PreviousVersion)];
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
        return Service.IsDev ? new(999, 0, 0, 0) : Assembly.GetExecutingAssembly().GetName().Version!;
    }

    [SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "fuck it")]
    private static Version GetPreviousPluginVersion()
    {
        // change to a smaller value to test changelog
        return Service.IsDev ? new(999, 0, 0, 0) : Service.Config.AssemblyVersion;
    }
}
