using BossMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UIDev
{
    public class Replay
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
        }

        public class OpEnterExitCombat : Operation
        {
            public bool Value;
            private bool _prev;

            public override void Redo(WorldState ws)
            {
                _prev = ws.PlayerInCombat;
                ws.PlayerInCombat = Value;
            }

            public override void Undo(WorldState ws)
            {
                ws.PlayerInCombat = _prev;
            }
        }

        public class OpPlayerIDChange : Operation
        {
            public uint Value;
            private uint _prev;

            public override void Redo(WorldState ws)
            {
                _prev = ws.PlayerActorID;
                ws.PlayerActorID = Value;
            }

            public override void Undo(WorldState ws)
            {
                ws.PlayerActorID = _prev;
            }
        }

        public class OpWaymarkChange : Operation
        {
            public WorldState.Waymark ID;
            public Vector3? Pos;
            private Vector3? _prev;

            public override void Redo(WorldState ws)
            {
                _prev = ws.GetWaymark(ID);
                ws.SetWaymark(ID, Pos);
            }

            public override void Undo(WorldState ws)
            {
                ws.SetWaymark(ID, _prev);
            }
        }

        public class OpActorCreate : Operation
        {
            public uint InstanceID;
            public uint OID;
            public string Name = "";
            public WorldState.ActorType Type;
            public Class Class;
            public Vector4 PosRot;
            public float HitboxRadius;
            public bool IsTargetable;

            public override void Redo(WorldState ws)
            {
                ws.AddActor(InstanceID, OID, Name, Type, Class, PosRot, HitboxRadius, IsTargetable);
            }

            public override void Undo(WorldState ws)
            {
                ws.RemoveActor(InstanceID);
            }
        }

        public class OpActorDestroy : Operation
        {
            public uint InstanceID;
            private uint OID;
            private string Name = "";
            private WorldState.ActorType Type;
            private Class Class;
            private Vector4 PosRot;
            private float HitboxRadius;
            private bool IsTargetable;

            public override void Redo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                OID = actor?.OID ?? 0;
                Name = actor?.Name ?? "";
                Type = actor?.Type ?? WorldState.ActorType.None;
                Class = actor?.Class ?? Class.None;
                PosRot = actor?.PosRot ?? new();
                HitboxRadius = actor?.HitboxRadius ?? 0;
                IsTargetable = actor?.IsTargetable ?? false;
                ws.RemoveActor(InstanceID);
            }

            public override void Undo(WorldState ws)
            {
                ws.AddActor(InstanceID, OID, Name, Type, Class, PosRot, HitboxRadius, IsTargetable);
            }
        }

        public class OpActorRename : Operation
        {
            public uint InstanceID;
            public string Name = "";
            private string _prev = "";

            public override void Redo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    _prev = actor.Name;
                    ws.RenameActor(actor, Name);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    ws.RenameActor(actor, _prev);
                }
            }
        }

        public class OpActorClassChange : Operation
        {
            public uint InstanceID;
            public Class Class;
            private Class _prevClass;

            public override void Redo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    _prevClass = actor.Class;
                    ws.ChangeActorClass(actor, Class);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    ws.ChangeActorClass(actor, _prevClass);
                }
            }
        }

        public class OpActorMove : Operation
        {
            public uint InstanceID;
            public Vector4 PosRot;
            private Vector4 _prevPosRot;

            public override void Redo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    _prevPosRot = actor.PosRot;
                    ws.MoveActor(actor, PosRot);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    ws.MoveActor(actor, _prevPosRot);
                }
            }
        }

        public class OpActorTargetable : Operation
        {
            public uint InstanceID;
            public bool Value;
            private bool _prev;

            public override void Redo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    _prev = actor.IsTargetable;
                    ws.ChangeActorIsTargetable(actor, Value);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    ws.ChangeActorIsTargetable(actor, _prev);
                }
            }
        }

        public class OpActorDead : Operation
        {
            public uint InstanceID;
            public bool Value;
            private bool _prev;

            public override void Redo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    _prev = actor.IsDead;
                    ws.ChangeActorIsDead(actor, Value);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    ws.ChangeActorIsDead(actor, _prev);
                }
            }
        }

        public class OpActorTarget : Operation
        {
            public uint InstanceID;
            public uint Value;
            private uint _prev;

            public override void Redo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    _prev = actor.TargetID;
                    ws.ChangeActorTarget(actor, Value);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    ws.ChangeActorTarget(actor, _prev);
                }
            }
        }

        public class OpActorCast : Operation
        {
            public uint InstanceID;
            public WorldState.CastInfo? Value;
            private WorldState.CastInfo? _prev;

            public override void Redo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    _prev = actor.CastInfo;
                    ws.UpdateCastInfo(actor, Value);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    ws.UpdateCastInfo(actor, _prev);
                }
            }
        }

        public class OpActorTether : Operation
        {
            public uint InstanceID;
            public WorldState.TetherInfo Value;
            private WorldState.TetherInfo _prev;

            public override void Redo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    _prev = actor.Tether;
                    ws.UpdateTether(actor, Value);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    ws.UpdateTether(actor, _prev);
                }
            }
        }

        public class OpActorStatus : Operation
        {
            public uint InstanceID;
            public int Index;
            public WorldState.Status Value;
            private WorldState.Status _prev;

            public override void Redo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    _prev = actor.Statuses[Index];
                    ws.UpdateStatus(actor, Index, Value);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    ws.UpdateStatus(actor, Index, _prev);
                }
            }
        }

        public class OpEventIcon : Operation
        {
            public uint InstanceID;
            public uint IconID;

            public override void Redo(WorldState ws)
            {
                ws.DispatchEventIcon((InstanceID, IconID));
            }

            public override void Undo(WorldState ws)
            {
            }
        }

        public class OpEventCast : Operation
        {
            public WorldState.CastResult Value = new();

            public override void Redo(WorldState ws)
            {
                ws.DispatchEventCast(Value);
            }

            public override void Undo(WorldState ws)
            {
            }
        }

        public class OpEventEnvControl : Operation
        {
            public uint FeatureID;
            public byte Index;
            public uint State;

            public override void Redo(WorldState ws)
            {
                ws.DispatchEventEnvControl((FeatureID, Index, State));
            }

            public override void Undo(WorldState ws)
            {
            }
        }

        public List<Operation> Ops = new();
    }
}
