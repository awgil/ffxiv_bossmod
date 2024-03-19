﻿using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.ReplayVisualization
{
    class EventList
    {
        private Replay _replay;
        private Action<DateTime> _scrollTo;
        private UITree _tree = new();
        private OpList? _opListRaw;
        private Dictionary<Replay.Encounter, OpList> _opListsFiltered = new();

        public EventList(Replay r, Action<DateTime> scrollTo)
        {
            _replay = r;
            _scrollTo = scrollTo;
        }

        public void Draw()
        {
            foreach (var n in _tree.Node("Full data"))
            {
                foreach (var no in _tree.Node("Raw ops", contextMenu: () => OpListContextMenu(_opListRaw)))
                {
                    if (_opListRaw == null)
                        _opListRaw = new(_replay, null, _replay.Ops, _scrollTo);
                    _opListRaw.Draw(_tree, _replay.Ops.First().Timestamp);
                }

                DrawContents(null, null);
                DrawUserMarkers();
            }
            foreach (var e in _tree.Nodes(_replay.Encounters, e => new($"{ModuleRegistry.FindByOID(e.OID)?.ModuleType.Name}: {e.InstanceID:X}, zone={e.Zone}, start={e.Time.Start:O}, duration={e.Time}, countdown on pull={e.CountdownOnPull:f3}")))
            {
                var moduleInfo = ModuleRegistry.FindByOID(e.OID);
                foreach (var n in _tree.Node("Raw ops", contextMenu: () => OpListContextMenu(_opListsFiltered.GetValueOrDefault(e))))
                {
                    if (!_opListsFiltered.ContainsKey(e))
                        _opListsFiltered[e] = new(_replay, moduleInfo, _replay.Ops.SkipWhile(o => o.Timestamp < e.Time.Start).TakeWhile(o => o.Timestamp <= e.Time.End), _scrollTo);
                    _opListsFiltered[e].Draw(_tree, e.Time.Start);
                }

                DrawContents(e, moduleInfo);
                DrawEncounterDetails(e, TimePrinter(e.Time.Start));
                DrawTimelines(e);
            }
        }

        private void DrawContents(Replay.Encounter? filter, ModuleRegistry.Info? moduleInfo)
        {
            var oidType = moduleInfo?.ObjectIDType;
            var aidType = moduleInfo?.ActionIDType;
            var sidType = moduleInfo?.StatusIDType;
            var tidType = moduleInfo?.TetherIDType;
            var iidType = moduleInfo?.IconIDType;
            var reference = filter?.Time.Start ?? _replay.Ops.First().Timestamp;
            var tp = TimePrinter(reference);
            var actions = filter != null ? _replay.EncounterActions(filter) : _replay.Actions;
            var statuses = filter != null ? _replay.EncounterStatuses(filter) : _replay.Statuses;
            var tethers = filter != null ? _replay.EncounterTethers(filter) : _replay.Tethers;
            var icons = filter != null ? _replay.EncounterIcons(filter) : _replay.Icons;
            var envControls = filter != null ? _replay.EncounterEnvControls(filter) : _replay.EnvControls;

            foreach (var n in _tree.Node("Participants"))
            {
                if (filter == null)
                {
                    DrawParticipants(_replay.Participants, actions, statuses, tp, reference, filter, aidType, sidType);
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
            Func<Replay.Action, bool> actionIsCrap = a => a.Source.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo;
            foreach (var n in _tree.Node("Interesting actions", !haveActions))
            {
                DrawActions(actions.Where(a => !actionIsCrap(a)), tp, aidType);
            }
            foreach (var n in _tree.Node("Other actions", !haveActions))
            {
                DrawActions(actions.Where(actionIsCrap), tp, aidType);
            }

            bool haveStatuses = statuses.Any();
            Func<Replay.Status, bool> statusIsCrap = s => (s.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo) || (s.Target.Type is ActorType.Pet or ActorType.Chocobo);
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
                _tree.LeafNodes(icons, i => $"{tp(i.Timestamp)}: {i.ID} ({iidType?.GetEnumName(i.ID)}) @ {ReplayUtils.ParticipantString(i.Target, i.Timestamp)}");
            }

            foreach (var n in _tree.Node("EnvControls", !envControls.Any()))
            {
                _tree.LeafNodes(envControls, ec => $"{tp(ec.Timestamp)}: {ec.Index:X2} = {ec.State:X8}");
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
            }
        }

        private string CastString(Replay.Cast c, DateTime reference, DateTime prev, Type? aidType)
        {
            return $"{new Replay.TimeRange(reference, c.Time.Start)} ({new Replay.TimeRange(prev, c.Time.Start)}) + {c.ExpectedCastTime + 0.3f:f2} ({c.Time}): {c.ID} ({aidType?.GetEnumName(c.ID.ID)}) @ {ReplayUtils.ParticipantString(c.Target, c.Time.Start)} {Utils.Vec3String(c.Location)} / {c.Rotation}";
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
            foreach (var n in _tree.Node("User markers", _replay.UserMarkers.Count == 0))
                _tree.LeafNodes(_replay.UserMarkers, kv => $"{kv.Key:O}: {kv.Value}");
        }

        private Func<DateTime, string> TimePrinter(DateTime start)
        {
            return t => new Replay.TimeRange(start, t).ToString();
        }

        private void OpenTimeline(Replay.Encounter enc, BitMask showPlayers)
        {
            new ReplayTimelineWindow(_replay, enc, showPlayers);
        }

        private void DrawTimelines(Replay.Encounter enc)
        {
            if (ImGui.Button("Show timeline"))
                OpenTimeline(enc, new());
            ImGui.SameLine();
            for (int i = 0; i < enc.PartyMembers.Count; i++)
            {
                var (p, c, l) = enc.PartyMembers[i];
                if (ImGui.Button($"{c}{l} {p.NameHistory.FirstOrDefault().Value}"))
                    OpenTimeline(enc, new(1u << i));
                ImGui.SameLine();
            }
            if (ImGui.Button("All"))
                OpenTimeline(enc, new((1u << enc.PartyMembers.Count) - 1));
        }

        private void OpListContextMenu(OpList? list)
        {
            if (ImGui.MenuItem("Clear filters"))
            {
                list?.ClearFilters();
            }
            if (list != null && ImGui.MenuItem("Show actor-size events", "", list.ShowActorSizeEvents, true))
            {
                list.ShowActorSizeEvents = !list.ShowActorSizeEvents;
            }
        }
    }
}
