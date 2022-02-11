using BossMod;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UIDev
{
    class WorldStateLogParser
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

            public static Operation? Parse(string[] payload)
            {
                OpZoneChange res = new();
                res.Zone = ushort.Parse(payload[2]);
                return res;
            }

            public static (Operation?, List<(uint, Vector4)>) ParseACT(string[] payload)
            {
                OpZoneChange res = new();
                res.Zone = ushort.Parse(payload[2], NumberStyles.HexNumber);
                return (res, new());
            }

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

            public static Operation? Parse(string[] payload)
            {
                OpEnterExitCombat res = new();
                res.Value = bool.Parse(payload[2]);
                return res;
            }

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

            public static Operation? Parse(string[] payload)
            {
                OpPlayerIDChange res = new();
                res.Value = ActorID(payload[2]);
                return res;
            }

            public static (Operation?, List<(uint, Vector4)>) ParseACT(string[] payload)
            {
                OpPlayerIDChange res = new();
                res.Value = uint.Parse(payload[2], NumberStyles.HexNumber);
                return (res, new());
            }

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

            public static Operation? Parse(string[] payload, bool set)
            {
                OpWaymarkChange res = new();
                res.ID = Enum.Parse<WorldState.Waymark>(payload[2]);
                if (set)
                    res.Pos = Vec3(payload[3]);
                return res;
            }

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
            public uint Class;
            public WorldState.ActorRole Role;
            public Vector3 Pos;
            public float Rot;
            public float HitboxRadius;
            public bool IsTargetable;

            public static Operation? Parse(string[] payload)
            {
                var parts = payload[2].Split('/');
                OpActorCreate res = new();
                res.InstanceID = uint.Parse(parts[0], NumberStyles.HexNumber);
                res.OID = uint.Parse(parts[1], NumberStyles.HexNumber);
                res.Name = parts[2];
                res.Type = Enum.Parse<WorldState.ActorType>(parts[3]);
                //res.Class = TODO...
                res.Role = Enum.Parse<WorldState.ActorRole>(payload[4]);
                res.Pos = new(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]));
                res.Rot = float.Parse(parts[7]) * MathF.PI / 180;
                res.IsTargetable = bool.Parse(payload[5]);
                res.HitboxRadius = float.Parse(payload[6]);
                return res;
            }

            public static (Operation?, List<(uint, Vector4)>) ParseACT(string[] payload)
            {
                OpActorCreate res = new();
                res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
                res.OID = uint.Parse(payload[10]);
                res.Name = payload[3];

                if ((res.InstanceID & 0xF0000000u) == 0x10000000u)
                    res.Type = WorldState.ActorType.Player;
                else if (uint.Parse(payload[6], NumberStyles.HexNumber) != 0) // owner id
                    res.Type = WorldState.ActorType.Pet;
                else
                    res.Type = WorldState.ActorType.Enemy;

                res.Class = uint.Parse(payload[4], NumberStyles.HexNumber);
                //res.Role = TODO...
                res.Pos = new(float.Parse(payload[17]), float.Parse(payload[19]), float.Parse(payload[18]));
                res.Rot = float.Parse(payload[20]);
                res.IsTargetable = true;
                return (res, new());
            }

            public override void Redo(WorldState ws)
            {
                ws.AddActor(InstanceID, OID, Name, Type, Class, Role, Pos, Rot, HitboxRadius, IsTargetable);
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
            private uint Class;
            private WorldState.ActorRole Role;
            private Vector3 Pos;
            private float Rot;
            private float HitboxRadius;
            private bool IsTargetable;

            public static Operation? Parse(string[] payload)
            {
                OpActorDestroy res = new();
                res.InstanceID = ActorID(payload[2]);
                return res;
            }

            public static (Operation?, List<(uint, Vector4)>) ParseACT(string[] payload)
            {
                OpActorDestroy res = new();
                List<(uint, Vector4)> pos = new();
                res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
                pos.Add((res.InstanceID, new Vector4(float.Parse(payload[17]), float.Parse(payload[19]), float.Parse(payload[18]), float.Parse(payload[20]))));
                return (res, pos);
            }

            public override void Redo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                OID = actor?.OID ?? 0;
                Name = actor?.Name ?? "";
                Type = actor?.Type ?? WorldState.ActorType.None;
                Class = actor?.ClassID ?? 0;
                Role = actor?.Role ?? WorldState.ActorRole.None;
                Pos = actor?.Position ?? new();
                Rot = actor?.Rotation ?? 0;
                HitboxRadius = actor?.HitboxRadius ?? 0;
                IsTargetable = actor?.IsTargetable ?? false;
                ws.RemoveActor(InstanceID);
            }

            public override void Undo(WorldState ws)
            {
                ws.AddActor(InstanceID, OID, Name, Type, Class, Role, Pos, Rot, HitboxRadius, IsTargetable);
            }
        }

        public class OpActorRename : Operation
        {
            public uint InstanceID;
            public string Name = "";
            private string _prev = "";

            public static Operation? Parse(string[] payload)
            {
                var parts = payload[2].Split('/');
                OpActorRename res = new();
                res.InstanceID = uint.Parse(parts[0], NumberStyles.HexNumber);
                res.Name = parts[2];
                return res;
            }

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
            public uint Class;
            public WorldState.ActorRole Role;
            private uint _prevClass;
            private WorldState.ActorRole _prevRole;

            public static Operation? Parse(string[] payload)
            {
                OpActorClassChange res = new();
                res.InstanceID = ActorID(payload[2]);
                res.Class = 0; // TODO...
                res.Role = Enum.Parse<WorldState.ActorRole>(payload[6]);
                return res;
            }

            public override void Redo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    _prevClass = actor.ClassID;
                    _prevRole = actor.Role;
                    ws.ChangeActorClassRole(actor, Class, Role);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    ws.ChangeActorClassRole(actor, _prevClass, _prevRole);
                }
            }
        }

        public class OpActorMove : Operation
        {
            public uint InstanceID;
            public Vector3 Pos;
            public float Rot;
            private Vector3 _prevPos;
            private float _prevRot;

            public static Operation? Parse(string[] payload)
            {
                var parts = payload[2].Split('/');
                OpActorMove res = new();
                res.InstanceID = uint.Parse(parts[0], NumberStyles.HexNumber);
                res.Pos = new(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]));
                res.Rot = float.Parse(parts[7]) * MathF.PI / 180;
                return res;
            }

            public override void Redo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    _prevPos = actor.Position;
                    _prevRot = actor.Rotation;
                    ws.MoveActor(actor, Pos, Rot);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    ws.MoveActor(actor, _prevPos, _prevRot);
                }
            }
        }

        public class OpActorTargetable : Operation
        {
            public uint InstanceID;
            public bool Value;
            private bool _prev;

            public static Operation? Parse(string[] payload, bool targetable)
            {
                OpActorTargetable res = new();
                res.InstanceID = ActorID(payload[2]);
                res.Value = targetable;
                return res;
            }

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

            public static Operation? Parse(string[] payload, bool dead)
            {
                OpActorDead res = new();
                res.InstanceID = ActorID(payload[2]);
                res.Value = dead;
                return res;
            }

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

            public static Operation? Parse(string[] payload)
            {
                OpActorTarget res = new();
                res.InstanceID = ActorID(payload[2]);
                res.Value = ActorID(payload[3]);
                return res;
            }

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

            public static Operation? Parse(string[] payload, bool start)
            {
                OpActorCast res = new();
                res.InstanceID = ActorID(payload[2]);
                if (start)
                {
                    res.Value = new();
                    (res.Value.ActionType, res.Value.ActionID) = Action(payload[3]);
                    res.Value.TargetID = ActorID(payload[4]);
                    res.Value.Location = Vec3(payload[5]);

                    var parts = payload[6].Split('/');
                    res.Value.CurrentTime = float.Parse(parts[0]);
                    res.Value.TotalTime = float.Parse(parts[1]);
                }
                return res;
            }

            public static (Operation?, List<(uint, Vector4)>) ParseACTStart(string[] payload)
            {
                OpActorCast res = new();
                List<(uint, Vector4)> pos = new();
                res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
                res.Value = new();
                res.Value.ActionType = WorldState.ActionType.Spell;
                res.Value.ActionID = uint.Parse(payload[4], NumberStyles.HexNumber);
                res.Value.TargetID = uint.Parse(payload[6], NumberStyles.HexNumber);
                res.Value.CurrentTime = res.Value.TotalTime = float.Parse(payload[8]);
                pos.Add((res.InstanceID, new Vector4(float.Parse(payload[9]), float.Parse(payload[11]), float.Parse(payload[10]), float.Parse(payload[12]))));
                return (res, pos);
            }

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

            public static Operation? Parse(string[] payload, bool tether)
            {
                OpActorTether res = new();
                res.InstanceID = ActorID(payload[2]);
                if (tether)
                {
                    res.Value.ID = uint.Parse(payload[3]);
                    res.Value.Target = ActorID(payload[4]);
                }
                return res;
            }

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

            public static Operation? Parse(string[] payload, bool gain)
            {
                OpActorStatus res = new();
                res.InstanceID = ActorID(payload[2]);
                res.Index = int.Parse(payload[3]);
                if (gain)
                {
                    int sep = payload[4].IndexOf(' ');
                    res.Value.ID = uint.Parse(sep >= 0 ? payload[4].AsSpan(0, sep) : payload[4].AsSpan());
                    res.Value.Param = byte.Parse(payload[5]);
                    res.Value.StackCount = byte.Parse(payload[6]);
                    res.Value.RemainingTime = float.Parse(payload[7]);
                    res.Value.SourceID = ActorID(payload[8]);
                }
                return res;
            }

            public override void Redo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    _prev = actor.Statuses[Index];
                    var newStatuses = (WorldState.Status[])actor.Statuses.Clone();
                    newStatuses[Index] = Value;
                    ws.UpdateStatuses(actor, newStatuses);
                }
            }

            public override void Undo(WorldState ws)
            {
                var actor = ws.FindActor(InstanceID);
                if (actor != null)
                {
                    var newStatuses = (WorldState.Status[])actor.Statuses.Clone();
                    newStatuses[Index] = _prev;
                    ws.UpdateStatuses(actor, newStatuses);
                }
            }
        }

        public class OpEventIcon : Operation
        {
            public uint InstanceID;
            public uint IconID;

            public static Operation? Parse(string[] payload)
            {
                OpEventIcon res = new();
                res.InstanceID = ActorID(payload[2]);
                res.IconID = uint.Parse(payload[3]);
                return res;
            }

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

            public static Operation? Parse(string[] payload)
            {
                OpEventCast res = new();
                res.Value.CasterID = ActorID(payload[2]);
                (res.Value.ActionType, res.Value.ActionID) = Action(payload[3]);
                res.Value.MainTargetID = ActorID(payload[4]);
                res.Value.AnimationLockTime = float.Parse(payload[5]);
                res.Value.MaxTargets = uint.Parse(payload[6]);
                for (int i = 7; i < payload.Length; ++i)
                {
                    var parts = payload[i].Split('!');
                    WorldState.CastResult.Target target = new();
                    target.ID = ActorID(parts[0]);
                    for (int j = 1; j < parts.Length; ++j)
                        target[j - 1] = ulong.Parse(parts[j], NumberStyles.HexNumber);
                    res.Value.Targets.Add(target);
                }
                return res;
            }

            public static (Operation?, List<(uint, Vector4)>) ParseACT(string[] payload, int networkDelta)
            {
                OpEventCast res = new();
                List<(uint, Vector4)> pos = new();
                res.Value.CasterID = uint.Parse(payload[2], NumberStyles.HexNumber);
                var aid = uint.Parse(payload[4], NumberStyles.HexNumber);
                res.Value.ActionType = (WorldState.ActionType)(aid >> 24);
                res.Value.ActionID = aid & 0x00FFFFFF;
                if (res.Value.ActionType != WorldState.ActionType.Spell)
                    res.Value.ActionID = (uint)((int)res.Value.ActionID - networkDelta);
                res.Value.MainTargetID = uint.Parse(payload[6], NumberStyles.HexNumber);
                pos.Add((res.Value.CasterID, new Vector4(float.Parse(payload[30]), float.Parse(payload[32]), float.Parse(payload[31]), float.Parse(payload[33]))));
                if (res.Value.MainTargetID != 0xE0000000u)
                {
                    WorldState.CastResult.Target target = new();
                    target.ID = res.Value.MainTargetID;
                    for (int i = 0; i < 8; ++i)
                    {
                        var lo = ulong.Parse(payload[8 + 2 * i], NumberStyles.HexNumber);
                        var hi = ulong.Parse(payload[9 + 2 * i], NumberStyles.HexNumber);
                        target[i] = (hi << 32) | lo;
                    }
                    res.Value.Targets.Add(target);
                    pos.Add((res.Value.MainTargetID, new Vector4(float.Parse(payload[40]), float.Parse(payload[42]), float.Parse(payload[41]), float.Parse(payload[43]))));
                }
                res.Value.SourceSequence = uint.Parse(payload[44], NumberStyles.HexNumber);
                res.Value.MaxTargets = uint.Parse(payload[46]);
                return (res, pos);
            }

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

            public static Operation? Parse(string[] payload)
            {
                OpEventEnvControl res = new();
                res.FeatureID = uint.Parse(payload[2], NumberStyles.HexNumber);
                res.Index = byte.Parse(payload[3], NumberStyles.HexNumber);
                res.State = uint.Parse(payload[4], NumberStyles.HexNumber);
                return res;
            }

            public override void Redo(WorldState ws)
            {
                ws.DispatchEventEnvControl((FeatureID, Index, State));
            }

            public override void Undo(WorldState ws)
            {
            }
        }

        public List<Operation> Ops = new();

        public static WorldStateLogParser ParseNativeLog(string path)
        {
            WorldStateLogParser res = new();
            try
            {
                using (var reader = new StreamReader(path))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Length == 0 || line[0] == '#')
                            continue; // empty line or comment

                        var elements = line.Split("|");
                        if (elements.Length < 2)
                            continue; // invalid string

                        Operation? op = elements[1] switch
                        {
                            "ZC" => OpZoneChange.Parse(elements),
                            "PC" => OpEnterExitCombat.Parse(elements),
                            "PI" => OpPlayerIDChange.Parse(elements),
                            "W+" => OpWaymarkChange.Parse(elements, true),
                            "W-" => OpWaymarkChange.Parse(elements, false),
                            "A+" => OpActorCreate.Parse(elements),
                            "A-" => OpActorDestroy.Parse(elements),
                            "AN" => OpActorRename.Parse(elements),
                            "AC" => OpActorClassChange.Parse(elements),
                            "AM" => OpActorMove.Parse(elements),
                            "V+" => OpActorTargetable.Parse(elements, true),
                            "V-" => OpActorTargetable.Parse(elements, false),
                            "D+" => OpActorDead.Parse(elements, true),
                            "D-" => OpActorDead.Parse(elements, false),
                            "AT" => OpActorTarget.Parse(elements),
                            "C+" => OpActorCast.Parse(elements, true),
                            "C-" => OpActorCast.Parse(elements, false),
                            "T+" => OpActorTether.Parse(elements, true),
                            "T-" => OpActorTether.Parse(elements, false),
                            "S+" => OpActorStatus.Parse(elements, true),
                            "S-" => OpActorStatus.Parse(elements, false),
                            "I+" => OpEventIcon.Parse(elements),
                            "C!" => OpEventCast.Parse(elements),
                            "EC" => OpEventEnvControl.Parse(elements),
                            _ => null
                        };

                        if (op != null)
                        {
                            op.Timestamp = DateTime.Parse(elements[0]);
                            res.Ops.Add(op);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Service.Log($"Failed to read {path}: {e}");
            }
            return res;
        }

        public static WorldStateLogParser ParseActLog(string path, int networkDelta)
        {
            WorldState ws = new();
            WorldStateLogParser res = new();
            try
            {
                using (var reader = new StreamReader(path))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Length == 0 || line[0] == '#')
                            continue; // empty line or comment

                        var elements = line.Split("|");
                        if (elements.Length < 2)
                            continue; // invalid string

                        var timestamp = DateTime.Parse(elements[1]);
                        (Operation? newOp, List<(uint, Vector4)> newPos) = int.Parse(elements[0]) switch
                        {
                            1 => OpZoneChange.ParseACT(elements),
                            2 => OpPlayerIDChange.ParseACT(elements),
                            3 => OpActorCreate.ParseACT(elements),
                            4 => OpActorDestroy.ParseACT(elements),
                            20 => OpActorCast.ParseACTStart(elements),
                            21 => OpEventCast.ParseACT(elements, networkDelta),
                            22 => OpEventCast.ParseACT(elements, networkDelta),
                            // TODO: ...
                            _ => (null, new List<(uint, Vector4)>())
                        };

                        var newCastOp = newOp as OpEventCast;
                        if (newCastOp != null)
                        {
                            var prevCastOp = res.Ops.LastOrDefault() as OpEventCast;
                            if (prevCastOp != null &&
                                prevCastOp.Value.CasterID == newCastOp.Value.CasterID &&
                                prevCastOp.Value.ActionID == newCastOp.Value.ActionID &&
                                prevCastOp.Value.ActionType == newCastOp.Value.ActionType &&
                                prevCastOp.Value.MaxTargets == newCastOp.Value.MaxTargets &&
                                prevCastOp.Value.SourceSequence == newCastOp.Value.SourceSequence)
                            {
                                prevCastOp.Value.Targets.Add(newCastOp.Value.Targets.First());
                                newOp = prevCastOp;
                                res.Ops.RemoveAt(res.Ops.Count - 1);
                            }

                            var cast = ws.FindActor(newCastOp.Value.CasterID)?.CastInfo;
                            if (cast != null && cast.ActionID == newCastOp.Value.ActionID && cast.ActionType == newCastOp.Value.ActionType)
                            {
                                OpActorCast endCastOp = new();
                                endCastOp.Timestamp = timestamp;
                                endCastOp.InstanceID = newCastOp.Value.CasterID;
                                endCastOp.Redo(ws);
                                res.Ops.Add(endCastOp);
                            }
                        }

                        foreach ((var id, var posRot) in newPos)
                        {
                            var actor = ws.FindActor(id);
                            if (actor != null && posRot != new Vector4(actor.Position, actor.Rotation))
                            {
                                OpActorMove moveOp = new();
                                moveOp.Timestamp = timestamp;
                                moveOp.InstanceID = id;
                                moveOp.Pos = new(posRot.X, posRot.Y, posRot.Z);
                                moveOp.Rot = posRot.W;
                                moveOp.Redo(ws);
                                res.Ops.Add(moveOp);
                            }
                        }

                        if (newOp != null)
                        {
                            newOp.Timestamp = timestamp;
                            newOp.Redo(ws);
                            res.Ops.Add(newOp);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Service.Log($"Failed to read {path}: {e}");
            }
            return res;
        }

        private static Vector3 Vec3(string repr)
        {
            var parts = repr.Split('/');
            return new(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }

        private static uint ActorID(string actor)
        {
            var sep = actor.IndexOf('/');
            return uint.Parse(sep >= 0 ? actor.AsSpan(0, sep) : actor.AsSpan(), NumberStyles.HexNumber);
        }

        private static (WorldState.ActionType, uint) Action(string repr)
        {
            var parts = repr.Split(' ');
            var type = parts.Length > 0 ? Enum.Parse<WorldState.ActionType>(parts[0]) : WorldState.ActionType.None;
            var id = parts.Length > 1 ? uint.Parse(parts[1]) : 0;
            return (type, id);
        }
    }
}
