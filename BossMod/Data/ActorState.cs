using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BossMod
{
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

            protected static string StrActor(WorldState? ws, ulong instanceID)
            {
                var actor = ws?.Actors.Find(instanceID);
                return actor != null ? $"{actor.InstanceID:X8}/{actor.OID:X}/{actor.Name}/{actor.Type}/{StrVec3(actor.PosRot.XYZ())}/{actor.Rotation}" : $"{instanceID:X8}";
            }
            protected static string StrHPMP(ActorHP hp, uint curMP) => $"{hp.Cur}/{hp.Max}/{hp.Shield}/{curMP}";
        }

        public IEnumerable<Operation> CompareToInitial()
        {
            foreach (var act in this)
            {
                yield return new OpCreate() { InstanceID = act.InstanceID, OID = act.OID, SpawnIndex = act.SpawnIndex, Name = act.Name, Type = act.Type, Class = act.Class, PosRot = act.PosRot, HitboxRadius = act.HitboxRadius, HP = act.HP, CurMP = act.CurMP, IsTargetable = act.IsTargetable, IsAlly = act.IsAlly, OwnerID = act.OwnerID };
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
        public event EventHandler<Actor>? Added;
        public class OpCreate : Operation
        {
            public uint OID;
            public int SpawnIndex;
            public string Name = "";
            public ActorType Type;
            public Class Class;
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
                var actor = ws.Actors._actors[InstanceID] = new Actor(InstanceID, OID, SpawnIndex, Name, Type, Class, PosRot, HitboxRadius, HP, CurMP, IsTargetable, IsAlly, OwnerID);
                ws.Actors.Added?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => $"ACT+|{StrActor(ws, InstanceID)}|{Class}|{IsTargetable}|{HitboxRadius:f3}|{StrActor(ws, OwnerID)}|{StrHPMP(HP, CurMP)}|{IsAlly}|{SpawnIndex}";
        }

        public event EventHandler<Actor>? Removed;
        public class OpDestroy : Operation
        {
            protected override void ExecActor(WorldState ws, Actor actor)
            {
                actor.IsDestroyed = true;
                if (actor.InCombat) // exit combat
                {
                    actor.InCombat = false;
                    ws.Actors.InCombatChanged?.Invoke(ws, actor);
                }
                if (actor.Tether.Target != 0) // untether
                {
                    ws.Actors.Untethered?.Invoke(ws, actor);
                    actor.Tether = new();
                }
                if (actor.CastInfo != null) // stop casting
                {
                    ws.Actors.CastFinished?.Invoke(ws, actor);
                    actor.CastInfo = null;
                }
                for (int i = 0; i < actor.Statuses.Length; ++i)
                {
                    if (actor.Statuses[i].ID != 0) // clear statuses
                    {
                        ws.Actors.StatusLose?.Invoke(ws, (actor, i));
                        actor.Statuses[i] = new();
                    }
                }
                ws.Actors.Removed?.Invoke(ws, actor);
                ws.Actors._actors.Remove(InstanceID);
            }

            public override string Str(WorldState? ws) => $"ACT-|{InstanceID:X8}";
        }

        public event EventHandler<Actor>? Renamed;
        public class OpRename : Operation
        {
            public string Name = "";

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                actor.Name = Name;
                ws.Actors.Renamed?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => $"NAME|{StrActor(ws, InstanceID)}";
        }

        public event EventHandler<Actor>? ClassChanged;
        public class OpClassChange : Operation
        {
            public Class Class;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                actor.Class = Class;
                ws.Actors.ClassChanged?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => $"CLSR|{StrActor(ws, InstanceID)}|?|{Class}"; // TODO: remove legacy entry here...
        }

        public event EventHandler<Actor>? Moved;
        public class OpMove : Operation
        {
            public Vector4 PosRot;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                actor.PosRot = PosRot;
                ws.Actors.Moved?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => $"MOVE|{StrActor(ws, InstanceID)}";
        }

        public event EventHandler<Actor>? SizeChanged;
        public class OpSizeChange : Operation
        {
            public float HitboxRadius;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                actor.HitboxRadius = HitboxRadius;
                ws.Actors.SizeChanged?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => $"ACSZ|{StrActor(ws, InstanceID)}|{HitboxRadius:f3}";
        }

        public event EventHandler<Actor>? HPMPChanged;
        public class OpHPMP : Operation
        {
            public ActorHP HP;
            public uint CurMP;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                actor.HP = HP;
                actor.CurMP = CurMP;
                ws.Actors.HPMPChanged?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => $"HP  |{StrActor(ws, InstanceID)}|{StrHPMP(HP, CurMP)}";
        }

        public event EventHandler<Actor>? IsTargetableChanged;
        public class OpTargetable : Operation
        {
            public bool Value;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                actor.IsTargetable = Value;
                ws.Actors.IsTargetableChanged?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => $"{(Value ? "ATG+" : "ATG-")}|{StrActor(ws, InstanceID)}";
        }

        public event EventHandler<Actor>? IsAllyChanged;
        public class OpAlly : Operation
        {
            public bool Value;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                actor.IsAlly = Value;
                ws.Actors.IsAllyChanged?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => $"ALLY|{StrActor(ws, InstanceID)}|{Value}";
        }

        public event EventHandler<Actor>? IsDeadChanged;
        public class OpDead : Operation
        {
            public bool Value;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                actor.IsDead = Value;
                ws.Actors.IsDeadChanged?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => $"{(Value ? "DIE+" : "DIE-")}|{StrActor(ws, InstanceID)}";
        }

        public event EventHandler<Actor>? InCombatChanged;
        public class OpCombat : Operation
        {
            public bool Value;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                actor.InCombat = Value;
                ws.Actors.InCombatChanged?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => $"{(Value ? "COM+" : "COM-")}|{StrActor(ws, InstanceID)}";
        }

        public event EventHandler<Actor>? ModelStateChanged;
        public class OpModelState : Operation
        {
            public ActorModelState Value;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                actor.ModelState = Value;
                ws.Actors.ModelStateChanged?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => $"MDLS|{StrActor(ws, InstanceID)}|{Value.ModelState}|{Value.AnimState1}|{Value.AnimState2}";
        }

        public event EventHandler<Actor>? EventStateChanged;
        public class OpEventState : Operation
        {
            public byte Value;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                actor.EventState = Value;
                ws.Actors.EventStateChanged?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => $"EVTS|{StrActor(ws, InstanceID)}|{Value}";
        }

        public event EventHandler<Actor>? TargetChanged;
        public class OpTarget : Operation
        {
            public ulong Value;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                actor.TargetID = Value;
                ws.Actors.TargetChanged?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => $"TARG|{StrActor(ws, InstanceID)}|{StrActor(ws, Value)}";
        }

        // note: this is currently based on network events rather than per-frame state inspection
        public event EventHandler<Actor>? Tethered;
        public event EventHandler<Actor>? Untethered; // note that actor structure still contains previous tether info when this is invoked; invoked if actor disappears without untethering
        public class OpTether : Operation
        {
            public ActorTetherInfo Value;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                if (actor.Tether.Target != 0)
                    ws.Actors.Untethered?.Invoke(ws, actor);
                actor.Tether = Value;
                if (Value.Target != 0)
                    ws.Actors.Tethered?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => $"TETH|{StrActor(ws, InstanceID)}|{Value.ID}|{StrActor(ws, Value.Target)}";
        }

        public event EventHandler<Actor>? CastStarted;
        public event EventHandler<Actor>? CastFinished; // note that actor structure still contains cast details when this is invoked; invoked if actor disappears without finishing cast
        public class OpCastInfo : Operation
        {
            public ActorCastInfo? Value;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                if (actor.CastInfo != null)
                    ws.Actors.CastFinished?.Invoke(ws, actor);
                actor.CastInfo = Value;
                if (Value != null)
                    ws.Actors.CastStarted?.Invoke(ws, actor);
            }

            public override string Str(WorldState? ws) => Value != null
                ? $"CST+|{StrActor(ws, InstanceID)}|{Value.Action}|{StrActor(ws, Value.TargetID)}|{StrVec3(Value.Location)}|{Utils.CastTimeString(Value, ws?.CurrentTime ?? new())}|{Value.Interruptible}|{Value.Rotation}"
                : $"CST-|{StrActor(ws, InstanceID)}";
        }

        // note: this is inherently an event, it can't be accessed from actor fields
        public event EventHandler<(Actor, ActorCastEvent)>? CastEvent;
        public class OpCastEvent : Operation
        {
            public ActorCastEvent Value = new();

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                if (actor.CastInfo?.Action == Value.Action)
                    actor.CastInfo.EventHappened = true;
                ws.Actors.CastEvent?.Invoke(ws, (actor, Value));
            }

            public override string Str(WorldState? ws)
            {
                var sb = new StringBuilder($"CST!|{StrActor(ws, InstanceID)}|{Value.Action}|{StrActor(ws, Value.MainTargetID)}|{Value.AnimationLockTime:f2}|{Value.MaxTargets}|{StrVec3(Value.TargetPos)}|{Value.GlobalSequence}|{Value.SourceSequence}");
                foreach (var t in Value.Targets)
                {
                    sb.Append($"|{StrActor(ws, t.ID)}");
                    for (int i = 0; i < 8; ++i)
                        if (t.Effects[i] != 0)
                            sb.Append($"!{t.Effects[i]:X16}");
                }
                return sb.ToString();
            }
        }

        // note: this is inherently an event, it can't be accessed from actor fields
        public event EventHandler<(Actor Source, uint Seq, int TargetIndex)>? EffectResult;
        public class OpEffectResult : Operation
        {
            public uint Seq;
            public int TargetIndex;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                ws.Actors.EffectResult?.Invoke(ws, (actor, Seq, TargetIndex));
            }

            public override string Str(WorldState? ws) => $"ER  |{StrActor(ws, InstanceID)}|{Seq}|{TargetIndex}";
        }

        public event EventHandler<(Actor, int)>? StatusGain; // called when status appears -or- when extra or expiration time is changed
        public event EventHandler<(Actor, int)>? StatusLose; // note that status structure still contains details when this is invoked; invoked if actor disappears
        public class OpStatus : Operation
        {
            public int Index;
            public ActorStatus Value;
            public ActorStatus PrevValue { get; private set; }

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                PrevValue = actor.Statuses[Index];
                if (PrevValue.ID != 0 && (PrevValue.ID != Value.ID || PrevValue.SourceID != Value.SourceID))
                    ws.Actors.StatusLose?.Invoke(ws, (actor, Index));
                actor.Statuses[Index] = Value;
                if (Value.ID != 0)
                    ws.Actors.StatusGain?.Invoke(ws, (actor, Index));
            }

            public override string Str(WorldState? ws) => Value.ID != 0
                ? $"STA+|{StrActor(ws, InstanceID)}|{Index}|{Utils.StatusString(Value.ID)}|{Value.Extra:X4}|{Utils.StatusTimeString(Value.ExpireAt, ws?.CurrentTime ?? new())}|{StrActor(ws, Value.SourceID)}"
                : $"STA-|{StrActor(ws, InstanceID)}|{Index}";
        }

        // TODO: this should really be an actor field, but I have no idea what triggers icon clear...
        public event EventHandler<(Actor, uint)>? IconAppeared;
        public class OpIcon : Operation
        {
            public uint IconID;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                ws.Actors.IconAppeared?.Invoke(ws, (actor, IconID));
            }

            public override string Str(WorldState? ws) => $"ICON|{StrActor(ws, InstanceID)}|{IconID}";
        }

        // TODO: this should be an actor field (?)
        public event EventHandler<(Actor, ushort)>? EventObjectStateChange;
        public class OpEventObjectStateChange : Operation
        {
            public ushort State;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                ws.Actors.EventObjectStateChange?.Invoke(ws, (actor, State));
            }

            public override string Str(WorldState? ws) => $"ESTA|{StrActor(ws, InstanceID)}|{State:X4}";
        }

        // TODO: this should be an actor field (?)
        public event EventHandler<(Actor, ushort, ushort)>? EventObjectAnimation;
        public class OpEventObjectAnimation : Operation
        {
            public ushort Param1;
            public ushort Param2;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                ws.Actors.EventObjectAnimation?.Invoke(ws, (actor, Param1, Param2));
            }

            public override string Str(WorldState? ws) => $"EANM|{StrActor(ws, InstanceID)}|{Param1:X4}|{Param2:X4}";
        }

        // TODO: this needs more reversing...
        public event EventHandler<(Actor, ushort)>? PlayActionTimelineEvent;
        public class OpPlayActionTimelineEvent : Operation
        {
            public ushort ActionTimelineID;

            protected override void ExecActor(WorldState ws, Actor actor)
            {
                ws.Actors.PlayActionTimelineEvent?.Invoke(ws, (actor, ActionTimelineID));
            }

            public override string Str(WorldState? ws) => $"PATE|{StrActor(ws, InstanceID)}|{ActionTimelineID:X4}";
        }
    }
}
