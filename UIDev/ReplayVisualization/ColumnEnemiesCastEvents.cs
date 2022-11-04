using BossMod;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;

namespace UIDev
{
    // a set of columns containing cast events (typically by non-players)
    // by default contains a single column showing all actions from all sources, but extra columns can be added and per-column filters can be assigned
    public class ColumnEnemiesCastEvents : Timeline.ColumnGroup
    {
        private class ActionFilter
        {
            public Dictionary<Replay.Participant, BitMask> FromSource = new();
            public BitMask? NullSource;
        }

        private StateMachineTree _tree;
        private List<int> _phaseBranches;
        private Replay.Encounter _encounter;
        private ModuleRegistry.Info? _moduleInfo;
        private List<Replay.Action> _actions;
        private Dictionary<ActionID, ActionFilter> _filters = new();

        public ColumnEnemiesCastEvents(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc)
            : base(timeline)
        {
            //Name = "Enemy cast events";
            _tree = tree;
            _phaseBranches = phaseBranches;
            _encounter = enc;
            _moduleInfo = ModuleRegistry.FindByOID(enc.OID);
            _actions = replay.EncounterActions(enc).Where(a => !(a.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo)).ToList();
            foreach (var a in _actions)
            {
                var f = _filters.GetOrAdd(a.ID);
                if (a.Source != null)
                    f.FromSource[a.Source] = new(1);
                else
                    f.NullSource = new(1);
            }
            AddColumn();
            RebuildEvents();
        }

        public void DrawConfig(UITree tree)
        {
            if (ImGui.Button("Add new!"))
                AddColumn();

            bool needRebuild = false;
            foreach (var na in tree.Nodes(_filters, kv => new($"{kv.Key} ({_moduleInfo?.ActionIDType?.GetEnumName(kv.Key.ID)})")))
            {
                if (na.Value.NullSource != null)
                {
                    var mask = na.Value.NullSource.Value;
                    if (DrawConfigColumns(ref mask, "(no source)"))
                    {
                        na.Value.NullSource = mask;
                        needRebuild = true;
                    }
                }

                foreach (var src in na.Value.FromSource)
                {
                    var mask = src.Value;
                    if (DrawConfigColumns(ref mask, $"{ReplayUtils.ParticipantString(src.Key)} ({_moduleInfo?.ObjectIDType?.GetEnumName(src.Key.OID)})"))
                    {
                        _filters[na.Key].FromSource[src.Key] = mask;
                        needRebuild = true;
                    }
                }
            }

            if (needRebuild)
                RebuildEvents();
        }

        private void AddColumn()
        {
            Add(new ColumnGenericHistory(Timeline, _tree, _phaseBranches));
        }

        private bool DrawConfigColumns(ref BitMask mask, string name)
        {
            bool changed = false;
            for (int c = 0; c < Columns.Count; ++c)
            {
                bool set = mask[c];
                if (ImGui.Checkbox($"###{name}/{c}", ref set))
                {
                    mask[c] = set;
                    changed = true;
                }
                ImGui.SameLine();
            }
            ImGui.TextUnformatted(name);
            return changed;
        }

        private void RebuildEvents()
        {
            foreach (var c in Columns.Cast<ColumnGenericHistory>())
                c.Entries.Clear();

            foreach (var a in _actions)
            {
                var cols = a.Source != null ? _filters[a.ID].FromSource[a.Source] : _filters[a.ID].NullSource!.Value;
                if (cols.None())
                    continue;

                var name = $"{a.ID} ({_moduleInfo?.ActionIDType?.GetEnumName(a.ID.ID)}) {ReplayUtils.ParticipantString(a.Source)} -> {ReplayUtils.ParticipantString(a.MainTarget)} #{a.GlobalSequence}";
                foreach (var c in cols.SetBits())
                {
                    var col = (ColumnGenericHistory)Columns[c];
                    col.AddHistoryEntryDot(_encounter.Time.Start, a.Timestamp, name, ColumnUtils.ActionHasDamageToPlayerEffects(a) ? 0xffffffff : 0x80808080).AddActionTooltip(a);
                }
            }
        }
    }
}
