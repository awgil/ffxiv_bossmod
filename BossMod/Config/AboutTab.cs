using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using System.Diagnostics;
using System.IO;

namespace BossMod;

public sealed class AboutTab(DirectoryInfo? replayDir)
{
    private readonly Color DiscordColor = Color.FromComponents(88, 101, 242);

    private string _lastErrorMessage = "";

    public void Draw()
    {
        using var wrap = ImRaii.TextWrapPos(0);

        ImGui.TextUnformatted("Boss Mod (vbm) provides boss fight radar, auto-rotation, cooldown planning, and AI. All of its modules can be toggled individually. Support for it can be found in the Discord server linked at the bottom of this tab.");

        if (ImGui.Button("Show changelog"))
            Service.Mediator.Publish(new OpenWindowMessage(typeof(ConfigChangelogWindow)));
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled($"Version {typeof(AboutTab).Assembly.GetName().Version}");

        ImGui.Spacing();
        DrawSection("Radar",
        [
            "Provides an on-screen window that contains an area mini-map showing player positions, boss position(s), various imminent AoEs, and other mechanics.",
            "Useful because you don't have to remember what ability names mean.",
            "See exactly whether you're getting clipped by incoming AoE or not.",
            "Enabled for supported bosses, visible in the \"Supported Bosses\" tab.",
        ]);
        ImGui.Spacing();
        DrawSection("Autorotation",
        [
            "Executes fully optimal rotations to the best of its ability.",
            "Go to the \"Autorotation Presets\" tab to create a preset.",
            "Maturity of each rotation module is present in a tooltip.",
            "Guide for using this feature can be found on the project's GitHub wiki.",
        ]);
        ImGui.Spacing();
        DrawSection("CD Planner",
        [
            "Creates a CD plan for supported bosses.",
            "Replaces autorotations in specific fights.",
            "Allows you to time specific abilities to cast at specific times.",
            "Guide for using this feature can be found on the project's GitHub wiki.",
        ]);
        ImGui.Spacing();
        DrawSection("AI",
        [
            "Automates movement during boss fights.",
            "Automatically moves your character based on safe zones determined by a boss's module, visible on the radar.",
            "Should not be used in any group content.",
            "Can be hooked by other plugins to automate entire duties.",
        ]);
        ImGui.Spacing();
        DrawSection("Replays",
        [
            "Useful for creating boss modules, analyzing problems with them, and making CD plans.",
            "When asking for help, make sure to provide a replay! Please note that replays will contain your player name!",
            "Enabled in Settings > Show replay management UI (or enable auto recording).",
            $"Files are located in '{replayDir}'.",
        ]);
        ImGui.Spacing();
        ImGui.Spacing();

        using (ImRaii.PushColor(ImGuiCol.Button, DiscordColor.ABGR))
            if (ImGui.Button("Puni.sh Discord"))
                _lastErrorMessage = OpenLink("https://discord.gg/Zzrcc8kmvy");
        UIMisc.SameLineOrWrap(UIMisc.ButtonWidth("Boss Mod Repository"));
        if (ImGui.Button("Boss Mod Repository"))
            _lastErrorMessage = OpenLink("https://github.com/awgil/ffxiv_bossmod");
        UIMisc.SameLineOrWrap(UIMisc.ButtonWidth("Boss Mod Wiki Tutorials"));
        if (ImGui.Button("Boss Mod Wiki Tutorials"))
            _lastErrorMessage = OpenLink("https://github.com/awgil/ffxiv_bossmod/wiki");
        UIMisc.SameLineOrWrap(UIMisc.ButtonWidth("Open Replay Folder"));
        if (ImGui.Button("Open Replay Folder") && replayDir != null)
            _lastErrorMessage = OpenDirectory(replayDir);

        if (_lastErrorMessage.Length > 0)
            UIMisc.Banner(FontAwesomeIcon.ExclamationTriangle, _lastErrorMessage, UIMisc.ColorDanger);
    }

    private static void DrawSection(string title, string[] bulletPoints)
    {
        UIMisc.SectionHeader(title);
        foreach (var point in bulletPoints)
        {
            ImGui.Bullet();
            ImGui.SameLine();
            ImGui.TextUnformatted(point);
        }
    }

    private string OpenLink(string link)
    {
        try
        {
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening link {link}: {e}");
            return $"Failed to open link '{link}', open it manually in the browser.";
        }
    }

    private string OpenDirectory(DirectoryInfo dir)
    {
        if (!dir.Exists)
            return $"Directory '{dir}' not found.";

        try
        {
            Process.Start(new ProcessStartInfo(dir.FullName) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening directory {dir}: {e}");
            return $"Failed to open folder '{dir}', open it manually.";
        }
    }
}