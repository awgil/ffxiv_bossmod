using BossMod.Network;
using BossMod.Network.ServerIPC;
using Dalamud.Bindings.ImGui;

namespace BossMod.ReplayVisualization;

sealed class IPCList(Replay replay, Replay.Encounter? enc, IEnumerable<WorldState.Operation> ops, Action<DateTime> scrollTo)
{
    sealed class Decoder(Replay replay) : PacketDecoder
    {
        protected override string DecodeActor(ulong instanceID)
        {
            // note that actors can be created with a few frames delay after packets arrive
            var now = Now;
            Replay.Participant? best = null;
            var bestDist = TimeSpan.MaxValue;
            var parts = replay.Participants;
            for (var i = 0; i < parts.Count; ++i)
            {
                var part = parts[i];
                if (part.InstanceID != instanceID)
                {
                    continue;
                }

                var dist = part.EffectiveExistence.Distance(now);
                if (best == null || dist < bestDist)
                {
                    best = part;
                    bestDist = dist;
                }
            }
            var p = best;
            var adjNow = p == null ? Now : Now < p.EffectiveExistence.Start ? p.EffectiveExistence.Start : Now > p.EffectiveExistence.End ? p.EffectiveExistence.End : Now;
            return p != null || instanceID == 0 ? ReplayUtils.ParticipantPosRotString(p, adjNow) : $"<unknown> {instanceID:X}";
        }

        protected override NetworkState.IDScrambleFields GetScramble()
        {
            NetworkState.OpIDScramble? lastScramble = null;
            var now = Now;
            var ops = replay.Ops;
            for (var i = 0; i < ops.Count; ++i)
            {
                if (ops[i].Timestamp > now)
                {
                    break;
                }

                if (ops[i] is NetworkState.OpIDScramble s)
                {
                    lastScramble = s;
                }
            }
            return lastScramble?.Fields ?? default;
        }
    }

    public readonly Replay.Encounter? Encounter = enc;
    private readonly Decoder _decoder = new(replay);
    private DateTime _relativeTS;
    private List<(int index, NetworkState.OpServerIPC op, Lazy<PacketDecoder.TextNode> data)>? _nodes;
    private readonly HashSet<PacketID> _filteredPackets = [
        PacketID.ActorMove,
        PacketID.UpdateHate,
        PacketID.UpdateHater,
        PacketID.UpdateHpMpTp,
        PacketID.ActorSetPos,
        PacketID.UpdateClassInfo,
        PacketID.PlayerStats,
        PacketID.CharaVisualEffect,
        PacketID.ItemInfo,
        PacketID.ContainerInfo
    ];
    private bool _filterInvert;

    public void Draw(UITree tree, DateTime reference)
    {
        if (_nodes == null)
        {
            _nodes = [];
            var idx = 0;
            foreach (var op in ops)
            {
                ++idx;
                if (op is NetworkState.OpServerIPC ipc && FilterOp(ipc))
                {
                    _nodes.Add((idx, ipc, new Lazy<PacketDecoder.TextNode>(() => _decoder.Decode(ipc.Packet, ipc.Timestamp))));
                }
            }
        }
        var timeRef = ImGui.GetIO().KeyShift && _relativeTS != default ? _relativeTS : reference;

        var c = new ImGuiListClipper();
        c.Begin(_nodes.Count, ImGui.GetFrameHeight() - 2);

        while (c.Step())
        {
            foreach (var n in tree.Nodes(_nodes[c.DisplayStart..c.DisplayEnd], n => new($"{(n.op.Timestamp - timeRef).TotalSeconds:f3}: {n.data.Value.Text}###{n.index}", n.data.Value.Children == null), n => ContextMenu(n.op), n => scrollTo(n.op.Timestamp), n => _relativeTS = n.op.Timestamp))
            {
                DrawNodes(tree, n.data.Value.Children);
            }
        }

        c.End();
    }

    public void ClearFilters()
    {
        _filteredPackets.Clear();
        _filterInvert = false;
        _nodes = null;
    }

    private void DrawNodes(UITree tree, List<PacketDecoder.TextNode>? nodes)
    {
        if (nodes == null)
        {
            return;
        }

        foreach (var n in tree.Nodes(nodes, n => new(n.Text, n.Children == null)))
        {
            DrawNodes(tree, n.Children);
        }
    }

    private bool FilterOp(NetworkState.OpServerIPC op) => _filterInvert ? _filteredPackets.Contains(op.Packet.ID) : !_filteredPackets.Contains(op.Packet.ID);

    private void ContextMenu(NetworkState.OpServerIPC op)
    {
        if (ImGui.MenuItem($"Filter out opcode {op.Packet.ID}"))
        {
            _filteredPackets.Add(op.Packet.ID);
            _filterInvert = false;
            _nodes = null;
        }
        if (ImGui.MenuItem($"Focus opcode {op.Packet.ID}"))
        {
            _filteredPackets.Clear();
            _filteredPackets.Add(op.Packet.ID);
            _filterInvert = true;
            _nodes = null;
        }
        ImGui.Separator();
        if (ImGui.MenuItem("Jump to timestamp", "double click"))
            scrollTo(op.Timestamp);
    }
}
