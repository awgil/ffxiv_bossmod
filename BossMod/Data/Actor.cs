using System;
using System.Numerics;

namespace BossMod
{
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

    public class ActorCastInfo
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
    public struct ActorTetherInfo
    {
        public uint Target; // instance id
        public uint ID;
    }

    public struct ActorStatus
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
        public bool IsDestroyed; // set to true when actor is removed from world; object might still be alive because of other references
        public bool IsTargetable;
        public bool IsDead;
        public bool InCombat;
        public uint OwnerID; // uuid of owner, for pets and similar
        public uint TargetID;
        public ActorCastInfo? CastInfo;
        public ActorTetherInfo Tether = new();
        public ActorStatus[] Statuses = new ActorStatus[30]; // empty slots have ID=0

        public Role Role => Class.GetRole();
        public Vector3 Position => new(PosRot.X, PosRot.Y, PosRot.Z);
        public float Rotation => PosRot.W;

        public Actor(uint instanceID, uint oid, string name, ActorType type, Class classID, Vector4 posRot, float hitboxRadius, bool targetable, uint ownerID)
        {
            InstanceID = instanceID;
            OID = oid;
            Name = name;
            Type = type;
            Class = classID;
            PosRot = posRot;
            HitboxRadius = hitboxRadius;
            IsTargetable = targetable;
            OwnerID = ownerID;
        }

        public ActorStatus? FindStatus(uint sid) => Array.Find(Statuses, x => x.ID == sid);
        public ActorStatus? FindStatus<SID>(SID sid) where SID : Enum => FindStatus((uint)(object)sid);
    }
}
