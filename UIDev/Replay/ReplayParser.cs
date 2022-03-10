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

        protected Replay Finish()
        {
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
            if (p.Type == ActorType.Player)
            {
                e.Players.Add(p);
            }
            else if (p.Type != ActorType.Pet && p.Type != ActorType.Chocobo)
            {
                e.Enemies.TryAdd(p.OID, new());
                e.Enemies[p.OID].Add(p);
            }
        }

        private void ActorAdded(object? sender, Actor actor)
        {
            var p = _participants[actor.InstanceID] = new() { InstanceID = actor.InstanceID, OID = actor.OID, Type = actor.Type, Name = actor.Name, Spawn = _ws.CurrentTime };
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
            if (actor.InCombat)
            {
                var m = ModuleRegistry.TypeForOID(actor.OID);
                if (m != null)
                {
                    var e = _encounters[actor.InstanceID] = new() { InstanceID = actor.InstanceID, OID = actor.OID, Start = _ws.CurrentTime, Zone = _ws.CurrentZone };
                    foreach (var p in _participants.Values)
                        AddParticipantToEncounter(e, p);
                    _res.Encounters.Add(e);
                }
            }
            else
            {
                var e = _encounters.GetValueOrDefault(actor.InstanceID);
                if (e != null)
                {
                    e.End = _ws.CurrentTime;
                    _encounters.Remove(actor.InstanceID);
                }
            }
        }

        private void CastStart(object? sender, Actor actor)
        {
            var c = actor.CastInfo!;
            _participants[actor.InstanceID].Casts.Add(new() { ID = c.Action, Start = _ws.CurrentTime, Target = _participants.GetValueOrDefault(c.TargetID) });
        }

        private void CastFinish(object? sender, Actor actor)
        {
            _participants[actor.InstanceID].Casts.Last().End = _ws.CurrentTime;
        }

        private void TetherAdd(object? sender, Actor actor)
        {
            var t = _tethers[actor.InstanceID] = new() { ID = actor.Tether.ID, Source = _participants[actor.InstanceID], Target = _participants.GetValueOrDefault(actor.Tether.Target), Appear = _ws.CurrentTime };
            foreach (var e in _encounters.Values)
                e.Tethers.Add(t);
        }

        private void TetherRemove(object? sender, Actor actor)
        {
            _tethers[actor.InstanceID].Disappear = _ws.CurrentTime;
            _tethers.Remove(actor.InstanceID);
        }

        private void StatusGain(object? sender, (Actor actor, int index) args)
        {
            if (args.actor.Type == ActorType.Pet)
                return; // ignore

            var s = args.actor.Statuses[args.index];
            var src = _participants.GetValueOrDefault(s.SourceID);
            if (src?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo)
                return; // ignore

            var r = _statuses[(args.actor.InstanceID, s.ID, s.SourceID)] = new() { ID = s.ID, Target = _participants[args.actor.InstanceID], Source = src, Apply = _ws.CurrentTime, Expire = s.ExpireAt, StartingExtra = s.Extra };
            foreach (var e in _encounters.Values)
                e.Statuses.Add(r);
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
            foreach (var e in _encounters.Values)
            {
                e.Icons.Add(new() { ID = args.iconID, Target = _participants.GetValueOrDefault(args.actorID), Timestamp = _ws.CurrentTime });
            }
        }

        private void EventCast(object? sender, CastEvent info)
        {
            var src = _participants[info.CasterID];
            var a = new Replay.Action() { ID = info.Action, Time = _ws.CurrentTime, Source = src, MainTarget = _participants.GetValueOrDefault(info.MainTargetID) };
            foreach (var t in info.Targets)
            {
                a.Targets.Add(new() { Target = _participants.GetValueOrDefault(t.ID) });
            }
            src.Actions.Add(a);

            if (src.Type != ActorType.Player && src.Type != ActorType.Pet && src.Type != ActorType.Chocobo)
            {
                foreach (var e in _encounters.Values)
                {
                    e.Actions.Add(a);
                }
            }
        }

        private void EventEnvControl(object? sender, (uint feature, byte index, uint state) args)
        {
            foreach (var e in _encounters.Values)
            {
                e.EnvControls.Add(new() { Feature = args.feature, Index = args.index, State = args.state, Timestamp = _ws.CurrentTime });
            }
        }
    }
}
