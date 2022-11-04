using BossMod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UIDev
{
    public static class ColumnUtils
    {
        public static ColumnGenericHistory.Entry AddHistoryEntryDot(this ColumnGenericHistory column, DateTime encStart, DateTime timestamp, string name, uint color)
        {
            var (node, delay) = column.Tree.AbsoluteTimeToNodeAndDelay((float)(timestamp - encStart).TotalSeconds, column.PhaseBranches);
            var e = new ColumnGenericHistory.Entry(ColumnGenericHistory.Entry.Type.Dot, node, delay, 0, name, color);
            column.Entries.Add(e);
            return e;
        }

        public static ColumnGenericHistory.Entry AddHistoryEntryLine(this ColumnGenericHistory column, DateTime encStart, DateTime timestamp, string name, uint color)
        {
            var (node, delay) = column.Tree.AbsoluteTimeToNodeAndDelay((float)(timestamp - encStart).TotalSeconds, column.PhaseBranches);
            var e = new ColumnGenericHistory.Entry(ColumnGenericHistory.Entry.Type.Line, node, delay, 0, name, color);
            column.Entries.Add(e);
            return e;
        }

        public static ColumnGenericHistory.Entry AddHistoryEntryRange(this ColumnGenericHistory column, DateTime encStart, DateTime rangeStart, float duration, string name, uint color)
        {
            var (node, delay) = column.Tree.AbsoluteTimeToNodeAndDelay((float)(rangeStart - encStart).TotalSeconds, column.PhaseBranches);
            var e = new ColumnGenericHistory.Entry(ColumnGenericHistory.Entry.Type.Range, node, delay, duration, name, color);
            column.Entries.Add(e);
            return e;
        }

        public static ColumnGenericHistory.Entry AddHistoryEntryRange(this ColumnGenericHistory column, DateTime encStart, DateTime rangeStart, DateTime rangeEnd, string name, uint color)
        {
            return AddHistoryEntryRange(column, encStart, rangeStart, (float)(rangeEnd - rangeStart).TotalSeconds, name, color);
        }

        public static ColumnGenericHistory.Entry AddHistoryEntryRange(this ColumnGenericHistory column, DateTime encStart, Replay.TimeRange range, string name, uint color)
        {
            return AddHistoryEntryRange(column, encStart, range.Start, range.Duration, name, color);
        }

        public static void AddActionTooltip(this ColumnGenericHistory.Entry entry, Replay.Action action)
        {
            foreach (var t in action.Targets)
            {
                entry.TooltipExtra.Add($"- {ReplayUtils.ParticipantString(t.Target)}");
                foreach (var e in t.Effects)
                {
                    entry.TooltipExtra.Add($"-- {ReplayUtils.ActionEffectString(e)}");
                }
            }
        }

        public static void AddCastTooltip(this ColumnGenericHistory.Entry entry, Replay.Cast cast)
        {
            entry.TooltipExtra.Add($"- cast expected {cast.ExpectedCastTime:f2}, actual {cast.Time}");
            entry.TooltipExtra.Add($"- target loc: {Utils.Vec3String(cast.Location)}, angle: {cast.Rotation}");
        }

        public static bool ActionHasDamageToPlayerEffects(Replay.Action action)
        {
            return action.Targets.Any(t => t.Target?.Type == ActorType.Player && t.Effects.Any(e => e.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage));
        }
    }
}
