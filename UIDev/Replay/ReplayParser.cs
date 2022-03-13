using BossMod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UIDev
{
    public class ReplayParser
    {
        protected Replay _res = new();
        protected WorldState _ws = new();
        private Dictionary<uint, Replay.Encounter> _encounters = new();
        private Dictionary<uint, Replay.Participant> _participants = new();
        private Dictionary<(uint, uint, uint), Replay.Status> _statuses = new();
        private Dictionary<uint, Replay.Tether> _tethers = new();

        protected ReplayParser()
        {
            _ws.Actors.Added += ActorAdded;
            _ws.Actors.Removed += ActorRemoved;
            _ws.Actors.InCombatChanged += ActorCombat;
            _ws.Actors.IsTargetableChanged += ActorTargetable;
            _ws.Actors.CastStarted += CastStart;
            _ws.Actors.CastFinished += CastFinish;
            _ws.Actors.Tethered += TetherAdd;
            _ws.Actors.Untethered += TetherRemove;
            _ws.Actors.StatusGain += StatusGain;
            _ws.Actors.StatusLose += StatusLose;
            _ws.Events.Icon += EventIcon;
            _ws.Events.Cast += EventCast;
            _ws.Events.EnvControl += EventEnvControl;
        }

        protected void AddOp(DateTime timestamp, ReplayOps.Operation op)
        {
            _ws.CurrentTime = op.Timestamp = timestamp;
            op.Redo(_ws);
            _res.Ops.Add(op);
        }

        protected Replay Finish(string path = "")
        {
            _res.Path = path;
            foreach (var enc in _encounters.Values)
            {
                enc.End = _ws.CurrentTime;
            }
            foreach (var p in _participants.Values)
            {
                p.Despawn = _ws.CurrentTime;
                if (p.Casts.LastOrDefault()?.End == new DateTime())
                    p.Casts.Last().End = _ws.CurrentTime;
            }
            foreach (var s in _statuses.Values)
            {
                s.Fade = _ws.CurrentTime;
            }
            foreach (var t in _tethers.Values)
            {
                t.Disappear = _ws.CurrentTime;
            }
            return _res;
        }

        private void AddParticipantToEncounter(Replay.Encounter e, Replay.Participant p)
        {
            e.Participants.TryAdd(p.OID, new());
            e.Participants[p.OID].Add(p);
        }

        private void StartEncounter(Actor actor)
        {
            if (_encounters.ContainsKey(actor.InstanceID))
                return;

            var m = ModuleRegistry.TypeForOID(actor.OID);
            if (m == null)
                return;

            var e = _encounters[actor.InstanceID] = new()
            {
                InstanceID = actor.InstanceID,
                OID = actor.OID,
                Start = _ws.CurrentTime,
                Zone = _ws.CurrentZone,
                FirstAction = _res.Actions.Count,
                FirstStatus = _res.Statuses.Count,
                FirstTether = _res.Tethers.Count,
                FirstIcon = _res.Icons.Count,
                FirstEnvControl = _res.EnvControls.Count
            };
            foreach (var p in _participants.Values)
                AddParticipantToEncounter(e, p);
            _res.Encounters.Add(e);
        }

        private void FinishEncounter(Actor actor)
        {
            var e = _encounters.GetValueOrDefault(actor.InstanceID);
            if (e == null)
                return;

            e.End = _ws.CurrentTime;
            _encounters.Remove(actor.InstanceID);
        }

        private void ActorAdded(object? sender, Actor actor)
        {
            var p = _participants[actor.InstanceID] = new() { InstanceID = actor.InstanceID, OID = actor.OID, Type = actor.Type, Name = actor.Name, Spawn = _ws.CurrentTime };
            _res.Participants.Add(p);
            foreach (var e in _encounters.Values)
                AddParticipantToEncounter(e, p);
        }

        private void ActorRemoved(object? sender, Actor actor)
        {
            _participants[actor.InstanceID].Despawn = _ws.CurrentTime;
            _participants.Remove(actor.InstanceID);
        }

        private void ActorCombat(object? sender, Actor actor)
        {
            if (!actor.InCombat)
                FinishEncounter(actor);
            else if (actor.IsTargetable)
                StartEncounter(actor);
        }

        private void ActorTargetable(object? sender, Actor actor)
        {
            if (actor.InCombat && actor.IsTargetable)
                StartEncounter(actor);
        }

        private void CastStart(object? sender, Actor actor)
        {
            var c = actor.CastInfo!;
            _participants[actor.InstanceID].Casts.Add(new() { ID = c.Action, Start = _ws.CurrentTime, Target = _participants.GetValueOrDefault(c.TargetID), Location = c.Location });
        }

        private void CastFinish(object? sender, Actor actor)
        {
            _participants[actor.InstanceID].Casts.Last().End = _ws.CurrentTime;
        }

        private void TetherAdd(object? sender, Actor actor)
        {
            var t = _tethers[actor.InstanceID] = new() { ID = actor.Tether.ID, Source = _participants[actor.InstanceID], Target = _participants.GetValueOrDefault(actor.Tether.Target), Appear = _ws.CurrentTime };
            _res.Tethers.Add(t);
        }

        private void TetherRemove(object? sender, Actor actor)
        {
            _tethers[actor.InstanceID].Disappear = _ws.CurrentTime;
            _tethers.Remove(actor.InstanceID);
        }

        private void StatusGain(object? sender, (Actor actor, int index) args)
        {
            var p = _participants[args.actor.InstanceID];
            var s = args.actor.Statuses[args.index];
            var src = _participants.GetValueOrDefault(s.SourceID);
            var r = _statuses[(args.actor.InstanceID, s.ID, s.SourceID)] = new() { ID = s.ID, Target = p, Source = src, Apply = _ws.CurrentTime, Expire = s.ExpireAt, StartingExtra = s.Extra };
            p.HasAnyStatuses = true;
            _res.Statuses.Add(r);
        }

        private void StatusLose(object? sender, (Actor actor, int index) args)
        {
            var s = args.actor.Statuses[args.index];
            var r = _statuses.GetValueOrDefault((args.actor.InstanceID, s.ID, s.SourceID));
            if (r == null)
                return;

            r.Fade = _ws.CurrentTime;
            _statuses.Remove((args.actor.InstanceID, s.ID, s.SourceID));
        }

        private void EventIcon(object? sender, (uint actorID, uint iconID) args)
        {
            _res.Icons.Add(new() { ID = args.iconID, Target = _participants.GetValueOrDefault(args.actorID), Timestamp = _ws.CurrentTime });
        }

        private void EventCast(object? sender, CastEvent info)
        {
            var p = _participants.GetValueOrDefault(info.CasterID);
            if (p == null)
            {
                Service.Log($"Skipping {info.Action} cast from unknown actor {info.CasterID:X}");
                return;
            }
            var a = new Replay.Action() { ID = info.Action, Time = _ws.CurrentTime, Source = p, MainTarget = _participants.GetValueOrDefault(info.MainTargetID) };
            foreach (var t in info.Targets)
            {
                a.Targets.Add(new() { Target = _participants.GetValueOrDefault(t.ID), Effects = t.Effects });
            }
            p.HasAnyActions = true;
            _res.Actions.Add(a);
        }

        private void EventEnvControl(object? sender, (uint feature, byte index, uint state) args)
        {
            _res.EnvControls.Add(new() { Feature = args.feature, Index = args.index, State = args.state, Timestamp = _ws.CurrentTime });
        }
    }
}
