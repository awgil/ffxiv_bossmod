using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    // this class represents parts of a world state that are interesting to boss modules
    // it does not know anything about dalamud, so it can be used for UI test - there is a separate utility that updates it based on game state every frame
    public class WorldState
    {
        private ushort _currentZone;
        public event EventHandler<ushort>? CurrentZoneChanged;
        public ushort CurrentZone
        {
            get => _currentZone;
            set
            {
                if (_currentZone != value)
                {
                    _currentZone = value;
                    CurrentZoneChanged?.Invoke(this, value);
                }
            }
        }

        private bool _playerInCombat;
        public event EventHandler<bool>? PlayerInCombatChanged;
        public bool PlayerInCombat
        {
            get => _playerInCombat;
            set
            {
                if (_playerInCombat != value)
                {
                    _playerInCombat = value;
                    PlayerInCombatChanged?.Invoke(this, value);
                }
            }
        }

        private uint _playerActorID;
        public event EventHandler<uint>? PlayerActorIDChanged;
        public uint PlayerActorID
        {
            get => _playerActorID;
            set
            {
                if (_playerActorID != value)
                {
                    _playerActorID = value;
                    PlayerActorIDChanged?.Invoke(this, value);
                }
            }
        }

        // objkind << 8 + objsubkind
        public enum ActorType
        {
            None = 0,
            Player = 0x104,
            Unknown = 0x201,
            Pet = 0x202,
            Chocobo = 0x203,
            Enemy = 0x205,
            EventNpc = 0x300,
            Treasure = 0x400,
            Aetheryte = 0x500,
            GatheringPoint = 0x600,
            EventObj = 0x700,
            MountType = 0x800,
            Companion = 0x900,
            Retainer = 0xA00,
            Area = 0xB00,
            Housing = 0xC00,
            Cutscene = 0xD00,
            CardStand = 0xE00,
        }

        public class CastInfo
        {
            public byte ActionType;
            public uint ActionID;
            public uint TargetID;
            public Vector3 Location;
            public float CurrentTime;
            public float TotalTime;
        }

        public struct Status
        {
            public uint ID;
            public byte Param;
            public byte StackCount;
            public float RemainingTime;
            public uint SourceID;
        }

        public class Actor
        {
            public uint InstanceID; // 'uuid'
            public uint OID;
            public ActorType Type;
            public Vector3 Position = new();
            public float Rotation; // 0 = pointing S, pi/2 = pointing E, pi = pointing N, -pi/2 = pointing W
            public float HitboxRadius;
            public uint TargetID;
            public CastInfo? CastInfo;
            public Status[] Statuses = new Status[30]; // empty slots have ID=0

            public Actor(uint instanceID, uint oid, ActorType type, Vector3 pos, float rot, float hitboxRadius, uint targetID)
            {
                InstanceID = instanceID;
                OID = oid;
                Type = type;
                Position = pos;
                Rotation = rot;
                HitboxRadius = hitboxRadius;
                TargetID = targetID;
            }
        }

        private Dictionary<uint, Actor> _actors = new();
        public IReadOnlyDictionary<uint, Actor> Actors => _actors;
        public Actor? FindActor(uint instanceID)
        {
            Actor? res;
            Actors.TryGetValue(instanceID, out res);
            return res;
        }

        public event EventHandler<Actor>? ActorCreated;
        public Actor AddActor(uint instanceID, uint oid, ActorType type, Vector3 pos, float rot, float hitboxRadius, uint targetID)
        {
            var act = _actors[instanceID] = new Actor(instanceID, oid, type, pos, rot, hitboxRadius, targetID);
            ActorCreated?.Invoke(this, act);
            return act;
        }

        public event EventHandler<Actor>? ActorDestroyed;
        public void RemoveActor(uint instanceID)
        {
            ActorDestroyed?.Invoke(this, _actors[instanceID]);
            _actors.Remove(instanceID);
        }

        public event EventHandler<(Actor, Vector3, float)>? ActorMoved; // actor already contains new position, old is passed as extra args
        public void MoveActor(Actor act, Vector3 newPos, float newRot)
        {
            if (act.Position != newPos || act.Rotation != newRot)
            {
                var prevPos = act.Position;
                var prevRot = act.Rotation;
                act.Position = newPos;
                act.Rotation = newRot;
                ActorMoved?.Invoke(this, (act, prevPos, prevRot));
            }
        }

        public event EventHandler<(Actor, uint)>? ActorTargetChanged; // actor already contains new target, old is passed as extra arg
        public void ChangeActorTarget(Actor act, uint newTarget)
        {
            if (act.TargetID != newTarget)
            {
                var prevTarget = act.TargetID;
                act.TargetID = newTarget;
                ActorTargetChanged?.Invoke(this, (act, prevTarget));
            }
        }

        public event EventHandler<Actor>? ActorCastStarted;
        public event EventHandler<Actor>? ActorCastFinished; // note that actor structure still contains cast details when this is invoked; not invoked if actor disappears without finishing cast?..
        public void UpdateCastInfo(Actor act, CastInfo? cast)
        {
            if (cast == null && act.CastInfo == null)
                return; // was not casting and is not casting

            if (cast != null && act.CastInfo != null && cast.ActionType == act.CastInfo.ActionType && cast.ActionID == act.CastInfo.ActionID && cast.TargetID == act.CastInfo.TargetID)
            {
                // continuing casting same spell
                act.CastInfo.CurrentTime = cast.CurrentTime;
                act.CastInfo.TotalTime = cast.TotalTime;
                return;
            }

            if (act.CastInfo != null)
            {
                // finish previous cast
                ActorCastFinished?.Invoke(this, act);
            }
            act.CastInfo = cast;
            if (act.CastInfo != null)
            {
                // start new cast
                ActorCastStarted?.Invoke(this, act);
            }
        }

        // argument = actor + status index; TODO stack/param notifications?...
        public event EventHandler<(Actor, int)>? ActorStatusGain;
        public event EventHandler<(Actor, int)>? ActorStatusLose; // note that status structure still contains details when this is invoked; not invoked if actor disappears
        public void UpdateStatuses(Actor act, Status[] statuses)
        {
            for (int i = 0; i < act.Statuses.Length; ++i)
            {
                if (act.Statuses[i].ID == statuses[i].ID && act.Statuses[i].SourceID == statuses[i].SourceID)
                {
                    // status was and still is active; just update details
                    act.Statuses[i].Param = statuses[i].Param; // what is it? can it be changed for live status, or does it mean status fade+apply?
                    act.Statuses[i].StackCount = statuses[i].StackCount; // this probably warrants a notification...
                    act.Statuses[i].RemainingTime = statuses[i].RemainingTime;
                    continue;
                }

                if (act.Statuses[i].ID != 0)
                {
                    // remove previous status
                    ActorStatusLose?.Invoke(this, (act, i));
                }
                act.Statuses[i] = statuses[i];
                if (act.Statuses[i].ID != 0)
                {
                    // apply new status
                    ActorStatusGain?.Invoke(this, (act, i));
                }
            }
        }
    }
}
