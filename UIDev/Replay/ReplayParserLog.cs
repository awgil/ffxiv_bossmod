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
    using static ReplayOps;

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
        private ulong _lastFakeContentID = 0; // pc will always have id=1
        private ulong _playerID = 0;

        private void ParseLine(string[] payload)
        {
            var timestamp = DateTime.Parse(payload[0]);
            switch (payload[1])
            {
                case "VER ": ParseVersion(timestamp, payload); break;
                case "ZONE": ParseZoneChange(timestamp, payload); break;
                case "PCOM": ParseEnterExitCombat(timestamp, payload); break;
                case "PID ": ParsePlayerIDChange(timestamp, payload); break;
                case "WAY+": ParseWaymarkChange(timestamp, payload, true); break;
                case "WAY-": ParseWaymarkChange(timestamp, payload, false); break;
                case "ACT+": ParseActorCreate(timestamp, payload); break;
                case "ACT-": ParseActorDestroy(timestamp, payload); break;
                case "NAME": ParseActorRename(timestamp, payload); break;
                case "CLSR": ParseActorClassChange(timestamp, payload); break;
                case "MOVE": ParseActorMove(timestamp, payload); break;
                case "HP  ": ParseActorHP(timestamp, payload); break;
                case "ATG+": ParseActorTargetable(timestamp, payload, true); break;
                case "ATG-": ParseActorTargetable(timestamp, payload, false); break;
                case "DIE+": ParseActorDead(timestamp, payload, true); break;
                case "DIE-": ParseActorDead(timestamp, payload, false); break;
                case "COM+": ParseActorCombat(timestamp, payload, true); break;
                case "COM-": ParseActorCombat(timestamp, payload, false); break;
                case "TARG": ParseActorTarget(timestamp, payload); break;
                case "CST+": ParseActorCast(timestamp, payload, true); break;
                case "CST-": ParseActorCast(timestamp, payload, false); break;
                case "TET+": ParseActorTether(timestamp, payload, true); break;
                case "TET-": ParseActorTether(timestamp, payload, false); break;
                case "STA+": ParseActorStatus(timestamp, payload, true); break;
                case "STA-": ParseActorStatus(timestamp, payload, false); break;
                case "STA!": ParseActorStatus(timestamp, payload, true); break;
                case "PAR+": ParsePartyJoin(timestamp, payload); break;
                case "PAR-": ParsePartyLeave(timestamp, payload); break;
                case "PAR!": ParsePartyAssign(timestamp, payload); break;
                case "ICON": ParseEventIcon(timestamp, payload); break;
                case "CST!": ParseEventCast(timestamp, payload); break;
                case "DIRU": ParseEventDirectorUpdate(timestamp, payload); break;
                case "ENVC": ParseEventEnvControl(timestamp, payload); break;
            }
        }

        private void ParseVersion(DateTime timestamp, string[] payload)
        {
            _version = int.Parse(payload[2]);
        }

        private void ParseZoneChange(DateTime timestamp, string[] payload)
        {
            OpZoneChange res = new();
            res.Zone = ushort.Parse(payload[2]);
            AddOp(timestamp, res);
        }

        private void ParseEnterExitCombat(DateTime timestamp, string[] payload)
        {
            bool value = bool.Parse(payload[2]);
            foreach (var act in _ws.Actors.Where(a => a.IsTargetable))
            {
                OpActorCombat op = new();
                op.InstanceID = act.InstanceID;
                op.Value = value;
                AddOp(timestamp, op);
            }
        }

        private void ParsePlayerIDChange(DateTime timestamp, string[] payload)
        {
            for (int i = PartyState.MaxSize - 1; i >= 0; --i)
            {
                if (_ws.Party.ContentIDs[i] != 0)
                {
                    OpPartyLeave opLeave = new();
                    opLeave.ContentID = _ws.Party.ContentIDs[i];
                    opLeave.InstanceID = _ws.Party.Members[i]?.InstanceID ?? 0;
                    AddOp(timestamp, opLeave);
                }
            }
            _lastFakeContentID = 0;

            _playerID = ActorID(payload[2]);
            if (_ws.Actors.Find(_playerID) != null)
                BuildV0Party(timestamp);
        }

        private void ParseWaymarkChange(DateTime timestamp, string[] payload, bool set)
        {
            OpWaymarkChange res = new();
            res.ID = Enum.Parse<Waymark>(payload[2]);
            if (set)
                res.Pos = Vec3(payload[3]);
            AddOp(timestamp, res);
        }

        private void ParseActorCreate(DateTime timestamp, string[] payload)
        {
            var parts = payload[2].Split('/');
            OpActorCreate res = new();
            res.InstanceID = ulong.Parse(parts[0], NumberStyles.HexNumber);
            res.OID = uint.Parse(parts[1], NumberStyles.HexNumber);
            res.Name = parts[2];
            res.Type = Enum.Parse<ActorType>(parts[3]);
            res.Class = Enum.Parse<Class>(payload[3]);
            res.PosRot = new(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]) * MathF.PI / 180);
            res.IsTargetable = bool.Parse(payload[4]);
            res.HitboxRadius = float.Parse(payload[5]);
            res.OwnerID = payload.Length > 6 ? ActorID(payload[6]) : 0;
            (res.HPCur, res.HPMax) = payload.Length > 7 ? CurMax(payload[7]) : (0, 0);
            AddOp(timestamp, res);

            if (_version == 0 && res.Type == ActorType.Player)
            {
                if (res.InstanceID == _playerID)
                {
                    BuildV0Party(timestamp);
                }
                else
                {
                    OpPartyJoin join = new();
                    join.ContentID = ++_lastFakeContentID;
                    join.InstanceID = res.InstanceID;
                    AddOp(timestamp, join);
                }
            }
        }

        private void ParseActorDestroy(DateTime timestamp, string[] payload)
        {
            var instanceID = ActorID(payload[2]);
            if (_version == 0 && _playerID != 0)
            {
                var slot = _ws.Party.FindSlot(instanceID);
                if (slot >= 0)
                {
                    OpPartyLeave leave = new();
                    leave.ContentID = _ws.Party.ContentIDs[slot];
                    leave.InstanceID = instanceID;
                    AddOp(timestamp, leave);
                }
            }

            OpActorDestroy res = new();
            res.InstanceID = instanceID;
            AddOp(timestamp, res);
        }

        private void ParseActorRename(DateTime timestamp, string[] payload)
        {
            var parts = payload[2].Split('/');
            OpActorRename res = new();
            res.InstanceID = ulong.Parse(parts[0], NumberStyles.HexNumber);
            res.Name = parts[2];
            AddOp(timestamp, res);
        }

        private void ParseActorClassChange(DateTime timestamp, string[] payload)
        {
            OpActorClassChange res = new();
            res.InstanceID = ActorID(payload[2]);
            res.Class = Enum.Parse<Class>(payload[4]);
            AddOp(timestamp, res);
        }

        private void ParseActorMove(DateTime timestamp, string[] payload)
        {
            var parts = payload[2].Split('/');
            OpActorMove res = new();
            res.InstanceID = ulong.Parse(parts[0], NumberStyles.HexNumber);
            res.PosRot = new(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]) * MathF.PI / 180);
            AddOp(timestamp, res);
        }

        private void ParseActorHP(DateTime timestamp, string[] payload)
        {
            OpActorHP res = new();
            res.InstanceID = ActorID(payload[2]);
            (res.Cur, res.Max) = CurMax(payload[3]);
            AddOp(timestamp, res);
        }

        private void ParseActorTargetable(DateTime timestamp, string[] payload, bool targetable)
        {
            OpActorTargetable res = new();
            res.InstanceID = ActorID(payload[2]);
            res.Value = targetable;
            AddOp(timestamp, res);
        }

        private void ParseActorDead(DateTime timestamp, string[] payload, bool dead)
        {
            OpActorDead res = new();
            res.InstanceID = ActorID(payload[2]);
            res.Value = dead;
            AddOp(timestamp, res);
        }

        private void ParseActorCombat(DateTime timestamp, string[] payload, bool value)
        {
            OpActorCombat res = new();
            res.InstanceID = ActorID(payload[2]);
            res.Value = value;
            AddOp(timestamp, res);
        }

        private void ParseActorTarget(DateTime timestamp, string[] payload)
        {
            OpActorTarget res = new();
            res.InstanceID = ActorID(payload[2]);
            res.Value = ActorID(payload[3]);
            AddOp(timestamp, res);
        }

        private void ParseActorCast(DateTime timestamp, string[] payload, bool start)
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
                res.Value.FinishAt = DateTime.Parse(payload[0]).AddSeconds(res.Value.TotalTime - float.Parse(parts[0]));
            }
            AddOp(timestamp, res);
        }

        private void ParseActorTether(DateTime timestamp, string[] payload, bool tether)
        {
            OpActorTether res = new();
            res.InstanceID = ActorID(payload[2]);
            if (tether)
            {
                res.Value.ID = uint.Parse(payload[3]);
                res.Value.Target = ActorID(payload[4]);
            }
            AddOp(timestamp, res);
        }

        private void ParseActorStatus(DateTime timestamp, string[] payload, bool gainOrUpdate)
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
                res.Value.ExpireAt = DateTime.Parse(payload[0]).AddSeconds(float.Parse(payload[6]));
            }
            AddOp(timestamp, res);
        }

        private void ParsePartyJoin(DateTime timestamp, string[] payload)
        {
            OpPartyJoin res = new();
            res.ContentID = ulong.Parse(payload[3], NumberStyles.HexNumber);
            res.InstanceID = ulong.Parse(payload[4], NumberStyles.HexNumber);
            AddOp(timestamp, res);
        }

        private void ParsePartyLeave(DateTime timestamp, string[] payload)
        {
            OpPartyLeave res = new();
            res.ContentID = ulong.Parse(payload[3], NumberStyles.HexNumber);
            res.InstanceID = ulong.Parse(payload[4], NumberStyles.HexNumber);
            AddOp(timestamp, res);
        }

        private void ParsePartyAssign(DateTime timestamp, string[] payload)
        {
            OpPartyAssign res = new();
            res.ContentID = ulong.Parse(payload[3], NumberStyles.HexNumber);
            res.InstanceID = ulong.Parse(payload[4], NumberStyles.HexNumber);
            AddOp(timestamp, res);
        }

        private void ParseEventIcon(DateTime timestamp, string[] payload)
        {
            OpEventIcon res = new();
            res.InstanceID = ActorID(payload[2]);
            res.IconID = uint.Parse(payload[3]);
            AddOp(timestamp, res);
        }

        private void ParseEventCast(DateTime timestamp, string[] payload)
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
                CastEvent.Target target = new();
                target.ID = ActorID(parts[0]);
                for (int j = 1; j < parts.Length; ++j)
                    target.Effects[j - 1] = ulong.Parse(parts[j], NumberStyles.HexNumber);
                res.Value.Targets.Add(target);
            }
            AddOp(timestamp, res);
        }

        private void ParseEventDirectorUpdate(DateTime timestamp, string[] payload)
        {
            OpEventDirectorUpdate res = new();
            res.DirectorID = uint.Parse(payload[2], NumberStyles.HexNumber);
            res.UpdateID = uint.Parse(payload[3], NumberStyles.HexNumber);
            res.Param1 = uint.Parse(payload[4], NumberStyles.HexNumber);
            res.Param2 = uint.Parse(payload[5], NumberStyles.HexNumber);
            res.Param3 = uint.Parse(payload[6], NumberStyles.HexNumber);
            res.Param4 = uint.Parse(payload[7], NumberStyles.HexNumber);
            AddOp(timestamp, res);
        }

        private void ParseEventEnvControl(DateTime timestamp, string[] payload)
        {
            OpEventEnvControl res = new();
            res.FeatureID = uint.Parse(payload[2], NumberStyles.HexNumber);
            res.Index = byte.Parse(payload[3], NumberStyles.HexNumber);
            res.State = uint.Parse(payload[4], NumberStyles.HexNumber);
            AddOp(timestamp, res);
        }

        private void BuildV0Party(DateTime timestamp)
        {
            OpPartyJoin pcJoin = new();
            pcJoin.ContentID = ++_lastFakeContentID;
            pcJoin.InstanceID = _playerID;
            AddOp(timestamp, pcJoin);

            foreach (var player in _ws.Actors.Where(a => a.Type == ActorType.Player && a.InstanceID != _playerID))
            {
                OpPartyJoin otherJoin = new();
                otherJoin.ContentID = ++_lastFakeContentID;
                otherJoin.InstanceID = player.InstanceID;
                AddOp(timestamp, otherJoin);
            }
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
