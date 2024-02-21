﻿using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BossMod.ReplayVisualization
{
    class OpList
    {
        private Replay _replay;
        private ModuleRegistry.Info? _moduleInfo;
        private IEnumerable<WorldState.Operation> _ops;
        private DateTime _relativeTS;
        private Action<DateTime> _scrollTo;
        private List<(int Index, DateTime Timestamp, string Text, Action<UITree>? Children, Action? ContextMenu)> _nodes = new();
        private HashSet<uint> _filteredOIDs = new();
        private HashSet<ActionID> _filteredActions = new();
        private HashSet<uint> _filteredStatuses = new();
        private HashSet<uint> _filteredDirectorUpdateTypes = new();
        private bool _showActorSizeEvents = true;
        private bool _nodesUpToDate;

        public bool ShowActorSizeEvents
        {
            get => _showActorSizeEvents;
            set {
                _showActorSizeEvents = value;
                _nodesUpToDate = false;
            }
        }

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
                _nodes.Clear();
                int i = 0;
                foreach (var op in _ops)
                {
                    if (FilterOp(op))
                    {
                        _nodes.Add((i, op.Timestamp, OpName(op), OpChildren(op), OpContextMenu(op)));
                    }
                    ++i;
                }
                _nodesUpToDate = true;
            }

            var timeRef = ImGui.GetIO().KeyShift && _relativeTS != default ? _relativeTS : reference;
            foreach (var node in _nodes)
            {
                foreach (var n in tree.Node($"{(node.Timestamp - timeRef).TotalSeconds:f3}: {node.Text}###{node.Index}", node.Children == null, 0xffffffff, node.ContextMenu, () => _scrollTo(node.Timestamp), () => _relativeTS = node.Timestamp))
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
            _filteredDirectorUpdateTypes.Clear();
            _nodesUpToDate = false;
        }

        private bool FilterInterestingActor(ulong instanceID, DateTime timestamp, bool allowPlayers)
        {
            var p = _replay.FindParticipant(instanceID, timestamp)!;
            if ((p.OwnerID & 0xFF000000) == 0x10000000)
                return false; // player's pet/area
            if (p.Type == ActorType.Player && !allowPlayers)
                return false;
            if (_filteredOIDs.Contains(p.OID))
                return false;
            return true;
        }

        private bool FilterInterestingStatus(Replay.Status s)
        {
            if (s.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo)
                return false; // don't care about statuses applied by players
            if (s.Target.Type is ActorType.Pet)
                return false; // don't care about statuses applied to pets
            if (s.ID is 43 or 44 or 418)
                return false; // don't care about resurrect-related statuses
            if (_filteredOIDs.Contains(s.Target.OID))
                return false; // don't care about filtered out targets
            if (_filteredStatuses.Contains(s.ID))
                return false; // don't care about filtered out statuses
            return true;
        }
        private bool FilterInterestingStatuses(ulong instanceID, int index, DateTime timestamp) => FindStatuses(instanceID, index, timestamp).Any(FilterInterestingStatus);

        private bool FilterOp(WorldState.Operation o)
        {
            return o switch
            {
                WorldState.OpFrameStart => false,
                WorldState.OpDirectorUpdate op => !_filteredDirectorUpdateTypes.Contains(op.UpdateID),
                ActorState.OpCreate op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
                ActorState.OpDestroy op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
                ActorState.OpMove => false,
                ActorState.OpSizeChange op => _showActorSizeEvents && FilterInterestingActor(op.InstanceID, op.Timestamp, false),
                ActorState.OpHPMP => false,
                ActorState.OpTargetable op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
                ActorState.OpDead op => FilterInterestingActor(op.InstanceID, op.Timestamp, true),
                ActorState.OpCombat => false,
                ActorState.OpEventState op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
                ActorState.OpTarget op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
                ActorState.OpCastInfo op => FilterInterestingActor(op.InstanceID, op.Timestamp, false) && !_filteredActions.Contains(FindCast(_replay.FindParticipant(op.InstanceID, op.Timestamp), op.Timestamp, op.Value != null)?.ID ?? new()),
                ActorState.OpCastEvent op => FilterInterestingActor(op.InstanceID, op.Timestamp, false) && !_filteredActions.Contains(op.Value.Action),
                ActorState.OpEffectResult => false,
                ActorState.OpStatus op => FilterInterestingStatuses(op.InstanceID, op.Index, op.Timestamp),
                ClientState.OpActionRequest => false,
                //ClientState.OpActionReject => false,
                _ => true
            };
        }

        private string DumpOp(WorldState.Operation op)
        {
            using (var stream = new MemoryStream(1024))
            {
                var writer = new ReplayRecorder.TextOutput(stream, null);
                op.Write(writer);
                writer.Flush();
                stream.Position = 0;
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                var start = Array.IndexOf(bytes, (byte)'|') + 1;
                return Encoding.UTF8.GetString(bytes, start, bytes.Length - start);
            }
        }

        private string OpName(WorldState.Operation o)
        {
            return o switch
            {
                ActorState.OpCreate op => $"Actor create: {ActorString(op.InstanceID, op.Timestamp)} #{op.SpawnIndex}",
                ActorState.OpDestroy op => $"Actor destroy: {ActorString(op.InstanceID, op.Timestamp)}",
                ActorState.OpRename op => $"Actor rename: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Name}",
                ActorState.OpClassChange op => $"Actor class change: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Class}",
                ActorState.OpTargetable op => $"{(op.Value ? "Targetable" : "Untargetable")}: {ActorString(op.InstanceID, op.Timestamp)}",
                ActorState.OpDead op => $"{(op.Value ? "Die" : "Resurrect")}: {ActorString(op.InstanceID, op.Timestamp)}",
                ActorState.OpEventState op => $"Event state: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Value}",
                ActorState.OpTarget op => $"Target: {ActorString(op.InstanceID, op.Timestamp)} -> {ActorString(op.Value, op.Timestamp)}",
                ActorState.OpTether op => $"Tether: {ActorString(op.InstanceID, op.Timestamp)} {op.Value.ID} ({_moduleInfo?.TetherIDType?.GetEnumName(op.Value.ID)}) @ {ActorString(op.Value.Target, op.Timestamp)}",
                ActorState.OpCastInfo op => $"Cast {(op.Value != null ? "started" : "ended")}: {CastString(op.InstanceID, op.Timestamp, op.Value != null)}",
                ActorState.OpCastEvent op => $"Cast event: {ActorString(op.InstanceID, op.Timestamp)}: {op.Value.Action} ({_moduleInfo?.ActionIDType?.GetEnumName(op.Value.Action.ID)}) @ {CastEventTargetString(op.Value, op.Timestamp)} ({op.Value.Targets.Count} targets affected) #{op.Value.GlobalSequence}",
                ActorState.OpStatus op => $"Status change: {ActorString(op.InstanceID, op.Timestamp)} #{op.Index}: {StatusesString(op.InstanceID, op.Index, op.Timestamp)}",
                ActorState.OpIcon op => $"Icon: {ActorString(op.InstanceID, op.Timestamp)} -> {op.IconID} ({_moduleInfo?.IconIDType?.GetEnumName(op.IconID)})",
                ActorState.OpEventObjectStateChange op => $"EObjState: {ActorString(op.InstanceID, op.Timestamp)} = {op.State:X4}",
                ActorState.OpEventObjectAnimation op => $"EObjAnim: {ActorString(op.InstanceID, op.Timestamp)} = {((uint)op.Param1 << 16) | op.Param2:X8}",
                ActorState.OpPlayActionTimelineEvent op => $"Play action timeline: {ActorString(op.InstanceID, op.Timestamp)} = {op.ActionTimelineID:X4}",
                ActorState.OpEventNpcYell op => $"Yell: {ActorString(op.InstanceID, op.Timestamp)} = {op.Message} '{Service.LuminaRow<Lumina.Excel.GeneratedSheets.NpcYell>(op.Message)?.Text}'",
                _ => DumpOp(o)
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
            var action = _replay.Actions.Find(a => a.GlobalSequence == op.Value.GlobalSequence);
            if (action != null && action.Timestamp == op.Timestamp && action.Source.InstanceID == op.InstanceID)
            {
                foreach (var t in tree.Nodes(action.Targets, t => new(ReplayUtils.ActionTargetString(t, op.Timestamp))))
                {
                    tree.LeafNodes(t.Effects, ReplayUtils.ActionEffectString);
                }
            }
            else
            {
                foreach (var t in tree.Nodes(op.Value.Targets, t => new(ActorString(t.ID, op.Timestamp))))
                {
                    tree.LeafNodes(t.Effects, ReplayUtils.ActionEffectString);
                }
            }
        }

        private Action? OpContextMenu(WorldState.Operation o)
        {
            return o switch
            {
                WorldState.OpDirectorUpdate op => () => ContextMenuDirectorUpdate(op),
                ActorState.OpStatus op => () => ContextMenuActorStatus(op),
                ActorState.OpCastInfo op => () => ContextMenuActorCast(op),
                ActorState.OpCastEvent op => () => ContextMenuEventCast(op),
                ActorState.Operation op => () => ContextMenuActor(op),
                _ => null,
            };
        }

        private void ContextMenuDirectorUpdate(WorldState.OpDirectorUpdate op)
        {
            if (ImGui.MenuItem($"Filter out type {op.UpdateID:X8}"))
            {
                _filteredDirectorUpdateTypes.Add(op.UpdateID);
                _nodesUpToDate = false;
            }
        }

        private void ContextMenuActor(ActorState.Operation op)
        {
            var oid = _replay.FindParticipant(op.InstanceID, op.Timestamp)!.OID;
            if (ImGui.MenuItem($"Filter out OID {oid:X}"))
            {
                _filteredOIDs.Add(oid);
                _nodesUpToDate = false;
            }
        }

        private void ContextMenuActorStatus(ActorState.OpStatus op)
        {
            ContextMenuActor(op);
            foreach (var s in FindStatuses(op.InstanceID, op.Index, op.Timestamp))
            {
                if (ImGui.MenuItem($"Filter out {Utils.StatusString(s.ID)}"))
                {
                    _filteredStatuses.Add(s.ID);
                    _nodesUpToDate = false;
                }
            }
        }

        private void ContextMenuActorCast(ActorState.OpCastInfo op)
        {
            ContextMenuActor(op);
            var cast = FindCast(_replay.FindParticipant(op.InstanceID, op.Timestamp), op.Timestamp, op.Value != null);
            if (cast != null && ImGui.MenuItem($"Filter out {cast.ID}"))
            {
                _filteredActions.Add(cast.ID);
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

        private IEnumerable<Replay.Status> FindStatuses(ulong instanceID, int index, DateTime timestamp) => _replay.Statuses.Where(s => s.Target.InstanceID == instanceID && s.Index == index && (s.Time.Start == timestamp || s.Time.End == timestamp));
        private Replay.Cast? FindCast(Replay.Participant? participant, DateTime timestamp, bool start) => participant?.Casts.Find(c => (start ? c.Time.Start : c.Time.End) == timestamp);

        private string ActorString(Replay.Participant? p, DateTime timestamp)
        {
            return p != null ? $"{ReplayUtils.ParticipantString(p, timestamp)} ({_moduleInfo?.ObjectIDType?.GetEnumName(p.OID)}) {Utils.PosRotString(p.PosRotAt(timestamp))}" : "<none>";
        }

        private string ActorString(ulong instanceID, DateTime timestamp)
        {
            var p = _replay.FindParticipant(instanceID, timestamp);
            return p != null || instanceID == 0 ? ActorString(p, timestamp) : $"<unknown> {instanceID:X}";
        }

        private string CastEventTargetString(ActorCastEvent ev, DateTime timestamp)
        {
            if (ev.MainTargetID != 0 && ev.MainTargetID != 0xE0000000u)
                return ActorString(ev.MainTargetID, timestamp);
            else
                return Utils.Vec3String(ev.TargetPos);
        }

        private string CastString(ulong instanceID, DateTime timestamp, bool start)
        {
            var p = _replay.FindParticipant(instanceID, timestamp);
            var c = FindCast(p, timestamp, start);
            if (c == null)
                return $"{ActorString(p, timestamp)}: <unknown cast>";
            return $"{ActorString(p, timestamp)}: {c.ID} ({_moduleInfo?.ActionIDType?.GetEnumName(c.ID.ID)}), {c.ExpectedCastTime:f2}s ({c.Time} actual){(c.Interruptible ? " (interruptible)" : "")} @ {ReplayUtils.ParticipantString(c.Target, timestamp)} {Utils.Vec3String(c.Location)} / {c.Rotation}";
        }

        private string StatusesString(ulong instanceID, int index, DateTime timestamp)
        {
            IEnumerable<string> Classify(Replay.Status s)
            {
                if (s.Time.Start == timestamp)
                    yield return "gain";
                if (s.Time.End == timestamp)
                    yield return "lose";
            }
            return string.Join("; ", FindStatuses(instanceID, index, timestamp).Select(s => $"{string.Join("/", Classify(s))} {Utils.StatusString(s.ID)} ({_moduleInfo?.StatusIDType?.GetEnumName(s.ID)}) ({s.StartingExtra:X}), {s.InitialDuration:f2}s / {s.Time}, from {ActorString(s.Source, timestamp)}"));
        }
    }
}
