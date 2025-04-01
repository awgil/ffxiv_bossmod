﻿namespace BossMod;

// objkind << 8 + objsubkind
public enum ActorType : ushort
{
    None = 0,
    Player = 0x104,
    Part = 0x201,
    Pet = 0x202,
    Chocobo = 0x203,
    Enemy = 0x205,
    DutySupport = 0x209,
    Helper = 0x20B,
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

public sealed record class ActorCastInfo
{
    public const float NPCFinishDelay = 0.3f; // for whatever reason, npc spells have reported remaining cast time consistently 0.3s smaller than reality

    public ActionID Action;
    public ulong TargetID;
    public Angle Rotation;
    public Vector3 Location;
    public float ElapsedTime;
    public float TotalTime;
    public bool Interruptible;
    public bool EventHappened;

    public WPos LocXZ => new(Location.XZ());
    public float RemainingTime => TotalTime - ElapsedTime;
    public float NPCTotalTime => TotalTime + NPCFinishDelay;
    public float NPCRemainingTime => NPCTotalTime - ElapsedTime;
    public float AdjustedTotalTime => TotalTime + Action.CastTimeExtra();

    public bool IsSpell() => Action.Type == ActionType.Spell;
    public bool IsSpell<AID>(AID aid) where AID : Enum => Action == ActionID.MakeSpell(aid);
}

// note: 'main' target could be completely different and unrelated to actual affected targets
public sealed record class ActorCastEvent(ActionID Action, ulong MainTargetID, float AnimationLockTime, uint MaxTargets, Vector3 TargetPos, uint GlobalSequence, uint SourceSequence, Angle Rotation)
{
    public readonly record struct Target(ulong ID, ActionEffects Effects);

    public readonly List<Target> Targets = [];

    public WPos TargetXZ => new(TargetPos.XZ());

    public bool IsSpell() => Action.Type == ActionType.Spell;
    public bool IsSpell<AID>(AID aid) where AID : Enum => Action == ActionID.MakeSpell(aid);
}

public record struct ActorHPMP(uint CurHP, uint MaxHP, uint Shield, uint CurMP, uint MaxMP);

// note on tethers - it is N:1 type of relation, actor can be tethered to 0 or 1 actors, but can itself have multiple actors tethering themselves to itself
// target is an instance id
public record struct ActorTetherInfo(uint ID, ulong Target);

public record struct ActorStatus(uint ID, ushort Extra, DateTime ExpireAt, ulong SourceID);

public record struct ActorModelState(byte ModelState, byte AnimState1, byte AnimState2);

public record struct ActorForayInfo(byte Level, byte Element);

public readonly record struct ActorIncomingEffect(uint GlobalSequence, int TargetIndex, ulong SourceInstanceId, ActionID Action, ActionEffects Effects);
public record struct PendingEffect(uint GlobalSequence, int TargetIndex, ulong SourceInstanceId, DateTime Expiration);
public record struct PendingEffectDelta(PendingEffect Effect, int Value);
public record struct PendingEffectStatus(PendingEffect Effect, uint StatusId);
public record struct PendingEffectStatusExtra(PendingEffect Effect, uint StatusId, byte ExtraLo);

public sealed class Actor(ulong instanceID, uint oid, int spawnIndex, string name, uint nameID, ActorType type, Class classID, int level, Vector4 posRot, float hitboxRadius = 1, ActorHPMP hpmp = default, bool targetable = true, bool ally = false, ulong ownerID = 0, uint fateID = 0)
{
    public ulong InstanceID = instanceID; // 'uuid'
    public uint OID = oid;
    public int SpawnIndex = spawnIndex; // [0-200) = character (even for normal, odd for dependents like mounts), [200-246) = client-side, [246, 286) = event object, [286, 426) = ???, [426-526) = ???, [526,596) = ???
    public uint FateID = fateID;
    public string Name = name;
    public uint NameID = nameID;
    public ActorType Type = type;
    public Class Class = classID;
    public int Level = level;
    public Vector4 PosRot = posRot; // W = rotation: 0 = pointing S, pi/2 = pointing E, pi = pointing N, -pi/2 = pointing W
    public Vector4 PrevPosRot = posRot; // during previous frame; can be used to calculate speed etc
    public float HitboxRadius = hitboxRadius;
    public ActorHPMP HPMP = hpmp;
    public bool IsDestroyed; // set to true when actor is removed from world; object might still be alive because of other references
    public bool IsTargetable = targetable;
    public bool IsAlly = ally;
    public bool IsDead;
    public bool InCombat;
    public bool AggroPlayer; // determines whether a given actor shows in the player's UI enemy list
    public ActorModelState ModelState;
    public ActorForayInfo ForayInfo;
    public byte EventState; // not sure about the field meaning...
    public ulong OwnerID = ownerID; // uuid of owner, for pets and similar
    public ulong TargetID;
    public uint MountId; // ID of current mount, 0 if not mounted
    public ActorCastInfo? CastInfo;
    public ActorTetherInfo Tether;
    public ActorStatus[] Statuses = new ActorStatus[60]; // empty slots have ID=0
    public ActorIncomingEffect[] IncomingEffects = new ActorIncomingEffect[32];

    // all pending lists are sorted by expiration time
    public List<PendingEffectDelta> PendingHPDifferences = []; // damage and heal effects applied to the target that were not confirmed yet
    public List<PendingEffectDelta> PendingMPDifferences = [];
    public List<PendingEffectStatusExtra> PendingStatuses = [];
    public List<PendingEffectStatus> PendingDispels = [];
    public List<PendingEffect> PendingKnockbacks = [];

    public Role Role => Class.GetRole();
    public ClassCategory ClassCategory => Class.GetClassCategory();
    public WPos Position => new(PosRot.X, PosRot.Z);
    public WPos PrevPosition => new(PrevPosRot.X, PrevPosRot.Z);
    public WDir LastFrameMovement => Position - PrevPosition;
    public Angle Rotation => PosRot.W.Radians();
    public bool Omnidirectional
    {
        get => Utils.CharacterIsOmnidirectional(OID) || field;
        set;
    }

    public bool IsDeadOrDestroyed => IsDead || IsDestroyed;
    public bool IsFriendlyNPC => Type == ActorType.Enemy && IsAlly && IsTargetable;
    public bool IsStrikingDummy => NameID == 541; // this is a hack, but striking dummies are special in some ways
    public int CharacterSpawnIndex => SpawnIndex < 200 && (SpawnIndex & 1) == 0 ? (SpawnIndex >> 1) : -1; // [0,100) for 'real' characters, -1 otherwise
    public float HPRatio => (float)HPMP.CurHP / HPMP.MaxHP;
    public int PendingHPDiffence => PendingHPDifferences.Sum(p => p.Value);
    public int PendingMPDiffence => PendingMPDifferences.Sum(p => p.Value);
    public int PredictedHPRaw => (int)HPMP.CurHP + PendingHPDiffence;
    public int PredictedMPRaw => (int)HPMP.CurMP + PendingMPDiffence;
    public int PredictedHPClamped => Math.Clamp(PredictedHPRaw, 0, (int)HPMP.MaxHP);
    public bool PredictedDead => PredictedHPRaw <= 1 && !IsStrikingDummy;
    public float PredictedHPRatio => (float)PredictedHPRaw / HPMP.MaxHP;

    // if expirationForPredicted is not null, search pending first, and return one if found; in that case only low byte of extra will be set
    public ActorStatus? FindStatus(uint sid, DateTime? expirationForPending = null)
    {
        if (expirationForPending != null)
            foreach (ref var s in PendingStatuses.AsSpan())
                if (s.StatusId == sid)
                    return new(sid, s.ExtraLo, expirationForPending.Value, s.Effect.SourceInstanceId);
        foreach (ref var s in Statuses.AsSpan())
            if (s.ID == sid)
                return s;
        return null;
    }

    public ActorStatus? FindStatus(uint sid, ulong source, DateTime? expirationForPending = null)
    {
        if (expirationForPending != null)
            foreach (ref var s in PendingStatuses.AsSpan())
                if (s.StatusId == sid && s.Effect.SourceInstanceId == source)
                    return new(sid, s.ExtraLo, expirationForPending.Value, s.Effect.SourceInstanceId);
        foreach (ref var s in Statuses.AsSpan())
            if (s.ID == sid && s.SourceID == source)
                return s;
        return null;
    }

    public ActorStatus? FindStatus<SID>(SID sid, DateTime? expirationForPending = null) where SID : Enum => FindStatus((uint)(object)sid, expirationForPending);
    public ActorStatus? FindStatus<SID>(SID sid, ulong source, DateTime? expirationForPending = null) where SID : Enum => FindStatus((uint)(object)sid, source, expirationForPending);

    public WDir DirectionTo(WPos other) => (other - Position).Normalized();
    public WDir DirectionTo(Actor other) => DirectionTo(other.Position);
    public Angle AngleTo(Actor other) => Angle.FromDirection(other.Position - Position);

    public float DistanceToHitbox(Actor? other) => other == null ? float.MaxValue : (other.Position - Position).Length() - other.HitboxRadius - HitboxRadius;
    public float DistanceToPoint(WPos pos) => (pos - Position).Length();

    public override string ToString() => $"{OID:X} '{Name}' <{InstanceID:X}>";
}
