using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    // objkind << 8 + objsubkind
    public enum ActorType : ushort
    {
        None = 0,
        Player = 0x104,
        Part = 0x201,
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

    public class ActorCastInfo
    {
        public ActionID Action;
        public ulong TargetID;
        public Angle Rotation;
        public Vector3 Location;
        public float TotalTime;
        public DateTime FinishAt;
        public bool Interruptible;
        public bool EventHappened;

        public WPos LocXZ => new(Location.XZ());

        public bool IsSpell() => Action.Type == ActionType.Spell;
        public bool IsSpell<AID>(AID aid) where AID : Enum => Action == ActionID.MakeSpell(aid);
    }

    public class ActorCastEvent
    {
        public struct Target
        {
            public ulong ID;
            public ActionEffects Effects;
        }

        public ActionID Action;
        public ulong MainTargetID; // note that actual affected targets could be completely different
        public float AnimationLockTime;
        public uint MaxTargets;
        public List<Target> Targets = new();
        public Vector3 TargetPos;
        public uint SourceSequence;
        public uint GlobalSequence;

        public WPos TargetXZ => new(TargetPos.XZ());

        public bool IsSpell() => Action.Type == ActionType.Spell;
        public bool IsSpell<AID>(AID aid) where AID : Enum => Action == ActionID.MakeSpell(aid);
    }

    public struct ActorHP
    {
        public uint Cur;
        public uint Max;
        public uint Shield;
    }

    // note on tethers - it is N:1 type of relation, actor can be tethered to 0 or 1 actors, but can itself have multiple actors tethering themselves to itself
    public struct ActorTetherInfo
    {
        public ulong Target; // instance id
        public uint ID;
    }

    public struct ActorStatus
    {
        public uint ID;
        public ulong SourceID;
        public ushort Extra;
        public DateTime ExpireAt;
    }

    public struct ActorModelState
    {
        public byte ModelState;
        public byte AnimState1;
        public byte AnimState2;
    }

    public class Actor
    {
        public ulong InstanceID; // 'uuid'
        public uint OID;
        public int SpawnIndex; // [0-200) = character (even for normal, odd for dependents like mounts), [200-246) = client-side, [246, 286) = event object, [286, 426) = ???, [426-526) = ???, [526,596) = ???
        public string Name;
        public ActorType Type;
        public Class Class;
        public Vector4 PosRot = new(); // W = rotation: 0 = pointing S, pi/2 = pointing E, pi = pointing N, -pi/2 = pointing W
        public float HitboxRadius;
        public ActorHP HP;
        public uint CurMP;
        public bool IsDestroyed; // set to true when actor is removed from world; object might still be alive because of other references
        public bool IsTargetable;
        public bool IsAlly;
        public bool IsDead;
        public bool InCombat;
        public ActorModelState ModelState;
        public byte EventState; // not sure about the field meaning...
        public ulong OwnerID; // uuid of owner, for pets and similar
        public ulong TargetID;
        public ActorCastInfo? CastInfo;
        public ActorTetherInfo Tether = new();
        public ActorStatus[] Statuses = new ActorStatus[60]; // empty slots have ID=0

        public Role Role => Class.GetRole();
        public WPos Position => new(PosRot.X, PosRot.Z);
        public Angle Rotation => PosRot.W.Radians();
        public bool Omnidirectional => Utils.CharacterIsOmnidirectional(OID);

        public Actor(ulong instanceID, uint oid, int spawnIndex, string name, ActorType type, Class classID, Vector4 posRot, float hitboxRadius = 1, ActorHP hp = new(), uint mp = 0, bool targetable = true, bool ally = false, ulong ownerID = 0)
        {
            InstanceID = instanceID;
            OID = oid;
            SpawnIndex = spawnIndex;
            Name = name;
            Type = type;
            Class = classID;
            PosRot = posRot;
            HitboxRadius = hitboxRadius;
            HP = hp;
            CurMP = mp;
            IsTargetable = targetable;
            IsAlly = ally;
            OwnerID = ownerID;
        }

        public ActorStatus? FindStatus(uint sid)
        {
            var i = Array.FindIndex(Statuses, x => x.ID == sid);
            return i >= 0 ? Statuses[i] : null;
        }

        public ActorStatus? FindStatus(uint sid, ulong source)
        {
            var i = Array.FindIndex(Statuses, x => x.ID == sid && x.SourceID == source);
            return i >= 0 ? Statuses[i] : null;
        }

        public ActorStatus? FindStatus<SID>(SID sid) where SID : Enum => FindStatus((uint)(object)sid);
        public ActorStatus? FindStatus<SID>(SID sid, ulong source) where SID : Enum => FindStatus((uint)(object)sid, source);

        public override string ToString() => $"{OID:X} '{Name}' <{InstanceID:X}>";
    }
}
