using BossMod.Autorotation;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod;

public class ColumnPlannerTrackStrategy(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, StrategyConfig config, int level, BossModuleRegistry.Info? moduleInfo, StrategyValue defaultOverride)
    : ColumnPlannerTrack(timeline, tree, phaseBranches, config.UIName)
{
    public StrategyValue DefaultOverride { get; private set; } = defaultOverride;

    protected override StrategyValue GetDefaultValue()
    {
        var res = new StrategyValue();
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
        using (ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(0, paddingY)))
        {
            var textSize = ImGui.CalcTextSize(Name);
            var originAdj = topLeft - ImGui.GetWindowPos();
            using (ImRaii.Group())
            {
                ImGui.SetCursorPos(originAdj);
                ImGui.Selectable("", DefaultOverride.Option > 0, ImGuiSelectableFlags.None, textSize with { X = Width });
                ImGui.SetCursorPos(originAdj + new Vector2((Width - textSize.X) * 0.5f, 0));
                ImGui.Text(Name);
            }
        }

        using (var popup = ImRaii.Popup("settings"))
        {
            if (popup)
            {
                var def = DefaultOverride;
                if (UIStrategyValue.DrawEditorOption(ref def, config, level, "Plan default"))
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
        var opt = config.Options[e.Value.Option];
        e.Window.Color = e.Value.Option > 0 && e.Value.Option <= Timeline.Colors.PlannerWindow.Length ? Timeline.Colors.PlannerWindow[e.Value.Option - 1] : Timeline.Colors.PlannerFallback;
        e.CooldownLength = opt.Cooldown;
        e.EffectLength = opt.Effect;
    }

    protected override List<string> DescribeElement(Element e) => UIStrategyValue.Preview(ref e.Value, config, moduleInfo);
    protected override bool EditElement(Element e) => UIStrategyValue.DrawEditor(ref e.Value, config, moduleInfo, level) | EditElementWindow(e);
}
