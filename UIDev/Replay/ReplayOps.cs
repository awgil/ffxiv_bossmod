using BossMod;
using System;
using System.Numerics;

namespace UIDev
{
    public static class ReplayOps
    {
        public abstract class Operation
        {
            public DateTime Timestamp;

            public abstract void Redo(WorldState ws);
            public abstract void Undo(WorldState ws);
        }

        public class OpZoneChange : Operation
        {
            public ushort Zone;
            private ushort _prev;

            public override void Redo(WorldState ws)
            {
                _prev = ws.CurrentZone;
                ws.CurrentZone = Zone;
            }

            public override void Undo(WorldState ws)
            {
                ws.CurrentZone = _prev;
            }

            public override string ToString()
            {
                return $"Zone change: {Zone}";
            }
        }

        public class OpWaymarkChange : Operation
        {
            public Waymark ID;
            public Vector3? Pos;
            private Vector3? _prev;

            public override void Redo(WorldState ws)
            {
                _prev = ws.Waymarks[ID];
                ws.Waymarks[ID] = Pos;
            }

            public override void Undo(WorldState ws)
            {
                ws.Waymarks[ID] = _prev;
            }

            public override string ToString()
            {
                return $"Waymark change: {ID} = {(Pos != null ? Utils.Vec3String(Pos.Value) : "removed")}";
            }
        }

        public class OpActorCreate : Operation
        {
            public ulong InstanceID;
            public uint OID;
            public string Name = "";
            public ActorType Type;
            public Class Class;
            public Vector4 PosRot;
            public float HitboxRadius;
            public uint HPCur;
            public uint HPMax;
            public bool IsTargetable;
            public ulong OwnerID;

            public override void Redo(WorldState ws)
            {
                ws.Actors.Add(InstanceID, OID, Name, Type, Class, PosRot, HitboxRadius, HPCur, HPMax, IsTargetable, OwnerID);
            }

            public override void Undo(WorldState ws)
            {
                ws.Actors.Remove(InstanceID);
            }

            public override string ToString()
            {
                return $"Actor create: {InstanceID:X} {OID:X} '{Name}' {Type} {Class} {Utils.PosRotString(PosRot)} r={HitboxRadius} hp={HPCur}/{HPMax} targetable={IsTargetable} owner={OwnerID:X}";
            }
        }

        public class OpActorDestroy : Operation
        {
            public ulong InstanceID;
            private uint OID;
            private string Name = "";
            private ActorType Type;
            private Class Class;
            private Vector4 PosRot;
            private float HitboxRadius;
            private uint HPCur;
            private uint HPMax;
            private bool IsTargetable;
            private ulong OwnerID;

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                OID = actor?.OID ?? 0;
                Name = actor?.Name ?? "";
                Type = actor?.Type ?? ActorType.None;
                Class = actor?.Class ?? Class.None;
                PosRot = actor?.PosRot ?? new();
                HitboxRadius = actor?.HitboxRadius ?? 0;
                HPCur = actor?.HPCur ?? 0;
                HPMax = actor?.HPMax ?? 0;
                IsTargetable = actor?.IsTargetable ?? false;
                OwnerID = actor?.OwnerID ?? 0;
                ws.Actors.Remove(InstanceID);
            }

            public override void Undo(WorldState ws)
            {
                ws.Actors.Add(InstanceID, OID, Name, Type, Class, PosRot, HitboxRadius, HPCur, HPMax, IsTargetable, OwnerID);
            }

            public override string ToString()
            {
                return $"Actor destroy: {InstanceID:X}";
            }
        }

        public class OpActorRename : Operation
        {
            public ulong InstanceID;
            public string Name = "";
            private string _prev = "";

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    _prev = actor.Name;
                    ws.Actors.Rename(actor, Name);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.Rename(actor, _prev);
                }
            }

            public override string ToString()
            {
                return $"Actor rename: {InstanceID:X} -> '{Name}'";
            }
        }

        public class OpActorClassChange : Operation
        {
            public ulong InstanceID;
            public Class Class;
            private Class _prevClass;

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    _prevClass = actor.Class;
                    ws.Actors.ChangeClass(actor, Class);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.ChangeClass(actor, _prevClass);
                }
            }

            public override string ToString()
            {
                return $"Actor class change: {InstanceID:X} -> {Class}";
            }
        }

        public class OpActorMove : Operation
        {
            public ulong InstanceID;
            public Vector4 PosRot;
            private Vector4 _prevPosRot;

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    _prevPosRot = actor.PosRot;
                    ws.Actors.Move(actor, PosRot);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.Move(actor, _prevPosRot);
                }
            }

            public override string ToString()
            {
                return $"Actor move: {InstanceID:X} -> {Utils.PosRotString(PosRot)}";
            }
        }

        public class OpActorHP : Operation
        {
            public ulong InstanceID;
            public uint Cur;
            public uint Max;
            private uint _prevCur;
            private uint _prevMax;

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    _prevCur = actor.HPCur;
                    _prevMax = actor.HPMax;
                    ws.Actors.UpdateHP(actor, Cur, Max);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.UpdateHP(actor, _prevCur, _prevMax);
                }
            }

            public override string ToString()
            {
                return $"Actor HP change: {InstanceID:X} -> {Cur}/{Max}";
            }
        }

        public class OpActorTargetable : Operation
        {
            public ulong InstanceID;
            public bool Value;
            private bool _prev;

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    _prev = actor.IsTargetable;
                    ws.Actors.ChangeIsTargetable(actor, Value);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.ChangeIsTargetable(actor, _prev);
                }
            }

            public override string ToString()
            {
                return $"Actor targetable: {InstanceID:X} -> {Value}";
            }
        }

        public class OpActorDead : Operation
        {
            public ulong InstanceID;
            public bool Value;
            private bool _prev;

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    _prev = actor.IsDead;
                    ws.Actors.ChangeIsDead(actor, Value);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.ChangeIsDead(actor, _prev);
                }
            }

            public override string ToString()
            {
                return $"Actor dead: {InstanceID:X} -> {Value}";
            }
        }

        public class OpActorCombat : Operation
        {
            public ulong InstanceID;
            public bool Value;
            private bool _prev;

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    _prev = actor.InCombat;
                    ws.Actors.ChangeInCombat(actor, Value);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.ChangeInCombat(actor, _prev);
                }
            }

            public override string ToString()
            {
                return $"Actor combat: {InstanceID:X} -> {Value}";
            }
        }

        public class OpActorTarget : Operation
        {
            public ulong InstanceID;
            public ulong Value;
            private ulong _prev;

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    _prev = actor.TargetID;
                    ws.Actors.ChangeTarget(actor, Value);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.ChangeTarget(actor, _prev);
                }
            }

            public override string ToString()
            {
                return $"Actor target: {InstanceID:X} -> {Value:X}";
            }
        }

        public class OpActorCast : Operation
        {
            public ulong InstanceID;
            public ActorCastInfo? Value;
            private ActorCastInfo? _prev;

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    _prev = actor.CastInfo;
                    ws.Actors.UpdateCastInfo(actor, Value);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.UpdateCastInfo(actor, _prev);
                }
            }

            public override string ToString()
            {
                return $"Actor cast: {InstanceID:X} -> {(Value != null ? $"{Value.Action} @ {Value.TargetID:X} / {Utils.Vec3String(Value.Location)}, {(Value.FinishAt - Timestamp).TotalSeconds:f2}/{Value.TotalTime:f2}s" : "end")}";
            }
        }

        public class OpActorTether : Operation
        {
            public ulong InstanceID;
            public ActorTetherInfo Value;
            private ActorTetherInfo _prev;

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    _prev = actor.Tether;
                    ws.Actors.UpdateTether(actor, Value);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.UpdateTether(actor, _prev);
                }
            }

            public override string ToString()
            {
                return $"Actor tether: {InstanceID:X} -> {Value.ID} @ {Value.Target:X}";
            }
        }

        public class OpActorStatus : Operation
        {
            public ulong InstanceID;
            public int Index;
            public ActorStatus Value;
            private ActorStatus _prev;

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    _prev = actor.Statuses[Index];
                    ws.Actors.UpdateStatus(actor, Index, Value);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.UpdateStatus(actor, Index, _prev);
                }
            }

            public override string ToString()
            {
                return $"Actor status: {InstanceID:X} [{Index}] -> {Value.ID} ({Value.Extra:X4}) from {Value.SourceID:X}, {(Value.ExpireAt - Timestamp).TotalSeconds:f2}s left";
            }
        }

        public class OpPartyJoin : Operation
        {
            public ulong ContentID;
            public ulong InstanceID;
            private int _slot;

            public override void Redo(WorldState ws)
            {
                _slot = ws.Party.Add(ContentID, InstanceID, ws.Party.ContentIDs[0] == 0);
            }

            public override void Undo(WorldState ws)
            {
                ws.Party.Remove(_slot);
            }

            public override string ToString()
            {
                return $"Party join: {ContentID:X} {InstanceID:X}";
            }
        }

        public class OpPartyLeave : Operation
        {
            public ulong ContentID;
            public ulong InstanceID;

            public override void Redo(WorldState ws)
            {
                var slot = ws.Party.ContentIDs.IndexOf(ContentID);
                ws.Party.Remove(slot);
            }

            public override void Undo(WorldState ws)
            {
                ws.Party.Add(ContentID, InstanceID, ws.Party.ContentIDs[0] == 0);
            }

            public override string ToString()
            {
                return $"Party leave: {ContentID:X} {InstanceID:X}";
            }
        }

        public class OpPartyAssign : Operation
        {
            public ulong ContentID;
            public ulong InstanceID;
            private ulong _prevID;

            public override void Redo(WorldState ws)
            {
                var slot = ws.Party.ContentIDs.IndexOf(ContentID);
                _prevID = ws.Party.ActorIDs[slot];
                ws.Party.AssignActor(slot, ContentID, InstanceID);
            }

            public override void Undo(WorldState ws)
            {
                var slot = ws.Party.ContentIDs.IndexOf(ContentID);
                ws.Party.AssignActor(slot, ContentID, _prevID);
            }

            public override string ToString()
            {
                return $"Party assign: {ContentID:X} {InstanceID:X}";
            }
        }

        public class OpEventIcon : Operation
        {
            public ulong InstanceID;
            public uint IconID;

            public override void Redo(WorldState ws)
            {
                ws.Events.DispatchIcon((InstanceID, IconID));
            }

            public override void Undo(WorldState ws)
            {
            }

            public override string ToString()
            {
                return $"Icon: {InstanceID:X} -> {IconID}";
            }
        }

        public class OpEventCast : Operation
        {
            public CastEvent Value = new();

            public override void Redo(WorldState ws)
            {
                ws.Events.DispatchCast(Value);
            }

            public override void Undo(WorldState ws)
            {
            }

            public override string ToString()
            {
                return $"Cast: {Value.CasterID:X} -> {Value.Action} @ {Value.MainTargetID:X}";
            }
        }

        public class OpEventEnvControl : Operation
        {
            public uint FeatureID;
            public byte Index;
            public uint State;

            public override void Redo(WorldState ws)
            {
                ws.Events.DispatchEnvControl((FeatureID, Index, State));
            }

            public override void Undo(WorldState ws)
            {
            }

            public override string ToString()
            {
                return $"EnvControl: {FeatureID:X8}.{Index:X2} = {State:X8}";
            }
        }
    }
}
