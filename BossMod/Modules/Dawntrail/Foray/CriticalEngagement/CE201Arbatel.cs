namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE201Arbatel;

public enum OID : uint
{
    Arbatel = 0x4BD3,
    Helper = 0x233C,
    Page8 = 0x4BD6, // R1.5
    Page16 = 0x4BD5, // R3.22
    Page64 = 0x4BD4, // R2.4
    Page512 = 0x4BD7 // R1.95
}

public enum AID : uint
{
    AutoAttack = 49056, // Arbatel->player, no cast, single-target

    Teleport = 48246, // Arbatel->location, no cast, single-target
    MarginaliaCast = 47327, // Helper->self, 5.0s cast, ???
    Marginalia = 47328, // Arbatel->self, 5.0s cast, single-target
    CoverToCoverForward = 47302, // Arbatel->self, 4.0s cast, range 30 180-degree cone
    CoverToCoverBackwards = 47303, // Arbatel->self, 1.0s cast, range 30 180-degree cone
    UnboundInk = 49492, // Arbatel->self, 4.0s cast, range 9 circle
    BookDropCast = 47319, // Arbatel->self, 3.0s cast, single-target

    BookDrop = 47322, // 4BD8->self, 8.0s cast, range 3 circle, tower
    BigBurst = 47323, // Helper->self, 1.0s cast, ???, tower fail

    ThunderII = 47324, // Helper->self, 4.0s cast, range 50 width 5 rect
    FireIICast = 47326, // Arbatel->self, 4.5+0.5s cast, ???
    FireII = 47325, // Helper->self, 5.0s cast, range 60 45-degree cone
    ArcaneRule = 47304, // Arbatel->self, 6.0s cast, single-target
    QuadRule = 47305, // Helper->self, 6.0s cast, range 25 width 10 cross
    HorizontalRule = 47306, // Helper->self, 2.0s cast, range 50 width 6 rect
    BlotCast = 47300, // Arbatel->self, 3.0s cast, ???
    Blot = 47301, // Helper->location, 8.0s cast, range 15 circle

    SummonCast = 49055, // Arbatel->self, 3.0s cast, ???
    Summon = 47307, // Helper->location, 3.0s cast, range 4 circle
    KnowledgeLevelCorrectionCast = 47296, // Arbatel->self, 5.0s cast, ???
    KnowledgeLevelCorrection = 47297, // Helper->self, no cast, ???

    // 2 casters to circumvent 32 player aoe limit
    PrimeKnowledgeLevelDeathCast = 47318, // Page512->self, 11.0s cast, single-target
    PrimeKnowledgeLevelDeath180Cast1 = 50561, // Helper->self, 11.0s cast, range 25 180-degree cone
    PrimeKnowledgeLevelDeath180Cast2 = 49879, // Helper->self, 11.0s cast, range 25 180-degree cone
    PrimeKnowledgeLevelDeath120Cast1 = 50560, // Helper->self, 11.0s cast, range 25 120-degree cone
    PrimeKnowledgeLevelDeath120Cast2 = 47314, // Helper->self, 11.0s cast, range 25 120-degree cone

    KnowledgeLevel3FlareCast = 47316, // Page16->self, 11.0s cast, single-target
    KnowledgeLevel3Flare180Cast1 = 50555, // Helper->self, 11.0s cast, range 25 180-degree cone
    KnowledgeLevel3Flare180Cast2 = 47309, // Helper->self, 11.0s cast, range 25 180-degree cone
    KnowledgeLevel3Flare120Cast1 = 50558, // Helper->self, 11.0s cast, range 25 120-degree cone
    KnowledgeLevel3Flare120Cast2 = 47312, // Helper->self, 11.0s cast, range 25 120-degree cone

    KnowledgeLevel4HolyCast = 47317, // Page8->self, 11.0s cast, single-target
    KnowledgeLevel4Holy120Cast1 = 50559, // Helper->self, 11.0s cast, range 25 120-degree cone
    KnowledgeLevel4Holy120Cast2 = 47313, // Helper->self, 11.0s cast, range 25 120-degree cone
    KnowledgeLevel4Holy180Cast1 = 50556, // Helper->self, 11.0s cast, range 25 180-degree cone
    KnowledgeLevel4Holy180Cast2 = 47310, // Helper->self, 11.0s cast, range 25 180-degree cone

    KnowledgeLevel5DeathCast = 47315, // Page64->self, 11.0s cast, single-target
    KnowledgeLevel5Death120Cast1 = 50557, // Helper->self, 11.0s cast, range 25 120-degree cone
    KnowledgeLevel5Death120Cast2 = 47311, // Helper->self, 11.0s cast, range 25 120-degree cone
    KnowledgeLevel5Death180Cast1 = 50554, // Helper->self, 11.0s cast, range 25 180-degree cone
    KnowledgeLevel5Death180Cast2 = 47315 // Helper->self, 11.0s cast, range 25 180-degree cone
}

public enum SID : uint
{
    Invincibility = 4875, // none->Page8/Page64/Page512/4BD8/Page16, extra=0x0
    Correction1 = 5014, // none->player, extra=0x0
    Correction2 = 5015, // none->player, extra=0x0
    Correction3 = 5016, // none->player, extra=0x0
    Correction4 = 5017, // none->player, extra=0x0
    Correction5 = 5018 // none->player, extra=0x0
}

[SkipLocalsInit]
sealed class KnowledgeLevelCorrection(BossModule module) : Components.RaidwideCast(module, (uint)AID.KnowledgeLevelCorrectionCast);
[SkipLocalsInit]
sealed class Summon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Summon, 4f);
[SkipLocalsInit]
sealed class Marginalia(BossModule module) : Components.RaidwideCast(module, (uint)AID.Marginalia);
[SkipLocalsInit]
sealed class UnboundInk(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnboundInk, 9f);
[SkipLocalsInit]
sealed class BookDrop(BossModule module) : Components.CastTowersOpenWorld(module, (uint)AID.BookDrop, 3f, 3, 5);
[SkipLocalsInit]
sealed class ThunderII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThunderII, new AOEShapeRect(50f, 2.5f), 10);
[SkipLocalsInit]
sealed class FireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireII, new AOEShapeCone(60f, 22.5f.Degrees()));
[SkipLocalsInit]
sealed class QuadRule(BossModule module) : Components.SimpleAOEs(module, (uint)AID.QuadRule, new AOEShapeCross(25f, 5f));
[SkipLocalsInit]
sealed class HorizontalRule(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HorizontalRule, new AOEShapeRect(50f, 3f));
[SkipLocalsInit]
sealed class Blot : Components.SimpleAOEs
{
    public Blot(BossModule module) : base(module, (uint)AID.Blot, 15f, 6)
    {
        MaxDangerColor = 3;
    }
}
[SkipLocalsInit]
sealed class Invincibility(BossModule module) : Components.InvincibleStatus(module, (uint)SID.Invincibility);

[SkipLocalsInit]
sealed class CoverToCover(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    private readonly AOEShapeCone cone = new(30f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CoverToCoverForward)
        {
            AddAOE();
            AddAOE(180f.Degrees(), 4.2d);
        }
        void AddAOE(Angle offset = default, double delay = default)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            var pos = delay != default ? loc - 5f * rot.ToDirection() : loc;
            var rot2 = rot + offset;
            _aoes.Add(new(cone, pos, rot2, Module.CastFinishAt(spell, delay), shapeDistance: cone.Distance(pos, rot2)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count is var count && count != 0 && spell.Action.ID is (uint)AID.CoverToCoverForward or (uint)AID.CoverToCoverBackwards)
        {
            _aoes.RemoveAt(0);
            if (count == 2)
            {
                ref var aoe2 = ref _aoes.Ref(0);
                var rot = aoe2.Rotation;
                aoe2.Origin -= 5f * rot.ToDirection();
                aoe2.ShapeDistance = cone.Distance(aoe2.Origin, rot);
            }
        }
    }
}

[SkipLocalsInit]
sealed class KnowledgeLevel(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(3)];
    private readonly AOEShapeCone shapeHalf = new(25f, 90f.Degrees());
    private readonly AOEShapeCone shapeThird = new(25f, 60f.Degrees());
    private readonly Dictionary<ulong, uint> playerDebuffs = [];

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var level = status.ID switch
        {
            (uint)SID.Correction1 => 1u,
            (uint)SID.Correction2 => 2u,
            (uint)SID.Correction3 => 3u,
            (uint)SID.Correction4 => 4u,
            (uint)SID.Correction5 => 5u,
            _ => 0u
        };

        if (level == 0u)
        {
            return;
        }

        playerDebuffs[actor.InstanceID] = level;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        (AOEShape shape, ulong actorID)? data = spell.Action.ID switch
        {
            (uint)AID.PrimeKnowledgeLevelDeath180Cast1 => (shapeHalf, 1ul),
            (uint)AID.PrimeKnowledgeLevelDeath120Cast1 => (shapeThird, 1ul),
            (uint)AID.KnowledgeLevel3Flare180Cast1 => (shapeHalf, 3ul),
            (uint)AID.KnowledgeLevel3Flare120Cast1 => (shapeThird, 3ul),
            (uint)AID.KnowledgeLevel4Holy180Cast1 => (shapeHalf, 4ul),
            (uint)AID.KnowledgeLevel4Holy120Cast1 => (shapeThird, 4ul),
            (uint)AID.KnowledgeLevel5Death120Cast1 => (shapeThird, 5ul),
            (uint)AID.KnowledgeLevel5Death180Cast1 => (shapeHalf, 5ul),
            _ => null
        };

        if (data is (AOEShape, ulong) d)
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var loc = spell.LocXZ;
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].Origin == loc) // cast1 happens twice, while cast2 happens once but sometimes creates a fake cast in the wrong angle?
                {
                    return;
                }
            }
            var rot = spell.Rotation;
            var shape = d.shape;
            _aoes.Add(new(shape, loc, rot, Module.CastFinishAt(spell), actorID: d.actorID, shapeDistance: shape.Distance(loc, rot)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.PrimeKnowledgeLevelDeath180Cast2 or (uint)AID.PrimeKnowledgeLevelDeath120Cast2 or (uint)AID.KnowledgeLevel3Flare180Cast2 or
        (uint)AID.KnowledgeLevel3Flare120Cast2 or (uint)AID.KnowledgeLevel4Holy180Cast2 or (uint)AID.KnowledgeLevel4Holy120Cast2 or (uint)AID.KnowledgeLevel5Death120Cast2 or (uint)AID.KnowledgeLevel5Death180Cast2)
        {
            _aoes.Clear();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;
        if (len == 0)
        {
            return aoes;
        }

        playerDebuffs.TryGetValue(actor.InstanceID, out var playerdebuff);

        var playerActingLevel = playerdebuff + actor.ForayInfo.Level;
        var write = 0;

        for (var read = 0; read < len; ++read)
        {
            if (!IsRisky((uint)aoes[read].ActorID))
            {
                continue;
            }

            if (write != read)
            {
                (aoes[write], aoes[read]) = (aoes[read], aoes[write]);
            }

            ++write;
        }

        return aoes[..write];

        bool IsRisky(uint actorID)
        {
            var level = playerActingLevel;

            return actorID == 1u ? level.IsPrime() : level.IsDivisible(actorID);
        }
    }
}

[SkipLocalsInit]
sealed class ArbatelStates : StateMachineBuilder
{
    public ArbatelStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<KnowledgeLevelCorrection>()
            .ActivateOnEnter<Summon>()
            .ActivateOnEnter<KnowledgeLevel>()
            .ActivateOnEnter<Marginalia>()
            .ActivateOnEnter<CoverToCover>()
            .ActivateOnEnter<UnboundInk>()
            .ActivateOnEnter<BookDrop>()
            .ActivateOnEnter<ThunderII>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<QuadRule>()
            .ActivateOnEnter<HorizontalRule>()
            .ActivateOnEnter<Invincibility>()
            .ActivateOnEnter<Blot>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(ArbatelStates),
    ConfigType = null, // replace null with typeof(ArbatelConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Arbatel,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14520u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Arbatel(WorldState ws, Actor primary) : BossModule(ws, primary, new(658.991f, 658.991f), new ArenaBoundsCircle(25f));
