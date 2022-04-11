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
        private Action<DateTime> _scrollTo;
        private List<(DateTime Timestamp, string Text, Action<Tree>? Children)> _nodes = new();

        public OpList(Replay r, IEnumerable<ReplayOps.Operation> ops, Action<DateTime> scrollTo)
        {
            _replay = r;
            _scrollTo = scrollTo;
            foreach (var op in ops.Where(FilterOp))
            {
                _nodes.Add((op.Timestamp, OpName(op), OpChildren(op)));
            }
        }

        public void Draw(Tree tree, DateTime reference)
        {
            //foreach (var n in _tree.Node("Settings"))
            //{
            //    DrawSettings();
            //}

            foreach (var node in _nodes)
            {
                bool? activated = null;
                foreach (var n in tree.Node($"{(node.Timestamp - reference).TotalSeconds:f3}: {node.Text}", node.Children == null))
                {
                    activated = ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);
                    if (node.Children != null)
                        node.Children(tree);
                }

                if (activated == null)
                    activated = ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);
                if (activated.Value)
                    _scrollTo(node.Timestamp);
            }
        }

        private bool IsActorPlayerRelated(uint instanceID, DateTime timestamp)
        {
            var p = _replay.Participants.Find(p => p.InstanceID == instanceID && p.Existence.Contains(timestamp));
            return p?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo or ActorType.Area;
        }

        private bool FilterOp(ReplayOps.Operation o)
        {
            return o switch
            {
                ReplayOps.OpActorCreate op => !IsActorPlayerRelated(op.InstanceID, op.Timestamp),
                ReplayOps.OpActorDestroy op => !IsActorPlayerRelated(op.InstanceID, op.Timestamp),
                ReplayOps.OpActorMove => false,
                ReplayOps.OpActorHP => false,
                ReplayOps.OpActorTargetable op => !IsActorPlayerRelated(op.InstanceID, op.Timestamp),
                ReplayOps.OpActorCombat => false,
                ReplayOps.OpActorTarget => false, // reconsider...
                ReplayOps.OpActorCast op => !IsActorPlayerRelated(op.InstanceID, op.Timestamp),
                ReplayOps.OpActorStatus op => !(FindStatus(op.InstanceID, op.Index, op.Timestamp, op.Value.ID != 0)?.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo),
                ReplayOps.OpEventCast op => !IsActorPlayerRelated(op.Value.CasterID, op.Timestamp),
                _ => true
            };
        }

        private string OpName(ReplayOps.Operation o)
        {
            return o switch
            {
                ReplayOps.OpActorCreate op => $"Actor create: {ActorString(op.InstanceID, op.Timestamp)}",
                ReplayOps.OpActorDestroy op => $"Actor destroy: {ActorString(op.InstanceID, op.Timestamp)}",
                ReplayOps.OpActorRename op => $"Actor rename: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Name}",
                ReplayOps.OpActorClassChange op => $"Actor class change: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Class}",
                ReplayOps.OpActorTargetable op => $"Actor targetable: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Value}",
                ReplayOps.OpActorDead op => $"Actor dead: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Value}",
                ReplayOps.OpActorCast op => $"Actor cast {(op.Value != null ? "started" : "ended")}: {CastString(op.InstanceID, op.Timestamp)}",
                ReplayOps.OpActorTether op => $"Actor tether: {ActorString(op.InstanceID, op.Timestamp)} {op.Value.ID} @ {ActorString(op.Value.Target, op.Timestamp)}",
                ReplayOps.OpActorStatus op => $"Actor status {(op.Value.ID != 0 ? "gain" : "lose")}: {StatusString(op.InstanceID, op.Index, op.Timestamp, op.Value.ID != 0)}",
                ReplayOps.OpEventIcon op => $"Actor icon: {ActorString(op.InstanceID, op.Timestamp)} -> {op.IconID}",
                ReplayOps.OpEventCast op => $"Cast event: {ActorString(op.Value.CasterID, op.Timestamp)}: {op.Value.Action} @ {ActorString(op.Value.MainTargetID, op.Timestamp)} ({op.Value.Targets.Count} targets affected)",
                _ => o.ToString() ?? o.GetType().Name
            };
        }

        private Action<Tree>? OpChildren(ReplayOps.Operation o)
        {
            return o switch
            {
                ReplayOps.OpEventCast op => op.Value.Targets.Count != 0 ? tree => DrawEventCast(tree, op) : null,
                _ => null
            };
        }

        private void DrawEventCast(Tree tree, ReplayOps.OpEventCast op)
        {
            foreach (var t in tree.Nodes(op.Value.Targets, t => (ActorString(t.ID, op.Timestamp), false)))
            {
                tree.LeafNodes(t.Effects, ReplayUtils.ActionEffectString);
            }
        }

        private Replay.Participant? FindParticipant(uint instanceID, DateTime timestamp) => _replay.Participants.Find(p => p.InstanceID == instanceID && p.Existence.Contains(timestamp));
        private Replay.Status? FindStatus(uint instanceID, int index, DateTime timestamp, bool gain) => _replay.Statuses.Find(s => s.Target?.InstanceID == instanceID && s.Index == index && (gain ? s.Time.Start : s.Time.End) == timestamp);

        private string ActorString(uint instanceID, DateTime timestamp)
        {
            return ReplayUtils.ParticipantString(FindParticipant(instanceID, timestamp));
        }

        private string CastString(uint instanceID, DateTime timestamp)
        {
            var p = FindParticipant(instanceID, timestamp);
            var c = p?.Casts.Find(c => c.Time.Contains(timestamp))!;
            return $"{ReplayUtils.ParticipantString(p)}: {c.ID}, {c.ExpectedCastTime:f2}s ({c.Time} actual) @ {ReplayUtils.ParticipantString(c.Target)} {Utils.Vec3String(c.Location)}";
        }

        private string StatusString(uint instanceID, int index, DateTime timestamp, bool gain)
        {
            var s = FindStatus(instanceID, index, timestamp, gain)!;
            return $"{ReplayUtils.ParticipantString(s.Target)}: {Utils.StatusString(s!.ID)} ({s.StartingExtra:X}), {s.InitialDuration:f2}s / {s.Time}, from {ReplayUtils.ParticipantString(s.Source)}";
        }
    }
}
