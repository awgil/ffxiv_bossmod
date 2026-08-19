using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;

namespace BossMod;

// overlay that shows upcoming cooldown-plan actions as a scrolling timeline (similar to WoW's TimelineReminders/LiquidReminders addons)
// entries move at a constant configurable speed toward a fixed 'now' line; anything too far out to fit on screen at that speed
// piles up stacked at the far edge (so the window always looks 'full') and only starts actually sliding once it would fit,
// continue drifting past the line while their cast window is open, and disappear once the window closes
public class BossModuleTimelineRemindersWindow : UIWindow
{
    private readonly RotationModuleManager _rotation;
    private const float NearFraction = 0.2f; // 'now' line sits this far from the edge of the window it's closest to

    public BossModuleTimelineRemindersWindow(RotationModuleManager rotation) : base("Timeline reminders", false, new(750, 120))
    {
        _rotation = rotation;
        RespectCloseHotkey = false;
    }

    private BossModuleConfig Config => Service.Config.Get<BossModuleConfig>();
    private bool? _lastHorizontal;

    public override void PreOpenCheck()
    {
        IsOpen = Config.TimelineRemindersEnabled && (Config.ShowDemo || _rotation.Planner?.Plan != null);
        Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        if (Config.Lock)
            Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;

        // apply a direction-appropriate default size whenever the user switches between horizontal and vertical directions,
        // so eg switching to a vertical direction doesn't leave the window stuck wide-and-short
        var horizontal = Config.TimelineRemindersDirection is BossModuleConfig.TimelineRemindersDir.RightToLeft or BossModuleConfig.TimelineRemindersDir.LeftToRight;
        if (_lastHorizontal != horizontal)
        {
            _lastHorizontal = horizontal;
            Size = horizontal ? new(750, 120) : new(120, 750);
            SizeCondition = ImGuiCond.Always;
        }
        else
        {
            Size = null;
        }
    }

    // fake entries so the window can be sized/positioned without an active plan, mirroring the radar's ShowDemo mode
    private static readonly List<PlanExecution.UpcomingEntry> _demoEntries =
    [
        new("Demo action 1", 0, 0xff4080ff, -2, 2),
        new("Demo action 2", 0, 0xff00c0ff, 8, 22),
        new("Demo action 3", 0, 0xffa0ff40, 15, 40),
    ];

    public override void Draw()
    {
        var planner = _rotation.Planner;
        var entries = planner?.Plan != null
            ? planner.GetUpcomingEntries(_rotation.WorldState, _rotation.PlayerSlot, Config.TimelineRemindersLookahead, Config.TimelineRemindersMaxPerTrack)
            : _demoEntries;

        var iconSize = Config.TimelineRemindersIconSize;
        var pos = ImGui.GetWindowPos();
        var size = ImGui.GetWindowSize();

        var dir = Config.TimelineRemindersDirection;
        var horizontal = dir is BossModuleConfig.TimelineRemindersDir.RightToLeft or BossModuleConfig.TimelineRemindersDir.LeftToRight;
        // 'arrival' end is the edge entries travel toward (where the now-line sits); low = left/top, high = right/bottom
        var arrivalIsLow = dir is BossModuleConfig.TimelineRemindersDir.RightToLeft or BossModuleConfig.TimelineRemindersDir.BottomToTop;
        var sign = arrivalIsLow ? 1f : -1f;

        var primaryStart = horizontal ? pos.X : pos.Y;
        var primarySize = horizontal ? size.X : size.Y;
        var nowCoord = primaryStart + primarySize * (arrivalIsLow ? NearFraction : 1 - NearFraction);
        var crossCenter = horizontal ? pos.Y + size.Y * 0.5f : pos.X + size.X * 0.5f;
        var pixelsPerSecond = Config.TimelineRemindersSpeed;

        // show upcoming spells at the oposite end of the window
        var farSpan = arrivalIsLow ? primaryStart + primarySize - nowCoord : nowCoord - primaryStart;
        var maxVisibleSeconds = MathF.Max((farSpan - iconSize) / pixelsPerSecond, 0f);

        //only show next stacked spells countdown
        float? soonestStacked = null;
        foreach (var e in entries)
        {
            if (e.TimeUntilStart > maxVisibleSeconds && (soonestStacked == null || e.TimeUntilStart < soonestStacked))
                soonestStacked = e.TimeUntilStart;
        }

        var drawList = ImGui.GetWindowDrawList();
        if (horizontal)
            drawList.AddLine(new(nowCoord, pos.Y), new(nowCoord, pos.Y + size.Y), 0xffffffff, 2);
        else
            drawList.AddLine(new(pos.X, nowCoord), new(pos.X + size.X, nowCoord), 0xffffffff, 2);

        // entries are sorted soonest-first; draw farthest-first so the soonest one ends up on top when icons overlap
        for (int idx = entries.Count - 1; idx >= 0; --idx)
        {
            var e = entries[idx];
            var coord = nowCoord + sign * MathF.Min(e.TimeUntilStart, maxVisibleSeconds) * pixelsPerSecond;
            if (coord < primaryStart - iconSize || coord > primaryStart + primarySize + iconSize)
                continue;

            var iconPos = horizontal
                ? new Vector2(coord - iconSize * 0.5f, crossCenter - iconSize * 0.5f)
                : new Vector2(crossCenter - iconSize * 0.5f, coord - iconSize * 0.5f);
            if (e.IconId != 0 && Service.Texture.TryGetFromGameIcon(e.IconId, out var tex))
            {
                var wrap = tex.GetWrapOrEmpty();
                drawList.AddImage(wrap.Handle, iconPos, iconPos + new Vector2(iconSize));
            }
            else
            {
                drawList.AddRectFilled(iconPos, iconPos + new Vector2(iconSize), e.Color != 0 ? e.Color : 0xff808080);
            }

            if (e.TimeUntilStart <= maxVisibleSeconds || e.TimeUntilStart == soonestStacked)
            {
                var countdown = (e.TimeUntilStart <= 0 ? "+" : "") + (-e.TimeUntilStart).ToString("f0");
                var fontSize = Config.TimelineRemindersFontSize;
                var textSize = ImGui.CalcTextSizeA(ImGui.GetFont(), fontSize, 1000, -1, countdown, out _);
                var textPos = Config.TimelineRemindersTextPosition switch
                {
                    BossModuleConfig.TimelineRemindersTextPos.Top => new Vector2(iconPos.X + iconSize * 0.5f - textSize.X * 0.5f, iconPos.Y - textSize.Y - 2),
                    BossModuleConfig.TimelineRemindersTextPos.Left => new Vector2(iconPos.X - textSize.X - 2, iconPos.Y + iconSize * 0.5f - textSize.Y * 0.5f),
                    BossModuleConfig.TimelineRemindersTextPos.Right => new Vector2(iconPos.X + iconSize + 2, iconPos.Y + iconSize * 0.5f - textSize.Y * 0.5f),
                    _ => new Vector2(iconPos.X + iconSize * 0.5f - textSize.X * 0.5f, iconPos.Y + iconSize + 2),
                };
                drawList.AddText(ImGui.GetFont(), fontSize, textPos, 0xffffffff, countdown);
            }

            if (ImGui.IsMouseHoveringRect(iconPos, iconPos + new Vector2(iconSize)))
                ImGui.SetTooltip(e.Name);
        }
    }
}
