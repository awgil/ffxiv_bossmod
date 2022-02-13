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
    using static Replay;

    static class ReplayParserLog
    {
        public static Replay Parse(string path)
        {
            Replay res = new();
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
                            "ZONE" => ParseZoneChange(elements),
                            "PCOM" => ParseEnterExitCombat(elements),
                            "PID " => ParsePlayerIDChange(elements),
                            "WAY+" => ParseWaymarkChange(elements, true),
                            "WAY-" => ParseWaymarkChange(elements, false),
                            "ACT+" => ParseActorCreate(elements),
                            "ACT-" => ParseActorDestroy(elements),
                            "NAME" => ParseActorRename(elements),
                            "CLSR" => ParseActorClassChange(elements),
                            "MOVE" => ParseActorMove(elements),
                            "ATG+" => ParseActorTargetable(elements, true),
                            "ATG-" => ParseActorTargetable(elements, false),
                            "DIE+" => ParseActorDead(elements, true),
                            "DIE-" => ParseActorDead(elements, false),
                            "TARG" => ParseActorTarget(elements),
                            "CST+" => ParseActorCast(elements, true),
                            "CST-" => ParseActorCast(elements, false),
                            "TET+" => ParseActorTether(elements, true),
                            "TET-" => ParseActorTether(elements, false),
                            "STA+" => ParseActorStatus(elements, true),
                            "STA-" => ParseActorStatus(elements, false),
                            "STA!" => ParseActorStatus(elements, true),
                            "ICON" => ParseEventIcon(elements),
                            "CST!" => ParseEventCast(elements),
                            "ENVC" => ParseEventEnvControl(elements),
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

        private static OpZoneChange ParseZoneChange(string[] payload)
        {
            OpZoneChange res = new();
            res.Zone = ushort.Parse(payload[2]);
            return res;
        }

        private static OpEnterExitCombat ParseEnterExitCombat(string[] payload)
        {
            OpEnterExitCombat res = new();
            res.Value = bool.Parse(payload[2]);
            return res;
        }

        private static OpPlayerIDChange ParsePlayerIDChange(string[] payload)
        {
            OpPlayerIDChange res = new();
            res.Value = ActorID(payload[2]);
            return res;
        }

        private static OpWaymarkChange ParseWaymarkChange(string[] payload, bool set)
        {
            OpWaymarkChange res = new();
            res.ID = Enum.Parse<WorldState.Waymark>(payload[2]);
            if (set)
                res.Pos = Vec3(payload[3]);
            return res;
        }

        private static OpActorCreate ParseActorCreate(string[] payload)
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

        private static OpActorDestroy ParseActorDestroy(string[] payload)
        {
            OpActorDestroy res = new();
            res.InstanceID = ActorID(payload[2]);
            return res;
        }

        private static OpActorRename ParseActorRename(string[] payload)
        {
            var parts = payload[2].Split('/');
            OpActorRename res = new();
            res.InstanceID = uint.Parse(parts[0], NumberStyles.HexNumber);
            res.Name = parts[2];
            return res;
        }

        private static OpActorClassChange ParseActorClassChange(string[] payload)
        {
            OpActorClassChange res = new();
            res.InstanceID = ActorID(payload[2]);
            res.Class = Enum.Parse<Class>(payload[4]);
            return res;
        }

        private static OpActorMove ParseActorMove(string[] payload)
        {
            var parts = payload[2].Split('/');
            OpActorMove res = new();
            res.InstanceID = uint.Parse(parts[0], NumberStyles.HexNumber);
            res.PosRot = new(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]) * MathF.PI / 180);
            return res;
        }

        private static OpActorTargetable ParseActorTargetable(string[] payload, bool targetable)
        {
            OpActorTargetable res = new();
            res.InstanceID = ActorID(payload[2]);
            res.Value = targetable;
            return res;
        }

        private static OpActorDead ParseActorDead(string[] payload, bool dead)
        {
            OpActorDead res = new();
            res.InstanceID = ActorID(payload[2]);
            res.Value = dead;
            return res;
        }

        private static OpActorTarget ParseActorTarget(string[] payload)
        {
            OpActorTarget res = new();
            res.InstanceID = ActorID(payload[2]);
            res.Value = ActorID(payload[3]);
            return res;
        }

        private static OpActorCast ParseActorCast(string[] payload, bool start)
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

        private static OpActorTether ParseActorTether(string[] payload, bool tether)
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

        private static OpActorStatus ParseActorStatus(string[] payload, bool gainOrUpdate)
        {
            OpActorStatus res = new();
            res.InstanceID = ActorID(payload[2]);
            res.Index = int.Parse(payload[3]);
            if (gainOrUpdate)
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

        private static OpEventIcon ParseEventIcon(string[] payload)
        {
            OpEventIcon res = new();
            res.InstanceID = ActorID(payload[2]);
            res.IconID = uint.Parse(payload[3]);
            return res;
        }

        private static OpEventCast ParseEventCast(string[] payload)
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

        private static OpEventEnvControl ParseEventEnvControl(string[] payload)
        {
            OpEventEnvControl res = new();
            res.FeatureID = uint.Parse(payload[2], NumberStyles.HexNumber);
            res.Index = byte.Parse(payload[3], NumberStyles.HexNumber);
            res.State = uint.Parse(payload[4], NumberStyles.HexNumber);
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
    }
}
