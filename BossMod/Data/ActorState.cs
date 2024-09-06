namespace BossMod;

// a set of existing actors in world; part of the world state structure
// TODO: consider indexing by spawnindex?..
public sealed class ActorState : IEnumerable<Actor>
{
    private readonly Dictionary<ulong, Actor> _actors = [];

    public IEnumerator<Actor> GetEnumerator() => _actors.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _actors.Values.GetEnumerator();

    public Actor? Find(ulong instanceID) => instanceID is not 0 and not 0xE0000000 ? _actors.GetValueOrDefault(instanceID) : null;

    // all actor-related operations have instance ID to which they are applied
    // in addition to worldstate's modification event, extra event with actor pointer is dispatched for all actor events
    public abstract record class Operation(ulong InstanceID) : WorldState.Operation
    {
        protected abstract void ExecActor(WorldState ws, Actor actor);
        protected override void Exec(WorldState ws) => ExecActor(ws, ws.Actors._actors[InstanceID]);
    }

    public IEnumerable<Operation> CompareToInitial()
    {
        foreach (var act in this)
        {
            yield return new OpCreate(act.InstanceID, act.OID, act.SpawnIndex, act.Name, act.NameID, act.Type, act.Class, act.Level, act.PosRot, act.HitboxRadius, act.HPMP, act.IsTargetable, act.IsAlly, act.OwnerID, act.FateID);
            if (act.IsDead)
                yield return new OpDead(act.InstanceID, true);
            if (act.InCombat)
                yield return new OpCombat(act.InstanceID, true);
            if (act.ModelState != default)
                yield return new OpModelState(act.InstanceID, act.ModelState);
            if (act.EventState != 0)
                yield return new OpEventState(act.InstanceID, act.EventState);
            if (act.TargetID != 0)
                yield return new OpTarget(act.InstanceID, act.TargetID);
            if (act.MountId != 0)
                yield return new OpMount(act.InstanceID, act.MountId);
            if (act.Tether.ID != 0)
                yield return new OpTether(act.InstanceID, act.Tether);
            if (act.CastInfo != null)
                yield return new OpCastInfo(act.InstanceID, act.CastInfo);
            for (int i = 0; i < act.Statuses.Length; ++i)
                if (act.Statuses[i].ID != 0)
                    yield return new OpStatus(act.InstanceID, i, act.Statuses[i]);
        }
    }

    public void Tick(float dt)
    {
        foreach (var act in this)
        {
            act.PrevPosRot = act.PosRot;
            if (act.CastInfo != null)
                act.CastInfo.ElapsedTime += dt;
        }
    }

    // implementation of operations
    public Event<Actor> Added = new();
    public sealed record class OpCreate(ulong InstanceID, uint OID, int SpawnIndex, string Name, uint NameID, ActorType Type, Class Class, int Level, Vector4 PosRot, float HitboxRadius,
        ActorHPMP HPMP, bool IsTargetable, bool IsAlly, ulong OwnerID, uint FateID)
        : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor) { }
        protected override void Exec(WorldState ws)
        {
            var actor = ws.Actors._actors[InstanceID] = new Actor(InstanceID, OID, SpawnIndex, Name, NameID, Type, Class, Level, PosRot, HitboxRadius, HPMP, IsTargetable, IsAlly, OwnerID, FateID);
            ws.Actors.Added.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ACT+"u8)
            .Emit(InstanceID, "X8")
            .Emit(OID, "X")
            .Emit(SpawnIndex)
            .Emit(Name)
            .Emit(NameID)
            .Emit((ushort)Type, "X4")
            .Emit(Class)
            .Emit(Level)
            .Emit(PosRot.XYZ())
            .Emit(PosRot.W.Radians())
            .Emit(HitboxRadius, "f3")
            .Emit(HPMP.CurHP)
            .Emit(HPMP.MaxHP)
            .Emit(HPMP.Shield)
            .Emit(HPMP.CurMP)
            .Emit(IsTargetable)
            .Emit(IsAlly)
            .EmitActor(OwnerID)
            .Emit(FateID);
    }

    public Event<Actor> Removed = new();
    public sealed record class OpDestroy(ulong InstanceID) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.IsDestroyed = true;
            if (actor.InCombat) // exit combat
            {
                actor.InCombat = false;
                ws.Actors.InCombatChanged.Fire(actor);
            }
            if (actor.Tether.Target != 0) // untether
            {
                ws.Actors.Untethered.Fire(actor);
                actor.Tether = default;
            }
            if (actor.CastInfo != null) // stop casting
            {
                ws.Actors.CastFinished.Fire(actor);
                actor.CastInfo = null;
            }
            for (int i = 0; i < actor.Statuses.Length; ++i)
            {
                if (actor.Statuses[i].ID != 0) // clear statuses
                {
                    ws.Actors.StatusLose.Fire(actor, i);
                    actor.Statuses[i] = default;
                }
            }
            ws.Actors.Removed.Fire(actor);
            ws.Actors._actors.Remove(InstanceID);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ACT-"u8).Emit(InstanceID, "X8");
    }

    public Event<Actor> Renamed = new();
    public sealed record class OpRename(ulong InstanceID, string Name, uint NameID) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.Name = Name;
            actor.NameID = NameID;
            ws.Actors.Renamed.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("NAME"u8).Emit(InstanceID, "X8").Emit(Name).Emit(NameID);
    }

    public Event<Actor> ClassChanged = new();
    public sealed record class OpClassChange(ulong InstanceID, Class Class, int Level) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.Class = Class;
            actor.Level = Level;
            ws.Actors.ClassChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLSR"u8).EmitActor(InstanceID).Emit().Emit(Class).Emit(Level);
    }

    public Event<Actor> Moved = new();
    public sealed record class OpMove(ulong InstanceID, Vector4 PosRot) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.PosRot = PosRot;
            ws.Actors.Moved.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("MOVE"u8).Emit(InstanceID, "X8").Emit(PosRot.XYZ()).Emit(PosRot.W.Radians());
    }

    public Event<Actor> SizeChanged = new();
    public sealed record class OpSizeChange(ulong InstanceID, float HitboxRadius) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.HitboxRadius = HitboxRadius;
            ws.Actors.SizeChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ACSZ"u8).EmitActor(InstanceID).Emit(HitboxRadius, "f3");
    }

    public Event<Actor> HPMPChanged = new();
    public sealed record class OpHPMP(ulong InstanceID, ActorHPMP HPMP) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.HPMP = HPMP;
            ws.Actors.HPMPChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("HP  "u8).EmitActor(InstanceID).Emit(HPMP.CurHP).Emit(HPMP.MaxHP).Emit(HPMP.Shield).Emit(HPMP.CurMP);
    }

    public Event<Actor> IsTargetableChanged = new();
    public sealed record class OpTargetable(ulong InstanceID, bool Value) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.IsTargetable = Value;
            ws.Actors.IsTargetableChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC(Value ? "ATG+"u8 : "ATG-"u8).EmitActor(InstanceID);
    }

    public Event<Actor> IsAllyChanged = new();
    public sealed record class OpAlly(ulong InstanceID, bool Value) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.IsAlly = Value;
            ws.Actors.IsAllyChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ALLY"u8).EmitActor(InstanceID).Emit(Value);
    }

    public Event<Actor> IsDeadChanged = new();
    public sealed record class OpDead(ulong InstanceID, bool Value) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.IsDead = Value;
            ws.Actors.IsDeadChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC(Value ? "DIE+"u8 : "DIE-"u8).EmitActor(InstanceID);
    }

    public Event<Actor> InCombatChanged = new();
    public sealed record class OpCombat(ulong InstanceID, bool Value) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.InCombat = Value;
            ws.Actors.InCombatChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC(Value ? "COM+"u8 : "COM-"u8).EmitActor(InstanceID);
    }

    public Event<Actor> AggroPlayerChanged = new();
    public sealed record class OpAggroPlayer(ulong InstanceID, bool Has) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.AggroPlayer = Has;
            ws.Actors.AggroPlayerChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("NENP"u8).EmitActor(InstanceID).Emit(Has);
    }

    public Event<Actor> ModelStateChanged = new();
    public sealed record class OpModelState(ulong InstanceID, ActorModelState Value) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.ModelState = Value;
            ws.Actors.ModelStateChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("MDLS"u8).EmitActor(InstanceID).Emit(Value.ModelState).Emit(Value.AnimState1).Emit(Value.AnimState2);
    }

    public Event<Actor> EventStateChanged = new();
    public sealed record class OpEventState(ulong InstanceID, byte Value) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.EventState = Value;
            ws.Actors.EventStateChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("EVTS"u8).EmitActor(InstanceID).Emit(Value);
    }

    public Event<Actor> TargetChanged = new();
    public sealed record class OpTarget(ulong InstanceID, ulong Value) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.TargetID = Value;
            ws.Actors.TargetChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("TARG"u8).EmitActor(InstanceID).EmitActor(Value);
    }

    public Event<Actor> MountChanged = new();
    public sealed record class OpMount(ulong InstanceID, uint Value) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.MountId = Value;
            ws.Actors.MountChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("MNTD"u8).EmitActor(InstanceID).Emit(Value);
    }

    // note: this is currently based on network events rather than per-frame state inspection
    public Event<Actor> Tethered = new();
    public Event<Actor> Untethered = new(); // note that actor structure still contains previous tether info when this is invoked; invoked if actor disappears without untethering
    public sealed record class OpTether(ulong InstanceID, ActorTetherInfo Value) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            if (actor.Tether.Target != 0)
                ws.Actors.Untethered.Fire(actor);
            actor.Tether = Value;
            if (Value.Target != 0)
                ws.Actors.Tethered.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("TETH"u8).EmitActor(InstanceID).Emit(Value.ID).EmitActor(Value.Target);
    }

    public Event<Actor> CastStarted = new();
    public Event<Actor> CastFinished = new(); // note that actor structure still contains cast details when this is invoked; invoked if actor disappears without finishing cast
    public sealed record class OpCastInfo(ulong InstanceID, ActorCastInfo? Value) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            if (actor.CastInfo != null)
                ws.Actors.CastFinished.Fire(actor);
            actor.CastInfo = Value != null ? Value with { } : null;
            if (Value != null)
                ws.Actors.CastStarted.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            if (Value != null)
                output.EmitFourCC("CST+"u8).EmitActor(InstanceID).Emit(Value.Action).EmitActor(Value.TargetID).Emit(Value.Location).EmitFloatPair(Value.ElapsedTime, Value.TotalTime).Emit(Value.Interruptible).Emit(Value.Rotation);
            else
                output.EmitFourCC("CST-"u8).EmitActor(InstanceID);
        }
    }

    // note: this is inherently an event, it can't be accessed from actor fields
    public Event<Actor, ActorCastEvent> CastEvent = new();
    public sealed record class OpCastEvent(ulong InstanceID, ActorCastEvent Value) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            if (actor.CastInfo?.Action == Value.Action)
                actor.CastInfo.EventHappened = true;
            ws.Actors.CastEvent.Fire(actor, Value);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CST!"u8)
            .EmitActor(InstanceID)
            .Emit(Value.Action)
            .EmitActor(Value.MainTargetID)
            .Emit(Value.AnimationLockTime, "f2")
            .Emit(Value.MaxTargets)
            .Emit(Value.TargetPos)
            .Emit(Value.GlobalSequence)
            .Emit(Value.SourceSequence)
            .Emit(Value.Targets);
    }

    // note: this is inherently an event, it can't be accessed from actor fields
    public Event<Actor, uint, int> EffectResult = new();
    public sealed record class OpEffectResult(ulong InstanceID, uint Seq, int TargetIndex) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor) => ws.Actors.EffectResult.Fire(actor, Seq, TargetIndex);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ER  "u8).EmitActor(InstanceID).Emit(Seq).Emit(TargetIndex);
    }

    public Event<Actor, int> StatusGain = new(); // called when status appears -or- when extra or expiration time is changed
    public Event<Actor, int> StatusLose = new(); // note that status structure still contains details when this is invoked; invoked if actor disappears
    public sealed record class OpStatus(ulong InstanceID, int Index, ActorStatus Value) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            ref var prev = ref actor.Statuses[Index];
            if (prev.ID != 0 && (prev.ID != Value.ID || prev.SourceID != Value.SourceID))
                ws.Actors.StatusLose.Fire(actor, Index);
            actor.Statuses[Index] = Value;
            if (Value.ID != 0)
                ws.Actors.StatusGain.Fire(actor, Index);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            if (Value.ID != 0)
                output.EmitFourCC("STA+"u8).EmitActor(InstanceID).Emit(Index).Emit(Value);
            else
                output.EmitFourCC("STA-"u8).EmitActor(InstanceID).Emit(Index);
        }
    }

    // TODO: this should really be an actor field, but I have no idea what triggers icon clear...
    public Event<Actor, uint> IconAppeared = new();
    public sealed record class OpIcon(ulong InstanceID, uint IconID) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor) => ws.Actors.IconAppeared.Fire(actor, IconID);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ICON"u8).EmitActor(InstanceID).Emit(IconID);
    }

    // TODO: this should be an actor field (?)
    public Event<Actor, ushort> EventObjectStateChange = new();
    public sealed record class OpEventObjectStateChange(ulong InstanceID, ushort State) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor) => ws.Actors.EventObjectStateChange.Fire(actor, State);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ESTA"u8).EmitActor(InstanceID).Emit(State, "X4");
    }

    // TODO: this should be an actor field (?)
    public Event<Actor, ushort, ushort> EventObjectAnimation = new();
    public sealed record class OpEventObjectAnimation(ulong InstanceID, ushort Param1, ushort Param2) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor) => ws.Actors.EventObjectAnimation.Fire(actor, Param1, Param2);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("EANM"u8).EmitActor(InstanceID).Emit(Param1, "X4").Emit(Param2, "X4");
    }

    // TODO: this needs more reversing...
    public Event<Actor, ushort> PlayActionTimelineEvent = new();
    public sealed record class OpPlayActionTimelineEvent(ulong InstanceID, ushort ActionTimelineID) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor) => ws.Actors.PlayActionTimelineEvent.Fire(actor, ActionTimelineID);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("PATE"u8).EmitActor(InstanceID).Emit(ActionTimelineID, "X4");
    }

    public Event<Actor, ushort> EventNpcYell = new();
    public sealed record class OpEventNpcYell(ulong InstanceID, ushort Message) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor) => ws.Actors.EventNpcYell.Fire(actor, Message);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("NYEL"u8).EmitActor(InstanceID).Emit(Message);
    }
}
