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
        }

        public class OpZoneChange : Operation
        {
            public ushort Zone;

            public override void Redo(WorldState ws)
            {
                ws.CurrentZone = Zone;
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

            public override void Redo(WorldState ws)
            {
                ws.Waymarks[ID] = Pos;
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

            public override string ToString()
            {
                return $"Actor create: {InstanceID:X} {OID:X} '{Name}' {Type} {Class} {Utils.PosRotString(PosRot)} r={HitboxRadius} hp={HPCur}/{HPMax} targetable={IsTargetable} owner={OwnerID:X}";
            }
        }

        public class OpActorDestroy : Operation
        {
            public ulong InstanceID;

            public override void Redo(WorldState ws)
            {
                ws.Actors.Remove(InstanceID);
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

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.Rename(actor, Name);
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

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.ChangeClass(actor, Class);
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

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.Move(actor, PosRot);
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

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.UpdateHP(actor, Cur, Max);
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

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.ChangeIsTargetable(actor, Value);
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

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.ChangeIsDead(actor, Value);
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

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.ChangeInCombat(actor, Value);
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

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.ChangeTarget(actor, Value);
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

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.UpdateCastInfo(actor, Value);
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

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.UpdateTether(actor, Value);
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

            public override void Redo(WorldState ws)
            {
                var actor = ws.Actors.Find(InstanceID);
                if (actor != null)
                {
                    ws.Actors.UpdateStatus(actor, Index, Value);
                }
            }

            public override string ToString()
            {
                return $"Actor status: {InstanceID:X} [{Index}] -> {Value.ID} ({Value.Extra:X4}) from {Value.SourceID:X}, {(Value.ExpireAt - Timestamp).TotalSeconds:f2}s left";
            }
        }

        public class OpPartyModify : Operation
        {
            public int Slot;
            public ulong ContentID;
            public ulong InstanceID;

            public override void Redo(WorldState ws)
            {
                ws.Party.Modify(Slot, ContentID, InstanceID);
            }

            public override string ToString()
            {
                return $"Party modify: #{Slot} {ContentID:X}:{InstanceID:X}";
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

            public override string ToString()
            {
                return $"Cast: {Value.CasterID:X} -> {Value.Action} @ {Value.MainTargetID:X}";
            }
        }

        public class OpEventDirectorUpdate : Operation
        {
            public uint DirectorID;
            public uint UpdateID;
            public uint Param1;
            public uint Param2;
            public uint Param3;
            public uint Param4;

            public override void Redo(WorldState ws)
            {
                ws.Events.DispatchDirectorUpdate((DirectorID, UpdateID, Param1, Param2, Param3, Param4));
            }

            public override string ToString()
            {
                return $"DirectorUpdate: {DirectorID:X8}.{UpdateID:X8} = {Param1:X8} {Param2:X8} {Param3:X8} {Param4:X8}";
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

            public override string ToString()
            {
                return $"EnvControl: {FeatureID:X8}.{Index:X2} = {State:X8}";
            }
        }
    }
}
