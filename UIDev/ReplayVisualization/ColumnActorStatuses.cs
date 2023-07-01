using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UIDev
{
    public class ColumnActorStatuses : Timeline.ColumnGroup
    {
        private StateMachineTree _tree;
        private List<int> _phaseBranches;
        private Replay _replay;
        private Replay.Encounter _enc;
        private Replay.Participant _target;

        private ColumnSeparator _sep;
        private Dictionary<uint, Dictionary<ulong, (Replay.Participant? source, ColumnGenericHistory? col)>> _columns = new();

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
            foreach (var s in replay.EncounterStatuses(enc).Where(s => s.Target == actor))
                _columns.GetOrAdd(s.ID)[s.Source?.InstanceID ?? 0] = (s.Source, null);
        }

        public void DrawConfig(UITree tree)
        {
            foreach (var (id, sub) in tree.Nodes(_columns, kv => new(Utils.StatusString(kv.Key))))
            {
                foreach (var (sourceID, (source, col)) in sub)
                {
                    bool visible = col?.Width > 0;
                    if (ImGui.Checkbox(ReplayUtils.ParticipantString(source), ref visible))
                    {
                        var actualCol = col ?? BuildColumn(id, source);
                        actualCol.Width = visible ? ColumnGenericHistory.DefaultWidth : 0;
                        if (col == null)
                            sub[sourceID] = (source, actualCol);
                    }
                }
            }
            _sep.Width = Columns.Any(c => c != _sep && c.Width > 0) ? 1 : 0;
        }

        private ColumnGenericHistory BuildColumn(uint statusID, Replay.Participant? source)
        {
            var res = AddBefore(new ColumnGenericHistory(Timeline, _tree, _phaseBranches), _sep);
            DateTime prevEnd = default;
            foreach (var s in _replay.EncounterStatuses(_enc).Where(s => s.ID == statusID && s.Source == source && s.Target == _target))
            {
                var e = res.AddHistoryEntryRange(_enc.Time.Start, s.Time, $"{Utils.StatusString(statusID)} ({s.StartingExtra:X}) on {ReplayUtils.ParticipantString(_target)} from {ReplayUtils.ParticipantString(source)}", 0x80808080);
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
                                var actionName = $"{a.ID} -> {ReplayUtils.ParticipantString(a.MainTarget)} #{a.GlobalSequence}";
                                res.AddHistoryEntryDot(_enc.Time.Start, a.Timestamp, actionName, 0xffffffff).AddActionTooltip(a);
                            }
                        }
                    }
                }
            }
            return res;
        }
    }
}
