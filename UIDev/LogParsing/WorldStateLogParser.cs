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

            public static (Operation?, List<(uint, Vector4)>) ParseACT(string[] payload)
            {
                OpWaymarkChange res = new();
                res.ID = (WorldState.Waymark)uint.Parse(payload[3]);
                if (payload[2] == "Add")
                    res.Pos = new(float.Parse(payload[6]), float.Parse(payload[8]), float.Parse(payload[7]));
                return (res, new());
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
            public Class Class;
            public Vector4 PosRot;
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
                res.Class = Enum.Parse<Class>(payload[3]);
                res.PosRot = new(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]) * MathF.PI / 180);
                res.IsTargetable = bool.Parse(payload[4]);
                res.HitboxRadius = float.Parse(payload[5]);
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

                res.Class = (Class)uint.Parse(payload[4], NumberStyles.HexNumber);
                res.PosRot = ACTPosRot(payload, 17) ?? new();
                res.IsTargetable = true;
                return (res, new());
            }

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
                ACTAddPos(pos, res.InstanceID, payload, 17);
                return (res, pos);
            }

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
            public Class Class;
            private Class _prevClass;

            public static Operation? Parse(string[] payload)
            {
                OpActorClassChange res = new();
                res.InstanceID = ActorID(payload[2]);
                res.Class = Enum.Parse<Class>(payload[4]);
                return res;
            }

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

            public static Operation? Parse(string[] payload)
            {
                var parts = payload[2].Split('/');
                OpActorMove res = new();
                res.InstanceID = uint.Parse(parts[0], NumberStyles.HexNumber);
                res.PosRot = new(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]) * MathF.PI / 180);
                return res;
            }

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

            public static Operation? Parse(string[] payload, bool targetable)
            {
                OpActorTargetable res = new();
                res.InstanceID = ActorID(payload[2]);
                res.Value = targetable;
                return res;
            }

            public static (Operation?, List<(uint, Vector4)>) ParseACT(string[] payload)
            {
                OpActorTargetable res = new();
                res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
                res.Value = byte.Parse(payload[6], NumberStyles.HexNumber) != 0;
                return (res, new());
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

            public static (Operation?, List<(uint, Vector4)>) ParseACT(string[] payload)
            {
                OpActorDead res = new();
                res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
                res.Value = true;
                return (res, new());
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
                    res.Value.Action = Action(payload[3]);
                    res.Value.TargetID = ActorID(payload[4]);
                    res.Value.Location = Vec3(payload[5]);

                    var parts = payload[6].Split('/');
                    res.Value.TotalTime = float.Parse(parts[1]);
                    res.Value.FinishAt = DateTime.Parse(payload[0]).AddSeconds(float.Parse(parts[0]));
                }
                return res;
            }

            public static (Operation?, List<(uint, Vector4)>) ParseACT(string[] payload, bool start)
            {
                OpActorCast res = new();
                List<(uint, Vector4)> pos = new();
                res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
                if (start)
                {
                    res.Value = new();
                    res.Value.Action = new(ActionType.Spell, uint.Parse(payload[4], NumberStyles.HexNumber));
                    res.Value.TargetID = uint.Parse(payload[6], NumberStyles.HexNumber);
                    res.Value.TotalTime = float.Parse(payload[8]);
                    res.Value.FinishAt = DateTime.Parse(payload[1]).AddSeconds(res.Value.TotalTime);
                    ACTAddPos(pos, res.InstanceID, payload, 9);
                }
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

            public static (Operation?, List<(uint, Vector4)>) ParseACT(string[] payload)
            {
                OpActorTether res = new();
                res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
                res.Value.Target = uint.Parse(payload[4], NumberStyles.HexNumber);
                res.Value.ID = uint.Parse(payload[8], NumberStyles.HexNumber);
                return (res, new());
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
                    res.Value.SourceID = ActorID(payload[7]);
                    res.Value.Extra = ushort.Parse(payload[5], NumberStyles.HexNumber);
                    var timeLeft = float.Parse(payload[6]);
                    res.Value.ExpireAt = timeLeft != 0 ? DateTime.Parse(payload[0]).AddSeconds(timeLeft) : DateTime.MaxValue;
                }
                return res;
            }

            public static (Operation?, List<(uint, Vector4)>) ParseACT(string[] payload, WorldState ws, bool gain)
            {
                OpActorStatus res = new();
                res.InstanceID = uint.Parse(payload[7], NumberStyles.HexNumber);
                var actor = ws.FindActor(res.InstanceID);
                if (actor != null)
                {
                    var id = uint.Parse(payload[2], NumberStyles.HexNumber);
                    var source = uint.Parse(payload[5], NumberStyles.HexNumber);
                    res.Index = Array.FindIndex(actor.Statuses, x => x.ID == id && x.SourceID == source);
                    if (gain)
                    {
                        if (res.Index == -1)
                            res.Index = Array.FindIndex(actor.Statuses, x => x.ID == 0); // new buff
                        res.Value.ID = id;
                        res.Value.SourceID = source;
                        res.Value.Extra = ushort.Parse(payload[9], NumberStyles.HexNumber);
                        res.Value.ExpireAt = DateTime.Parse(payload[1]).AddSeconds(float.Parse(payload[4]));
                    }
                    else if (res.Index == -1)
                    {
                        return (null, new()); // lose non-existent, ignore...
                    }
                }
                return (res, new());
            }

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

            public static Operation? Parse(string[] payload)
            {
                OpEventIcon res = new();
                res.InstanceID = ActorID(payload[2]);
                res.IconID = uint.Parse(payload[3]);
                return res;
            }

            public static (Operation?, List<(uint, Vector4)>) ParseACT(string[] payload, int networkDelta)
            {
                OpEventIcon res = new();
                res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
                res.IconID = (uint)((int)uint.Parse(payload[6], NumberStyles.HexNumber) - networkDelta);
                return (res, new());
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
                res.Value.Action = Action(payload[3]);
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

                var araw = uint.Parse(payload[4], NumberStyles.HexNumber);
                var atype = (ActionType)(araw >> 24);
                if (atype == ActionType.None)
                    atype = ActionType.Spell;
                var aid = araw & 0x00FFFFFF;
                if (atype != ActionType.Spell)
                    aid = (uint)((int)aid - networkDelta);
                res.Value.Action = new(atype, aid);

                res.Value.MainTargetID = uint.Parse(payload[6], NumberStyles.HexNumber);
                ACTAddPos(pos, res.Value.CasterID, payload, 40);
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
                    ACTAddPos(pos, res.Value.MainTargetID, payload, 30);
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
                            "ZONE" => OpZoneChange.Parse(elements),
                            "PCOM" => OpEnterExitCombat.Parse(elements),
                            "PID " => OpPlayerIDChange.Parse(elements),
                            "WAY+" => OpWaymarkChange.Parse(elements, true),
                            "WAY-" => OpWaymarkChange.Parse(elements, false),
                            "ACT+" => OpActorCreate.Parse(elements),
                            "ACT-" => OpActorDestroy.Parse(elements),
                            "NAME" => OpActorRename.Parse(elements),
                            "CLSR" => OpActorClassChange.Parse(elements),
                            "MOVE" => OpActorMove.Parse(elements),
                            "ATG+" => OpActorTargetable.Parse(elements, true),
                            "ATG-" => OpActorTargetable.Parse(elements, false),
                            "DIE+" => OpActorDead.Parse(elements, true),
                            "DIE-" => OpActorDead.Parse(elements, false),
                            "TARG" => OpActorTarget.Parse(elements),
                            "CST+" => OpActorCast.Parse(elements, true),
                            "CST-" => OpActorCast.Parse(elements, false),
                            "TET+" => OpActorTether.Parse(elements, true),
                            "TET-" => OpActorTether.Parse(elements, false),
                            "STA+" => OpActorStatus.Parse(elements, true),
                            "STA-" => OpActorStatus.Parse(elements, false),
                            "STA!" => OpActorStatus.Parse(elements, true),
                            "ICON" => OpEventIcon.Parse(elements),
                            "CST!" => OpEventCast.Parse(elements),
                            "ENVC" => OpEventEnvControl.Parse(elements),
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
            uint inCombatWith = 0;
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
                        if (res.Ops.Count > 0 && res.Ops.Last().Timestamp > timestamp)
                            timestamp = res.Ops.Last().Timestamp;

                        (Operation? newOp, List<(uint, Vector4)> newPos) = int.Parse(elements[0]) switch
                        {
                            1 => OpZoneChange.ParseACT(elements),
                            2 => OpPlayerIDChange.ParseACT(elements),
                            3 => OpActorCreate.ParseACT(elements),
                            4 => OpActorDestroy.ParseACT(elements),
                            20 => OpActorCast.ParseACT(elements, true),
                            21 => OpEventCast.ParseACT(elements, networkDelta),
                            22 => OpEventCast.ParseACT(elements, networkDelta),
                            23 => OpActorCast.ParseACT(elements, false),
                            24 => ParseACTCoords(elements, 2, 13),
                            25 => OpActorDead.ParseACT(elements),
                            26 => OpActorStatus.ParseACT(elements, ws, true),
                            27 => OpEventIcon.ParseACT(elements, networkDelta),
                            28 => OpWaymarkChange.ParseACT(elements),
                            30 => OpActorStatus.ParseACT(elements, ws, false),
                            34 => OpActorTargetable.ParseACT(elements),
                            35 => OpActorTether.ParseACT(elements),
                            37 => ParseACTCoords(elements, 2, 11),
                            38 => ParseACTCoords(elements, 2, 11),
                            39 => ParseACTCoords(elements, 2, 10),
                            _ => (null, new())
                        };

                        var newCastOp = newOp as OpEventCast;
                        if (newCastOp != null)
                        {
                            var prevCastOp = res.Ops.LastOrDefault() as OpEventCast;
                            if (prevCastOp != null &&
                                prevCastOp.Value.CasterID == newCastOp.Value.CasterID &&
                                prevCastOp.Value.Action == newCastOp.Value.Action &&
                                prevCastOp.Value.MaxTargets == newCastOp.Value.MaxTargets &&
                                prevCastOp.Value.SourceSequence == newCastOp.Value.SourceSequence)
                            {
                                prevCastOp.Value.Targets.Add(newCastOp.Value.Targets.First());
                                newOp = prevCastOp;
                                res.Ops.RemoveAt(res.Ops.Count - 1);
                            }

                            var cast = ws.FindActor(newCastOp.Value.CasterID)?.CastInfo;
                            if (cast != null && cast.Action == newCastOp.Value.Action)
                            {
                                OpActorCast endCastOp = new();
                                endCastOp.Timestamp = timestamp;
                                endCastOp.InstanceID = newCastOp.Value.CasterID;
                                endCastOp.Redo(ws);
                                res.Ops.Add(endCastOp);
                            }

                            if (newCastOp.Value.CasterID == ws.PlayerActorID && inCombatWith == 0 && newCastOp.Value.Targets.Count > 0)
                            {
                                var target = ws.FindActor(newCastOp.Value.Targets.First().ID);
                                if (target?.Type == WorldState.ActorType.Enemy)
                                {
                                    inCombatWith = target.InstanceID;
                                    OpEnterExitCombat combatOp = new();
                                    combatOp.Timestamp = timestamp;
                                    combatOp.Value = true;
                                    res.Ops.Add(combatOp);
                                }
                            }
                        }

                        var newRemoveOp = newOp as OpActorDestroy;
                        if (newRemoveOp?.InstanceID == inCombatWith)
                        {
                            inCombatWith = 0;
                            OpEnterExitCombat combatOp = new();
                            combatOp.Timestamp = timestamp;
                            combatOp.Value = false;
                            res.Ops.Add(combatOp);
                        }

                        foreach ((var id, var posRot) in newPos)
                        {
                            var actor = ws.FindActor(id);
                            if (actor != null && posRot != new Vector4(actor.Position, actor.Rotation))
                            {
                                OpActorMove moveOp = new();
                                moveOp.Timestamp = timestamp;
                                moveOp.InstanceID = id;
                                moveOp.PosRot = posRot;
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

        private static ActionID Action(string repr)
        {
            var parts = repr.Split(' ');
            var type = parts.Length > 0 ? Enum.Parse<ActionType>(parts[0]) : ActionType.None;
            var id = parts.Length > 1 ? uint.Parse(parts[1]) : 0;
            return new(type, id);
        }

        private static Vector4? ACTPosRot(string[] payload, int startIndex)
        {
            return payload[startIndex].Length > 0 ? new(float.Parse(payload[startIndex]), float.Parse(payload[startIndex + 2]), float.Parse(payload[startIndex + 1]), float.Parse(payload[startIndex + 3])) : null;
        }

        private static void ACTAddPos(List<(uint, Vector4)> list, uint id, string[] payload, int startIndex)
        {
            var res = ACTPosRot(payload, startIndex);
            if (res != null)
                list.Add((id, res.Value));
        }

        private static (Operation?, List<(uint, Vector4)>) ParseACTCoords(string[] payload, int actorIndex, int startIndex)
        {
            List<(uint, Vector4)> pos = new();
            ACTAddPos(pos, uint.Parse(payload[actorIndex], NumberStyles.HexNumber), payload, startIndex);
            return (null, pos);
        }
    }
}
