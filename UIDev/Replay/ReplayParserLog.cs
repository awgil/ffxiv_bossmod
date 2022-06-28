using BossMod;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace UIDev
{
    class ReplayParserLog : ReplayParser
    {
        public static Replay Parse(string path)
        {
            try
            {
                ReplayParserLog parser = new();
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

                        parser.ParseLine(elements);
                    }
                }
                return parser.Finish(path);
            }
            catch (Exception e)
            {
                Service.Log($"Failed to read {path}: {e}");
                return new();
            }
        }

        private int _version = 0;

        private void ParseLine(string[] payload)
        {
            if (_version > 0 && _version < 5)
            {
                var timestamp = DateTime.Parse(payload[0]);
                if (_res.Ops.Count == 0 || _res.Ops.Last().Timestamp < timestamp)
                    AddOp(new WorldState.OpFrameStart() { NewTimestamp = timestamp });
            }
            switch (payload[1])
            {
                case "VER ": ParseVersion(payload); break;
                case "FRAM": ParseFrameStart(payload); break;
                case "ZONE": ParseZoneChange(payload); break;
                case "DIRU": ParseDirectorUpdate(payload); break;
                case "ENVC": ParseEnvControl(payload); break;
                case "WAY+": ParseWaymarkChange(payload, true); break;
                case "WAY-": ParseWaymarkChange(payload, false); break;
                case "ACT+": ParseActorCreate(payload); break;
                case "ACT-": ParseActorDestroy(payload); break;
                case "NAME": ParseActorRename(payload); break;
                case "CLSR": ParseActorClassChange(payload); break;
                case "MOVE": ParseActorMove(payload); break;
                case "HP  ": ParseActorHP(payload); break;
                case "ATG+": ParseActorTargetable(payload, true); break;
                case "ATG-": ParseActorTargetable(payload, false); break;
                case "DIE+": ParseActorDead(payload, true); break;
                case "DIE-": ParseActorDead(payload, false); break;
                case "COM+": ParseActorCombat(payload, true); break;
                case "COM-": ParseActorCombat(payload, false); break;
                case "TARG": ParseActorTarget(payload); break;
                case "TETH": ParseActorTether(payload, true); break;
                case "TET+": ParseActorTether(payload, true); break; // legacy (up to v4)
                case "TET-": ParseActorTether(payload, false); break; // legacy (up to v4)
                case "CST+": ParseActorCastInfo(payload, true); break;
                case "CST-": ParseActorCastInfo(payload, false); break;
                case "CST!": ParseActorCastEvent(payload); break;
                case "STA+": ParseActorStatus(payload, true); break;
                case "STA-": ParseActorStatus(payload, false); break;
                case "STA!": ParseActorStatus(payload, true); break;
                case "ICON": ParseActorIcon(payload); break;
                case "PAR ": ParsePartyModify(payload); break;
                case "PAR+": ParsePartyJoin(payload); break; // legacy (up to v3)
                case "PAR-": ParsePartyLeave(payload); break; // legacy (up to v3)
                case "PAR!": ParsePartyAssign(payload); break; // legacy (up to v3)
            }
        }

        private void ParseVersion(string[] payload)
        {
            _version = int.Parse(payload[2]);
            if (_version < 2)
                throw new Exception($"Version {_version} is too old and is no longer supported, sorry");
        }

        private void ParseFrameStart(string[] payload)
        {
            AddOp(new WorldState.OpFrameStart() { NewTimestamp = DateTime.Parse(payload[0]) });
        }

        private void ParseZoneChange(string[] payload)
        {
            AddOp(new WorldState.OpZoneChange() { Zone = ushort.Parse(payload[2]) });
        }

        private void ParseDirectorUpdate(string[] payload)
        {
            AddOp(new WorldState.OpDirectorUpdate() {
                DirectorID = uint.Parse(payload[2], NumberStyles.HexNumber),
                UpdateID = uint.Parse(payload[3], NumberStyles.HexNumber),
                Param1 = uint.Parse(payload[4], NumberStyles.HexNumber),
                Param2 = uint.Parse(payload[5], NumberStyles.HexNumber),
                Param3 = uint.Parse(payload[6], NumberStyles.HexNumber),
                Param4 = uint.Parse(payload[7], NumberStyles.HexNumber),
            });
        }

        private void ParseEnvControl(string[] payload)
        {
            AddOp(new WorldState.OpEnvControl()
            {
                DirectorID = uint.Parse(payload[2], NumberStyles.HexNumber),
                Index = byte.Parse(payload[3], NumberStyles.HexNumber),
                State = uint.Parse(payload[4], NumberStyles.HexNumber),
            });
        }

        private void ParseWaymarkChange(string[] payload, bool set)
        {
            AddOp(new WaymarkState.OpWaymarkChange()
            {
                ID = Enum.Parse<Waymark>(payload[2]),
                Pos = set ? Vec3(payload[3]) : null,
            });
        }

        private void ParseActorCreate(string[] payload)
        {
            var parts = payload[2].Split('/');
            var (hpCur, hpMax) = payload.Length > 7 ? CurMax(payload[7]) : (0, 0);
            AddOp(new ActorState.OpCreate()
            {
                InstanceID = ulong.Parse(parts[0], NumberStyles.HexNumber),
                OID = uint.Parse(parts[1], NumberStyles.HexNumber),
                Name = parts[2],
                Type = Enum.Parse<ActorType>(parts[3]),
                Class = Enum.Parse<Class>(payload[3]),
                PosRot = new(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]).Degrees().Rad),
                HitboxRadius = float.Parse(payload[5]),
                HPCur = hpCur,
                HPMax = hpMax,
                IsTargetable = bool.Parse(payload[4]),
                OwnerID = payload.Length > 6 ? ActorID(payload[6]) : 0,
            });
        }

        private void ParseActorDestroy(string[] payload)
        {
            AddOp(new ActorState.OpDestroy() { InstanceID = ActorID(payload[2]) });
        }

        private void ParseActorRename(string[] payload)
        {
            var parts = payload[2].Split('/');
            AddOp(new ActorState.OpRename() { InstanceID = ulong.Parse(parts[0], NumberStyles.HexNumber), Name = parts[2] });
        }

        private void ParseActorClassChange(string[] payload)
        {
            AddOp(new ActorState.OpClassChange() { InstanceID = ActorID(payload[2]), Class = Enum.Parse<Class>(payload[4]) });
        }

        private void ParseActorMove(string[] payload)
        {
            var parts = payload[2].Split('/');
            AddOp(new ActorState.OpMove() { InstanceID = ulong.Parse(parts[0], NumberStyles.HexNumber), PosRot = new(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]).Degrees().Rad) });
        }

        private void ParseActorHP(string[] payload)
        {
            var (hpCur, hpMax) = CurMax(payload[3]);
            AddOp(new ActorState.OpHP() { InstanceID = ActorID(payload[2]), Cur = hpCur, Max = hpMax });
        }

        private void ParseActorTargetable(string[] payload, bool targetable)
        {
            AddOp(new ActorState.OpTargetable() { InstanceID = ActorID(payload[2]), Value = targetable });
        }

        private void ParseActorDead(string[] payload, bool dead)
        {
            AddOp(new ActorState.OpDead() { InstanceID = ActorID(payload[2]), Value = dead });
        }

        private void ParseActorCombat(string[] payload, bool value)
        {
            AddOp(new ActorState.OpCombat() { InstanceID = ActorID(payload[2]), Value = value });
        }

        private void ParseActorTarget(string[] payload)
        {
            AddOp(new ActorState.OpTarget() { InstanceID = ActorID(payload[2]), Value = ActorID(payload[3]) });
        }

        private void ParseActorTether(string[] payload, bool tether)
        {
            AddOp(new ActorState.OpTether() { InstanceID = ActorID(payload[2]), Value = tether ? new() { ID = uint.Parse(payload[3]), Target = ActorID(payload[4]) } : new() });
        }

        private void ParseActorCastInfo(string[] payload, bool start)
        {
            ActorCastInfo? value = null;
            if (start)
            {
                var parts = payload[6].Split('/');
                var totalTime = float.Parse(parts[1]);
                value = new()
                {
                    Action = Action(payload[3]),
                    TargetID = ActorID(payload[4]),
                    Location = Vec3(payload[5]),
                    TotalTime = totalTime,
                    FinishAt = DateTime.Parse(payload[0]).AddSeconds(totalTime - float.Parse(parts[0])),
                };
            }
            AddOp(new ActorState.OpCastInfo() { InstanceID = ActorID(payload[2]), Value = value });
        }

        private void ParseActorCastEvent(string[] payload)
        {
            ActorCastEvent value = new()
            {
                Action = Action(payload[3]),
                MainTargetID = ActorID(payload[4]),
                AnimationLockTime = float.Parse(payload[5]),
                MaxTargets = uint.Parse(payload[6]),
            };
            for (int i = 7; i < payload.Length; ++i)
            {
                var parts = payload[i].Split('!');
                ActorCastEvent.Target target = new();
                target.ID = ActorID(parts[0]);
                for (int j = 1; j < parts.Length; ++j)
                    target.Effects[j - 1] = ulong.Parse(parts[j], NumberStyles.HexNumber);
                value.Targets.Add(target);
            }
            AddOp(new ActorState.OpCastEvent() { InstanceID = ActorID(payload[2]), Value = value });
        }

        private void ParseActorStatus(string[] payload, bool gainOrUpdate)
        {
            ActorStatus value = new();
            if (gainOrUpdate)
            {
                int sep = payload[4].IndexOf(' ');
                value.ID = uint.Parse(sep >= 0 ? payload[4].AsSpan(0, sep) : payload[4].AsSpan());
                value.SourceID = ActorID(payload[7]);
                value.Extra = ushort.Parse(payload[5], NumberStyles.HexNumber);
                value.ExpireAt = DateTime.Parse(payload[0]).AddSeconds(float.Parse(payload[6]));
            }
            AddOp(new ActorState.OpStatus()
            {
                InstanceID = ActorID(payload[2]),
                Index = int.Parse(payload[3]),
                Value = value,
            });
        }

        private void ParseActorIcon(string[] payload)
        {
            AddOp(new ActorState.OpIcon() { InstanceID = ActorID(payload[2]), IconID = uint.Parse(payload[3]) });
        }

        private void ParsePartyModify(string[] payload)
        {
            AddOp(new PartyState.OpModify()
            {
                Slot = int.Parse(payload[2]),
                ContentID = ulong.Parse(payload[3], NumberStyles.HexNumber),
                InstanceID = ulong.Parse(payload[4], NumberStyles.HexNumber),
            });
        }

        private void ParsePartyJoin(string[] payload)
        {
            AddOp(new PartyState.OpModify()
            {
                Slot = int.Parse(payload[2]),
                ContentID = ulong.Parse(payload[3], NumberStyles.HexNumber),
                InstanceID = ulong.Parse(payload[4], NumberStyles.HexNumber),
            });
        }

        private void ParsePartyLeave(string[] payload)
        {
            AddOp(new PartyState.OpModify() { Slot = int.Parse(payload[2]) });
        }

        private void ParsePartyAssign(string[] payload)
        {
            AddOp(new PartyState.OpModify()
            {
                Slot = int.Parse(payload[2]),
                ContentID = ulong.Parse(payload[3], NumberStyles.HexNumber),
                InstanceID = ulong.Parse(payload[4], NumberStyles.HexNumber),
            });
        }

        private static Vector3 Vec3(string repr)
        {
            var parts = repr.Split('/');
            return new(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }

        private static ulong ActorID(string actor)
        {
            var sep = actor.IndexOf('/');
            return ulong.Parse(sep >= 0 ? actor.AsSpan(0, sep) : actor.AsSpan(), NumberStyles.HexNumber);
        }

        private static (uint, uint) CurMax(string repr)
        {
            var parts = repr.Split('/');
            return (uint.Parse(parts[0]), uint.Parse(parts[1]));
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
