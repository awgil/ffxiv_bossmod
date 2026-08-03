namespace BossMod.RealmReborn.Dungeon.D17PharosSirius.D173Siren;

public enum OID : uint
{
    Siren = 0x8EF,
    Helper = 0x233C,
    ZombieStormPrivate = 0x8F0,
    ZombieStormSergeant = 0x8F1
}

public enum AID : uint
{
    AutoAttack = 1482, // Siren->player, no cast, range 7+R ?-degree cone
    DeathlyVerse = 1483, // Siren->player, 1.0s cast, single-target
    SongOfTorment = 1486, // Siren->player, 1.5s cast, single-target
    _AutoAttack1 = 872, // ZombieStormPrivate->player, no cast, single-target
    DeathlyCadenza = 1487, // Siren->self, 3.0s cast, range 50+R circle
    FeralLunge = 1484, // Siren->self, 3.0s cast, range 50+R width 12 rect
    LunaticVoice = 1485, // Siren->self, 4.0s cast, range 50+R circle
    Wallop = 1658, // ZombieStormPrivate->self, 2.5s cast, range 3+R width 3 rect
    DeathThroes = 1539, // ZombieStormSergeant->player, no cast, single-target
    Zombify = 1675, // 1B2->player, no cast, single-target : Maybe this is when player receives confused status?
}

public enum SID : uint
{
    SirenSong = 370, // Siren->player, extra=0x0
    Confused = 11, // 1B2->player, extra=0x0
    Bleeding = 273, // Siren->player, extra=0x0
    DeathThroes = 378, // 8F1->player, extra=0x0
}

// Cleave angle is an estimate.
sealed class AutoCleave(BossModule module)
    : Components.Cleave(module, (uint)AID.AutoAttack, new AOEShapeCone(7f, 65f.Degrees()), [(uint)OID.Siren]);

// Donut aoe. Stand in center to to avoid 'siren song' debuff.
sealed class DeathlyCadenza(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.DeathlyCadenza, new AOEShapeDonut(4f, 30f));

sealed class FeralLunge(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.FeralLunge, new AOEShapeRect(50f, 6f));

sealed class DeathlyVerse(BossModule module) : Components.CastHint(module, (uint)AID.DeathlyVerse,
    "Heal Siren Song Status to full or player will get confuse status.");

sealed class SongOfTorment(BossModule module)
    : Components.CastHint(module, (uint)AID.SongOfTorment, "Esuna bleeding debuff from tank");

sealed class LunaticVoice(BossModule module) : Components.RaidwideCast(module, (uint)AID.LunaticVoice,
    "Reduced Immunity status can be removed with Esuna");

sealed class ZombiePrivate(BossModule module) : Components.Adds(module, (uint)OID.ZombieStormPrivate);

sealed class Wallop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Wallop, new AOEShapeRect(3f, 1.5f));

sealed class ZombieSergeant(BossModule module) : Components.Adds(module, (uint)OID.ZombieStormSergeant);

[SkipLocalsInit]
sealed class SirenStates : StateMachineBuilder
{
    public SirenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoCleave>()
            .ActivateOnEnter<DeathlyCadenza>()
            .ActivateOnEnter<FeralLunge>()
            .ActivateOnEnter<DeathlyVerse>()
            .ActivateOnEnter<SongOfTorment>()
            .ActivateOnEnter<LunaticVoice>()
            .ActivateOnEnter<ZombiePrivate>()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<ZombieSergeant>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(SirenStates),
    ConfigType = null, // replace null with typeof(SirenConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Siren,
    Contributors = "wen",
    Expansion = BossModuleInfo.Expansion.RealmReborn,
    Category = BossModuleInfo.Category.Dungeon,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 17u,
    NameID = 2265u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Siren(WorldState ws, Actor primary)
    : BossModule(ws, primary, new(0f, 0f), new ArenaBoundsCircle(24f));
