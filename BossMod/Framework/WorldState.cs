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

        public enum Waymark : byte { A, B, C, D, N1, N2, N3, N4, Count }
        private Vector3?[] _waymarks = new Vector3?[8];
        public event EventHandler<(Waymark, Vector3?)>? WaymarkChanged;
        public Vector3? GetWaymark(Waymark i) => _waymarks[(int)i];
        public void SetWaymark(Waymark i, Vector3? v)
        {
            if (GetWaymark(i) != v)
            {
                _waymarks[(int)i] = v;
                WaymarkChanged?.Invoke(this, (i, v));
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

        // this matches values in ClassJob excel sheet
        public enum ActorRole
        {
            None = 0,
            Tank = 1,
            Melee = 2,
            Ranged = 3,
            Healer = 4,
        }

        // matches FFXIVClientStructs.FFXIV.Client.Game.ActionType
        public enum ActionType : byte
        {
            None = 0,
            Spell = 1,
            Item = 2,
            KeyItem = 3,
            Ability = 4,
            General = 5,
            Companion = 6,
            CraftAction = 9,
            MainCommand = 10,
            PetAction = 11,
            Mount = 13,
            PvPAction = 14,
            Waymark = 15,
            ChocoboRaceAbility = 16,
            ChocoboRaceItem = 17,
            SquadronAction = 19,
            Accessory = 20
        }

        public class CastInfo
        {
            public ActionType ActionType;
            public uint ActionID;
            public uint TargetID;
            public Vector3 Location;
            public float CurrentTime;
            public float TotalTime;
        }

        // note on tethers - it is N:1 type of relation, actor can be tethered to 0 or 1 actors, but can itself have multiple actors tethering themselves to itself
        public struct TetherInfo
        {
            public uint Target; // instance id
            public uint ID;
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
            public uint ClassID;
            public ActorRole Role;
            public Vector3 Position = new();
            public float Rotation; // 0 = pointing S, pi/2 = pointing E, pi = pointing N, -pi/2 = pointing W
            public float HitboxRadius;
            public bool IsTargetable;
            public bool IsDead;
            public uint TargetID;
            public CastInfo? CastInfo;
            public TetherInfo Tether = new();
            public Status[] Statuses = new Status[30]; // empty slots have ID=0

            public Actor(uint instanceID, uint oid, ActorType type, uint classID, ActorRole role, Vector3 pos, float rot, float hitboxRadius, bool targetable)
            {
                InstanceID = instanceID;
                OID = oid;
                Type = type;
                ClassID = classID;
                Role = role;
                Position = pos;
                Rotation = rot;
                HitboxRadius = hitboxRadius;
                IsTargetable = targetable;
            }

            public Status? FindStatus(uint sid)
            {
                return Array.Find(Statuses, x => x.ID == sid);
            }
        }

        private Dictionary<uint, Actor> _actors = new();
        public IReadOnlyDictionary<uint, Actor> Actors => _actors;
        public Actor? FindActor(uint instanceID)
        {
            return Actors.GetValueOrDefault(instanceID);
        }

        public Actor? FindPlayer()
        {
            return Actors.GetValueOrDefault(PlayerActorID);
        }

        public event EventHandler<Actor>? ActorCreated;
        public Actor AddActor(uint instanceID, uint oid, ActorType type, uint classID, ActorRole role, Vector3 pos, float rot, float hitboxRadius, bool targetable)
        {
            var act = _actors[instanceID] = new Actor(instanceID, oid, type, classID, role, pos, rot, hitboxRadius, targetable);
            ActorCreated?.Invoke(this, act);
            return act;
        }

        public event EventHandler<Actor>? ActorDestroyed;
        public void RemoveActor(uint instanceID)
        {
            var actor = FindActor(instanceID);
            if (actor == null)
                return; // nothing to remove

            UpdateCastInfo(actor, null); // stop casting
            UpdateTether(actor, new()); // untether
            UpdateStatuses(actor, new Status[30]); // clear statuses
            ActorDestroyed?.Invoke(this, actor);
            _actors.Remove(instanceID);
        }

        public event EventHandler<(Actor, uint, ActorRole)>? ActorClassRoleChanged; // actor already contains new position, old is passed as extra args
        public void ChangeActorClassRole(Actor act, uint newClass, ActorRole newRole)
        {
            if (act.ClassID != newClass || act.Role != newRole)
            {
                var prevClass = act.ClassID;
                var prevRole = act.Role;
                act.ClassID = newClass;
                act.Role = newRole;
                ActorClassRoleChanged?.Invoke(this, (act, prevClass, prevRole));
            }
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

        public event EventHandler<Actor>? ActorIsTargetableChanged; // actor contains new state, old is inverted
        public void ChangeActorIsTargetable(Actor act, bool newTargetable)
        {
            if (act.IsTargetable != newTargetable)
            {
                act.IsTargetable = newTargetable;
                ActorIsTargetableChanged?.Invoke(this, act);
            }
        }

        public event EventHandler<Actor>? ActorIsDeadChanged; // actor contains new state, old is inverted
        public void ChangeActorIsDead(Actor act, bool newDead)
        {
            if (act.IsDead != newDead)
            {
                act.IsDead = newDead;
                ActorIsDeadChanged?.Invoke(this, act);
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
        public event EventHandler<Actor>? ActorCastFinished; // note that actor structure still contains cast details when this is invoked; invoked if actor disappears without finishing cast
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

        public event EventHandler<Actor>? ActorTethered;
        public event EventHandler<Actor>? ActorUntethered; // note that actor structure still contains previous tether info when this is invoked; invoked if actor disappears without untethering
        public void UpdateTether(Actor act, TetherInfo tether)
        {
            if (act.Tether.Target == tether.Target && act.Tether.ID == tether.ID)
                return; // nothing changes

            if (act.Tether.Target != 0)
            {
                ActorUntethered?.Invoke(this, act);
            }
            act.Tether = tether;
            if (act.Tether.Target != 0)
            {
                ActorTethered?.Invoke(this, act);
            }
        }

        // argument = actor + status index; TODO stack/param notifications?...
        public event EventHandler<(Actor, int)>? ActorStatusGain;
        public event EventHandler<(Actor, int)>? ActorStatusLose; // note that status structure still contains details when this is invoked; invoked if actor disappears
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

        // instant events
        public event EventHandler<(uint, uint)>? EventIcon; // TODO: this should really be an actor field, but I have no idea what triggers icon clear...
        public void DispatchEventIcon(uint actorID, uint iconID)
        {
            EventIcon?.Invoke(this, (actorID, iconID));
        }

        public class CastResult
        {
            public uint CasterID;
            public uint MainTargetID; // note that actual affected targets could be completely different
            public uint ActionID;
            public ActionType ActionType;
            public float AnimationLockTime;
            public uint MaxTargets;
            public uint NumTargets; // note: consider storing per-target ID and effects here...
        }
        public event EventHandler<CastResult>? EventCast;
        public void DispatchEventCast(CastResult info)
        {
            EventCast?.Invoke(this, info);
        }

        public event EventHandler<(uint, byte, uint)>? EventEnvControl;
        public void DispatchEventEnvControl(uint featureID, byte index, uint state)
        {
            EventEnvControl?.Invoke(this, (featureID, index, state));
        }
    }
}
