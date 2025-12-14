using Dalamud.Bindings.ImGui;
using System.IO;
using System.Text;

namespace BossMod.ReplayVisualization;

class OpList(Replay replay, Replay.Encounter? enc, BossModuleRegistry.Info? moduleInfo, IEnumerable<WorldState.Operation> ops, Action<DateTime> scrollTo)
{
    public readonly Replay.Encounter? Encounter = enc;
    public readonly BossModuleRegistry.Info? ModuleInfo = moduleInfo;
    private DateTime _relativeTS;
    private readonly List<(int Index, DateTime Timestamp, string Text, Action<UITree>? Children, Action? ContextMenu)> _nodes = [];
    private readonly HashSet<uint> _filteredOIDs = [];
    private readonly HashSet<ActionID> _filteredActions = [];
    private readonly HashSet<uint> _filteredStatuses = [];
    private readonly HashSet<uint> _filteredDirectorUpdateTypes = [];
    private bool _nodesUpToDate;

    public bool ShowActorSizeEvents
    {
        get;
        set
        {
            field = value;
            _nodesUpToDate = false;
        }
    } = true;

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
            foreach (var op in ops)
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
            foreach (var n in tree.Node($"{(node.Timestamp - timeRef).TotalSeconds:f3}: {node.Text}###{node.Index}", node.Children == null, 0xffffffff, node.ContextMenu, () => scrollTo(node.Timestamp), () => _relativeTS = node.Timestamp))
            {
                node.Children?.Invoke(tree);
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
        var p = replay.FindParticipant(instanceID, timestamp)!;
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
        if (s.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo or ActorType.DutySupport)
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
            ActorState.OpSizeChange op => ShowActorSizeEvents && FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpHPMP => false,
            ActorState.OpTargetable op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpDead op => FilterInterestingActor(op.InstanceID, op.Timestamp, true),
            ActorState.OpCombat op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpEventState op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpTarget op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpCastInfo op => FilterInterestingActor(op.InstanceID, op.Timestamp, false) && !_filteredActions.Contains(FindCast(replay.FindParticipant(op.InstanceID, op.Timestamp), op.Timestamp, op.Value != null)?.ID ?? new()),
            ActorState.OpCastEvent op => FilterInterestingActor(op.InstanceID, op.Timestamp, false) && !_filteredActions.Contains(op.Value.Action),
            ActorState.OpStatus op => FilterInterestingStatuses(op.InstanceID, op.Index, op.Timestamp),
            ActorState.OpEffectResult => false,
            PartyState.OpLimitBreakChange => false,
            ClientState.OpActionRequest => false,
            ClientState.OpForcedMovementDirectionChange => false,
            //ClientState.OpActionReject => false,
            ClientState.OpProcTimersChange => false,
            ClientState.OpAnimationLockChange => false,
            ClientState.OpComboChange => false,
            ClientState.OpCooldown => false,
            ClientState.OpHateChange => false,
            ActorState.OpIncomingEffect => false,
            NetworkState.OpServerIPC => false,
            _ => true
        };
    }

    private string DumpOp(WorldState.Operation op)
    {
        using var stream = new MemoryStream(1024);
        var writer = new ReplayRecorder.TextOutput(stream, null);
        op.Write(writer);
        writer.Flush();
        stream.Position = 0;
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        var start = Array.IndexOf(bytes, (byte)'|') + 1;
        return Encoding.UTF8.GetString(bytes, start, bytes.Length - start);
    }

    private string OpName(WorldState.Operation o)
    {
        return o switch
        {
            ActorState.OpCreate op => $"Actor create: {ActorString(op.InstanceID, op.Timestamp)} #{op.SpawnIndex}",
            ActorState.OpDestroy op => $"Actor destroy: {ActorString(op.InstanceID, op.Timestamp)}",
            ActorState.OpRename op => $"Actor rename: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Name}",
            ActorState.OpClassChange op => $"Actor class change: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Class} L{op.Level}",
            ActorState.OpTargetable op => $"{(op.Value ? "Targetable" : "Untargetable")}: {ActorString(op.InstanceID, op.Timestamp)}",
            ActorState.OpDead op => $"{(op.Value ? "Die" : "Resurrect")}: {ActorString(op.InstanceID, op.Timestamp)}",
            ActorState.OpAggroPlayer op => $"Aggro player: {ActorString(op.InstanceID, op.Timestamp)} = {op.Has}",
            ActorState.OpEventState op => $"Event state: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Value}",
            ActorState.OpTarget op => $"Target: {ActorString(op.InstanceID, op.Timestamp)} -> {ActorString(op.Value, op.Timestamp)}",
            ActorState.OpMount op => $"Mount: {ActorString(op.InstanceID, op.Timestamp)} = {Service.LuminaRow<Lumina.Excel.Sheets.Mount>(op.Value)?.Singular ?? "<unknown>"}",
            ActorState.OpTether op => $"Tether: {ActorString(op.InstanceID, op.Timestamp)} {op.Value.ID} ({ModuleInfo?.TetherIDType?.GetEnumName(op.Value.ID)}) @ {ActorString(op.Value.Target, op.Timestamp)}",
            ActorState.OpCastInfo op => $"Cast {(op.Value != null ? "started" : "ended")}: {CastString(op.InstanceID, op.Timestamp, op.Value != null)}",
            ActorState.OpCastEvent op => $"Cast event: {ActorString(op.InstanceID, op.Timestamp)}: {op.Value.Action} ({ModuleInfo?.ActionIDType?.GetEnumName(op.Value.Action.ID)}) @ {CastEventTargetString(op.Value, op.Timestamp)} ({op.Value.Targets.Count} targets affected) #{op.Value.GlobalSequence}",
            ActorState.OpStatus op => $"Status change: {ActorString(op.InstanceID, op.Timestamp)} #{op.Index}: {StatusesString(op.InstanceID, op.Index, op.Timestamp)}",
            ActorState.OpIcon op => $"Icon: {ActorString(op.InstanceID, op.Timestamp)} -> {ActorString(op.TargetID, op.Timestamp)}: {op.IconID} ({ModuleInfo?.IconIDType?.GetEnumName(op.IconID)})",
            ActorState.OpVFX op => $"VFX: {ActorString(op.InstanceID, op.Timestamp)} -> {ActorString(op.TargetID, op.Timestamp)}: {op.VfxID}",
            ActorState.OpEventObjectStateChange op => $"EObjState: {ActorString(op.InstanceID, op.Timestamp)} = {op.State:X4}",
            ActorState.OpEventObjectAnimation op => $"EObjAnim: {ActorString(op.InstanceID, op.Timestamp)} = {((uint)op.Param1 << 16) | op.Param2:X8}",
            ActorState.OpPlayActionTimelineEvent op => $"Play action timeline: {ActorString(op.InstanceID, op.Timestamp)} = {op.ActionTimelineID:X4}",
            ActorState.OpEventNpcYell op => $"Yell: {ActorString(op.InstanceID, op.Timestamp)} = {op.Message} '{Service.LuminaRow<Lumina.Excel.Sheets.NpcYell>(op.Message)?.Text}'",
            ClientState.OpDutyActionsChange op => $"Player duty actions change: {string.Join(", ", op.Slots)}",
            ClientState.OpBozjaHolsterChange op => $"Player bozja holster change: {string.Join(", ", op.Contents.Select(e => $"{e.count}x {e.entry}"))}",
            ClientState.OpPlayerStatsChange op => $"Player stats: sks={op.Value.SkillSpeed}, sps={op.Value.SpellSpeed}, haste={op.Value.Haste}",
            ClientState.OpBlueMageSpellsChange op => $"Player BLU spellbook: {string.Join(", ", op.Values.Select(v => new ActionID(ActionType.Spell, v)))}",
            ClientState.OpClassJobLevelsChange op => $"Player levels: {string.Join(", ", op.Values)}",
            ClientState.OpActiveFateChange op => $"FATE: {op.Value.ID} '{Service.LuminaRow<Lumina.Excel.Sheets.Fate>(op.Value.ID)?.Name}' {op.Value.Progress}%",
            ClientState.OpActivePetChange op => $"Player pet: {ActorString(op.Value.InstanceID, op.Timestamp)}",
            ClientState.OpInventoryChange op => $"Item quantity: {op.ItemId % 500000} '{Service.LuminaRow<Lumina.Excel.Sheets.Item>(op.ItemId % 500000)?.Name}' (hq={op.ItemId > 1000000}) x{op.Quantity}",
            PartyState.OpModify op => $"Party slot {op.Slot}: {op.Member.InstanceId:X8} {op.Member.Name}",
            WorldState.OpMapEffect op => $"MapEffect: {op.Index:X2} {op.State:X8}",
            WorldState.OpLegacyMapEffect op => $"MapEffect (legacy): seq={op.Sequence} param={op.Param} data={string.Join(" ", op.Data.Select(d => d.ToString("X2")))}",
            WorldState.OpSystemLogMessage op => $"LogMessage {op.MessageId}: '{Service.LuminaRow<Lumina.Excel.Sheets.LogMessage>(op.MessageId)?.Text}' [{string.Join(", ", op.Args)}]",
            WorldState.OpZoneChange op => $"Zone change: {op.Zone} ({Service.LuminaRow<Lumina.Excel.Sheets.TerritoryType>(op.Zone)?.PlaceName.Value.Name}) / {op.CFCID} ({(op.CFCID > 0 ? Service.LuminaRow<Lumina.Excel.Sheets.ContentFinderCondition>(op.CFCID)?.Name : "n/a")})",
            WaymarkState.OpSignChange op => op.Target == 0 ? $"Sign: {op.ID} cleared" : $"Sign: {op.ID} on {ActorString(op.Target, op.Timestamp)}",
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
        var action = replay.Actions.Find(a => a.GlobalSequence == op.Value.GlobalSequence);
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
        var oid = replay.FindParticipant(op.InstanceID, op.Timestamp)!.OID;
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
        var cast = FindCast(replay.FindParticipant(op.InstanceID, op.Timestamp), op.Timestamp, op.Value != null);
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

    private IEnumerable<Replay.Status> FindStatuses(ulong instanceID, int index, DateTime timestamp) => replay.Statuses.Where(s => s.Target.InstanceID == instanceID && s.Index == index && (s.Time.Start == timestamp || s.Time.End == timestamp));
    private Replay.Cast? FindCast(Replay.Participant? participant, DateTime timestamp, bool start) => participant?.Casts.Find(c => (start ? c.Time.Start : c.Time.End) == timestamp);

    private string ActorString(Replay.Participant? p, DateTime timestamp)
        => p != null ? $"{ReplayUtils.ParticipantString(p, timestamp)} ({ModuleInfo?.ObjectIDType?.GetEnumName(p.OID)}) {Utils.PosRotString(p.PosRotAt(timestamp))}" : "<none>";

    private string ActorString(ulong instanceID, DateTime timestamp)
    {
        var p = replay.FindParticipant(instanceID, timestamp);
        return p != null || instanceID == 0 ? ActorString(p, timestamp) : $"<unknown> {instanceID:X}";
    }

    private string CastEventTargetString(ActorCastEvent ev, DateTime timestamp) => $"{ActorString(ev.MainTargetID, timestamp)} / {Utils.Vec3String(ev.TargetPos)} / {ev.Rotation}";

    private string CastString(ulong instanceID, DateTime timestamp, bool start)
    {
        var p = replay.FindParticipant(instanceID, timestamp);
        var c = FindCast(p, timestamp, start);
        if (c == null)
            return $"{ActorString(p, timestamp)}: <unknown cast>";
        return $"{ActorString(p, timestamp)}: {c.ID} ({ModuleInfo?.ActionIDType?.GetEnumName(c.ID.ID)}), {c.ExpectedCastTime:f2}s ({c.Time} actual){(c.Interruptible ? " (interruptible)" : "")} @ {ReplayUtils.ParticipantPosRotString(c.Target, timestamp)} / {Utils.Vec3String(c.Location)} / {c.Rotation}";
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
        return string.Join("; ", FindStatuses(instanceID, index, timestamp).Select(s => $"{string.Join("/", Classify(s))} {Utils.StatusString(s.ID)} ({ModuleInfo?.StatusIDType?.GetEnumName(s.ID)}) ({s.StartingExtra:X}), {s.InitialDuration:f2}s / {s.Time}, from {ActorString(s.Source, timestamp)}"));
    }
}
