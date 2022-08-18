using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UIDev
{
    class OpList
    {
        private Replay _replay;
        private ModuleRegistry.Info? _moduleInfo;
        private IEnumerable<WorldState.Operation> _ops;
        private Action<DateTime> _scrollTo;
        private List<(DateTime Timestamp, string Text, Action<UITree>? Children, Action? ContextMenu)> _nodes = new();
        private HashSet<uint> _filteredOIDs = new();
        private HashSet<ActionID> _filteredActions = new();
        private HashSet<uint> _filteredStatuses = new();
        private bool _nodesUpToDate;

        public OpList(Replay r, ModuleRegistry.Info? moduleInfo, IEnumerable<WorldState.Operation> ops, Action<DateTime> scrollTo)
        {
            _replay = r;
            _scrollTo = scrollTo;
            _moduleInfo = moduleInfo;
            _ops = ops;
        }

        public void Draw(UITree tree, DateTime reference)
        {
            //foreach (var n in _tree.Node("Settings"))
            //{
            //    DrawSettings();
            //}

            if (!_nodesUpToDate)
            {
                var aidType = _moduleInfo?.ActionIDType;
                _nodes.Clear();
                foreach (var op in _ops.Where(FilterOp))
                {
                    _nodes.Add((op.Timestamp, OpName(op, aidType), OpChildren(op), OpContextMenu(op)));
                }
                _nodesUpToDate = true;
            }

            foreach (var node in _nodes)
            {
                foreach (var n in tree.Node($"{(node.Timestamp - reference).TotalSeconds:f3}: {node.Text}", node.Children == null, 0xffffffff, node.ContextMenu, () => _scrollTo(node.Timestamp)))
                {
                    if (node.Children != null)
                        node.Children(tree);
                }
            }
        }

        public void ClearFilters()
        {
            _filteredOIDs.Clear();
            _filteredActions.Clear();
            _filteredStatuses.Clear();
            _nodesUpToDate = false;
        }

        private bool FilterInterestingActor(ulong instanceID, DateTime timestamp, bool allowPlayers)
        {
            var p = FindParticipant(instanceID, timestamp)!;
            if (p.Type is ActorType.Pet or ActorType.Chocobo or ActorType.Area)
                return false;
            if (p.Type == ActorType.Player && !allowPlayers)
                return false;
            if (_filteredOIDs.Contains(p.OID))
                return false;
            return true;
        }

        private bool FilterInterestingStatus(ulong instanceID, int index, DateTime timestamp, bool gain)
        {
            var s = FindStatus(instanceID, index, timestamp, gain)!;
            if (s.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo)
                return false; // don't care about statuses applied by players
            if (s.Target?.Type is ActorType.Pet)
                return false; // don't care about statuses applied to pets
            if (s.ID is 43 or 44 or 418)
                return false; // don't care about resurrect-related statuses
            if (_filteredStatuses.Contains(s.ID))
                return false; // don't care about filtered out statuses
            return true;
        }

        private bool FilterOp(WorldState.Operation o)
        {
            return o switch
            {
                WorldState.OpFrameStart => false,
                ActorState.OpCreate op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
                ActorState.OpDestroy op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
                ActorState.OpMove => false,
                ActorState.OpHP => false,
                ActorState.OpTargetable op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
                ActorState.OpDead op => FilterInterestingActor(op.InstanceID, op.Timestamp, true),
                ActorState.OpCombat => false,
                ActorState.OpTarget => false, // reconsider...
                ActorState.OpCastInfo op => FilterInterestingActor(op.InstanceID, op.Timestamp, false) && !_filteredActions.Contains(FindCast(FindParticipant(op.InstanceID, op.Timestamp), op.Timestamp, op.Value != null)!.ID),
                ActorState.OpCastEvent op => FilterInterestingActor(op.InstanceID, op.Timestamp, false) && !_filteredActions.Contains(op.Value.Action),
                ActorState.OpStatus op => FilterInterestingStatus(op.InstanceID, op.Index, op.Timestamp, op.Value.ID != 0),
                _ => true
            };
        }

        private string OpName(WorldState.Operation o, Type? aidType)
        {
            return o switch
            {
                ActorState.OpCreate op => $"Actor create: {ActorString(op.InstanceID, op.Timestamp)}",
                ActorState.OpDestroy op => $"Actor destroy: {ActorString(op.InstanceID, op.Timestamp)}",
                ActorState.OpRename op => $"Actor rename: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Name}",
                ActorState.OpClassChange op => $"Actor class change: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Class}",
                ActorState.OpTargetable op => $"{(op.Value ? "Targetable" : "Untargetable")}: {ActorString(op.InstanceID, op.Timestamp)}",
                ActorState.OpDead op => $"{(op.Value ? "Die" : "Resurrect")}: {ActorString(op.InstanceID, op.Timestamp)}",
                ActorState.OpTether op => $"Tether: {ActorString(op.InstanceID, op.Timestamp)} {op.Value.ID} @ {ActorString(op.Value.Target, op.Timestamp)}",
                ActorState.OpCastInfo op => $"Cast {(op.Value != null ? "started" : "ended")}: {CastString(op.InstanceID, op.Timestamp, aidType, op.Value != null)}",
                ActorState.OpCastEvent op => $"Cast event: {ActorString(op.InstanceID, op.Timestamp)}: {op.Value.Action} ({aidType?.GetEnumName(op.Value.Action.ID)}) @ {CastEventTargetString(op.Value, op.Timestamp)} ({op.Value.Targets.Count} targets affected) #{op.Value.GlobalSequence}",
                ActorState.OpStatus op => $"Status {(op.Value.ID != 0 ? "gain" : "lose")}: {StatusString(op.InstanceID, op.Index, op.Timestamp, op.Value.ID != 0)}",
                ActorState.OpIcon op => $"Icon: {ActorString(op.InstanceID, op.Timestamp)} -> {op.IconID}",
                _ => o.ToString() ?? o.GetType().Name
            };
        }

        private Action<UITree>? OpChildren(WorldState.Operation o)
        {
            return o switch
            {
                ActorState.OpCastEvent op => op.Value.Targets.Count != 0 ? tree => DrawEventCast(tree, op) : null,
                _ => null
            };
        }

        private void DrawEventCast(UITree tree, ActorState.OpCastEvent op)
        {
            foreach (var t in tree.Nodes(op.Value.Targets, t => new(ActorString(t.ID, op.Timestamp))))
            {
                tree.LeafNodes(t.Effects, ReplayUtils.ActionEffectString);
            }
        }

        private Action? OpContextMenu(WorldState.Operation o)
        {
            return o switch
            {
                ActorState.OpStatus op => () => ContextMenuActorStatus(op),
                ActorState.OpCastInfo op => () => ContextMenuActorCast(op),
                ActorState.OpCastEvent op => () => ContextMenuEventCast(op),
                ActorState.Operation op => () => ContextMenuActor(op),
                _ => null,
            };
        }

        private void ContextMenuActor(ActorState.Operation op)
        {
            var oid = FindParticipant(op.InstanceID, op.Timestamp)!.OID;
            if (ImGui.MenuItem($"Filter out OID {oid:X}"))
            {
                _filteredOIDs.Add(oid);
                _nodesUpToDate = false;
            }
        }

        private void ContextMenuActorStatus(ActorState.OpStatus op)
        {
            ContextMenuActor(op);
            var s = FindStatus(op.InstanceID, op.Index, op.Timestamp, op.Value.ID != 0)!;
            if (ImGui.MenuItem($"Filter out {Utils.StatusString(s.ID)}"))
            {
                _filteredStatuses.Add(s.ID);
                _nodesUpToDate = false;
            }
        }

        private void ContextMenuActorCast(ActorState.OpCastInfo op)
        {
            ContextMenuActor(op);
            var id = FindCast(FindParticipant(op.InstanceID, op.Timestamp), op.Timestamp, op.Value != null)!.ID;
            if (ImGui.MenuItem($"Filter out {id}"))
            {
                _filteredActions.Add(id);
                _nodesUpToDate = false;
            }
        }

        private void ContextMenuEventCast(ActorState.OpCastEvent op)
        {
            ContextMenuActor(op);
            if (ImGui.MenuItem($"Filter out {op.Value.Action}"))
            {
                _filteredActions.Add(op.Value.Action);
                _nodesUpToDate = false;
            }
        }

        private Replay.Participant? FindParticipant(ulong instanceID, DateTime timestamp) => _replay.Participants.Find(p => p.InstanceID == instanceID && p.Existence.Contains(timestamp));
        private Replay.Status? FindStatus(ulong instanceID, int index, DateTime timestamp, bool gain) => _replay.Statuses.Find(s => s.Target?.InstanceID == instanceID && s.Index == index && (gain ? s.Time.Start : s.Time.End) == timestamp);
        private Replay.Cast? FindCast(Replay.Participant? participant, DateTime timestamp, bool start) => participant?.Casts.Find(c => (start ? c.Time.Start : c.Time.End) == timestamp);

        private string ActorString(ulong instanceID, DateTime timestamp)
        {
            return ReplayUtils.ParticipantPosRotString(FindParticipant(instanceID, timestamp), timestamp);
        }

        private string CastEventTargetString(ActorCastEvent ev, DateTime timestamp)
        {
            if (ev.MainTargetID != 0 && ev.MainTargetID != 0xE0000000u)
                return ActorString(ev.MainTargetID, timestamp);
            else
                return Utils.Vec3String(ev.TargetPos);
        }

        private string CastString(ulong instanceID, DateTime timestamp, Type? aidType, bool start)
        {
            var p = FindParticipant(instanceID, timestamp);
            var c = FindCast(p, timestamp, start)!;
            return $"{ReplayUtils.ParticipantPosRotString(p, timestamp)}: {c.ID} ({aidType?.GetEnumName(c.ID.ID)}), {c.ExpectedCastTime:f2}s ({c.Time} actual) @ {ReplayUtils.ParticipantString(c.Target)} {Utils.Vec3String(c.Location)}";
        }

        private string StatusString(ulong instanceID, int index, DateTime timestamp, bool gain)
        {
            var s = FindStatus(instanceID, index, timestamp, gain)!;
            return $"{ReplayUtils.ParticipantPosRotString(s.Target, timestamp)}: {Utils.StatusString(s!.ID)} ({s.StartingExtra:X}), {s.InitialDuration:f2}s / {s.Time}, from {ReplayUtils.ParticipantPosRotString(s.Source, timestamp)}";
        }
    }
}
