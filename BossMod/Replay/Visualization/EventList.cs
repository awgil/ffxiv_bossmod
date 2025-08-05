using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using System.Runtime.InteropServices;

namespace BossMod.ReplayVisualization;

class EventList(Replay r, Action<DateTime> scrollTo, PlanDatabase planDB, ReplayDetailsWindow timelineSync)
{
    record struct Lists(OpList? Ops, IPCList? IPCs);

    private readonly UITree _tree = new();
    private Lists _listsRaw;
    private readonly Dictionary<Replay.Encounter, Lists> _listsFiltered = [];

    public void Draw()
    {
        foreach (var n in _tree.Node("Full data"))
        {
            foreach (var no in _tree.Node("Raw ops", contextMenu: () => OpListContextMenu(_listsRaw.Ops)))
            {
                _listsRaw.Ops ??= new(r, null, null, r.Ops, scrollTo);
                _listsRaw.Ops.Draw(_tree, r.Ops[0].Timestamp);
            }
            foreach (var no in _tree.Node("Server IPCs", contextMenu: () => IPCListContextMenu(_listsRaw.IPCs, null)))
            {
                _listsRaw.IPCs ??= new(r, null, r.Ops, scrollTo);
                _listsRaw.IPCs.Draw(_tree, r.Ops[0].Timestamp);
            }

            DrawContents(null, null);
            DrawUserMarkers();
        }
        foreach (var e in _tree.Nodes(r.Encounters, e => new($"{BossModuleRegistry.FindByOID(e.OID)?.ModuleType.Name}: {e.InstanceID:X}, zone={e.Zone}, start={e.Time.Start:O}, duration={e.Time}, countdown on pull={e.CountdownOnPull:f3}")))
        {
            var moduleInfo = BossModuleRegistry.FindByOID(e.OID);
            ref var lists = ref CollectionsMarshal.GetValueRefOrAddDefault(_listsFiltered, e, out _);
            foreach (var n in _tree.Node("Raw ops", contextMenu: () => OpListContextMenu(_listsFiltered[e].Ops)))
            {
                lists.Ops ??= new(r, e, moduleInfo, r.Ops.SkipWhile(o => o.Timestamp < e.Time.Start).TakeWhile(o => o.Timestamp <= e.Time.End), scrollTo);
                lists.Ops.Draw(_tree, e.Time.Start);
            }
            foreach (var n in _tree.Node("Server IPCs", contextMenu: () => IPCListContextMenu(_listsFiltered[e].IPCs, moduleInfo)))
            {
                lists.IPCs ??= new(r, e, r.Ops.SkipWhile(o => o.Timestamp < e.Time.Start).TakeWhile(o => o.Timestamp <= e.Time.End), scrollTo);
                lists.IPCs.Draw(_tree, e.Time.Start);
            }

            DrawContents(e, moduleInfo);
            DrawEncounterDetails(e, TimePrinter(e.Time.Start));
            DrawTimelines(e);
        }
    }

    private void DrawContents(Replay.Encounter? filter, BossModuleRegistry.Info? moduleInfo)
    {
        var oidType = moduleInfo?.ObjectIDType;
        var aidType = moduleInfo?.ActionIDType;
        var sidType = moduleInfo?.StatusIDType;
        var tidType = moduleInfo?.TetherIDType;
        var iidType = moduleInfo?.IconIDType;
        var reference = filter?.Time.Start ?? r.Ops[0].Timestamp;
        var tp = TimePrinter(reference);
        var actions = filter != null ? r.EncounterActions(filter) : r.Actions;
        var statuses = filter != null ? r.EncounterStatuses(filter) : r.Statuses;
        var tethers = filter != null ? r.EncounterTethers(filter) : r.Tethers;
        var icons = filter != null ? r.EncounterIcons(filter) : r.Icons;
        var envControls = filter != null ? r.EncounterEnvControls(filter) : r.EnvControls;
        var dirus = filter != null ? r.EncounterDirectorUpdates(filter) : r.DirectorUpdates;

        foreach (var n in _tree.Node("Participants"))
        {
            if (filter == null)
            {
                DrawParticipants(r.Participants, actions, statuses, tp, reference, filter, aidType, sidType);
            }
            else
            {
                foreach (var (oid, list) in _tree.Nodes(filter.ParticipantsByOID, kv => new($"{kv.Key:X} '{oidType?.GetEnumName(kv.Key)}' ({kv.Value.Count} objects)")))
                {
                    DrawParticipants(list, actions, statuses, tp, reference, filter, aidType, sidType);
                }
            }
        }

        var boss = filter?.ParticipantsByOID[filter.OID].Find(p => p.InstanceID == filter.InstanceID);
        if (boss != null)
        {
            foreach (var n in _tree.Node("Boss casts", boss.Casts.Count == 0))
            {
                DrawCasts(boss.Casts, reference, aidType);
            }
        }

        bool haveActions = actions.Any();
        bool actionIsCrap(Replay.Action a) => a.Source.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo || a.Source.AllyAt(a.Timestamp) || Service.LuminaRow<Lumina.Excel.Sheets.Action>(a.ID.ID)?.ActionCategory.RowId == 1;
        foreach (var n in _tree.Node("Interesting actions", !haveActions))
        {
            DrawActions(actions.Where(a => !actionIsCrap(a)), tp, aidType);
        }
        foreach (var n in _tree.Node("Other actions", !haveActions))
        {
            DrawActions(actions.Where(actionIsCrap), tp, aidType);
        }

        bool haveStatuses = statuses.Any();
        bool statusIsCrap(Replay.Status s) => s.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo || s.Target.Type is ActorType.Pet or ActorType.Chocobo;
        foreach (var n in _tree.Node("Interesting statuses", !haveStatuses))
        {
            DrawStatuses(statuses.Where(s => !statusIsCrap(s)), tp, sidType);
        }
        foreach (var n in _tree.Node("Other statuses", !haveStatuses))
        {
            DrawStatuses(statuses.Where(statusIsCrap), tp, sidType);
        }

        foreach (var n in _tree.Node("Tethers", !tethers.Any()))
        {
            _tree.LeafNodes(tethers, t => $"{tp(t.Time.Start)} + {t.Time}: {t.ID} ({tidType?.GetEnumName(t.ID)}) @ {ReplayUtils.ParticipantString(t.Source, t.Time.Start)} -> {ReplayUtils.ParticipantString(t.Target, t.Time.Start)}");
        }

        foreach (var n in _tree.Node("Icons", !icons.Any()))
        {
            _tree.LeafNodes(icons, i => $"{tp(i.Timestamp)}: {i.ID} ({iidType?.GetEnumName(i.ID)}) @ {ReplayUtils.ParticipantString(i.Source, i.Timestamp)} -> {ReplayUtils.ParticipantString(i.Target, i.Timestamp)}");
        }

        foreach (var n in _tree.Node("EnvControls", !envControls.Any()))
        {
            if (envControls.Any())
            {
                foreach (var n2 in _tree.Node("All"))
                {
                    _tree.LeafNodes(envControls, ec => $"{tp(ec.Timestamp)}: {ec.Index:X2} = {ec.State:X8}");
                }
            }
            foreach (var index in _tree.Nodes(new SortedSet<byte>(envControls.Select(ec => ec.Index)), index => new($"Index {index:X2}")))
            {
                _tree.LeafNodes(envControls.Where(ec => ec.Index == index), ec => $"{tp(ec.Timestamp)}: {ec.Index:X2} = {ec.State:X8}");
            }
        }

        foreach (var n in _tree.Node("Director updates", !dirus.Any()))
        {
            if (dirus.Any())
                foreach (var n2 in _tree.Node("All"))
                    _tree.LeafNodes(dirus, d => $"{tp(d.Timestamp)}: {d.UpdateID:X8} [0x{d.Param1:X}, 0x{d.Param2:X}, 0x{d.Param3:X}, 0x{d.Param4:X}]");

            foreach (var ix in _tree.Nodes(new SortedSet<uint>(dirus.Select(d => d.UpdateID)), index => new($"ID {index:X4}")))
            {
                _tree.LeafNodes(dirus.Where(d => d.UpdateID == ix), d => $"{tp(d.Timestamp)}: {d.UpdateID:X8} [0x{d.Param1:X}, 0x{d.Param2:X}, 0x{d.Param3:X}, 0x{d.Param4:X}]");
            }
        }
    }

    private void DrawParticipants(IEnumerable<Replay.Participant> list, IEnumerable<Replay.Action> actions, IEnumerable<Replay.Status> statuses, Func<DateTime, string> tp, DateTime reference, Replay.Encounter? filter, Type? aidType, Type? sidType)
    {
        foreach (var p in _tree.Nodes(list, p => new($"{ReplayUtils.ParticipantString(p, p.WorldExistence.FirstOrDefault().Start)}: first seen at {tp(p.EffectiveExistence.Start)}, last seen at {tp(p.EffectiveExistence.End)}")))
        {
            foreach (var n in _tree.Node("Existence", p.WorldExistence.Count == 0))
            {
                _tree.LeafNodes(p.WorldExistence, r => $"{tp(r.Start)}-{tp(r.End)} ({r})");
            }
            foreach (var n in _tree.Node("Casts", p.Casts.Count == 0))
            {
                DrawCasts(p.Casts, reference, aidType);
            }
            foreach (var an in _tree.Node("Actions", !p.HasAnyActions))
            {
                DrawActions(actions.Where(a => a.Source == p), tp, aidType);
            }
            foreach (var an in _tree.Node("Affected by actions", !p.IsTargetOfAnyActions))
            {
                DrawActions(actions.Where(a => a.Targets.Any(t => t.Target == p)), tp, aidType);
            }
            foreach (var an in _tree.Node("Statuses", !p.HasAnyStatuses))
            {
                DrawStatuses(statuses.Where(s => s.Target == p), tp, sidType);
            }
            foreach (var an in _tree.Node("Targetable", p.TargetableHistory.Count == 0))
            {
                _tree.LeafNodes(p.TargetableHistory, r => $"{tp(r.Key)} = {r.Value}");
            }
            foreach (var an in _tree.Node("EObjAnim", p.EventObjectAnimation.Count == 0))
            {
                _tree.LeafNodes(p.EventObjectAnimation, r => $"{tp(r.Key)} = {r.Value:X8}");
            }
            foreach (var an in _tree.Node("Event state", p.EventState.Count == 0))
            {
                _tree.LeafNodes(p.EventState, r => $"{tp(r.Key)} = {r.Value}");
            }
            foreach (var an in _tree.Node("Action timeline events", p.ActionTimeline.Count == 0))
            {
                _tree.LeafNodes(p.ActionTimeline, r => $"{tp(r.Key)} = {r.Value:X4}");
            }
        }
    }

    private string CastString(Replay.Cast c, DateTime reference, DateTime prev, Type? aidType)
    {
        return $"{new Replay.TimeRange(reference, c.Time.Start)} ({new Replay.TimeRange(prev, c.Time.Start)}) + {c.ExpectedCastTime + 0.3f:f2} ({c.Time}): {c.ID} ({aidType?.GetEnumName(c.ID.ID)}) @ {ReplayUtils.ParticipantPosRotString(c.Target, c.Time.Start)} / {Utils.Vec3String(c.Location)} / {c.Rotation}";
    }

    private void DrawCasts(IEnumerable<Replay.Cast> list, DateTime reference, Type? aidType)
    {
        var prev = reference;
        foreach (var c in _tree.Nodes(list, c => new(CastString(c, reference, prev, aidType), true)))
        {
            prev = c.Time.End;
        }
    }

    private string ActionString(Replay.Action a, Func<DateTime, string> tp, Type? aidType)
    {
        return $"{tp(a.Timestamp)}: {a.ID} ({aidType?.GetEnumName(a.ID.ID)}): {ReplayUtils.ParticipantPosRotString(a.Source, a.Timestamp)} -> {ReplayUtils.ParticipantString(a.MainTarget, a.Timestamp)} {Utils.Vec3String(a.TargetPos)} ({a.Targets.Count} affected) #{a.GlobalSequence}";
    }

    private void DrawActions(IEnumerable<Replay.Action> list, Func<DateTime, string> tp, Type? aidType)
    {
        foreach (var a in _tree.Nodes(list, a => new(ActionString(a, tp, aidType), a.Targets.Count == 0)))
        {
            foreach (var t in _tree.Nodes(a.Targets, t => new(ReplayUtils.ActionTargetString(t, a.Timestamp))))
            {
                _tree.LeafNodes(t.Effects, ReplayUtils.ActionEffectString);
            }
        }
    }

    private string StatusString(Replay.Status s, Func<DateTime, string> tp, Type? sidType)
    {
        return $"{tp(s.Time.Start)} + {s.InitialDuration:f2} / {s.Time}: {Utils.StatusString(s.ID)} ({sidType?.GetEnumName(s.ID)}) ({s.StartingExtra:X}) @ {ReplayUtils.ParticipantString(s.Target, s.Time.Start)} from {ReplayUtils.ParticipantString(s.Source, s.Time.Start)}";
    }

    private void DrawStatuses(IEnumerable<Replay.Status> statuses, Func<DateTime, string> tp, Type? sidType)
    {
        _tree.LeafNodes(statuses, s => StatusString(s, tp, sidType));
    }

    private void DrawEncounterDetails(Replay.Encounter enc, Func<DateTime, string> tp)
    {
        foreach (var n in _tree.Node("State transitions", enc.States.Count == 0))
        {
            var enter = enc.Time.Start;
            foreach (var s in _tree.Nodes(enc.States, s => new($"{s.FullName:X}: {tp(enter)} - {tp(s.Exit)} = {new Replay.TimeRange(enter, s.Exit)} (expected {s.ExpectedDuration:f1})", true)))
            {
                enter = s.Exit;
            }
        }

        foreach (var n in _tree.Node("Errors", enc.Errors.Count == 0))
        {
            _tree.LeafNodes(enc.Errors, error => $"{tp(error.Timestamp)} [{error.CompType}] {error.Message}");
        }
    }

    private void DrawUserMarkers()
    {
        foreach (var n in _tree.Node("User markers", r.UserMarkers.Count == 0))
            _tree.LeafNodes(r.UserMarkers, kv => $"{kv.Key:O}: {kv.Value}");
    }

    private Func<DateTime, string> TimePrinter(DateTime start)
    {
        return t => new Replay.TimeRange(start, t).ToString();
    }

    private void OpenTimeline(Replay.Encounter enc, BitMask showPlayers)
    {
        _ = new ReplayTimelineWindow(r, enc, showPlayers, planDB, timelineSync);
    }

    private void DrawTimelines(Replay.Encounter enc)
    {
        if (ImGui.Button("Show timeline"))
            OpenTimeline(enc, new());
        ImGui.SameLine();
        for (int i = 0; i < enc.PartyMembers.Count; i++)
        {
            var (p, c, l) = enc.PartyMembers[i];
            if (ImGui.Button($"{c}{l} {p.NameHistory.FirstOrDefault().Value.name}"))
                OpenTimeline(enc, new(1u << i));
            ImGui.SameLine();
        }
        if (ImGui.Button("All"))
            OpenTimeline(enc, new((1u << enc.PartyMembers.Count) - 1));
    }

    private void OpListContextMenu(OpList? list)
    {
        if (list == null)
            return;

        if (ImGui.MenuItem("Clear filters"))
        {
            list.ClearFilters();
        }
        if (ImGui.MenuItem("Show actor-size events", "", list.ShowActorSizeEvents, true))
        {
            list.ShowActorSizeEvents = !list.ShowActorSizeEvents;
        }
        if (ImGui.MenuItem("Pop out"))
        {
            var windowName = $"Raw ops: {r.Path}, {(list.Encounter != null ? $"{list.ModuleInfo?.ModuleType.Name}: {list.Encounter.InstanceID:X} @ {list.Encounter.Time.Start} + {list.Encounter.Time}" : "full")}";
            _ = new UISimpleWindow(windowName, () => list.Draw(new(), list.Encounter?.Time.Start ?? r.Ops[0].Timestamp), true, new(1000, 1000));
        }
    }

    private void IPCListContextMenu(IPCList? list, BossModuleRegistry.Info? moduleInfo)
    {
        if (list == null)
            return;

        if (ImGui.MenuItem("Clear filters"))
        {
            list.ClearFilters();
        }
        if (ImGui.MenuItem("Pop out"))
        {
            var windowName = $"Server IPCs: {r.Path}, {(list.Encounter != null ? $"{moduleInfo?.ModuleType.Name}: {list.Encounter.InstanceID:X} @ {list.Encounter.Time.Start} + {list.Encounter.Time}" : "full")}";
            _ = new UISimpleWindow(windowName, () => list.Draw(new(), list.Encounter?.Time.Start ?? r.Ops[0].Timestamp), true, new(1000, 1000));
        }
    }
}
