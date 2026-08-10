namespace BossMod;

// a set of existing actors in world; part of the world state structure
// TODO: consider indexing by spawnindex?..
public sealed class ActorState : IEnumerable<Actor>
{
    public readonly Dictionary<ulong, Actor> Actors = [];

    public IEnumerator<Actor> GetEnumerator() => Actors.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Actors.Values.GetEnumerator();

    public const uint StatusIDDirectionalDisregard = 3808u;

    public Actor? Find(ulong instanceID) => instanceID is not 0u and not 0xE0000000 ? Actors.GetValueOrDefault(instanceID) : null;

    // all actor-related operations have instance ID to which they are applied
    // in addition to worldstate's modification event, extra event with actor pointer is dispatched for all actor events
    public abstract class Operation(ulong instanceID) : WorldState.Operation
    {
        public readonly ulong InstanceID = instanceID;

        protected abstract void ExecActor(WorldState ws, Actor actor);
        protected override void Exec(WorldState ws)
        {
            if (ws.Actors.Actors.TryGetValue(InstanceID, out var actor))
            {
                ExecActor(ws, actor);
            }
        }
    }

    public List<Operation> CompareToInitial()
    {
        List<Operation> ops = [with(Actors.Count * 5)];
        foreach (var act in Actors.Values)
        {
            var instanceID = act.InstanceID;
            ops.Add(new OpCreate(instanceID, act.OID, act.SpawnIndex, act.LayoutID, act.Name, act.NameID, act.Type, act.Class, act.Level, act.PosRot, act.HitboxRadius, act.HPMP, act.IsTargetable, act.IsAlly, act.OwnerID, act.FateID, act.Renderflags));
            if (act.IsDead)
            {
                ops.Add(new OpDead(instanceID, true));
            }
            if (act.InCombat)
            {
                ops.Add(new OpCombat(instanceID, true));
            }
            if (act.IsOpenTreasure)
            {
                ops.Add(new OpEventOpenTreasure(act.InstanceID));
            }
            if (act.Visibility != default)
            {
                ops.Add(new OpVisibility(act.InstanceID, act.Visibility));
            }
            if (act.ModelState != default)
            {
                ops.Add(new OpModelState(instanceID, act.ModelState));
            }
            if (act.EventState != default)
            {
                ops.Add(new OpEventState(instanceID, act.EventState));
            }
            if (act.TargetID != default)
            {
                ops.Add(new OpTarget(instanceID, act.TargetID));
            }
            if (act.MountId != default)
            {
                ops.Add(new OpMount(instanceID, act.MountId));
            }
            if (act.ForayInfo != default)
            {
                ops.Add(new OpForayInfo(act.InstanceID, act.ForayInfo));
            }
            if (act.Tether.ID != default)
            {
                ops.Add(new OpTether(instanceID, act.Tether));
            }
            if (act.CastInfo != null)
            {
                ops.Add(new OpCastInfo(instanceID, act.CastInfo));
            }

            for (var i = 0; i < Actor.NumStatuses; ++i)
            {
                ref readonly var status = ref act.Statuses[i];
                if (status.ID != default)
                {
                    ops.Add(new OpStatus(instanceID, i, status));
                }
            }

            for (var i = 0; i < Actor.NumIncomingEffects; ++i)
            {
                ref readonly var effect = ref act.IncomingEffects[i];
                if (effect.GlobalSequence != default)
                {
                    ops.Add(new OpIncomingEffect(instanceID, i, effect));
                }
            }
        }
        return ops;
    }

    public void Tick(in FrameState frame)
    {
        var ts = frame.Timestamp;
        foreach (var act in Actors.Values)
        {
            act.PrevPosRot = act.PosRot;
            var castinfo = act.CastInfo;
            if (castinfo != null)
            {
                act.CastInfo!.ElapsedTime = Math.Min(castinfo.ElapsedTime + frame.Duration, castinfo.AdjustedTotalTime);
            }

            var span = CollectionsMarshal.AsSpan(act.PendingHPDifferences);
            var len = span.Length;
            var dst = 0;

            for (var src = 0; src < len; ++src)
            {
                ref readonly var item = ref span[src];
                ref readonly var e = ref item.Effect;
                if (e.Expiration >= ts)
                {
                    ref var s = ref span[dst];
                    s = item;
                    ++dst;
                }
            }

            if (dst < len)
            {
                act.PendingHPDifferences.RemoveRange(dst, len - dst);
            }

            span = CollectionsMarshal.AsSpan(act.PendingMPDifferences);
            len = span.Length;
            dst = 0;

            for (var src = 0; src < len; ++src)
            {
                ref readonly var item = ref span[src];
                ref readonly var e = ref item.Effect;
                if (e.Expiration >= ts)
                {
                    ref var s = ref span[dst];
                    s = item;
                    ++dst;
                }
            }

            if (dst < len)
            {
                act.PendingMPDifferences.RemoveRange(dst, len - dst);
            }

            var span2 = CollectionsMarshal.AsSpan(act.PendingStatuses);
            len = span2.Length;
            dst = 0;

            for (var src = 0; src < len; ++src)
            {
                ref readonly var item = ref span2[src];
                ref readonly var e = ref item.Effect;
                if (e.Expiration >= ts)
                {
                    ref var s = ref span2[dst];
                    s = item;
                    ++dst;
                }
            }

            if (dst < len)
            {
                act.PendingStatuses.RemoveRange(dst, len - dst);
            }

            var span3 = CollectionsMarshal.AsSpan(act.PendingDispels);
            len = span3.Length;
            dst = 0;

            for (var src = 0; src < len; ++src)
            {
                ref readonly var item = ref span3[src];
                ref readonly var e = ref item.Effect;
                if (e.Expiration >= ts)
                {
                    ref var s = ref span3[dst];
                    s = item;
                    ++dst;
                }
            }

            if (dst < len)
            {
                act.PendingDispels.RemoveRange(dst, len - dst);
            }

            var span4 = CollectionsMarshal.AsSpan(act.PendingKnockbacks);
            len = span4.Length;
            dst = 0;

            for (var src = 0; src < len; ++src)
            {
                ref readonly var e = ref span4[src];
                if (e.Expiration >= ts)
                {
                    ref var s = ref span4[dst];
                    s = e;
                    ++dst;
                }
            }

            if (dst < len)
            {
                act.PendingKnockbacks.RemoveRange(dst, len - dst);
            }
        }
    }

    private void AddPendingEffects(Actor source, ActorCastEvent ev, DateTime timestamp)
    {
        var expiration = timestamp.AddSeconds(3d);
        var count = ev.Targets.Count;
        var targets = CollectionsMarshal.AsSpan(ev.Targets);
        for (var i = 0; i < count; ++i)
        {
            ref var t = ref targets[i];
            var targetID = t.ID;
            var target = targetID == source.InstanceID ? source : Find(targetID); // most common case by far is self-target
            if (target == null)
            {
                continue;
            }

            var effects = t.Effects.ValidEffects();
            var len = effects.Length;
            for (var j = 0; j < len; ++j)
            {
                ref readonly var eff = ref effects[j];
                var type = eff.Type;
                if (type > ActionEffectType.LoseStatusEffectSource)
                {
                    continue;
                }
                var effSource = eff.FromTarget ? target : source;
                var effTarget = eff.AtSource ? source : target;
                var header = new PendingEffect(ev.GlobalSequence, i, effSource.InstanceID, expiration, false);
                switch (type)
                {
                    case ActionEffectType.Damage:
                    case ActionEffectType.BlockedDamage:
                    case ActionEffectType.ParriedDamage:
                        // note: if actual damage will not result in hp change (eg overkill by other pending effects, invulnerability effects), we won't get confirmation
                        effTarget.PendingHPDifferences.Add(new(header, -eff.DamageHealValue));
                        break;
                    case ActionEffectType.Heal:
                        // note: if actual heal will not result in hp change (eg 100% overheal), we won't get confirmation
                        effTarget.PendingHPDifferences.Add(new(header, +eff.DamageHealValue));
                        break;
                    case ActionEffectType.MpLoss:
                        effTarget.PendingMPDifferences.Add(new(header, -eff.Value));
                        break;
                    case ActionEffectType.MpGain:
                        effTarget.PendingMPDifferences.Add(new(header, +eff.Value));
                        break;
                    case ActionEffectType.ApplyStatusEffectTarget:
                    case ActionEffectType.ApplyStatusEffectSource:
                        // note: effect reapplication (eg kardia) or some 'instant' effects (eg ast draw/earthly star) won't get confirmations
                        effTarget.PendingStatuses.Add(new(header, eff.Value, eff.Param2));
                        break;
                    case ActionEffectType.RecoveredFromStatusEffect:
                    case ActionEffectType.LoseStatusEffectTarget:
                    case ActionEffectType.LoseStatusEffectSource:
                        effTarget.PendingDispels.Add(new(header, eff.Value));
                        break;
                }
            }
        }
    }

    // implementation of operations
    public Event<Actor> Added = new();
    public sealed class OpCreate(ulong instanceID, uint oid, int spawnIndex, uint layoutID, string name, uint nameID, ActorType type, Class clasz, int level, Vector4 posRot, float hitboxRadius,
        ActorHPMP hpmp, bool isTargetable, bool isAlly, ulong ownerID, uint fateID, int renderflags)
        : Operation(instanceID)
    {
        public readonly uint OID = oid;
        public readonly int SpawnIndex = spawnIndex;
        public readonly uint LayoutID = layoutID;
        public readonly string Name = name;
        public readonly uint NameID = nameID;
        public readonly ActorType Type = type;
        public readonly Class Class = clasz;
        public readonly int Level = level;
        public readonly Vector4 PosRot = posRot;
        public readonly float HitboxRadius = hitboxRadius;
        public readonly ActorHPMP HPMP = hpmp;
        public readonly bool IsTargetable = isTargetable;
        public readonly bool IsAlly = isAlly;
        public readonly ulong OwnerID = ownerID;
        public readonly uint FateID = fateID;
        public readonly int Renderflags = renderflags;

        protected override void ExecActor(WorldState ws, Actor actor) { }
        protected override void Exec(WorldState ws)
        {
            var actor = ws.Actors.Actors[InstanceID] = new Actor(InstanceID, OID, SpawnIndex, LayoutID, Name, NameID, Type, Class, Level, PosRot, HitboxRadius, HPMP, IsTargetable, IsAlly, OwnerID, FateID, Renderflags);
            ws.Actors.Added.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            ref readonly var hpmp = ref HPMP;
            output.EmitFourCC("ACT+"u8)
           .Emit(InstanceID, "X8")
           .Emit(OID, "X")
           .Emit(SpawnIndex)
           .Emit(LayoutID, "X")
           .Emit(Name)
           .Emit(NameID)
           .Emit((ushort)Type, "X4")
           .Emit(Class)
           .Emit(Level)
           .Emit(PosRot.XYZ())
           .Emit(PosRot.W.Radians())
           .Emit(HitboxRadius, "f3")
           .Emit(hpmp.CurHP)
           .Emit(hpmp.MaxHP)
           .Emit(hpmp.Shield)
           .Emit(hpmp.CurMP)
           .Emit(hpmp.MaxMP)
           .Emit(IsTargetable)
           .Emit(IsAlly)
           .EmitActor(OwnerID)
           .Emit(FateID)
           .Emit(Renderflags);
        }
    }

    public Event<Actor> Removed = new();
    public sealed class OpDestroy(ulong InstanceID) : Operation(InstanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.IsDestroyed = true;
            var wsactors = ws.Actors;
            if (actor.InCombat) // exit combat
            {
                actor.InCombat = false;
                wsactors.InCombatChanged.Fire(actor);
            }
            if (actor.Tether.Target != default) // untether
            {
                wsactors.Untethered.Fire(actor);
                actor.Tether = default;
            }
            if (actor.CastInfo != null) // stop casting
            {
                wsactors.CastFinished.Fire(actor);
                actor.CastInfo = null;
            }
            for (var i = 0; i < Actor.NumStatuses; ++i)
            {
                ref var status = ref actor.Statuses[i];
                if (status.ID != default) // clear statuses
                {
                    wsactors.StatusLose.Fire(actor, i);
                    status = default;
                }
            }
            wsactors.Removed.Fire(actor);
            wsactors.Actors.Remove(InstanceID);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ACT-"u8).Emit(InstanceID, "X8");
    }

    public Event<Actor> Renamed = new();
    public sealed class OpRename(ulong instanceID, string name, uint nameID) : Operation(instanceID)
    {
        public readonly string Name = name;
        public readonly uint NameID = nameID;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.Name = Name;
            actor.NameID = NameID;
            ws.Actors.Renamed.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("NAME"u8).Emit(InstanceID, "X8").Emit(Name).Emit(NameID);
    }

    public Event<Actor> ClassChanged = new();
    public sealed class OpClassChange(ulong instanceID, Class clasz, int level) : Operation(instanceID)
    {
        public readonly Class Class = clasz;
        public readonly int Level = level;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.Class = Class;
            actor.Level = Level;
            ws.Actors.ClassChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLSR"u8).EmitActor(InstanceID).Emit().Emit(Class).Emit(Level);
    }

    public Event<Actor> Moved = new();
    public sealed class OpMove(ulong instanceID, Vector4 posRot) : Operation(instanceID)
    {
        public readonly Vector4 PosRot = posRot;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.PosRot = PosRot;
            ws.Actors.Moved.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("MOVE"u8).Emit(InstanceID, "X8").Emit(PosRot.XYZ()).Emit(PosRot.W.Radians());
    }

    public Event<Actor> SizeChanged = new();
    public sealed class OpSizeChange(ulong instanceID, float hitboxRadius) : Operation(instanceID)
    {
        public readonly float HitboxRadius = hitboxRadius;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.HitboxRadius = HitboxRadius;
            ws.Actors.SizeChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ACSZ"u8).EmitActor(InstanceID).Emit(HitboxRadius, "f3");
    }

    public Event<Actor> HPMPChanged = new();
    public sealed class OpHPMP(ulong instanceID, ActorHPMP hpmp) : Operation(instanceID)
    {
        public readonly ActorHPMP HPMP = hpmp;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.HPMP = HPMP;
            ws.Actors.HPMPChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("HP  "u8).EmitActor(InstanceID).Emit(HPMP.CurHP).Emit(HPMP.MaxHP).Emit(HPMP.Shield).Emit(HPMP.CurMP).Emit(HPMP.MaxMP);
    }

    public Event<Actor> IsTargetableChanged = new();
    public sealed class OpTargetable(ulong instanceID, bool value) : Operation(instanceID)
    {
        public readonly bool Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.IsTargetable = Value;
            ws.Actors.IsTargetableChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC(Value ? "ATG+"u8 : "ATG-"u8).EmitActor(InstanceID);
    }

    public Event<Actor> VisibilityChanged = new();
    public sealed class OpVisibility(ulong instanceID, Visibility value) : Operation(instanceID)
    {
        public readonly Visibility Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.Visibility = Value;
            ws.Actors.VisibilityChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("AVIS"u8).EmitActor(InstanceID).Emit(Value.Encode());
    }

    public Event<Actor> RenderflagsChanged = new();
    public sealed class OpRenderflags(ulong instanceID, int value) : Operation(instanceID)
    {
        public readonly int Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.Renderflags = Value;
            ws.Actors.RenderflagsChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("RFLG"u8).EmitActor(InstanceID).Emit(Value);
    }

    public Event<Actor> IsAllyChanged = new();
    public sealed class OpAlly(ulong instanceID, bool value) : Operation(instanceID)
    {
        public readonly bool Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.IsAlly = Value;
            ws.Actors.IsAllyChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ALLY"u8).EmitActor(InstanceID).Emit(Value);
    }

    public Event<Actor> IsDeadChanged = new();
    public sealed class OpDead(ulong instanceID, bool value) : Operation(instanceID)
    {
        public readonly bool Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.IsDead = Value;
            ws.Actors.IsDeadChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC(Value ? "DIE+"u8 : "DIE-"u8).EmitActor(InstanceID);
    }

    public Event<Actor> InCombatChanged = new();
    public sealed class OpCombat(ulong instanceID, bool value) : Operation(instanceID)
    {
        public readonly bool Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.InCombat = Value;
            ws.Actors.InCombatChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC(Value ? "COM+"u8 : "COM-"u8).EmitActor(InstanceID);
    }

    public Event<Actor> AggroPlayerChanged = new();
    public sealed class OpAggroPlayer(ulong instanceID, bool has) : Operation(instanceID)
    {
        public readonly bool Has = has;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.AggroPlayer = Has;
            ws.Actors.AggroPlayerChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("NENP"u8).EmitActor(InstanceID).Emit(Has);
    }

    public Event<Actor> ModelStateChanged = new();
    public sealed class OpModelState(ulong instanceID, ActorModelState value) : Operation(instanceID)
    {
        public readonly ActorModelState Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.ModelState = Value;
            ws.Actors.ModelStateChanged.Fire(actor);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            ref readonly var v = ref Value;
            output.EmitFourCC("MDLS"u8).EmitActor(InstanceID).Emit(v.ModelState).Emit(v.AnimState1).Emit(v.AnimState2);
        }
    }

    public Event<Actor> EventStateChanged = new();
    public sealed class OpEventState(ulong instanceID, byte value) : Operation(instanceID)
    {
        public readonly byte Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.EventState = Value;
            ws.Actors.EventStateChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("EVTS"u8).EmitActor(InstanceID).Emit(Value);
    }

    public Event<Actor> TargetChanged = new();
    public sealed class OpTarget(ulong instanceID, ulong value) : Operation(instanceID)
    {
        public readonly ulong Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.TargetID = Value;
            ws.Actors.TargetChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("TARG"u8).EmitActor(InstanceID).EmitActor(Value);
    }

    public Event<Actor> MountChanged = new();
    public sealed class OpMount(ulong instanceID, uint value) : Operation(instanceID)
    {
        public readonly uint Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.MountId = Value;
            ws.Actors.MountChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("MNTD"u8).EmitActor(InstanceID).Emit(Value);
    }

    public Event<Actor> ForayInfoChanged = new();
    public sealed class OpForayInfo(ulong instanceID, ActorForayInfo value) : Operation(instanceID)
    {
        public readonly ActorForayInfo Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.ForayInfo = Value;
            ws.Actors.ForayInfoChanged.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("FORA"u8).EmitActor(InstanceID).Emit(Value.Level).Emit(Value.Element);
    }

    // note: this is currently based on network events rather than per-frame state inspection
    public Event<Actor> Tethered = new();
    public Event<Actor> Untethered = new(); // note that actor structure still contains previous tether info when this is invoked; invoked if actor disappears without untethering
    public sealed class OpTether(ulong instanceID, ActorTetherInfo value) : Operation(instanceID)
    {
        public readonly ActorTetherInfo Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            if (actor.Tether.Target != default)
            {
                ws.Actors.Untethered.Fire(actor);
            }

            actor.Tether = Value;
            if (Value.Target != default)
            {
                ws.Actors.Tethered.Fire(actor);
            }
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("TETH"u8).EmitActor(InstanceID).Emit(Value.ID).EmitActor(Value.Target);
    }

    public Event<Actor> CastStarted = new();
    public Event<Actor> CastFinished = new(); // note that actor structure still contains cast details when this is invoked; invoked if actor disappears without finishing cast
    public sealed class OpCastInfo(ulong instanceID, ActorCastInfo? value) : Operation(instanceID)
    {
        public readonly ActorCastInfo? Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            var wsactors = ws.Actors;
            if (actor.CastInfo != null)
            {
                wsactors.CastFinished.Fire(actor);
            }

            actor.CastInfo = Value?.Clone();
            if (Value != null)
            {
                wsactors.CastStarted.Fire(actor);
            }
        }
        public override void Write(ReplayRecorder.Output output)
        {
            if (Value != null)
            {
                output.EmitFourCC("CST+"u8).EmitActor(InstanceID).Emit(Value.Action).EmitActor(Value.TargetID).Emit(Value.Location).EmitFloatPair(Value.ElapsedTime, Value.TotalTime).Emit(Value.Interruptible).Emit(Value.Rotation);
            }
            else
            {
                output.EmitFourCC("CST-"u8).EmitActor(InstanceID);
            }
        }
    }

    // note: this is inherently an event, it can't be accessed from actor fields
    public Event<Actor, ActorCastEvent> CastEvent = new();
    public sealed class OpCastEvent(ulong instanceID, ActorCastEvent value) : Operation(instanceID)
    {
        public readonly ActorCastEvent Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            var wsactors = ws.Actors;
            if (actor.CastInfo?.Action == Value.Action)
            {
                actor.CastInfo.EventHappened = true;
            }

            wsactors.AddPendingEffects(actor, Value, ws.CurrentTime);
            wsactors.CastEvent.Fire(actor, Value);
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
            .Emit(Value.Rotation)
            .Emit(Value.Targets);
    }

    // note: this is inherently an event, it can't be accessed from actor fields
    public Event<Actor, uint, int> EffectResult = new();
    public sealed class OpEffectResult(ulong instanceID, uint seq, int targetIndex) : Operation(instanceID)
    {
        public readonly uint Seq = seq;
        public readonly int TargetIndex = targetIndex;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            var php = CollectionsMarshal.AsSpan(actor.PendingHPDifferences);
            var lenPHP = php.Length;
            for (var i = 0; i < lenPHP; ++i)
            {
                ref readonly var hp = ref php[i];
                ref readonly var p = ref hp.Effect;
                if (p.GlobalSequence == Seq && p.TargetIndex == TargetIndex)
                {
                    actor.PendingHPDifferences.RemoveAt(i);
                    break;
                }
            }
            var pmp = CollectionsMarshal.AsSpan(actor.PendingMPDifferences);
            var lenPMP = pmp.Length;
            for (var i = 0; i < lenPMP; ++i)
            {
                ref readonly var mp = ref pmp[i];
                ref readonly var p = ref mp.Effect;
                if (p.GlobalSequence == Seq && p.TargetIndex == TargetIndex)
                {
                    actor.PendingMPDifferences.RemoveAt(i);
                    break;
                }
            }
            var ps = CollectionsMarshal.AsSpan(actor.PendingStatuses);
            var lenPS = ps.Length;
            for (var i = 0; i < lenPS; ++i)
            {
                ref readonly var s = ref ps[i];
                ref readonly var p = ref s.Effect;
                if (p.GlobalSequence == Seq && p.TargetIndex == TargetIndex)
                {
                    actor.PendingStatuses.RemoveAt(i);
                    break;
                }
            }
            var pd = CollectionsMarshal.AsSpan(actor.PendingDispels);
            var lenPD = pd.Length;
            for (var i = 0; i < lenPD; ++i)
            {
                ref readonly var d = ref pd[i];
                ref readonly var p = ref d.Effect;
                if (p.GlobalSequence == Seq && p.TargetIndex == TargetIndex)
                {
                    actor.PendingDispels.RemoveAt(i);
                    break;
                }
            }
            ws.Actors.EffectResult.Fire(actor, Seq, TargetIndex);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ER  "u8).EmitActor(InstanceID).Emit(Seq).Emit(TargetIndex);
    }

    public Event<Actor, int> StatusGain = new(); // called when status appears -or- when extra or expiration time is changed
    public Event<Actor, int> StatusLose = new(); // note that status structure still contains details when this is invoked; invoked if actor disappears
    public sealed class OpStatus(ulong instanceID, int index, ActorStatus value) : Operation(instanceID)
    {
        public readonly int Index = index;
        public readonly ActorStatus Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            ref readonly var prev = ref actor.Statuses[Index];
            ref readonly var v = ref Value;
            if (prev.ID != default && (prev.ID != v.ID || prev.SourceID != v.SourceID))
            {
                ws.Actors.StatusLose.Fire(actor, Index);
                if (prev.ID == StatusIDDirectionalDisregard)
                {
                    actor.Omnidirectional = false;
                }
            }
            actor.Statuses[Index] = v;
            var statuses = CollectionsMarshal.AsSpan(actor.PendingStatuses);
            var len = statuses.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var s = ref statuses[i];
                if (s.StatusId == v.ID && s.Effect.SourceInstanceID == v.SourceID)
                {
                    actor.PendingStatuses.RemoveAt(i);
                    break;
                }
            }

            if (v.ID != default)
            {
                ws.Actors.StatusGain.Fire(actor, Index);
                if (v.ID == StatusIDDirectionalDisregard)
                {
                    actor.Omnidirectional = true;
                }
            }
        }
        public override void Write(ReplayRecorder.Output output)
        {
            if (Value.ID != default)
            {
                output.EmitFourCC("STA+"u8).EmitActor(InstanceID).Emit(Index).Emit(Value);
            }
            else
            {
                output.EmitFourCC("STA-"u8).EmitActor(InstanceID).Emit(Index);
            }
        }
    }

    public Event<Actor, int> IncomingEffectAdd = new();
    public Event<Actor, int> IncomingEffectRemove = new();
    public sealed class OpIncomingEffect(ulong instanceID, int index, ActorIncomingEffect value) : Operation(instanceID)
    {
        public readonly int Index = index;
        public readonly ActorIncomingEffect Value = value;

        protected override void ExecActor(WorldState ws, Actor actor)
        {
            ref readonly var prev = ref actor.IncomingEffects[Index];
            var prevSeq = prev.GlobalSequence;
            var prevIdx = prev.TargetIndex;
            ref readonly var v = ref Value;
            if (prevSeq != default && (prevSeq != v.GlobalSequence || prevIdx != v.TargetIndex))
            {
                var effects = prev.Effects.ValidEffects();
                var lenE = effects.Length;
                for (var i = 0; i < lenE; ++i)
                {
                    ref readonly var e = ref effects[i];
                    if (e.Type is >= ActionEffectType.Knockback and <= ActionEffectType.AttractCustom3)
                    {
                        var pending = CollectionsMarshal.AsSpan(actor.PendingKnockbacks);
                        var len = pending.Length;
                        for (var j = 0; j < len; ++j)
                        {
                            ref var p = ref pending[j];
                            if (p.GlobalSequence == prevSeq && p.TargetIndex == prevIdx && !p.RequiresEffectResult)
                            {
                                actor.PendingKnockbacks.RemoveAt(j);
                                goto done;
                            }
                        }
                    }
                }
            done:
                ws.Actors.IncomingEffectRemove.Fire(actor, Index);
            }
            actor.IncomingEffects[Index] = v;
            if (v.GlobalSequence != default)
            {
                var effects = prev.Effects.ValidEffects();
                var lenE = effects.Length;
                for (var i = 0; i < lenE; ++i)
                {
                    var e = effects[i];
                    if (e.Type is >= ActionEffectType.Knockback and <= ActionEffectType.AttractCustom3)
                    {
                        // two annoying cases to handle with pending knockback:
                        // 1: effectresult never arrives
                        //    * happens if source dies
                        //    * happens always for some actions, such as Inhale from Traverse Gigant in Pilgrim's Traverse; effect is simply applied on the next globalseq
                        // 2. effecthandler entry disappears before effectresult arrives; happens when the knockback is not actually applied by the spell
                        //    * indicated by type=knockback dir=6; knockback is applied some time later by an ActorControl
                        var requiresEffectResult = e.Type == ActionEffectType.Knockback && Service.LuminaRow<Lumina.Excel.Sheets.Knockback>(e.Value)?.Direction == 6;
                        actor.PendingKnockbacks.Add(new(v.GlobalSequence, v.TargetIndex, v.SourceInstanceID, ws.FutureTime(3d), requiresEffectResult)); // note: sometimes effect can never be applied (eg if source dies shortly after actioneffect), so we need a timeout
                        break;
                    }
                }
                ws.Actors.IncomingEffectAdd.Fire(actor, Index);
            }
        }

        public override void Write(ReplayRecorder.Output output)
        {
            if (Value.GlobalSequence != default)
            {
                ref readonly var v = ref Value;
                output.EmitFourCC("AIE+"u8).EmitActor(InstanceID).Emit(Index).Emit(v.GlobalSequence).Emit(v.TargetIndex).EmitActor(v.SourceInstanceID).Emit(v.Action).Emit(v.Effects);
            }
            else
            {
                output.EmitFourCC("AIE-"u8).EmitActor(InstanceID).Emit(Index);
            }
        }
    }

    // icons are stored in actor's VfxContainer and expire after a fixed delay
    public Event<Actor, uint, ulong> IconAppeared = new();
    public sealed class OpIcon(ulong instanceID, uint iconID, ulong targetID) : Operation(instanceID)
    {
        public readonly uint IconID = iconID;
        public readonly ulong TargetID = targetID;

        protected override void ExecActor(WorldState ws, Actor actor) => ws.Actors.IconAppeared.Fire(actor, IconID, TargetID);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ICON"u8).EmitActor(InstanceID).Emit(IconID).EmitActor(TargetID);
    }

    // same as above, but only used in old content before Lockon replaced it
    public Event<Actor, uint, ulong> VFXAppeared = new();
    public sealed class OpVFX(ulong instanceID, uint vfxID, ulong targetID) : Operation(instanceID)
    {
        public readonly uint VfxID = vfxID;
        public readonly ulong TargetID = targetID;

        protected override void ExecActor(WorldState ws, Actor actor) => ws.Actors.VFXAppeared.Fire(actor, VfxID, TargetID);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("VFX "u8).EmitActor(InstanceID).Emit(VfxID).EmitActor(TargetID);
    }

    // TODO: this should be an actor field (?)
    public Event<Actor, ushort> EventObjectStateChange = new();
    public sealed class OpEventObjectStateChange(ulong instanceID, ushort state) : Operation(instanceID)
    {
        public readonly ushort State = state;

        protected override void ExecActor(WorldState ws, Actor actor) => ws.Actors.EventObjectStateChange.Fire(actor, State);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ESTA"u8).EmitActor(InstanceID).Emit(State, "X4");
    }

    // TODO: this should be an actor field (?)
    public Event<Actor, ushort, ushort> EventObjectAnimation = new();
    public sealed class OpEventObjectAnimation(ulong instanceID, ushort param1, ushort param2) : Operation(instanceID)
    {
        public readonly ushort Param1 = param1;
        public readonly ushort Param2 = param2;

        protected override void ExecActor(WorldState ws, Actor actor) => ws.Actors.EventObjectAnimation.Fire(actor, Param1, Param2);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("EANM"u8).EmitActor(InstanceID).Emit(Param1, "X4").Emit(Param2, "X4");
    }

    // TODO: this needs more reversing...
    public Event<Actor, ushort> PlayActionTimelineEvent = new();
    public sealed class OpPlayActionTimelineEvent(ulong instanceID, ushort actionTimelineID) : Operation(instanceID)
    {
        public readonly ushort ActionTimelineID = actionTimelineID;

        protected override void ExecActor(WorldState ws, Actor actor) => ws.Actors.PlayActionTimelineEvent.Fire(actor, ActionTimelineID);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("PATE"u8).EmitActor(InstanceID).Emit(ActionTimelineID, "X4");
    }

    public Event<Actor, List<(ulong, ushort)>> PlayActionTimelineSync = new();
    public sealed class OpPlayActionTimelineSync(ulong instanceID, List<(ulong, ushort)> actions) : Operation(instanceID)
    {
        public readonly List<(ulong, ushort)> Actions = actions;

        protected override void ExecActor(WorldState ws, Actor actor) => ws.Actors.PlayActionTimelineSync.Fire(actor, Actions);
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("PATS"u8).EmitActor(InstanceID).Emit(Actions.Count);
            var count = Actions.Count;
            for (var i = 0; i < count; ++i)
            {
                var a = Actions[i];
                output.EmitActor(a.Item1).Emit(a.Item2, "X4");
            }
        }
    }

    public Event<Actor, ushort> EventNpcYell = new();
    public sealed class OpEventNpcYell(ulong instanceID, ushort message) : Operation(instanceID)
    {
        public readonly ushort Message = message;

        protected override void ExecActor(WorldState ws, Actor actor) => ws.Actors.EventNpcYell.Fire(actor, Message);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("NYEL"u8).EmitActor(InstanceID).Emit(Message);
    }

    public Event<Actor> EventOpenTreasure = new();
    public sealed class OpEventOpenTreasure(ulong instanceID) : Operation(instanceID)
    {
        protected override void ExecActor(WorldState ws, Actor actor)
        {
            actor.IsOpenTreasure = true;
            ws.Actors.EventOpenTreasure.Fire(actor);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("OPNT"u8).EmitActor(InstanceID);
    }
}
