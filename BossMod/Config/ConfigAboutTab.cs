using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using SharpDX;
using System.Diagnostics;

namespace BossMod;

public sealed class ConfigAboutTab
{
    // Colors
    private static readonly System.Numerics.Vector4 discordColor = new(88 / 255f, 101 / 255f, 242 / 255f, 1);

    // Fonts
    // TODO: Make a separate and larger font for the headings
    private static readonly ImFontPtr headingFont = UiBuilder.DefaultFont;

    public static void Draw()
    {
        ImGui.TextWrapped("Boss Mod (vbm) provides boss fight radar, auto-rotation, cooldown planning, and AI. All of the its modules can be toggled individually. Support for it can be found in the Discord server linked at the bottom of this tab.");

        ImGui.PushFont(headingFont);
        ImGui.TextWrapped("Radar");
        ImGui.PopFont();

        ImGui.TextWrapped("The radar provides an on-screen window that contains area mini-map that shows player positions, boss position(s), various imminent AoEs, and other mechanics. This is useful because you don't have to remember what ability names mean and you can see exactly whether you're getting clipped by incoming AoE or not. It is only enabled for supported duties, which are visible in the \"Supported duties\" tab.");

        ImGui.PushFont(headingFont);
        ImGui.TextWrapped("Autorotation");
        ImGui.PopFont();

        ImGui.TextWrapped("Autorotation will execute fully optimal rotations to the best of its ability. When you go to the \"Autorotation Presets\" tab to create a preset, the maturity of each rotation module is present in a tooltip. A guide for using this feature can be found on the project's GitHub wiki, which is linked at the bottom of this tab.");

        ImGui.PushFont(headingFont);
        ImGui.TextWrapped("CD Planner");
        ImGui.PopFont();

        ImGui.TextWrapped("As long as a boss has a module supporting it, a CD plan can be created for it. This feature replaces autorotations in specific fights and allows you to time specific abilities to cast at specific times. A guide for using this feature can be found on the project's GitHub wiki, which is linked at the bottom of this tab.");

        ImGui.PushFont(headingFont);
        ImGui.TextWrapped("AI");
        ImGui.PopFont();

        ImGui.TextWrapped("The AI module was created to automate movement during boss fights. It can be hooked by other plugins to automate entire duties, or be used on its own to make a fight easier. It will automatically move your character based on safe zones determined by a boss's module, which are visible on the radar.");

        ImGui.NewLine();

        ImGui.PushStyleColor(ImGuiCol.Button, discordColor);
        if (ImGui.Button("Puni.sh Discord"))
            Process.Start("explorer.exe", "https://discord.gg/punishxiv");
        ImGui.PopStyleColor();

        ImGui.SameLine();

        if (ImGui.Button("Boss Mod Repository"))
            Process.Start("explorer.exe", "https://github.com/awgil/ffxiv_bossmod");

        ImGui.SameLine();

        if (ImGui.Button("Boss Mod Wiki Tutorials"))
            Process.Start("explorer.exe", "https://github.com/awgil/ffxiv_bossmod/wiki");
    }
}
