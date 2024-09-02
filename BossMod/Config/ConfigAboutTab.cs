using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Diagnostics;

namespace BossMod;

public sealed class ConfigAboutTab
{
    // Colors
    private readonly Color DiscordColor = Color.FromComponents(88, 101, 242);

    public void Draw()
    {
        using var wrap = ImRaii.TextWrapPos(0);
        Section("", "Boss Mod (vbm) provides boss fight radar, auto-rotation, cooldown planning, and AI. All of the its modules can be toggled individually. Support for it can be found in the Discord server linked at the bottom of this tab.");
        ImGui.NewLine();
        Section("Radar", "The radar provides an on-screen window that contains area mini-map that shows player positions, boss position(s), various imminent AoEs, and other mechanics. This is useful because you don't have to remember what ability names mean and you can see exactly whether you're getting clipped by incoming AoE or not. It is only enabled for supported duties, which are visible in the \"Supported duties\" tab.");
        ImGui.NewLine();
        Section("Autorotation", "Autorotation will execute fully optimal rotations to the best of its ability. When you go to the \"Autorotation Presets\" tab to create a preset, the maturity of each rotation module is present in a tooltip. A guide for using this feature can be found on the project's GitHub wiki, which is linked at the bottom of this tab.");
        ImGui.NewLine();
        Section("CD Planner", "As long as a boss has a module supporting it, a CD plan can be created for it. This feature replaces autorotations in specific fights and allows you to time specific abilities to cast at specific times. A guide for using this feature can be found on the project's GitHub wiki, which is linked at the bottom of this tab.");
        ImGui.NewLine();
        Section("AI", "The AI module was created to automate movement during boss fights. It can be hooked by other plugins to automate entire duties, or be used on its own to make a fight easier. It will automatically move your character based on safe zones determined by a boss's module, which are visible on the radar.");

        ImGui.NewLine();
        using (ImRaii.PushColor(ImGuiCol.Button, DiscordColor.ABGR))
            LinkButton("Puni.sh Discord", "https://discord.gg/punishxiv");

        ImGui.SameLine();
        LinkButton("Boss Mod Repository", "https://github.com/awgil/ffxiv_bossmod");

        ImGui.SameLine();
        LinkButton("Boss Mod Wiki Tutorials", "https://github.com/awgil/ffxiv_bossmod/wiki");
    }

    private void Section(string title, string text)
    {
        if (title.Length > 0)
        {
            // TODO: Make a separate and larger font for the headings
            using var headingFont = ImRaii.PushFont(ImGui.GetFont());
            ImGui.TextUnformatted(title);
        }
        ImGui.TextUnformatted(text);
    }

    private void LinkButton(string label, string url)
    {
        if (ImGui.Button(label))
            Process.Start("explorer.exe", url);
    }
}
