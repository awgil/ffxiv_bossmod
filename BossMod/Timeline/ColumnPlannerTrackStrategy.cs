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

    const float HeaderAngle = 60 * Angle.DegToRad;
    static readonly float HeaderCos = MathF.Cos(HeaderAngle);
    static readonly float HeaderSin = MathF.Sin(HeaderAngle);
    static readonly float HeaderTan = MathF.Tan(HeaderAngle);

    static Vector2 ClickableEdge => new(Timeline.TopMargin * HeaderCos, -Timeline.TopMargin * HeaderSin);

    public override void DrawHeader(Vector2 topLeft)
    {
        using var _ = ImRaii.PushId($"{config.OptionEnum}_{config.InternalName}");
        var paddingY = ImGui.GetStyle().ItemSpacing.Y;
        var cursor = ImGui.GetCursorPos();
        var isActuallyHovered = false;
        var wl = ImGui.GetWindowDrawList();
        var wp = ImGui.GetWindowPos();

        var viewMinY = cursor.Y + wl.GetClipRectMin().Y - Timeline.TopMargin;

        wl.PushClipRect(new(0, viewMinY), new(int.MaxValue, int.MaxValue), true);

        var originAdj = topLeft - wp;
        var mouseAbs = ImGui.GetMousePos();
        var mouseRelative = mouseAbs - topLeft;

        // check for hover on bounding box. if we manually try to do hover logic it interferes with other modals and popups etc
        ImGui.SetCursorPos(originAdj + new Vector2(0, ClickableEdge.Y));
        ImGui.Dummy(new Vector2(ClickableEdge.X + Width, -ClickableEdge.Y));

        if (ImGui.IsItemHovered() && mouseAbs.Y > viewMinY && mouseRelative.Y <= 0)
        {
            var offX = mouseRelative.X + mouseRelative.Y / HeaderTan;
            if (offX >= 0 && offX < Width)
            {
                isActuallyHovered = true;
                ImGui.AddQuadFilled(ImGui.GetWindowDrawList(), topLeft, topLeft + ClickableEdge, topLeft + ClickableEdge + new Vector2(Width, 0), topLeft + new Vector2(Width, 0), 0xFF404040);
            }
        }

        ImGui.SetCursorPos(originAdj + new Vector2(Width * 0.5f, 0) + new WDir(ImGui.GetFrameHeight() * 0.5f, 0).Rotate(new WDir(HeaderSin, HeaderCos)).ToVec2());
        UIMisc.TextRotated(Name, HeaderAngle);

        wl.PopClipRect();

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

        if (isActuallyHovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (config.UIName != Name)
                ImGui.SetTooltip(config.UIName);
            if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                ImGui.OpenPopup("settings");
        }

        ImGui.SetCursorPos(cursor);
    }

    protected override Color GetBackgroundColor() => DefaultOverride.Option > 0 ? Timeline.Colors.PlannerBackgroundHighlight : Timeline.Colors.PlannerBackground;

    protected override void RefreshElement(Element e)
    {
        if (e.Value is StrategyValueTrack t)
        {
            var opt = config.Options[t.Option];
            e.Window.Color = opt.Color > 0 ? new(opt.Color)
                : t.Option > 0 && t.Option <= Timeline.Colors.PlannerWindow.Length ? Timeline.Colors.PlannerWindow[t.Option - 1]
                : Timeline.Colors.PlannerFallback;
            e.CooldownLength = opt.Cooldown;
            e.EffectLength = opt.Effect;
        }
    }

    protected override List<string> DescribeElement(Element e) => UIStrategyValue.Preview(e.Value, config, moduleInfo);
    protected override bool EditElement(Element e) => UIStrategyValue.DrawEditor(e.Value, config, moduleInfo, level) | EditElementWindow(e);
}
