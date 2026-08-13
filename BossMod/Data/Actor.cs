namespace BossMod;

// objkind << 8 + objsubkind
public enum ActorType : ushort
{
    None = 0,
    Player = 0x104,
    Part = 0x201,
    Pet = 0x202,
    Chocobo = 0x203,
    Enemy = 0x205,
    Buddy = 0x209,
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

public sealed class ActorCastInfo
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

    public ActorCastInfo Clone() => (ActorCastInfo)MemberwiseClone();
}

// note: 'main' target could be completely different and unrelated to actual affected targets
public sealed class ActorCastEvent(ActionID action, ulong mainTargetID, float animationLockTime, uint maxTargets, Vector3 targetPos, uint globalSequence, uint sourceSequence, Angle rotation)
{
    public readonly ActionID Action = action;
    public readonly ulong MainTargetID = mainTargetID;
    public readonly float AnimationLockTime = animationLockTime;
    public readonly uint MaxTargets = maxTargets;
    public readonly Vector3 TargetPos = targetPos;
    public readonly uint GlobalSequence = globalSequence;
    public readonly uint SourceSequence = sourceSequence;
    public readonly Angle Rotation = rotation;

    public readonly struct Target(ulong id, ActionEffects effects)
    {
        public readonly ulong ID = id;
        public readonly ActionEffects Effects = effects;
    }

    public readonly List<Target> Targets = [];

    public WPos TargetXZ => new(TargetPos.XZ());

    public bool IsSpell() => Action.Type == ActionType.Spell;
    public bool IsSpell<AID>(AID aid) where AID : Enum => Action == ActionID.MakeSpell(aid);
}

public struct ActorHPMP(uint curHP, uint maxHP, uint shield, uint curMP, uint maxMP)
{
    public uint CurHP = curHP;
    public uint MaxHP = maxHP;
    public uint Shield = shield;
    public uint CurMP = curMP;
    public uint MaxMP = maxMP;

    public static bool operator ==(ActorHPMP left, ActorHPMP right) => left.CurHP == right.CurHP && left.MaxHP == right.MaxHP && left.Shield == right.Shield && left.CurMP == right.CurMP && left.MaxMP == right.MaxMP;
    public static bool operator !=(ActorHPMP left, ActorHPMP right) => left.CurHP != right.CurHP || left.MaxHP != right.MaxHP || left.Shield != right.Shield || left.CurMP != right.CurMP || left.MaxMP != right.MaxMP;

    public readonly bool Equals(ActorHPMP other) => this == other;
    public override readonly bool Equals(object? obj) => obj is ActorHPMP other && Equals(other);
    public override readonly int GetHashCode() => (CurHP, MaxHP, Shield, CurMP, MaxMP).GetHashCode();
}

// note on tethers - it is N:1 type of relation, actor can be tethered to 0 or 1 actors, but can itself have multiple actors tethering themselves to itself
// target is an instance id
public readonly struct ActorTetherInfo(uint id, ulong target)
{
    public readonly uint ID = id;
    public readonly ulong Target = target;
}

public struct ActorStatus(uint id, ushort extra, DateTime expireAt, ulong sourceID)
{
    public readonly uint ID = id;
    public readonly ushort Extra = extra;
    public DateTime ExpireAt = expireAt;
    public readonly ulong SourceID = sourceID;
}

public readonly struct ActorModelState(byte modelState, byte animState1, byte animState2)
{
    public readonly byte ModelState = modelState;
    public readonly byte AnimState1 = animState1;
    public readonly byte AnimState2 = animState2;

    public static bool operator ==(ActorModelState left, ActorModelState right) => left.ModelState == right.ModelState && left.AnimState1 == right.AnimState1 && left.AnimState2 == right.AnimState2;
    public static bool operator !=(ActorModelState left, ActorModelState right) => left.ModelState != right.ModelState || left.AnimState1 != right.AnimState1 || left.AnimState2 != right.AnimState2;

    public readonly bool Equals(ActorModelState other) => this == other;
    public override readonly bool Equals(object? obj) => obj is ActorModelState other && Equals(other);
    public override readonly int GetHashCode() => (ModelState, AnimState1, AnimState2).GetHashCode();

    public override string ToString() => $"ModelState: {ModelState}, AnimState1: {AnimState1}, AnimState2: {AnimState2}";
}

public readonly struct ActorForayInfo(byte level, byte element)
{
    public readonly byte Level = level;
    public readonly byte Element = element;

    public static bool operator ==(ActorForayInfo left, ActorForayInfo right) => left.Level == right.Level && left.Element == right.Element;
    public static bool operator !=(ActorForayInfo left, ActorForayInfo right) => left.Level != right.Level || left.Element != right.Element;

    public readonly bool Equals(ActorForayInfo other) => this == other;
    public override readonly bool Equals(object? obj) => obj is ActorForayInfo other && Equals(other);
    public override readonly int GetHashCode() => (Level, Element).GetHashCode();
}

public readonly struct ActorIncomingEffect(uint globalSequence, int targetIndex, ulong sourceInstanceID, ActionID action, ActionEffects effects)
{
    public readonly uint GlobalSequence = globalSequence;
    public readonly int TargetIndex = targetIndex;
    public readonly ulong SourceInstanceID = sourceInstanceID;
    public readonly ActionID Action = action;
    public readonly ActionEffects Effects = effects;
}

public readonly struct PendingEffect(uint globalSequence, int targetIndex, ulong sourceInstanceID, DateTime expiration, bool requiresEffectResult)
{
    public readonly uint GlobalSequence = globalSequence;
    public readonly int TargetIndex = targetIndex;
    public readonly ulong SourceInstanceID = sourceInstanceID;
    public readonly DateTime Expiration = expiration;
    public readonly bool RequiresEffectResult = requiresEffectResult;
}

public readonly struct PendingEffectDelta(PendingEffect effect, int value)
{
    public readonly PendingEffect Effect = effect;
    public readonly int Value = value;
}

public readonly struct PendingEffectStatus(PendingEffect effect, uint statusId)
{
    public readonly PendingEffect Effect = effect;
    public readonly uint StatusId = statusId;
}

public readonly struct PendingEffectStatusExtra(PendingEffect effect, uint statusId, byte extraLo)
{
    public readonly PendingEffect Effect = effect;
    public readonly uint StatusId = statusId;
    public readonly byte ExtraLo = extraLo;
}

public enum Visibility
{
    Unknown, // raycasting is disabled, or target is outside render distance
    Visible,
    Blocked
}

public sealed class Actor(ulong instanceID, uint oid, int spawnIndex, uint layoutID, string name, uint nameID, ActorType type, Class classID, int level, Vector4 posRot, float hitboxRadius = 1f, ActorHPMP hpmp = default, bool targetable = true, bool ally = false, ulong ownerID = default, uint fateID = default, int renderflags = 0)
{
    public ulong InstanceID = instanceID; // 'uuid'
    public uint OID = oid;
    public int SpawnIndex = spawnIndex; // [0-200) = character (even for normal, odd for dependents like mounts), [200-246) = client-side, [246, 286) = event object, [286, 426) = ???, [426-526) = ???, [526,596) = ???
    public uint LayoutID = layoutID;
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
    public Visibility Visibility = Visibility.Unknown;
    public bool IsDead;
    public bool InCombat;
    public bool AggroPlayer; // determines whether a given actor shows in the player's UI enemy list
    public bool IsOpenTreasure;
    public ActorModelState ModelState;
    public ActorForayInfo ForayInfo;
    public byte EventState; // not sure about the field meaning...
    public ulong OwnerID = ownerID; // uuid of owner, for pets and similar
    public ulong TargetID;
    public uint MountId; // ID of current mount, 0 if not mounted
    public int Renderflags = renderflags; // renderflags = 0 means visible, higher values seem to be invisible actors
    public ActorCastInfo? CastInfo;
    public ActorTetherInfo Tether;
    public const int NumStatuses = 60;
    public ActorStatus[] Statuses = new ActorStatus[NumStatuses]; // empty slots have ID=0
    public const int NumIncomingEffects = 32;
    public ActorIncomingEffect[] IncomingEffects = new ActorIncomingEffect[NumIncomingEffects];

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
    public Vector4 LastFrameMovementVec4 => PosRot - PrevPosRot;
    public Angle Rotation => PosRot.W.Radians();
    public bool Omnidirectional
    {
        get;
        set => field = value || Utils.CharacterIsOmnidirectional(OID);
    } = Utils.CharacterIsOmnidirectional(oid);

    public bool IsDeadOrDestroyed => IsDead || IsDestroyed;

    private static readonly HashSet<uint> ignoreNPC = [0xE19, 0xE18, 0xE1A, 0x2C11, 0x2C0F, 0x2C10, 0x2C0E, 0x2C12, 0x2EFE, 0x418F, 0x464E,
    0x4697, 0x35BC, 0x3657, 0x3658, 0x2ED7, 0x2EDB, 0x2EDA, 0x2E92, 0x2ECA, 0x2E94, 0x2E96, 0x2E90, 0x3265, 0x3264, 0x31A8, 0x391B,
    0x3EFA]; // friendly NPCs that should not count as party members
    public bool IsFriendlyNPC => Type == ActorType.Enemy && IsAlly && IsTargetable && !ignoreNPC.Contains(OID);
    public bool IsStrikingDummy => NameID == 541u; // this is a hack, but striking dummies are special in some ways
    public int CharacterSpawnIndex => SpawnIndex < 200 && (SpawnIndex & 1) == 0 ? (SpawnIndex >> 1) : -1; // [0,100) for 'real' characters, -1 otherwise
    public float HPRatio => (float)HPMP.CurHP / HPMP.MaxHP;
    public int PendingHPDifference
    {
        get
        {
            var sum = 0;
            var count = PendingHPDifferences.Count;
            for (var i = 0; i < count; ++i)
            {
                sum += PendingHPDifferences[i].Value;
            }
            return sum;
        }
    }

    public int PendingMPDifference
    {
        get
        {
            var sum = 0;
            var count = PendingMPDifferences.Count;
            for (var i = 0; i < count; ++i)
            {
                sum += PendingMPDifferences[i].Value;
            }
            return sum;
        }
    }
    public int PendingHPRaw => (int)HPMP.CurHP + PendingHPDifference;
    public int PendingMPRaw => (int)HPMP.CurMP + PendingMPDifference;
    public int PendingHPClamped => Math.Clamp(PendingHPRaw, 0, (int)HPMP.MaxHP);
    public bool PendingDead => PendingHPRaw <= 1 && !IsStrikingDummy;
    public float PendingHPRatio => (float)PendingHPRaw / HPMP.MaxHP;

    // if expirationForPredicted is not null, search pending first, and return one if found; in that case only low byte of extra will be set
    public ActorStatus? FindStatus(uint sid, DateTime? expirationForPending = null)
    {
        var sid_ = sid;
        if (expirationForPending != null)
        {
            var statusesP = CollectionsMarshal.AsSpan(PendingStatuses);
            var lenP = statusesP.Length;
            for (var i = 0; i < lenP; ++i)
            {
                ref var s = ref statusesP[i];
                if (s.StatusId == sid_)
                {
                    return new(sid, s.ExtraLo, expirationForPending.Value, s.Effect.SourceInstanceID);
                }
            }
        }
        var len = Statuses.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var s = ref Statuses[i];
            if (s.ID == sid_)
            {
                return s;
            }
        }
        return null;
    }

    public ActorStatus? FindStatus(uint sid, ulong source, DateTime? expirationForPending = null)
    {
        var sid_ = sid;
        if (expirationForPending != null)
        {
            var statusesP = CollectionsMarshal.AsSpan(PendingStatuses);
            var lenP = statusesP.Length;
            for (var i = 0; i < lenP; ++i)
            {
                ref var s = ref statusesP[i];
                if (s.StatusId == sid_ && s.Effect.SourceInstanceID == source)
                {
                    return new(sid_, s.ExtraLo, expirationForPending.Value, s.Effect.SourceInstanceID);
                }
            }
        }
        var len = Statuses.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var s = ref Statuses[i];
            if (s.ID == sid_ && s.SourceID == source)
            {
                return s;
            }
        }
        return null;
    }

    public ActorStatus? FindStatus<SID>(SID sid, DateTime? expirationForPending = null) where SID : Enum => FindStatus((uint)(object)sid, expirationForPending);

    public WDir DirectionTo(WPos other) => (other - Position).Normalized();
    public WDir DirectionTo(Actor other) => DirectionTo(other.Position);
    public Angle AngleTo(Actor other) => Angle.FromDirection(other.Position - Position);
    public Angle AngleTo(WPos other) => Angle.FromDirection(other - Position);

    public float DistanceToHitbox(Actor? other) => other == null ? float.MaxValue : (other.Position - Position).Length() - other.HitboxRadius - HitboxRadius;
    public float DistanceToPoint(WPos pos) => (pos - Position).Length();

    public override string ToString() => $"{OID:X} '{Name}' <{InstanceID:X}>";
}

public static class VisibilityExtensions
{
    extension(Visibility v)
    {
        public char Encode() => v switch
        {
            Visibility.Visible => 'V',
            Visibility.Blocked => 'B',
            _ => 'U'
        };

        public static Visibility Decode(char c) => c switch
        {
            'V' => Visibility.Visible,
            'B' => Visibility.Blocked,
            _ => Visibility.Unknown
        };
    }
}
