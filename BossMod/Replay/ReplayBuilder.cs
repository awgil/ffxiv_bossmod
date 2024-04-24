namespace BossMod;

// utility for building a replay by executing a series of recorded operations (eg. taken from a log) using private worldstate/module manager/etc
public sealed class ReplayBuilder : IDisposable
{
    private sealed class LoadedModuleData(BossModule module, Replay.Encounter enc) : IDisposable
    {
        public BossModule Module => module;
        public Replay.Encounter Encounter => enc;
        public int ActivePhaseIndex = -1;
        public StateMachine.State? ActiveState;
        private readonly EventSubscription _onError = module.Error.Subscribe((module, comp, message) => enc.Errors.Add(new(module.WorldState.CurrentTime, comp?.GetType(), message)));

        public void Dispose() => _onError.Dispose();
    }

    private readonly Replay _res;
    private readonly WorldState _ws;
    private readonly BossModuleManager _mgr;
    private readonly EventSubscriptions _subscribers;
    private readonly Dictionary<ulong, LoadedModuleData> _modules = [];
    private readonly Dictionary<ulong, Replay.Participant> _participants = []; // these are either existing actors, destroyed actors that can still be recreated, or never-created-but-referenced actors
    private readonly Dictionary<(ulong, int), Replay.Status> _statuses = [];
    private readonly Dictionary<ulong, Replay.Tether> _tethers = [];
    private readonly List<Replay.ClientAction> _pendingClientActions = [];

    public ReplayBuilder(string path)
    {
        _res = new() { Path = path };
        _ws = new(TimeSpan.TicksPerSecond, "pending");
        _mgr = new(_ws);
        _subscribers = new
        (
            _ws.Actors.Added.Subscribe(ActorAdded),
            _ws.Actors.Removed.Subscribe(ActorRemoved),
            _ws.Actors.Renamed.Subscribe(ActorRenamed),
            _ws.Actors.IsTargetableChanged.Subscribe(ActorTargetable),
            _ws.Actors.IsDeadChanged.Subscribe(ActorDead),
            _ws.Actors.Moved.Subscribe(ActorMoved),
            _ws.Actors.SizeChanged.Subscribe(ActorSize),
            _ws.Actors.HPMPChanged.Subscribe(ActorHPMP),
            _ws.Actors.CastStarted.Subscribe(CastStart),
            _ws.Actors.CastFinished.Subscribe(CastFinish),
            _ws.Actors.Tethered.Subscribe(TetherAdd),
            _ws.Actors.Untethered.Subscribe(TetherRemove),
            _ws.Actors.StatusGain.Subscribe(StatusGain),
            _ws.Actors.StatusLose.Subscribe(StatusLose),
            _ws.Actors.IconAppeared.Subscribe(EventIcon),
            _ws.Actors.CastEvent.Subscribe(EventCast),
            _ws.Actors.EffectResult.Subscribe(EventConfirm),
            _ws.UserMarkerAdded.Subscribe(EventUserMarker),
            _ws.CurrentZoneChanged.Subscribe(EventZoneChange),
            _ws.DirectorUpdate.Subscribe(EventDirectorUpdate),
            _ws.EnvControl.Subscribe(EventEnvControl),
            _ws.Client.ActionRequested.Subscribe(ClientActionRequested),
            _ws.Client.ActionRejected.Subscribe(ClientActionRejected),
            _mgr.ModuleLoaded.Subscribe(ModuleLoaded),
            _mgr.ModuleUnloaded.Subscribe(ModuleUnloaded)
        );
    }

    public void Dispose()
    {
        // note: it's not really necessary here, since we ultimately only remove internal subscriptions
        foreach (var m in _modules.Values)
            m.Dispose();
        _subscribers.Dispose();
        _mgr.Dispose();
    }

    public void Start(ulong qpf, string gameVersion)
    {
        _res.QPF = _ws.QPF = qpf;
        _res.GameVersion = _ws.GameVersion = gameVersion;
    }

    public void AddOp(WorldState.Operation op)
    {
        if (op is WorldState.OpFrameStart && _res.Ops.Count > 0)
            FinishFrame();
        _ws.Execute(op);
        _res.Ops.Add(op);
    }

    public Replay Finish()
    {
        FinishFrame();
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
                    foreach (var p in _participants.Values.Where(p => p.WorldExistence.Count > 0 && p.WorldExistence[^1].End == default)) // include only live actors
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
        if (p.WorldExistence.Count > 0 && p.WorldExistence[^1].End == default)
            p.WorldExistence.Ref(p.WorldExistence.Count - 1).End = _ws.CurrentTime;

        if (p.Casts.Count > 0 && p.Casts[^1].Time.End == default)
            p.Casts[^1].Time.End = _ws.CurrentTime;

        if (p.TargetableHistory.LastOrDefault().Value)
        {
            if (p.TargetableHistory.Keys[^1] < _ws.CurrentTime)
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
            throw new InvalidOperationException($"Unexpected actor add while participant still effectively exists: {actor}");
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
            if (p.WorldExistence[^1].End == default)
                throw new InvalidOperationException($"Actor add after actor add: {actor}");
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
        if (p.NameHistory.Count == 0 ? (actor.Name.Length > 0 || actor.NameID != 0) : p.NameHistory.Values[^1] != (actor.Name, actor.NameID))
            p.NameHistory.Add(_ws.CurrentTime, (actor.Name, actor.NameID));
        if (actor.IsTargetable)
            p.TargetableHistory.Add(_ws.CurrentTime, true);
        p.PosRotHistory.Add(_ws.CurrentTime, actor.PosRot);
        p.HPMPHistory.Add(_ws.CurrentTime, actor.HPMP);
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
        _participants[actor.InstanceID].HPMPHistory.Add(_ws.CurrentTime, actor.HPMP);
    }

    private void CastStart(Actor actor)
    {
        var c = actor.CastInfo!;
        var target = GetOrCreateOptionalParticipant(c.TargetID);
        var location = target?.PosRotAt(_ws.CurrentTime).XYZ() ?? c.Location;
        var cast = new Replay.Cast(c.Action, c.TotalTime, target, location, c.Rotation, c.Interruptible);
        cast.Time.Start = _ws.CurrentTime;
        _participants[actor.InstanceID].Casts.Add(cast);
        if (actor == _ws.Party.Player() && _pendingClientActions.Count > 0 && _pendingClientActions[^1].ID == c.Action)
        {
            cast.ClientAction = _pendingClientActions[^1];
            _pendingClientActions[^1].Cast = cast;
        }
    }

    private void CastFinish(Actor actor)
    {
        var cast = _participants[actor.InstanceID].Casts[^1];
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

    private void ModuleLoaded(BossModule module)
    {
        _modules.Add(module.PrimaryActor.InstanceID, new(module, new(module.PrimaryActor.InstanceID, module.PrimaryActor.OID, _ws.CurrentZone)));
    }

    private void ModuleUnloaded(BossModule module)
    {
        if (!_modules.Remove(module.PrimaryActor.InstanceID, out var data))
            throw new InvalidOperationException($"Module unloaded without being loaded before");

        if (data.ActiveState != null)
        {
            data.Encounter.Phases.Add(new(data.ActivePhaseIndex, data.ActiveState.ID, _ws.CurrentTime));
            data.Encounter.States.Add(new(data.ActiveState.ID, data.ActiveState.Name, data.ActiveState.Comment, data.ActiveState.Duration, _ws.CurrentTime));
        }
        data.Encounter.Time.End = _ws.CurrentTime;
    }
}
