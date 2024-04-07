namespace BossMod;

public class ReplayParser : IDisposable
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
            var enc = new Replay.Encounter(module.PrimaryActor.InstanceID, module.PrimaryActor.OID, _self._ws.CurrentZone);
            _self._modules[module.PrimaryActor.InstanceID] = new(module, enc);
            module.Error += OnError;
        }

        protected override void OnModuleUnloaded(BossModule module)
        {
            var data = _self._modules[module.PrimaryActor.InstanceID];
            if (data.ActiveState != null)
            {
                data.Encounter.Phases.Add(new(data.ActivePhaseIndex, data.ActiveState.ID, _self._ws.CurrentTime));
                data.Encounter.States.Add(new(data.ActiveState.ID, data.ActiveState.Name, data.ActiveState.Comment, data.ActiveState.Duration, _self._ws.CurrentTime));
            }
            data.Encounter.Time.End = _self._ws.CurrentTime;
            _self._modules.Remove(module.PrimaryActor.InstanceID);
            module.Error -= OnError;
        }

        private void OnError(BossModule module, BossComponent? comp, string message)
        {
            _self._modules[module.PrimaryActor.InstanceID].Encounter.Errors.Add(new(_self._ws.CurrentTime, comp?.GetType(), message));
        }
    }

    protected Replay _res = new();
    protected WorldState _ws = new(TimeSpan.TicksPerSecond, "pending");
    private BossModuleManagerWrapper _mgr;
    private Dictionary<ulong, LoadedModuleData> _modules = new();
    private Dictionary<ulong, Replay.Participant> _participants = new(); // these are either existing actors, destroyed actors that can still be recreated, or never-created-but-referenced actors
    private Dictionary<(ulong, int), Replay.Status> _statuses = new();
    private Dictionary<ulong, Replay.Tether> _tethers = new();
    private List<Replay.ClientAction> _pendingClientActions = new();

    protected ReplayParser()
    {
        _mgr = new(this);
        _ws.Actors.Added += ActorAdded;
        _ws.Actors.Removed += ActorRemoved;
        _ws.Actors.Renamed += ActorRenamed;
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
        _ws.Actors.EffectResult += EventConfirm;
        _ws.UserMarkerAdded += EventUserMarker;
        _ws.CurrentZoneChanged += EventZoneChange;
        _ws.DirectorUpdate += EventDirectorUpdate;
        _ws.EnvControl += EventEnvControl;
        _ws.Client.ActionRequested += ClientActionRequested;
        _ws.Client.ActionRejected += ClientActionRejected;
    }

    public virtual void Dispose()
    {
        _mgr.Dispose();
        _modules.Clear();
        _ws.Actors.Added -= ActorAdded;
        _ws.Actors.Removed -= ActorRemoved;
        _ws.Actors.Renamed -= ActorRenamed;
        _ws.Actors.IsTargetableChanged -= ActorTargetable;
        _ws.Actors.IsDeadChanged -= ActorDead;
        _ws.Actors.Moved -= ActorMoved;
        _ws.Actors.SizeChanged -= ActorSize;
        _ws.Actors.HPMPChanged -= ActorHPMP;
        _ws.Actors.CastStarted -= CastStart;
        _ws.Actors.CastFinished -= CastFinish;
        _ws.Actors.Tethered -= TetherAdd;
        _ws.Actors.Untethered -= TetherRemove;
        _ws.Actors.StatusGain -= StatusGain;
        _ws.Actors.StatusLose -= StatusLose;
        _ws.Actors.IconAppeared -= EventIcon;
        _ws.Actors.CastEvent -= EventCast;
        _ws.Actors.EffectResult -= EventConfirm;
        _ws.UserMarkerAdded -= EventUserMarker;
        _ws.CurrentZoneChanged -= EventZoneChange;
        _ws.DirectorUpdate -= EventDirectorUpdate;
        _ws.EnvControl -= EventEnvControl;
        _ws.Client.ActionRequested -= ClientActionRequested;
        _ws.Client.ActionRejected -= ClientActionRejected;
    }

    protected void Start(DateTime timestamp, ulong qpf, string gameVersion)
    {
        _res.QPF = _ws.QPF = qpf;
        _res.GameVersion = _ws.GameVersion = gameVersion;
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
        FinishFrame();
        _res.Path = path;
        foreach (var enc in _modules.Values)
        {
            if (enc.ActiveState != null)
            {
                enc.Encounter.Phases.Add(new(enc.ActivePhaseIndex, enc.ActiveState.ID, _ws.CurrentTime));
                enc.Encounter.States.Add(new(enc.ActiveState.ID, enc.ActiveState.Name, enc.ActiveState.Comment, enc.ActiveState.Duration, _ws.CurrentTime));
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
                    m.Encounter.CountdownOnPull = _ws.Client.CountdownRemaining ?? 10000;
                    m.Encounter.Time.Start = _ws.CurrentTime;
                    m.Encounter.FirstAction = _res.Actions.Count;
                    m.Encounter.FirstStatus = _res.Statuses.Count;
                    m.Encounter.FirstTether = _res.Tethers.Count;
                    m.Encounter.FirstIcon = _res.Icons.Count;
                    m.Encounter.FirstDirectorUpdate = _res.DirectorUpdates.Count;
                    m.Encounter.FirstEnvControl = _res.EnvControls.Count;
                    foreach (var p in _participants.Values.Where(p => p.WorldExistence.Count > 0 && p.WorldExistence.Last().End == default)) // include only live actors
                        m.Encounter.ParticipantsByOID.GetOrAdd(p.OID).Add(p);
                    foreach (var p in _ws.Party.WithoutSlot(true))
                        m.Encounter.PartyMembers.Add((_participants[p.InstanceID], p.Class, p.Level));
                    _res.Encounters.Add(m.Encounter);
                }
                else
                {
                    if (m.Module.StateMachine?.ActivePhaseIndex != m.ActivePhaseIndex)
                    {
                        m.Encounter.Phases.Add(new(m.ActivePhaseIndex, m.ActiveState.ID, _ws.CurrentTime));
                    }
                    m.Encounter.States.Add(new(m.ActiveState.ID, m.ActiveState.Name, m.ActiveState.Comment, m.ActiveState.Duration, _ws.CurrentTime));
                }
                m.ActivePhaseIndex = m.Module.StateMachine?.ActivePhaseIndex ?? -1;
                m.ActiveState = m.Module.StateMachine?.ActiveState;
            }
        }
    }

    private void FinalizeParticipant(Replay.Participant p)
    {
        if (p.EffectiveExistence.End > _ws.CurrentTime)
            p.EffectiveExistence.End = _ws.CurrentTime; // note that this would be extended by any new references
        if (p.WorldExistence.Count > 0 && p.WorldExistence.Last().End == default)
            p.WorldExistence.AsSpan()[p.WorldExistence.Count - 1].End = _ws.CurrentTime;

        if (p.Casts.Count > 0 && p.Casts.Last().Time.End == default)
            p.Casts.Last().Time.End = _ws.CurrentTime;

        if (p.TargetableHistory.LastOrDefault().Value)
        {
            if (p.TargetableHistory.Last().Key < _ws.CurrentTime)
                p.TargetableHistory.Add(_ws.CurrentTime, false);
            else
                p.TargetableHistory.RemoveAt(p.TargetableHistory.Count - 1);
        }
    }

    private Replay.Participant GetOrCreateParticipant(ulong instanceID, bool extendEffectiveExistence = true)
    {
        if (instanceID is 0 or 0xE0000000)
            throw new ArgumentException("Unexpected invalid instance-id");
        if (_participants.TryGetValue(instanceID, out var p))
        {
            if (extendEffectiveExistence && p.EffectiveExistence.End < _ws.CurrentTime)
                p.EffectiveExistence.End = _ws.CurrentTime;
            return p;
        }
        // new participant
        p = _participants[instanceID] = new(instanceID) { ZoneID = _ws.CurrentZone, CFCID = _ws.CurrentCFCID, EffectiveExistence = new(_ws.CurrentTime, _ws.CurrentTime) };
        _res.Participants.Add(p);
        return p;
    }
    private Replay.Participant? GetOrCreateOptionalParticipant(ulong instanceID) => instanceID is 0 or 0xE0000000 ? null : GetOrCreateParticipant(instanceID);

    private void ActorAdded(Actor actor)
    {
        var p = GetOrCreateParticipant(actor.InstanceID, false);
        if (p.EffectiveExistence.End > _ws.CurrentTime)
        {
            throw new Exception($"Unexpected actor add while participant still effectively exists: {actor}");
        }
        else if (p.WorldExistence.Count == 0)
        {
            // first add
            p.OID = actor.OID;
            p.Type = actor.Type;
            p.OwnerID = actor.OwnerID;
        }
        else if (p.OID == actor.OID && p.Type == actor.Type && p.OwnerID == actor.OwnerID)
        {
            // recreate after destruction
            if (p.WorldExistence.Last().End == default)
                throw new Exception($"Actor add after actor add: {actor}");
        }
        else
        {
            // looks like an instance-id reuse, finalize previous one and create new one
            Service.Log($"Instance id reuse: {actor} @ {_ws.CurrentTime:O}");
            FinalizeParticipant(p);
            _participants.Remove(actor.InstanceID);
            p = GetOrCreateParticipant(actor.InstanceID);
        }

        p.EffectiveExistence.End = DateTime.MaxValue; // until it is destroyed
        p.WorldExistence.Add(new(_ws.CurrentTime));
        if (p.NameHistory.Count == 0 ? (actor.Name.Length > 0 || actor.NameID != 0) : p.NameHistory.Values.Last() != (actor.Name, actor.NameID))
            p.NameHistory.Add(_ws.CurrentTime, (actor.Name, actor.NameID));
        if (actor.IsTargetable)
            p.TargetableHistory.Add(_ws.CurrentTime, true);
        p.PosRotHistory.Add(_ws.CurrentTime, actor.PosRot);
        p.HPMPHistory.Add(_ws.CurrentTime, (actor.HP, actor.CurMP));
        p.MinRadius = Math.Min(p.MinRadius, actor.HitboxRadius);
        p.MaxRadius = Math.Max(p.MaxRadius, actor.HitboxRadius);

        foreach (var e in _modules.Values.Where(e => e.ActiveState != null))
        {
            var encParticipants = e.Encounter.ParticipantsByOID.GetOrAdd(actor.OID);
            if (!encParticipants.Contains(p))
                encParticipants.Add(p);
        }
    }

    private void ActorRemoved(Actor actor)
    {
        FinalizeParticipant(_participants[actor.InstanceID]);
        // keep participant entry in case it is recreated later
    }

    private void ActorRenamed(Actor actor)
    {
        _participants[actor.InstanceID].NameHistory.Add(_ws.CurrentTime, (actor.Name, actor.NameID));
    }

    private void ActorTargetable(Actor actor)
    {
        _participants[actor.InstanceID].TargetableHistory.Add(_ws.CurrentTime, actor.IsTargetable);
    }

    private void ActorDead(Actor actor)
    {
        _participants[actor.InstanceID].DeadHistory.Add(_ws.CurrentTime, actor.IsDead);
    }

    private void ActorMoved(Actor actor)
    {
        _participants[actor.InstanceID].PosRotHistory.Add(_ws.CurrentTime, actor.PosRot);
    }

    private void ActorSize(Actor actor)
    {
        var p = _participants[actor.InstanceID];
        p.MinRadius = Math.Min(p.MinRadius, actor.HitboxRadius);
        p.MaxRadius = Math.Max(p.MaxRadius, actor.HitboxRadius);
    }

    private void ActorHPMP(Actor actor)
    {
        _participants[actor.InstanceID].HPMPHistory.Add(_ws.CurrentTime, (actor.HP, actor.CurMP));
    }

    private void CastStart(Actor actor)
    {
        var c = actor.CastInfo!;
        var target = GetOrCreateOptionalParticipant(c.TargetID);
        var location = target?.PosRotAt(_ws.CurrentTime).XYZ() ?? c.Location;
        var cast = new Replay.Cast(c.Action, c.TotalTime, target, location, c.Rotation, c.Interruptible);
        cast.Time.Start = _ws.CurrentTime;
        _participants[actor.InstanceID].Casts.Add(cast);
        if (actor == _ws.Party.Player() && _pendingClientActions.Count > 0 && _pendingClientActions.Last().ID == c.Action)
        {
            cast.ClientAction = _pendingClientActions.Last();
            _pendingClientActions.Last().Cast = cast;
        }
    }

    private void CastFinish(Actor actor)
    {
        var cast = _participants[actor.InstanceID].Casts.Last();
        cast.Time.End = _ws.CurrentTime;
        if (actor == _ws.Party.Player() && _pendingClientActions.FindIndex(a => a.Cast == cast) is var index && index >= 0)
            _pendingClientActions.RemoveAt(index);
    }

    private void TetherAdd(Actor actor)
    {
        var t = _tethers[actor.InstanceID] = new(actor.Tether.ID, _participants[actor.InstanceID], GetOrCreateParticipant(actor.Tether.Target));
        t.Time.Start = _ws.CurrentTime;
        _res.Tethers.Add(t);
    }

    private void TetherRemove(Actor actor)
    {
        _tethers[actor.InstanceID].Time.End = _ws.CurrentTime;
        _tethers.Remove(actor.InstanceID);
    }

    private void StatusGain(Actor actor, int index)
    {
        var r = _statuses.GetValueOrDefault((actor.InstanceID, index));
        if (r != null)
            r.Time.End = _ws.CurrentTime;

        var s = actor.Statuses[index];
        var tgt = _participants[actor.InstanceID];
        var src = GetOrCreateOptionalParticipant(s.SourceID);
        r = _statuses[(actor.InstanceID, index)] = new(s.ID, index, tgt, src, (float)(s.ExpireAt - _ws.CurrentTime).TotalSeconds, s.Extra);
        r.Time.Start = _ws.CurrentTime;
        tgt.HasAnyStatuses = true;
        _res.Statuses.Add(r);
    }

    private void StatusLose(Actor actor, int index)
    {
        var r = _statuses.GetValueOrDefault((actor.InstanceID, index));
        if (r == null)
            return;
        r.Time.End = _ws.CurrentTime;
        _statuses.Remove((actor.InstanceID, index));
    }

    private void EventIcon(Actor actor, uint iconID)
    {
        _res.Icons.Add(new(iconID, GetOrCreateParticipant(actor.InstanceID), _ws.CurrentTime));
    }

    private void EventCast(Actor actor, ActorCastEvent cast)
    {
        var p = GetOrCreateParticipant(actor.InstanceID);
        var mt = GetOrCreateOptionalParticipant(cast.MainTargetID);
        var a = new Replay.Action(cast.Action, _ws.CurrentTime, p, mt, mt?.PosRotAt(_ws.CurrentTime).XYZ() ?? cast.TargetPos, cast.AnimationLockTime, cast.GlobalSequence);
        foreach (var t in cast.Targets)
        {
            var target = GetOrCreateParticipant(t.ID);
            target.IsTargetOfAnyActions = true;
            a.Targets.Add(new(target, t.Effects));
        }
        p.HasAnyActions = true;
        _res.Actions.Add(a);

        if (actor == _ws.Party.Player() && _pendingClientActions.FindIndex(a => a.SourceSequence == cast.SourceSequence) is var index && index >= 0)
        {
            a.ClientAction = _pendingClientActions[index];
            _pendingClientActions[index].Action = a;
            _pendingClientActions.RemoveAt(index);
        }
    }

    private void EventConfirm(Actor source, uint seq, int targetIndex)
    {
        var a = _res.Actions.FindLast(a => a.GlobalSequence == seq);
        if (a == null)
        {
            Service.Log($"Skipping confirmation #{seq}/{targetIndex} for {source.InstanceID:X} for missing action");
            return;
        }

        if (targetIndex >= a.Targets.Count)
        {
            Service.Log($"Skipping confirmation #{seq}/{targetIndex} for {source.InstanceID:X} for out-of-range target index (action has {a.Targets.Count} targets)");
            return;
        }

        var t = a.Targets[targetIndex];
        var forSource = a.Source.InstanceID == source.InstanceID;
        var forTarget = t.Target.InstanceID == source.InstanceID;
        if (forSource)
        {
            if (t.ConfirmationSource == default)
                t.ConfirmationSource = _ws.CurrentTime;
            else
                Service.Log($"Double confirmation ${seq}/{targetIndex} for {source.InstanceID:X} (source)");
        }
        if (forTarget)
        {
            if (t.ConfirmationTarget == default)
                t.ConfirmationTarget = _ws.CurrentTime;
            else
                Service.Log($"Double confirmation ${seq}/{targetIndex} for {source.InstanceID:X} (target)");
        }
        if (!forSource && !forTarget)
        {
            Service.Log($"Skipping confirmation #{seq}/{targetIndex} for {source.InstanceID:X} for unexpected target (src={a.Source.InstanceID:X}, tgt={t.Target.InstanceID:X})");
        }
    }

    private void EventUserMarker(WorldState.OpUserMarker op)
    {
        _res.UserMarkers.Add(_ws.CurrentTime, op.Text);
    }

    private void EventZoneChange(WorldState.OpZoneChange op)
    {
        // heuristic: assume any new actors after zone change are actually new, if they reuse same instance id
        foreach (var (k, v) in _participants)
            if (v.EffectiveExistence.End < _ws.CurrentTime)
                _participants.Remove(k);
    }

    private void EventDirectorUpdate(WorldState.OpDirectorUpdate op)
    {
        _res.DirectorUpdates.Add(new(op.DirectorID, op.UpdateID, op.Param1, op.Param2, op.Param3, op.Param4, _ws.CurrentTime));
    }

    private void EventEnvControl(WorldState.OpEnvControl op)
    {
        _res.EnvControls.Add(new(op.Index, op.State, _ws.CurrentTime));
    }

    private void ClientActionRequested(ClientState.OpActionRequest op)
    {
        var past = op.Request.InitialCastTimeTotal > 0 ? op.Request.InitialCastTimeElapsed : op.Request.InitialAnimationLock is < 0.5f and >= 0.4f ? 0.5f - op.Request.InitialAnimationLock : 0; // TODO: consider logging explicitly
        var a = new Replay.ClientAction(op.Request.Action, op.Request.SourceSequence, GetOrCreateOptionalParticipant(op.Request.TargetID), op.Request.TargetPos, _ws.CurrentTime.AddSeconds(-past));
        _res.ClientActions.Add(a);
        _pendingClientActions.Add(a);
    }

    private void ClientActionRejected(ClientState.OpActionReject op)
    {
        int index = op.Value.SourceSequence != 0
            ? _pendingClientActions.FindIndex(a => a.SourceSequence == op.Value.SourceSequence)
            : _pendingClientActions.FindIndex(a => a.ID == op.Value.Action);
        if (index >= 0)
        {
            _pendingClientActions[index].Rejected = _ws.CurrentTime;
            _pendingClientActions.RemoveAt(index);
        }
    }
}
