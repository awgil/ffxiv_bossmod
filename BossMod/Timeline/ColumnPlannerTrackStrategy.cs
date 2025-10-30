using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

public class ColumnPlannerTrackStrategy(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, StrategyConfigTrack config, int level, BossModuleRegistry.Info? moduleInfo, StrategyValueTrack defaultOverride)
    : ColumnPlannerTrack(timeline, tree, phaseBranches, config.InternalName)
{
    public StrategyValueTrack DefaultOverride { get; private set; } = defaultOverride;

    protected override StrategyValueTrack GetDefaultValue()
    {
        var res = new StrategyValueTrack();
        for (int i = 1; i < config.Options.Count; ++i)
        {
            if (level >= config.Options[i].MinLevel && level <= config.Options[i].MaxLevel)
            {
                res.Option = i;
                break;
            }
        }
        return res;
    }

    public override void Draw()
    {
        base.Draw();

        var mousePos = ImGui.GetMousePos();
        if (!ScreenPosInTrack(mousePos))
            return;

        if (Entries.Any(e => e.EntryType == Entry.Type.Range && ScreenPosInEntry(mousePos, e)))
            return;

        if (DefaultOverride != default)
        {
            Timeline.AddTooltip([config.Options[DefaultOverride.Option].DisplayName]);
            Timeline.AddTooltip([
                "This value applies to the whole encounter, but will be overridden by entries.",
                $"It can be changed by selecting \"{Name}\" at the top of the timeline."
            ]);
        }
    }

    public override void DrawHeader(Vector2 topLeft)
    {
        using var _ = ImRaii.PushId($"{config.OptionEnum}_{config.InternalName}");
        var paddingY = ImGui.GetStyle().ItemSpacing.Y;
        var cursor = ImGui.GetCursorPos();
        var isHovered = false;
        using (ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(0, paddingY)))
        {
            var textSize = ImGui.CalcTextSize(Name);
            var originAdj = topLeft - ImGui.GetWindowPos();
            using (ImRaii.Group())
            {
                ImGui.SetCursorPos(originAdj);
                ImGui.Selectable("", DefaultOverride.Option > 0, ImGuiSelectableFlags.None, textSize with { X = Width });
                isHovered = ImGui.IsItemHovered();
                ImGui.SetCursorPos(originAdj + new Vector2((Width - textSize.X) * 0.5f, 0));
                ImGui.Text(Name);
            }
        }

        if (isHovered && config.InternalName != config.DisplayName && config.DisplayName.Length > 0)
            ImGui.SetTooltip(config.DisplayName);

        using (var popup = ImRaii.Popup("settings"))
        {
            if (popup)
            {
                var def = DefaultOverride;
                if (UIStrategyValue.DrawEditorTrackOption(def, config, level, "Plan default"))
                {
                    DefaultOverride = def;
                    NotifyModified();
                    ImGui.CloseCurrentPopup();
                }
                ImGui.Separator();
                if (ImGui.Selectable("Hide column"))
                    Width = 0;
            }
        }

        if (ImGui.IsItemClicked())
            ImGui.OpenPopup("settings");

        ImGui.SetCursorPos(cursor);
    }

    protected override Color GetBackgroundColor() => DefaultOverride.Option > 0 ? Timeline.Colors.PlannerBackgroundHighlight : Timeline.Colors.PlannerBackground;

    protected override void RefreshElement(Element e)
    {
        if (e.Value is StrategyValueTrack t)
        {
            var opt = config.Options[t.Option];
            e.Window.Color = t.Option > 0 && t.Option <= Timeline.Colors.PlannerWindow.Length ? Timeline.Colors.PlannerWindow[t.Option - 1] : Timeline.Colors.PlannerFallback;
            e.CooldownLength = opt.Cooldown;
            e.EffectLength = opt.Effect;
        }
    }

    protected override List<string> DescribeElement(Element e) => UIStrategyValue.Preview(e.Value, config, moduleInfo);
    protected override bool EditElement(Element e) => UIStrategyValue.DrawEditor(e.Value, config, moduleInfo, level) | EditElementWindow(e);
}
