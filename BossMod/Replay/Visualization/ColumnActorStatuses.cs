using ImGuiNET;
using static BossMod.Replay;

namespace BossMod.ReplayVisualization;

public class ColumnActorStatuses : Timeline.ColumnGroup
{
    private readonly StateMachineTree _tree;
    private readonly List<int> _phaseBranches;
    private readonly Replay _replay;
    private readonly Replay.Encounter _enc;
    private readonly Replay.Participant _target;

    private readonly ColumnSeparator _sep;
    private readonly List<(uint sid, Replay.Participant? source, ColumnGenericHistory? col)> _columns = [];

    public bool Visible => _sep.Width > 0;

    public ColumnActorStatuses(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant actor)
        : base(timeline)
    {
        //Name = "Statuses";
        _tree = tree;
        _phaseBranches = phaseBranches;
        _replay = replay;
        _enc = enc;
        _target = actor;
        _sep = Add(new ColumnSeparator(timeline, width: 0));
        foreach (var s in replay.EncounterStatuses(enc).Where(s => s.Target == actor && !_columns.Any(c => c.sid == s.ID && c.source == s.Source)))
            _columns.Add((s.ID, s.Source, null));
    }

    public void DrawConfig(UITree tree)
    {
        foreach (ref var c in _columns.AsSpan())
        {
            bool visible = c.col?.Width > 0;
            if (ImGui.Checkbox($"{Utils.StatusString(c.sid)} from {ReplayUtils.ParticipantString(c.source, c.source?.WorldExistence.FirstOrDefault().Start ?? default)}", ref visible))
            {
                c.col ??= BuildColumn(c.sid, c.source);
                c.col.Width = visible ? ColumnGenericHistory.DefaultWidth : 0;
            }
        }
        _sep.Width = Columns.Any(c => c != _sep && c.Width > 0) ? 1 : 0;
    }

    private ColumnGenericHistory BuildColumn(uint statusID, Participant? source)
    {
        var res = AddBefore(new ColumnGenericHistory(Timeline, _tree, _phaseBranches), _sep);
        DateTime prevEnd = default;
        foreach (var s in _replay.EncounterStatuses(_enc).Where(s => s.ID == statusID && s.Source == source && s.Target == _target))
        {
            var e = res.AddHistoryEntryRange(_enc.Time.Start, s.Time, $"{Utils.StatusString(statusID)} ({s.StartingExtra:X}) on {ReplayUtils.ParticipantString(_target, s.Time.Start)} from {ReplayUtils.ParticipantString(s.Source, s.Time.Start)}", 0x80808080);
            e.TooltipExtra.Add($"- initial duration: {s.InitialDuration:f3}");
            e.TooltipExtra.Add($"- final duration: {s.InitialDuration - s.Time.Duration:f3}");
            if (s.Time.Start == prevEnd)
                res.AddHistoryEntryLine(_enc.Time.Start, prevEnd, "", 0xffffffff);
            prevEnd = s.Time.End;
        }
        foreach (var a in _replay.EncounterActions(_enc))
        {
            foreach (var t in a.Targets)
            {
                foreach (var e in t.Effects)
                {
                    if (e.Type is ActionEffectType.ApplyStatusEffectTarget or ActionEffectType.ApplyStatusEffectSource && e.Value == statusID)
                    {
                        var src = e.FromTarget ? t.Target : a.Source;
                        var tgt = e.AtSource ? a.Source : t.Target;
                        if (src == source && tgt == _target)
                        {
                            var actionName = $"{a.ID} -> {ReplayUtils.ParticipantString(a.MainTarget, a.Timestamp)} #{a.GlobalSequence}";
                            res.AddHistoryEntryDot(_enc.Time.Start, a.Timestamp, actionName, 0xffffffff).AddActionTooltip(a);
                        }
                    }
                }
            }
        }
        return res;
    }
}
