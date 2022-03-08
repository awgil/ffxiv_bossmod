using BossMod;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace UIDev
{
    using static Replay;

    class ReplayParserAct
    {
        public static Replay Parse(string path, int networkDelta)
        {
            ReplayParserAct parser = new();
            parser._networkDelta = networkDelta;
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

                        parser.ParseLine(elements);
                    }
                }
            }
            catch (Exception e)
            {
                Service.Log($"Failed to read {path}: {e}");
            }
            return parser._res;
        }

        private Replay _res = new();
        private WorldState _ws = new();
        private uint _inCombatWith = 0;
        private int _networkDelta = 0;
        private ulong _lastPlayerContentID = 0;

        private static Vector4? ParsePosRot(string[] payload, int startIndex)
        {
            return payload[startIndex].Length > 0 ? new(float.Parse(payload[startIndex]), float.Parse(payload[startIndex + 2]), float.Parse(payload[startIndex + 1]), float.Parse(payload[startIndex + 3])) : null;
        }

        private void ParseLine(string[] payload)
        {
            var timestamp = DateTime.Parse(payload[1]);
            if (_res.Ops.Count > 0 && _res.Ops.Last().Timestamp > timestamp)
                timestamp = _res.Ops.Last().Timestamp;

            switch (int.Parse(payload[0]))
            {
                case 1: ParseTerritory(timestamp, payload); break;
                case 2: ParseChangePrimaryPlayer(timestamp, payload); break;
                case 3: ParseAddCombatant(timestamp, payload); break;
                case 4: ParseRemoveCombatant(timestamp, payload); break;
                case 11: ParsePartyList(timestamp, payload); break;
                case 20: ParseStartsCasting(timestamp, payload); break;
                case 21: ParseActionEffect(timestamp, payload, false); break;
                case 22: ParseActionEffect(timestamp, payload, true); break;
                case 23: ParseCancelAction(timestamp, payload); break;
                case 24: AddMove(timestamp, uint.Parse(payload[2], NumberStyles.HexNumber), payload, 13); break;
                case 25: ParseDeath(timestamp, payload); break;
                case 26: ParseStatusAdd(timestamp, payload); break;
                case 27: ParseTargetIcon(timestamp, payload); break;
                case 28: ParseWaymarkMarker(timestamp, payload); break;
                case 30: ParseStatusRemove(timestamp, payload); break;
                case 34: ParseNameToggle(timestamp, payload); break;
                case 35: ParseTether(timestamp, payload); break;
                case 37: AddMove(timestamp, uint.Parse(payload[2], NumberStyles.HexNumber), payload, 11); break;
                case 38: AddMove(timestamp, uint.Parse(payload[2], NumberStyles.HexNumber), payload, 11); break;
                case 39: AddMove(timestamp, uint.Parse(payload[2], NumberStyles.HexNumber), payload, 10); break;
            };
        }

        private void AddOp(DateTime timestamp, Operation op)
        {
            op.Timestamp = timestamp;
            op.Redo(_ws);
            _res.Ops.Add(op);
        }

        private void AddMove(DateTime timestamp, uint id, string[] payload, int startPos)
        {
            var actor = _ws.Actors.Find(id);
            if (actor == null)
                return;

            // implicit resurrect
            if (actor.IsDead && actor.Type == ActorType.Player)
            {
                OpActorDead op = new();
                op.InstanceID = id;
                op.Value = false;
                AddOp(timestamp, op);
            }

            var posRot = ParsePosRot(payload, startPos);
            if (posRot == null || posRot.Value == actor.PosRot)
                return;

            OpActorMove moveOp = new();
            moveOp.InstanceID = id;
            moveOp.PosRot = posRot.Value;
            AddOp(timestamp, moveOp);
        }

        private void ParseTerritory(DateTime timestamp, string[] payload)
        {
            for (int i = PartyState.MaxSize - 1; i >= 0; ++i)
            {
                if (_ws.Party.ContentIDs[i] != 0)
                {
                    OpPartyLeave op = new();
                    op.ContentID = _ws.Party.ContentIDs[i];
                    op.InstanceID = _ws.Party.Members[i]?.InstanceID ?? 0;
                    AddOp(timestamp, op);
                }
            }
            _lastPlayerContentID = 0;

            Actor? existing;
            while ((existing = _ws.Actors.FirstOrDefault()) != null)
            {
                OpActorDestroy op = new();
                op.InstanceID = existing.InstanceID;
                AddOp(timestamp, op);
            }

            OpZoneChange res = new();
            res.Zone = ushort.Parse(payload[2], NumberStyles.HexNumber);
            AddOp(timestamp, res);
        }

        private void ParseChangePrimaryPlayer(DateTime timestamp, string[] payload)
        {
            // do nothing, this is always followed by player-add it seems...
            //OpPlayerIDChange res = new();
            //res.Value = uint.Parse(payload[2], NumberStyles.HexNumber);
            //AddOp(timestamp, res);
        }

        private void ParseAddCombatant(DateTime timestamp, string[] payload)
        {
            OpActorCreate res = new();
            res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
            res.OID = uint.Parse(payload[10]);
            res.Name = payload[3];

            if ((res.InstanceID & 0xF0000000u) == 0x10000000u)
                res.Type = ActorType.Player;
            else if (uint.Parse(payload[6], NumberStyles.HexNumber) != 0) // owner id
                res.Type = ActorType.Pet;
            else
                res.Type = ActorType.Enemy;

            res.Class = (Class)uint.Parse(payload[4], NumberStyles.HexNumber);
            res.PosRot = ParsePosRot(payload, 17) ?? new();
            res.IsTargetable = true;

            var existing = _ws.Actors.Find(res.InstanceID);
            if (existing == null)
            {
                AddOp(timestamp, res);

                if (res.Type == ActorType.Player && _lastPlayerContentID == 0)
                {
                    OpPartyJoin opJoin = new();
                    opJoin.ContentID = ++_lastPlayerContentID;
                    opJoin.InstanceID = res.InstanceID;
                    AddOp(timestamp, res);
                }

                return;
            }

            // ACT uses actor recreate for a whole bunch of events we treat in a more granular way
            if (existing.OID != res.OID || existing.Type != res.Type)
            {
                OpActorDestroy removeOp = new();
                removeOp.InstanceID = res.InstanceID;
                AddOp(timestamp, removeOp);
                AddOp(timestamp, res);
                return;
            }

            if (existing.Name != res.Name)
            {
                OpActorRename op = new();
                op.InstanceID = res.InstanceID;
                op.Name = res.Name;
                AddOp(timestamp, op);
            }
            if (existing.Class != res.Class)
            {
                OpActorClassChange op = new();
                op.InstanceID = res.InstanceID;
                op.Class = res.Class;
                AddOp(timestamp, op);
            }
            if (existing.IsTargetable != res.IsTargetable)
            {
                OpActorTargetable op = new();
                op.InstanceID = res.InstanceID;
                op.Value = res.IsTargetable;
                AddOp(timestamp, op);
            }
        }

        private void ParseRemoveCombatant(DateTime timestamp, string[] payload)
        {
            OpActorDestroy res = new();
            res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
            AddMove(timestamp, res.InstanceID, payload, 17);
            AddOp(timestamp, res);

            // implicitly exit combat
            if (res.InstanceID == _inCombatWith)
            {
                _inCombatWith = 0;
                OpActorCombat combatOp = new();
                combatOp.InstanceID = _ws.Party.Player()?.InstanceID ?? 0;
                combatOp.Value = false;
                AddOp(timestamp, combatOp);
            }

            var slot = _ws.Party.FindSlot(res.InstanceID);
            if (slot >= 0)
            {
                OpPartyLeave opLeave = new();
                opLeave.ContentID = _ws.Party.ContentIDs[slot];
                opLeave.InstanceID = res.InstanceID;
                AddOp(timestamp, opLeave);
            }
        }

        private void ParsePartyList(DateTime timestamp, string[] payload)
        {
            int size = int.Parse(payload[2]);
            HashSet<uint> party = new();
            for (int i = 0; i < size; ++i)
                party.Add(uint.Parse(payload[3 + i], NumberStyles.HexNumber));

            for (int i = PartyState.MaxSize - 1; i >= 0; ++i)
            {
                var m = _ws.Party.Members[i];
                if (m != null && !party.Contains(m.InstanceID))
                {
                    OpPartyLeave op = new();
                    op.ContentID = _ws.Party.ContentIDs[i];
                    op.InstanceID = m.InstanceID;
                    AddOp(timestamp, op);
                }
            }

            foreach (uint instanceID in party)
            {
                if (_ws.Party.FindSlot(instanceID) == -1)
                {
                    OpPartyJoin op = new();
                    op.ContentID = ++_lastPlayerContentID;
                    op.InstanceID = instanceID;
                    AddOp(timestamp, op);
                }
            }
        }

        private void ParseStartsCasting(DateTime timestamp, string[] payload)
        {
            OpActorCast res = new();
            res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
            res.Value = new();
            res.Value.Action = new(ActionType.Spell, uint.Parse(payload[4], NumberStyles.HexNumber));
            res.Value.TargetID = uint.Parse(payload[6], NumberStyles.HexNumber);
            res.Value.TotalTime = float.Parse(payload[8]);
            res.Value.FinishAt = DateTime.Parse(payload[1]).AddSeconds(res.Value.TotalTime);
            AddMove(timestamp, res.InstanceID, payload, 9);
            AddOp(timestamp, res);

            // implicit resurrect
            var actor = _ws.Actors.Find(res.InstanceID);
            if (actor != null && actor.IsDead && actor.Type == ActorType.Player)
            {
                OpActorDead op = new();
                op.InstanceID = actor.InstanceID;
                op.Value = false;
                AddOp(timestamp, op);
            }
        }

        private void ParseActionEffect(DateTime timestamp, string[] payload, bool isAOE)
        {
            var araw = uint.Parse(payload[4], NumberStyles.HexNumber);
            var atype = (ActionType)(araw >> 24);
            if (atype == ActionType.None)
                atype = ActionType.Spell;
            var aid = araw & 0x00FFFFFF;
            if (atype != ActionType.Spell)
                aid = (uint)((int)aid - _networkDelta);

            OpEventCast res = new();
            res.Value.CasterID = uint.Parse(payload[2], NumberStyles.HexNumber);
            res.Value.Action = new(atype, aid);
            res.Value.MainTargetID = uint.Parse(payload[6], NumberStyles.HexNumber);
            res.Value.MaxTargets = uint.Parse(payload[46]);
            res.Value.SourceSequence = uint.Parse(payload[44], NumberStyles.HexNumber);

            // merge aoe targets
            if (isAOE)
            {
                var targetIndex = uint.Parse(payload[45]);
                if (targetIndex > 0)
                {
                    var prev = _res.Ops.LastOrDefault() as OpEventCast;
                    if (prev != null &&
                        prev.Value.CasterID == res.Value.CasterID &&
                        prev.Value.Action == res.Value.Action &&
                        prev.Value.MaxTargets == res.Value.MaxTargets &&
                        prev.Value.SourceSequence == res.Value.SourceSequence)
                    {
                        res = prev;
                        _res.Ops.RemoveAt(_res.Ops.Count - 1);
                    }
                }
            }

            AddMove(timestamp, res.Value.CasterID, payload, 40);
            if (res.Value.MainTargetID != 0xE0000000u)
            {
                CastEvent.Target target = new();
                target.ID = res.Value.MainTargetID;
                for (int i = 0; i < 8; ++i)
                {
                    var lo = ulong.Parse(payload[8 + 2 * i], NumberStyles.HexNumber);
                    var hi = ulong.Parse(payload[9 + 2 * i], NumberStyles.HexNumber);
                    target[i] = (hi << 32) | lo;
                }
                res.Value.Targets.Add(target);
                AddMove(timestamp, res.Value.MainTargetID, payload, 30);
            }
            AddOp(timestamp, res);

            // implicitly end cast
            var caster = _ws.Actors.Find(res.Value.CasterID);
            if (caster?.CastInfo?.Action == res.Value.Action)
            {
                OpActorCast endCastOp = new();
                endCastOp.InstanceID = res.Value.CasterID;
                AddOp(timestamp, endCastOp);
            }

            // implicitly enter combat
            if (res.Value.CasterID == _ws.Party.Player()?.InstanceID && _inCombatWith == 0 && res.Value.Targets.Count > 0)
            {
                var target = _ws.Actors.Find(res.Value.Targets.Last().ID);
                if (target?.Type == ActorType.Enemy)
                {
                    _inCombatWith = target.InstanceID;
                    OpActorCombat combatOp1 = new();
                    combatOp1.InstanceID = res.Value.CasterID;
                    combatOp1.Value = true;
                    AddOp(timestamp, combatOp1);

                    OpActorCombat combatOp2 = new();
                    combatOp2.InstanceID = res.Value.CasterID;
                    combatOp2.Value = true;
                    AddOp(timestamp, combatOp2);
                }
            }

            // implicit resurrect
            if (caster != null && caster.IsDead && caster.Type == ActorType.Player)
            {
                OpActorDead op = new();
                op.InstanceID = caster.InstanceID;
                op.Value = false;
                AddOp(timestamp, op);
            }
        }

        private void ParseCancelAction(DateTime timestamp, string[] payload)
        {
            OpActorCast res = new();
            res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
            AddOp(timestamp, res);
        }

        private void ParseDeath(DateTime timestamp, string[] payload)
        {
            OpActorDead res = new();
            res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
            res.Value = true;
            AddOp(timestamp, res);
        }

        private void ParseStatusAdd(DateTime timestamp, string[] payload)
        {
            var actor = _ws.Actors.Find(uint.Parse(payload[7], NumberStyles.HexNumber));
            if (actor == null)
                return;

            var id = uint.Parse(payload[2], NumberStyles.HexNumber);
            var source = uint.Parse(payload[5], NumberStyles.HexNumber);
            var index = Array.FindIndex(actor.Statuses, x => x.ID == id && x.SourceID == source);
            if (index == -1)
                index = Array.FindIndex(actor.Statuses, x => x.ID == 0); // new buff

            OpActorStatus res = new();
            res.InstanceID = actor.InstanceID;
            res.Index = index;
            res.Value.ID = id;
            res.Value.SourceID = source;
            res.Value.Extra = ushort.Parse(payload[9], NumberStyles.HexNumber);
            res.Value.ExpireAt = DateTime.Parse(payload[1]).AddSeconds(float.Parse(payload[4]));
            AddOp(timestamp, res);

            if (res.Value.ID == 0x1A2 && actor.IsDead)
            {
                // transcendent buff means resurrect happened
                OpActorDead op = new();
                op.InstanceID = actor.InstanceID;
                op.Value = false;
                AddOp(timestamp, op);
            }
        }

        private void ParseTargetIcon(DateTime timestamp, string[] payload)
        {
            OpEventIcon res = new();
            res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
            res.IconID = (uint)((int)uint.Parse(payload[6], NumberStyles.HexNumber) - _networkDelta);
            AddOp(timestamp, res);
        }

        private void ParseWaymarkMarker(DateTime timestamp, string[] payload)
        {
            OpWaymarkChange res = new();
            res.ID = (Waymark)uint.Parse(payload[3]);
            if (payload[2] == "Add")
                res.Pos = new(float.Parse(payload[6]), float.Parse(payload[8]), float.Parse(payload[7]));
            AddOp(timestamp, res);
        }

        private void ParseStatusRemove(DateTime timestamp, string[] payload)
        {
            var actor = _ws.Actors.Find(uint.Parse(payload[7], NumberStyles.HexNumber));
            if (actor == null)
                return;

            var id = uint.Parse(payload[2], NumberStyles.HexNumber);
            var source = uint.Parse(payload[5], NumberStyles.HexNumber);
            var index = Array.FindIndex(actor.Statuses, x => x.ID == id && x.SourceID == source);
            if (index == -1)
                return;

            OpActorStatus res = new();
            res.InstanceID = uint.Parse(payload[7], NumberStyles.HexNumber);
            res.Index = index;
            AddOp(timestamp, res);
        }

        private void ParseNameToggle(DateTime timestamp, string[] payload)
        {
            OpActorTargetable res = new();
            res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
            res.Value = byte.Parse(payload[6], NumberStyles.HexNumber) != 0;
            AddOp(timestamp, res);
        }

        private void ParseTether(DateTime timestamp, string[] payload)
        {
            OpActorTether res = new();
            res.InstanceID = uint.Parse(payload[2], NumberStyles.HexNumber);
            res.Value.Target = uint.Parse(payload[4], NumberStyles.HexNumber);
            res.Value.ID = uint.Parse(payload[8], NumberStyles.HexNumber);
            AddOp(timestamp, res);
        }
    }
}
