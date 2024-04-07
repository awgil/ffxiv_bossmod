namespace BossMod;

// a set of existing actors in world; part of the world state structure
public class ActorState : IEnumerable<Actor>
{
    private Dictionary<ulong, Actor> _actors = new();

    public IEnumerator<Actor> GetEnumerator() => _actors.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _actors.Values.GetEnumerator();

    public Actor? Find(ulong instanceID) => instanceID != 0 && instanceID != 0xE0000000 ? _actors.GetValueOrDefault(instanceID) : null;

    // all actor-related operations have instance ID to which they are applied
    // in addition to worldstate's modification event, extra event with actor pointer is dispatched for all actor events
    public abstract class Operation : WorldState.Operation
    {
        public ulong InstanceID;

        protected abstract void ExecActor(WorldState ws, Actor actor);
        protected override void Exec(WorldState ws) => ExecActor(ws, ws.Actors._actors[InstanceID]);
    }

    public IEnumerable<Operation> CompareToInitial()
    {
        foreach (var act in this)
        {
            yield return new OpCreate()
            {
                InstanceID = act.InstanceID,
                OID = act.OID,
                SpawnIndex = act.SpawnIndex,
                Name = act.Name,
                NameID = act.NameID,
                Type = act.Type,
                Class = act.Class,
                Level = act.Level,
                PosRot = act.PosRot,
                HitboxRadius = act.HitboxRadius,
                HP = act.HP,
                CurMP = act.CurMP,
                IsTargetable = act.IsTargetable,
                IsAlly = act.IsAlly,
                OwnerID = act.OwnerID
            };

            if (act.IsDead)
                yield return new OpDead() { InstanceID = act.InstanceID, Value = true };
            if (act.InCombat)
                yield return new OpCombat() { InstanceID = act.InstanceID, Value = true };
            if (act.ModelState.ModelState != 0 || act.ModelState.AnimState1 != 0 || act.ModelState.AnimState2 != 0)
                yield return new OpModelState() { InstanceID = act.InstanceID, Value = act.ModelState };
            if (act.TargetID != 0)
                yield return new OpTarget() { InstanceID = act.InstanceID, Value = act.TargetID };
            if (act.Tether.Target != 0)
                yield return new OpTether() { InstanceID = act.InstanceID, Value = act.Tether };
            if (act.CastInfo != null)
                yield return new OpCastInfo() { InstanceID = act.InstanceID, Value = act.CastInfo };
            for (int i = 0; i < act.Statuses.Length; ++i)
                if (act.Statuses[i].ID != 0)
                    yield return new OpStatus() { InstanceID = act.InstanceID, Index = i, Value = act.Statuses[i] };
        }
    }

    // implementation of operations
    public event Action<Actor>? Added;
    public class OpCreate : Operation
    {
        public uint OID;
        public int SpawnIndex;
        public string Name = "";
        public uint NameID;
        public ActorType Type;
        public Class Class;
        public int Level;
        public Vector4 PosRot;
        public float HitboxRadius;
        public ActorHP HP;
        public uint CurMP;
        public bool IsTargetable;
        public bool IsAlly;
        public ulong OwnerID;

        protected override void ExecActor(WorldState ws, Actor actor) { }
        protected override void Exec(WorldState ws)
        {
            var actor = ws.Actors._actors[InstanceID] = new Actor(InstanceID, OID, SpawnIndex, Name, NameID, Type, Class, Level, PosRot, HitboxRadius, HP, CurMP, IsTargetable, IsAlly, OwnerID);
            ws.Actors.Added?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "ACT+")
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
            .Emit(HP.Cur)
            .Emit(HP.Max)
            .Emit(HP.Shield)
            .Emit(CurMP)
            .Emit(IsTargetable)
            .Emit(IsAlly)
            .EmitActor(OwnerID);
    }

    public event Action<Actor>? Removed;
    public class OpDestroy : Operation
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.IsDestroyed = true;
            if (actor.InCombat) // exit combat
            {
                actor.InCombat = false;
                ws.Actors.InCombatChanged?.Invoke(actor);
            }
            if (actor.Tether.Target != 0) // untether
            {
                ws.Actors.Untethered?.Invoke(actor);
                actor.Tether = new();
            }
            if (actor.CastInfo != null) // stop casting
            {
                ws.Actors.CastFinished?.Invoke(actor);
                actor.CastInfo = null;
            }
            for (int i = 0; i < actor.Statuses.Length; ++i)
            {
                if (actor.Statuses[i].ID != 0) // clear statuses
                {
                    ws.Actors.StatusLose?.Invoke(actor, i);
                    actor.Statuses[i] = new();
                }
            }
            ws.Actors.Removed?.Invoke(actor);
            ws.Actors._actors.Remove(InstanceID);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "ACT-").Emit(InstanceID, "X8");
    }

    public event Action<Actor>? Renamed;
    public class OpRename : Operation
    {
        public string Name = "";
        public uint NameID;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.Name = Name;
            actor.NameID = NameID;
            ws.Actors.Renamed?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "NAME").Emit(InstanceID, "X8").Emit(Name).Emit(NameID);
    }

    public event Action<Actor>? ClassChanged;
    public class OpClassChange : Operation
    {
        public Class Class;
        public int Level;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.Class = Class;
            actor.Level = Level;
            ws.Actors.ClassChanged?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "CLSR").EmitActor(InstanceID).Emit().Emit(Class).Emit(Level);
    }

    public event Action<Actor>? Moved;
    public class OpMove : Operation
    {
        public Vector4 PosRot;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.PosRot = PosRot;
            ws.Actors.Moved?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "MOVE").Emit(InstanceID, "X8").Emit(PosRot.XYZ()).Emit(PosRot.W.Radians());
    }

    public event Action<Actor>? SizeChanged;
    public class OpSizeChange : Operation
    {
        public float HitboxRadius;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.HitboxRadius = HitboxRadius;
            ws.Actors.SizeChanged?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "ACSZ").EmitActor(InstanceID).Emit(HitboxRadius, "f3");
    }

    public event Action<Actor>? HPMPChanged;
    public class OpHPMP : Operation
    {
        public ActorHP HP;
        public uint CurMP;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.HP = HP;
            actor.CurMP = CurMP;
            ws.Actors.HPMPChanged?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "HP  ").EmitActor(InstanceID).Emit(HP.Cur).Emit(HP.Max).Emit(HP.Shield).Emit(CurMP);
    }

    public event Action<Actor>? IsTargetableChanged;
    public class OpTargetable : Operation
    {
        public bool Value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.IsTargetable = Value;
            ws.Actors.IsTargetableChanged?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, Value ? "ATG+" : "ATG-").EmitActor(InstanceID);
    }

    public event Action<Actor>? IsAllyChanged;
    public class OpAlly : Operation
    {
        public bool Value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.IsAlly = Value;
            ws.Actors.IsAllyChanged?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "ALLY").EmitActor(InstanceID).Emit(Value);
    }

    public event Action<Actor>? IsDeadChanged;
    public class OpDead : Operation
    {
        public bool Value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.IsDead = Value;
            ws.Actors.IsDeadChanged?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, Value ? "DIE+" : "DIE-").EmitActor(InstanceID);
    }

    public event Action<Actor>? InCombatChanged;
    public class OpCombat : Operation
    {
        public bool Value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.InCombat = Value;
            ws.Actors.InCombatChanged?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, Value ? "COM+" : "COM-").EmitActor(InstanceID);
    }

    public event Action<Actor>? ModelStateChanged;
    public class OpModelState : Operation
    {
        public ActorModelState Value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.ModelState = Value;
            ws.Actors.ModelStateChanged?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "MDLS").EmitActor(InstanceID).Emit(Value.ModelState).Emit(Value.AnimState1).Emit(Value.AnimState2);
    }

    public event Action<Actor>? EventStateChanged;
    public class OpEventState : Operation
    {
        public byte Value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.EventState = Value;
            ws.Actors.EventStateChanged?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "EVTS").EmitActor(InstanceID).Emit(Value);
    }

    public event Action<Actor>? TargetChanged;
    public class OpTarget : Operation
    {
        public ulong Value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.TargetID = Value;
            ws.Actors.TargetChanged?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "TARG").EmitActor(InstanceID).EmitActor(Value);
    }

    // note: this is currently based on network events rather than per-frame state inspection
    public event Action<Actor>? Tethered;
    public event Action<Actor>? Untethered; // note that actor structure still contains previous tether info when this is invoked; invoked if actor disappears without untethering
    public class OpTether : Operation
    {
        public ActorTetherInfo Value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            if (actor.Tether.Target != 0)
                ws.Actors.Untethered?.Invoke(actor);
            actor.Tether = Value;
            if (Value.Target != 0)
                ws.Actors.Tethered?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "TETH").EmitActor(InstanceID).Emit(Value.ID).EmitActor(Value.Target);
    }

    public event Action<Actor>? CastStarted;
    public event Action<Actor>? CastFinished; // note that actor structure still contains cast details when this is invoked; invoked if actor disappears without finishing cast
    public class OpCastInfo : Operation
    {
        public ActorCastInfo? Value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            if (actor.CastInfo != null)
                ws.Actors.CastFinished?.Invoke(actor);
            actor.CastInfo = Value;
            if (Value != null)
                ws.Actors.CastStarted?.Invoke(actor);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            if (Value != null)
                WriteTag(output, "CST+").EmitActor(InstanceID).Emit(Value.Action).EmitActor(Value.TargetID).Emit(Value.Location).EmitTimePair(Value.FinishAt, Value.TotalTime).Emit(Value.Interruptible).Emit(Value.Rotation);
            else
                WriteTag(output, "CST-").EmitActor(InstanceID);
        }
    }

    // note: this is inherently an event, it can't be accessed from actor fields
    public event Action<Actor, ActorCastEvent>? CastEvent;
    public class OpCastEvent : Operation
    {
        public ActorCastEvent Value = new();

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            if (actor.CastInfo?.Action == Value.Action)
                actor.CastInfo.EventHappened = true;
            ws.Actors.CastEvent?.Invoke(actor, Value);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "CST!")
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
    public event Action<Actor, uint, int>? EffectResult;
    public class OpEffectResult : Operation
    {
        public uint Seq;
        public int TargetIndex;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            ws.Actors.EffectResult?.Invoke(actor, Seq, TargetIndex);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "ER  ").EmitActor(InstanceID).Emit(Seq).Emit(TargetIndex);
    }

    public event Action<Actor, int>? StatusGain; // called when status appears -or- when extra or expiration time is changed
    public event Action<Actor, int>? StatusLose; // note that status structure still contains details when this is invoked; invoked if actor disappears
    public class OpStatus : Operation
    {
        public int Index;
        public ActorStatus Value;
        public ActorStatus PrevValue { get; private set; }

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            PrevValue = actor.Statuses[Index];
            if (PrevValue.ID != 0 && (PrevValue.ID != Value.ID || PrevValue.SourceID != Value.SourceID))
                ws.Actors.StatusLose?.Invoke(actor, Index);
            actor.Statuses[Index] = Value;
            if (Value.ID != 0)
                ws.Actors.StatusGain?.Invoke(actor, Index);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            if (Value.ID != 0)
                WriteTag(output, "STA+").EmitActor(InstanceID).Emit(Index).Emit(Value);
            else
                WriteTag(output, "STA-").EmitActor(InstanceID).Emit(Index);
        }
    }

    // TODO: this should really be an actor field, but I have no idea what triggers icon clear...
    public event Action<Actor, uint>? IconAppeared;
    public class OpIcon : Operation
    {
        public uint IconID;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            ws.Actors.IconAppeared?.Invoke(actor, IconID);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "ICON").EmitActor(InstanceID).Emit(IconID);
    }

    // TODO: this should be an actor field (?)
    public event Action<Actor, ushort>? EventObjectStateChange;
    public class OpEventObjectStateChange : Operation
    {
        public ushort State;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            ws.Actors.EventObjectStateChange?.Invoke(actor, State);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "ESTA").EmitActor(InstanceID).Emit(State, "X4");
    }

    // TODO: this should be an actor field (?)
    public event Action<Actor, ushort, ushort>? EventObjectAnimation;
    public class OpEventObjectAnimation : Operation
    {
        public ushort Param1;
        public ushort Param2;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            ws.Actors.EventObjectAnimation?.Invoke(actor, Param1, Param2);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "EANM").EmitActor(InstanceID).Emit(Param1, "X4").Emit(Param2, "X4");
    }

    // TODO: this needs more reversing...
    public event Action<Actor, ushort>? PlayActionTimelineEvent;
    public class OpPlayActionTimelineEvent : Operation
    {
        public ushort ActionTimelineID;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            ws.Actors.PlayActionTimelineEvent?.Invoke(actor, ActionTimelineID);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "PATE").EmitActor(InstanceID).Emit(ActionTimelineID, "X4");
    }

    public event Action<Actor, ushort>? EventNpcYell;
    public class OpEventNpcYell : Operation
    {
        public ushort Message;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            ws.Actors.EventNpcYell?.Invoke(actor, Message);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "NYEL").EmitActor(InstanceID).Emit(Message);
    }
}
