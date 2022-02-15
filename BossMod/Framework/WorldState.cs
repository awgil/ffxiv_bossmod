using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    // this class represents parts of a world state that are interesting to boss modules
    // it does not know anything about dalamud, so it can be used for UI test - there is a separate utility that updates it based on game state every frame
    public class WorldState
    {
        public DateTime CurrentTime;

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

        public class CastInfo
        {
            public ActionID Action;
            public uint TargetID;
            public Vector3 Location;
            public float TotalTime;
            public DateTime FinishAt;

            public bool IsSpell() => Action.Type == ActionType.Spell;
            public bool IsSpell<AID>(AID aid) where AID : Enum => Action == ActionID.MakeSpell(aid);
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
            public uint SourceID;
            public ushort Extra;
            public DateTime ExpireAt;
        }

        public class Actor
        {
            public uint InstanceID; // 'uuid'
            public uint OID;
            public string Name;
            public ActorType Type;
            public Class Class;
            public Vector4 PosRot = new(); // W = rotation: 0 = pointing S, pi/2 = pointing E, pi = pointing N, -pi/2 = pointing W
            public float HitboxRadius;
            public bool IsTargetable;
            public bool IsDead;
            public uint TargetID;
            public CastInfo? CastInfo;
            public TetherInfo Tether = new();
            public Status[] Statuses = new Status[30]; // empty slots have ID=0

            public Role Role => Class.GetRole();
            public Vector3 Position => new(PosRot.X, PosRot.Y, PosRot.Z);
            public float Rotation => PosRot.W;

            public Actor(uint instanceID, uint oid, string name, ActorType type, Class classID, Vector4 posRot, float hitboxRadius, bool targetable)
            {
                InstanceID = instanceID;
                OID = oid;
                Name = name;
                Type = type;
                Class = classID;
                PosRot = posRot;
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
        public Actor AddActor(uint instanceID, uint oid, string name, ActorType type, Class classID, Vector4 posRot, float hitboxRadius, bool targetable)
        {
            var act = _actors[instanceID] = new Actor(instanceID, oid, name, type, classID, posRot, hitboxRadius, targetable);
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
            for (int i = 0; i < actor.Statuses.Length; ++i)
                UpdateStatus(actor, i, new()); // clear statuses
            ActorDestroyed?.Invoke(this, actor);
            _actors.Remove(instanceID);
        }

        public event EventHandler<(Actor, string)>? ActorRenamed; // actor already has new name, old is passed as extra arg
        public void RenameActor(Actor act, string newName)
        {
            if (act.Name != newName)
            {
                var prevName = act.Name;
                act.Name = newName;
                ActorRenamed?.Invoke(this, (act, prevName));
            }
        }

        public event EventHandler<(Actor, Class)>? ActorClassChanged; // actor already has new class, old is passed as extra args
        public void ChangeActorClass(Actor act, Class newClass)
        {
            if (act.Class != newClass)
            {
                var prevClass = act.Class;
                act.Class = newClass;
                ActorClassChanged?.Invoke(this, (act, prevClass));
            }
        }

        public event EventHandler<(Actor, Vector4)>? ActorMoved; // actor already contains new position/rotation, old is passed as extra args
        public void MoveActor(Actor act, Vector4 newPosRot)
        {
            if (act.PosRot != newPosRot)
            {
                var prevPosRot = act.PosRot;
                act.PosRot = newPosRot;
                ActorMoved?.Invoke(this, (act, prevPosRot));
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

            if (cast != null && act.CastInfo != null && cast.Action == act.CastInfo.Action && cast.TargetID == act.CastInfo.TargetID)
            {
                // continuing casting same spell
                act.CastInfo.TotalTime = cast.TotalTime;
                act.CastInfo.FinishAt = cast.FinishAt;
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

        // argument = actor + status index
        public event EventHandler<(Actor, int)>? ActorStatusGain;
        public event EventHandler<(Actor, int)>? ActorStatusLose; // note that status structure still contains details when this is invoked; invoked if actor disappears
        public event EventHandler<(Actor, int, ushort, DateTime)>? ActorStatusChange; // invoked when extra or expiration time is changed; status contains new values, old are passed as extra args
        public void UpdateStatus(Actor act, int index, Status value)
        {
            if (act.Statuses[index].ID == value.ID && act.Statuses[index].SourceID == value.SourceID)
            {
                // status was and still is active; just update details
                if (value.ID != 0 && (act.Statuses[index].Extra != value.Extra || (act.Statuses[index].ExpireAt - value.ExpireAt).Duration().TotalSeconds > 2))
                {
                    var prevExtra = act.Statuses[index].Extra;
                    var prevExpire = act.Statuses[index].ExpireAt;
                    act.Statuses[index].Extra = value.Extra;
                    act.Statuses[index].ExpireAt = value.ExpireAt;
                    ActorStatusChange?.Invoke(this, (act, index, prevExtra, prevExpire));
                }
            }
            else
            {
                if (act.Statuses[index].ID != 0)
                {
                    // remove previous status
                    ActorStatusLose?.Invoke(this, (act, index));
                }
                act.Statuses[index] = value;
                if (act.Statuses[index].ID != 0)
                {
                    // apply new status
                    ActorStatusGain?.Invoke(this, (act, index));
                }
            }
        }

        // instant events
        public event EventHandler<(uint actorID, uint iconID)>? EventIcon; // TODO: this should really be an actor field, but I have no idea what triggers icon clear...
        public void DispatchEventIcon((uint actorID, uint iconID) args)
        {
            EventIcon?.Invoke(this, args);
        }

        public class CastResult
        {
            public unsafe struct Target
            {
                public uint ID;
                public fixed ulong Effects[8];

                public ulong this[int index] {
                    get => Effects[index];
                    set => Effects[index] = value;
                }
            }

            public uint CasterID;
            public uint MainTargetID; // note that actual affected targets could be completely different
            public ActionID Action;
            public float AnimationLockTime;
            public uint MaxTargets;
            public List<Target> Targets = new();
            public uint SourceSequence;

            public bool IsSpell() => Action.Type == ActionType.Spell;
            public bool IsSpell<AID>(AID aid) where AID : Enum => Action == ActionID.MakeSpell(aid);
        }
        public event EventHandler<CastResult>? EventCast;
        public void DispatchEventCast(CastResult info)
        {
            EventCast?.Invoke(this, info);
        }

        public event EventHandler<(uint featureID, byte index, uint state)>? EventEnvControl;
        public void DispatchEventEnvControl((uint featureID, byte index, uint state) args)
        {
            EventEnvControl?.Invoke(this, args);
        }
    }
}
