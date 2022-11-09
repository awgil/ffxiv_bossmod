using BossMod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UIDev
{
    public class ReplayParser
    {
        class LoadedModuleData
        {
            public BossModule Module;
            public Replay.Encounter Encounter;
            public int ActivePhaseIndex = -1;
            public StateMachine.State? ActiveState;

            public LoadedModuleData(BossModule module, Replay.Encounter enc)
            {
                Module = module;
                Encounter = enc;
            }
        }

        class BossModuleManagerWrapper : BossModuleManager
        {
            private ReplayParser _self;

            public BossModuleManagerWrapper(ReplayParser self) : base(self._ws) { _self = self; }

            protected override void OnModuleLoaded(BossModule module)
            {
                var enc = new Replay.Encounter()
                {
                    InstanceID = module.PrimaryActor.InstanceID,
                    OID = module.PrimaryActor.OID,
                    Zone = _self._ws.CurrentZone
                };
                _self._modules[module.PrimaryActor.InstanceID] = new(module, enc);
                module.Error += OnError;
            }

            protected override void OnModuleUnloaded(BossModule module)
            {
                var data = _self._modules[module.PrimaryActor.InstanceID];
                if (data.ActiveState != null)
                {
                    data.Encounter.Phases.Add(new() { ID = data.ActivePhaseIndex, LastStateID = data.ActiveState.ID, Exit = _self._ws.CurrentTime });
                    data.Encounter.States.Add(new() { ID = data.ActiveState.ID, Name = data.ActiveState.Name, Comment = data.ActiveState.Comment, ExpectedDuration = data.ActiveState.Duration, Exit = _self._ws.CurrentTime });
                }
                data.Encounter.Time.End = _self._ws.CurrentTime;
                _self._modules.Remove(module.PrimaryActor.InstanceID);
                module.Error -= OnError;
            }

            private void OnError(object? sender, (BossComponent? comp, string message) args)
            {
                var module = (BossModule)sender!;
                _self._modules[module.PrimaryActor.InstanceID].Encounter.Errors.Add(new() { Timestamp = _self._ws.CurrentTime, CompType = args.comp?.GetType(), Message = args.message });
            }
        }

        protected Replay _res = new();
        protected WorldState _ws = new();
        private BossModuleManagerWrapper _mgr;
        private Dictionary<ulong, LoadedModuleData> _modules = new();
        private Dictionary<ulong, Replay.Participant> _participants = new();
        private Dictionary<(ulong, int), Replay.Status> _statuses = new();
        private Dictionary<ulong, Replay.Tether> _tethers = new();

        protected ReplayParser()
        {
            _mgr = new(this);
            _ws.Actors.Added += ActorAdded;
            _ws.Actors.Removed += ActorRemoved;
            _ws.Actors.IsTargetableChanged += ActorTargetable;
            _ws.Actors.IsDeadChanged += ActorDead;
            _ws.Actors.Moved += ActorMoved;
            _ws.Actors.SizeChanged += ActorSize;
            _ws.Actors.HPMPChanged += ActorHPMP;
            _ws.Actors.CastStarted += CastStart;
            _ws.Actors.CastFinished += CastFinish;
            _ws.Actors.Tethered += TetherAdd;
            _ws.Actors.Untethered += TetherRemove;
            _ws.Actors.StatusGain += StatusGain;
            _ws.Actors.StatusLose += StatusLose;
            _ws.Actors.IconAppeared += EventIcon;
            _ws.Actors.CastEvent += EventCast;
            _ws.DirectorUpdate += EventDirectorUpdate;
            _ws.EnvControl += EventEnvControl;
        }

        protected void AddOp(WorldState.Operation op)
        {
            if (op is WorldState.OpFrameStart && _res.Ops.Count > 0)
                FinishFrame();
            _ws.Execute(op);
            _res.Ops.Add(op);
        }

        protected Replay Finish(string path = "")
        {
            //foreach (var a in _res.Actions)
            //{
            //    Service.Log($"{a.ID} {ReplayUtils.ParticipantString(a.Source)} -> {ReplayUtils.ParticipantString(a.MainTarget)}");
            //    foreach (var t in a.Targets)
            //    {
            //        Service.Log($"- {ReplayUtils.ParticipantString(t.Target)}");
            //        foreach (var e in t.Effects)
            //        {
            //            Service.Log($"-- {ReplayUtils.ActionEffectString(e)} ({ActionEffectParser.DescribeUnknown(e)})");
            //        }
            //    }
            //}

            FinishFrame();
            _res.Path = path;
            foreach (var enc in _modules.Values)
            {
                if (enc.ActiveState != null)
                {
                    enc.Encounter.Phases.Add(new() { ID = enc.ActivePhaseIndex, LastStateID = enc.ActiveState.ID, Exit = _ws.CurrentTime });
                    enc.Encounter.States.Add(new() { ID = enc.ActiveState.ID, Name = enc.ActiveState.Name, Comment = enc.ActiveState.Comment, ExpectedDuration = enc.ActiveState.Duration, Exit = _ws.CurrentTime });
                }
                enc.Encounter.Time.End = _ws.CurrentTime;
            }
            foreach (var p in _participants.Values)
            {
                FinalizeParticipant(p);
            }
            foreach (var s in _statuses.Values)
            {
                s.Time.End = _ws.CurrentTime;
            }
            foreach (var t in _tethers.Values)
            {
                t.Time.End = _ws.CurrentTime;
            }
            return _res;
        }

        private void FinishFrame()
        {
            _mgr.Update();
            foreach (var m in _modules.Values)
            {
                if (m.Module.StateMachine?.ActiveState != m.ActiveState)
                {
                    if (m.ActiveState == null)
                    {
                        m.Encounter.Time.Start = _ws.CurrentTime;
                        m.Encounter.FirstAction = _res.Actions.Count;
                        m.Encounter.FirstStatus = _res.Statuses.Count;
                        m.Encounter.FirstTether = _res.Tethers.Count;
                        m.Encounter.FirstIcon = _res.Icons.Count;
                        m.Encounter.FirstDirectorUpdate = _res.DirectorUpdates.Count;
                        m.Encounter.FirstEnvControl = _res.EnvControls.Count;
                        foreach (var p in _participants.Values)
                            m.Encounter.Participants.GetOrAdd(p.OID).Add(p);
                        foreach (var p in _ws.Party.WithoutSlot(true))
                            m.Encounter.PartyMembers.Add((_participants[p.InstanceID], p.Class));
                        _res.Encounters.Add(m.Encounter);
                    }
                    else
                    {
                        if (m.Module.StateMachine?.ActivePhaseIndex != m.ActivePhaseIndex)
                        {
                            m.Encounter.Phases.Add(new() { ID = m.ActivePhaseIndex, LastStateID = m.ActiveState.ID, Exit = _ws.CurrentTime });
                        }
                        m.Encounter.States.Add(new() { ID = m.ActiveState.ID, Name = m.ActiveState.Name, Comment = m.ActiveState.Comment, ExpectedDuration = m.ActiveState.Duration, Exit = _ws.CurrentTime });
                    }
                    m.ActivePhaseIndex = m.Module.StateMachine?.ActivePhaseIndex ?? -1;
                    m.ActiveState = m.Module.StateMachine?.ActiveState;
                }
            }
        }

        private void FinalizeParticipant(Replay.Participant p)
        {
            p.Existence.End = _ws.CurrentTime;
            if (p.Casts.LastOrDefault()?.Time.End == new DateTime())
                p.Casts.Last().Time.End = _ws.CurrentTime;
            if (p.TargetableHistory.LastOrDefault().Value)
            {
                if (p.TargetableHistory.Last().Key < _ws.CurrentTime)
                    p.TargetableHistory.Add(_ws.CurrentTime, false);
                else
                    p.TargetableHistory.RemoveAt(p.TargetableHistory.Count - 1);
            }
        }

        private void ActorAdded(object? sender, Actor actor)
        {
            var p = _participants[actor.InstanceID] = new() { InstanceID = actor.InstanceID, OID = actor.OID, Type = actor.Type, OwnerID = actor.OwnerID, Name = actor.Name, Existence = new(_ws.CurrentTime), MinRadius = actor.HitboxRadius, MaxRadius = actor.HitboxRadius };
            if (actor.IsTargetable)
                p.TargetableHistory.Add(_ws.CurrentTime, true);
            p.PosRotHistory.Add(_ws.CurrentTime, actor.PosRot);
            p.HPMPHistory.Add(_ws.CurrentTime, (actor.HP, actor.CurMP));
            _res.Participants.Add(p);
            foreach (var e in _modules.Values)
                if (e.ActiveState != null)
                    e.Encounter.Participants.GetOrAdd(p.OID).Add(p);
        }

        private void ActorRemoved(object? sender, Actor actor)
        {
            FinalizeParticipant(_participants[actor.InstanceID]);
            _participants.Remove(actor.InstanceID);
        }

        private void ActorTargetable(object? sender, Actor actor)
        {
            _participants[actor.InstanceID].TargetableHistory.Add(_ws.CurrentTime, actor.IsTargetable);
        }

        private void ActorDead(object? sender, Actor actor)
        {
            _participants[actor.InstanceID].DeadHistory.Add(_ws.CurrentTime, actor.IsDead);
        }

        private void ActorMoved(object? sender, Actor actor)
        {
            _participants[actor.InstanceID].PosRotHistory.Add(_ws.CurrentTime, actor.PosRot);
        }

        private void ActorSize(object? sender, Actor actor)
        {
            var p = _participants[actor.InstanceID];
            p.MinRadius = Math.Min(p.MinRadius, actor.HitboxRadius);
            p.MaxRadius = Math.Max(p.MaxRadius, actor.HitboxRadius);
        }

        private void ActorHPMP(object? sender, Actor actor)
        {
            _participants[actor.InstanceID].HPMPHistory.Add(_ws.CurrentTime, (actor.HP, actor.CurMP));
        }

        private void CastStart(object? sender, Actor actor)
        {
            var c = actor.CastInfo!;
            var target = _participants.GetValueOrDefault(c.TargetID);
            _participants[actor.InstanceID].Casts.Add(new() { ID = c.Action, ExpectedCastTime = c.TotalTime, Time = new(_ws.CurrentTime), Target = target, Location = c.TargetID == 0 ? c.Location : (_ws.Actors.Find(c.TargetID)?.PosRot.XYZ() ?? new()), Rotation = c.Rotation, Interruptible = c.Interruptible });
        }

        private void CastFinish(object? sender, Actor actor)
        {
            _participants[actor.InstanceID].Casts.Last().Time.End = _ws.CurrentTime;
        }

        private void TetherAdd(object? sender, Actor actor)
        {
            var t = _tethers[actor.InstanceID] = new() { ID = actor.Tether.ID, Source = _participants[actor.InstanceID], Target = _participants.GetValueOrDefault(actor.Tether.Target), Time = new(_ws.CurrentTime) };
            _res.Tethers.Add(t);
        }

        private void TetherRemove(object? sender, Actor actor)
        {
            _tethers[actor.InstanceID].Time.End = _ws.CurrentTime;
            _tethers.Remove(actor.InstanceID);
        }

        private void StatusGain(object? sender, (Actor actor, int index) args)
        {
            var r = _statuses.GetValueOrDefault((args.actor.InstanceID, args.index));
            if (r != null)
                r.Time.End = _ws.CurrentTime;

            var s = args.actor.Statuses[args.index];
            var tgt = _participants[args.actor.InstanceID];
            var src = _participants.GetValueOrDefault(s.SourceID);
            r = _statuses[(args.actor.InstanceID, args.index)] = new() { ID = s.ID, Index = args.index, Target = tgt, Source = src, InitialDuration = (float)(s.ExpireAt - _ws.CurrentTime).TotalSeconds, Time = new(_ws.CurrentTime), StartingExtra = s.Extra };
            tgt.HasAnyStatuses = true;
            _res.Statuses.Add(r);
        }

        private void StatusLose(object? sender, (Actor actor, int index) args)
        {
            var r = _statuses.GetValueOrDefault((args.actor.InstanceID, args.index));
            if (r == null)
                return;
            r.Time.End = _ws.CurrentTime;
            _statuses.Remove((args.actor.InstanceID, args.index));
        }

        private void EventIcon(object? sender, (Actor actor, uint iconID) args)
        {
            _res.Icons.Add(new() { ID = args.iconID, Target = _participants.GetValueOrDefault(args.actor.InstanceID), Timestamp = _ws.CurrentTime });
        }

        private void EventCast(object? sender, (Actor actor, ActorCastEvent cast) args)
        {
            var p = _participants.GetValueOrDefault(args.actor.InstanceID);
            if (p == null)
            {
                Service.Log($"Skipping {args.cast.Action} cast from unknown actor {args.actor.InstanceID:X}");
                return;
            }

            var a = new Replay.Action()
            {
                ID = args.cast.Action,
                Timestamp = _ws.CurrentTime,
                Source = p,
                MainTarget = _participants.GetValueOrDefault(args.cast.MainTargetID),
                TargetPos = _ws.Actors.Find(args.cast.MainTargetID)?.PosRot.XYZ() ?? args.cast.TargetPos,
                AnimationLock = args.cast.AnimationLockTime,
                GlobalSequence = args.cast.GlobalSequence,
            };
            foreach (var t in args.cast.Targets)
            {
                var target = _participants.GetValueOrDefault(t.ID);
                if (target != null)
                    target.IsTargetOfAnyActions = true;
                a.Targets.Add(new() { Target = target, Effects = t.Effects });
            }
            p.HasAnyActions = true;
            _res.Actions.Add(a);
        }

        private void EventDirectorUpdate(object? sender, WorldState.OpDirectorUpdate op)
        {
            _res.DirectorUpdates.Add(new() { DirectorID = op.DirectorID, UpdateID = op.UpdateID, Param1 = op.Param1, Param2 = op.Param2, Param3 = op.Param3, Param4 = op.Param4, Timestamp = _ws.CurrentTime });
        }

        private void EventEnvControl(object? sender, WorldState.OpEnvControl op)
        {
            _res.EnvControls.Add(new() { DirectorID = op.DirectorID, Index = op.Index, State = op.State, Timestamp = _ws.CurrentTime });
        }
    }
}
