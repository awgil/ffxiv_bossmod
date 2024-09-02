using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.IO;

namespace BossMod;

public sealed class ConfigAboutTab
{
    private static readonly Vector4 titleColor = new Vector4(1.0f, 0.65f, 0.0f, 1.0f);
    private static readonly Vector4 textColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
    private static readonly Vector4 sectionBgColor = new Vector4(0.15f, 0.15f, 0.15f, 1.0f);
    private static readonly Vector4 borderColor = new Vector4(0.7f, 0.7f, 0.7f, 0.8f);
    private static readonly Vector4 discordButtonColor = new Vector4(0.34f, 0.44f, 0.88f, 1.0f);
    private static readonly Vector4 buttonColor = new Vector4(0.2f, 0.5f, 0.8f, 1.0f);
    private static readonly ImFontPtr titleFont = ImGui.GetIO().Fonts.AddFontFromFileTTF("your-font-path.ttf", 28.0f);

    // Class-level variable to store the last error message
    private static string lastErrorMessage = string.Empty;

    public static void Draw()
    {
        ImGui.TextWrapped("Boss Mod (vbm) provides boss fight radar, auto-rotation, cooldown planning, and AI. All of its modules can be toggled individually. Support for it can be found in the Discord server linked at the bottom of this tab.");
        ImGui.Spacing();

        DrawSection("Radar", new[]
        {
            "Provides an on-screen window that contains an area mini-map showing player positions, boss position(s), various imminent AoEs, and other mechanics.",
            "Useful because you don't have to remember what ability names mean.",
            "See exactly whether you're getting clipped by incoming AoE or not.",
            "Enabled for supported duties, visible in the \"Supported Duties\" tab."
        });

        DrawSection("Autorotation", new[]
        {
            "Executes fully optimal rotations to the best of its ability.",
            "Go to the \"Autorotation Presets\" tab to create a preset.",
            "Maturity of each rotation module is present in a tooltip.",
            "Guide for using this feature can be found on the project's GitHub wiki."
        });

        DrawSection("CD Planner", new[]
        {
            "Creates a CD plan for bosses with supporting modules.",
            "Replaces autorotations in specific fights.",
            "Allows you to time specific abilities to cast at specific times.",
            "Guide for using this feature can be found on the project's GitHub wiki."
        });

        DrawSection("AI", new[]
        {
            "Should not be used in any group content.",
            "Automates movement during boss fights.",
            "Can be hooked by other plugins to automate entire duties.",
            "Automatically moves your character based on safe zones determined by a boss's module, visible on the radar."
        });

        DrawSection("Replays", new[]
        {
            "Useful for creating boss modules, analyzing problems with them, and making CD plans.",
            "When asking for help, make sure to provide a replay! Please note that replays will contain your player name!",
            "Enabled in Settings > Show replay management UI (or enable auto recording).",
            "Files are located in %%appdata%%\\XIVLauncher\\pluginConfigs\\BossMod\\replays."
        });

        ImGui.Spacing();

        float buttonWidth = 180f;
        DrawDiscordButton("Puni.sh Discord", "https://discord.gg/punishxiv", buttonWidth);
        ImGui.SameLine();
        DrawButton("Boss Mod Repository", "https://github.com/awgil/ffxiv_bossmod", buttonWidth);
        ImGui.SameLine();
        DrawButton("Boss Mod Wiki Tutorials", "https://github.com/awgil/ffxiv_bossmod/wiki", buttonWidth);
        ImGui.SameLine();
        DrawOpenReplayFolderButton("Open Replay Folder", buttonWidth);

        if (!string.IsNullOrEmpty(lastErrorMessage))
        {
            ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), lastErrorMessage);
        }
    }

    private static void DrawSection(string title, string[] bulletPoints)
    {
        ImGui.PushStyleColor(ImGuiCol.ChildBg, sectionBgColor);
        ImGui.PushStyleColor(ImGuiCol.Border, borderColor);
        ImGui.BeginChild(title, new Vector2(0, 150), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysUseWindowPadding);

        ImGui.PushFont(titleFont);
        ImGui.PushStyleColor(ImGuiCol.Text, titleColor);
        ImGui.TextUnformatted(title);
        ImGui.PopStyleColor();
        ImGui.PopFont();

        ImGui.Separator();

        ImGui.PushStyleColor(ImGuiCol.Text, textColor);
        foreach (var point in bulletPoints)
        {
            ImGui.Bullet();
            ImGui.SameLine();
            ImGui.TextWrapped(point);
        }
        ImGui.PopStyleColor();

        ImGui.EndChild();
        ImGui.PopStyleColor(2);

        ImGui.Spacing();
    }

    private static void DrawButton(string label, string url, float width)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Lerp(buttonColor, Vector4.One, 0.2f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Lerp(buttonColor, Vector4.One, 0.4f));
        if (ImGui.Button(label, new Vector2(width, 0)))
        {
            OpenUrl(url);
        }
        ImGui.PopStyleColor(3);
    }

    private static void DrawDiscordButton(string label, string url, float width)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, discordButtonColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Lerp(discordButtonColor, Vector4.One, 0.2f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Lerp(discordButtonColor, Vector4.One, 0.4f));
        if (ImGui.Button(label, new Vector2(width, 0)))
        {
            OpenUrl(url);
        }
        ImGui.PopStyleColor(3);
    }

    private static void DrawOpenReplayFolderButton(string label, float width)
    {
        string replayPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"XIVLauncher\pluginConfigs\BossMod\replays");

        ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Lerp(buttonColor, Vector4.One, 0.2f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Lerp(buttonColor, Vector4.One, 0.4f));
        if (ImGui.Button(label, new Vector2(width, 0)))
        {
            OpenFolder(replayPath);
        }
        ImGui.PopStyleColor(3);
    }

    private static void OpenUrl(string url)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
            lastErrorMessage = string.Empty; 
        }
        catch
        {
            lastErrorMessage = "Failed to open URL.";
        }
    }

    private static void OpenFolder(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                };
                Process.Start(psi);
                lastErrorMessage = string.Empty; 
            }
            else
            {
                lastErrorMessage = "Replay folder not found.";
            }
        }
        catch
        {
            lastErrorMessage = "Failed to open folder.";
        }
    }
}
