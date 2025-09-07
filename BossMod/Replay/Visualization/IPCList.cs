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
            var p = replay.Participants.Where(p => p.InstanceID == instanceID).MinBy(p => p.EffectiveExistence.Distance(Now));
            var adjNow = p == null ? Now : Now < p.EffectiveExistence.Start ? p.EffectiveExistence.Start : Now > p.EffectiveExistence.End ? p.EffectiveExistence.End : Now;
            return p != null || instanceID == 0 ? ReplayUtils.ParticipantPosRotString(p, adjNow) : $"<unknown> {instanceID:X}";
        }

        protected override NetworkState.IDScrambleFields GetScramble()
        {
            return replay.Ops.OfType<NetworkState.OpIDScramble>().TakeWhile(p => p.Timestamp <= Now).LastOrDefault()?.Fields ?? default;
        }
    }

    public readonly Replay.Encounter? Encounter = enc;
    private readonly Decoder _decoder = new(replay);
    private DateTime _relativeTS;
    private List<(int index, NetworkState.OpServerIPC op, PacketDecoder.TextNode data)>? _nodes;
    private readonly HashSet<PacketID> _filteredPackets = [
        PacketID.ActorMove,
        PacketID.ActorControl,
        PacketID.ActorControlSelf,
        PacketID.UpdateHate,
        PacketID.UpdateHater,
        PacketID.EffectResult1,
        PacketID.ActionEffect1,
        PacketID.ActionEffect8,
        PacketID.EffectResultBasic1,
        PacketID.StatusEffectList,
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
        int index = 0;
        _nodes ??= [.. ops.OfType<NetworkState.OpServerIPC>().Where(FilterOp).Select(op => (++index, op, _decoder.Decode(op.Packet, op.Timestamp)))];

        var timeRef = ImGui.GetIO().KeyShift && _relativeTS != default ? _relativeTS : reference;
        foreach (var n in tree.Nodes(_nodes, n => new($"{(n.op.Timestamp - timeRef).TotalSeconds:f3}: {n.data.Text}###{n.index}", n.data.Children == null), n => ContextMenu(n.op), n => scrollTo(n.op.Timestamp), n => _relativeTS = n.op.Timestamp))
            DrawNodes(tree, n.data.Children);
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
            return;
        foreach (var n in tree.Nodes(nodes, n => new(n.Text, n.Children == null)))
            DrawNodes(tree, n.Children);
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
    }
}
