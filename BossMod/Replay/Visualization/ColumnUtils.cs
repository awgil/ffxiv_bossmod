namespace BossMod.ReplayVisualization;

public static class ColumnUtils
{
    public static ColumnGenericHistory.Entry AddHistoryEntryDot(this ColumnGenericHistory column, DateTime encStart, DateTime timestamp, string name, uint color, float widthRel = 1.0f)
    {
        var (node, delay) = column.Tree.AbsoluteTimeToNodeAndDelay((float)(timestamp - encStart).TotalSeconds, column.PhaseBranches);
        var e = new ColumnGenericHistory.Entry(ColumnGenericHistory.Entry.Type.Dot, node, delay, 0, name, new(color), widthRel);
        column.Entries.Add(e);
        return e;
    }

    public static ColumnGenericHistory.Entry AddHistoryEntryLine(this ColumnGenericHistory column, DateTime encStart, DateTime timestamp, string name, uint color, float widthRel = 1.0f)
    {
        var (node, delay) = column.Tree.AbsoluteTimeToNodeAndDelay((float)(timestamp - encStart).TotalSeconds, column.PhaseBranches);
        var e = new ColumnGenericHistory.Entry(ColumnGenericHistory.Entry.Type.Line, node, delay, 0, name, new(color), widthRel);
        column.Entries.Add(e);
        return e;
    }

    public static ColumnGenericHistory.Entry AddHistoryEntryRange(this ColumnGenericHistory column, DateTime encStart, DateTime rangeStart, float duration, string name, uint color, float widthRel = 1.0f)
    {
        var (node, delay) = column.Tree.AbsoluteTimeToNodeAndDelay((float)(rangeStart - encStart).TotalSeconds, column.PhaseBranches);
        var e = new ColumnGenericHistory.Entry(ColumnGenericHistory.Entry.Type.Range, node, delay, duration, name, new(color), widthRel);
        column.Entries.Add(e);
        return e;
    }

    public static ColumnGenericHistory.Entry AddHistoryEntryRange(this ColumnGenericHistory column, DateTime encStart, DateTime rangeStart, DateTime rangeEnd, string name, uint color, float widthRel = 1.0f)
    {
        return AddHistoryEntryRange(column, encStart, rangeStart, (float)(rangeEnd - rangeStart).TotalSeconds, name, color, widthRel);
    }

    public static ColumnGenericHistory.Entry AddHistoryEntryRange(this ColumnGenericHistory column, DateTime encStart, Replay.TimeRange range, string name, uint color, float widthRel = 1.0f)
    {
        return AddHistoryEntryRange(column, encStart, range.Start, range.Duration, name, color, widthRel);
    }

    public static void AddActionTooltip(List<string> tooltip, Replay.Action action)
    {
        foreach (var t in action.Targets)
        {
            tooltip.Add($"- {ReplayUtils.ActionTargetString(t, action.Timestamp)}");
            foreach (var e in t.Effects)
            {
                tooltip.Add($"-- {ReplayUtils.ActionEffectString(e)}");
            }
        }
    }
    public static void AddActionTooltip(this ColumnGenericHistory.Entry entry, Replay.Action action) => entry.TooltipExtra = (res, _) => AddActionTooltip(res, action);

    public static void AddCastTooltip(List<string> tooltip, Replay.Cast cast)
    {
        tooltip.Add($"- cast expected {cast.ExpectedCastTime:f2}, actual {cast.Time}");
        tooltip.Add($"- target loc: {Utils.Vec3String(cast.Location)}, angle: {cast.Rotation}");
    }
    public static void AddCastTooltip(this ColumnGenericHistory.Entry entry, Replay.Cast cast) => entry.TooltipExtra = (res, _) => AddCastTooltip(res, cast);

    public static bool ActionHasDamageToPlayerEffects(Replay.Action action)
    {
        return action.Targets.Any(t => t.Target.Type == ActorType.Player && t.Effects.Any(e => e.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage));
    }
}

public interface IToggleableColumn
{
    public abstract bool Visible { get; set; }
}
