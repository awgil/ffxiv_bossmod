namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE205CursedResurgence;

public enum OID : uint
{
    ClaretDragonHelper = 0x233C, // R0.500, x19, Helper type
    Actor1ec094 = 0x1EC094, // R0.500, x4, EventObj type
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    Actor1ec096 = 0x1EC096, // R0.500, x1, EventObj type
    Actor1ec093 = 0x1EC093, // R0.500, x1, EventObj type
    ClaretDragon = 0x4C46, // R5.000, x1
    ClaretDragon2 = 0x4D25, // R1.000, x1
    Necrohaze = 0x4C47, // R1.500, x0 (spawn during fight)
    Actor1ec095 = 0x1EC095, // R0.500, x0 (spawn during fight), EventObj type
    AetherialWard = 0x4C48, // R7.000, x0 (spawn during fight)
}

public enum AID : uint
{
    UnknownAbility = 48279, // ClaretDragon2->self, no cast, ???
    AutoAttack = 48259, // ClaretDragon->player, no cast, single-target
    HowlingDarkness = 48277, // ClaretDragon->self, 5.0s cast, single-target
    HowlingDarkness1 = 48278, // ClaretDragonHelper->self, no cast, ???
    SnakingNecrobreath = 48260, // ClaretDragon->self, 6.0s cast, range 60 270.000-degree cone
    GraveMold = 48261, // ClaretDragon->self, 5.0s cast, single-target
    GraveMold1 = 48262, // ClaretDragonHelper->self, 6.0s cast, range 8 circle
    Necrohaze1 = 48263, // Necrohaze->self, no cast, range 5 circle
    Soar = 50488, // ClaretDragon->self, 4.0s cast, single-target
    UnknownAbility2 = 48302, // ClaretDragon->self, no cast, single-target
    Cauterize = 48264, // ClaretDragon->self, 6.0s cast, single-target
    Cauterize1 = 48265, // ClaretDragonHelper->self, 7.0s cast, range 40 width 10 rect
    Catching = 48267, // Necrohaze->self, no cast, range 30 width 10 rect
    UnknownWeaponskill = 48266, // ClaretDragon->self, no cast, single-target
    AetherialWard = 48271, // ClaretDragon->self, 4.0+0.5s cast, single-target
    Necrohaze2 = 50484, // ClaretDragonHelper->self, 4.0s cast, range 5 circle
    UnknownAbility3 = 48275, // ClaretDragon->self, no cast, single-target
    Necrohaze3 = 48269, // ClaretDragonHelper->self, no cast, range 5 circle
    Necrohaze4 = 48268, // ClaretDragonHelper->location, no cast, range 5 circle
    UnknownAbility4 = 48276, // ClaretDragon->self, no cast, single-target
    BreathInThrees = 48270, // ClaretDragon->self, 5.0s cast, range 60 120.000-degree cone
    BreathInThrees1 = 48248, // ClaretDragon->self, 2.5s cast, range 60 120.000-degree cone
}

public enum SID : uint
{
    GradualZombification = 5059, // Necrohaze/ClaretDragonHelper->player, extra=0x1
    ZombieProof = 5138, // Necrohaze/ClaretDragonHelper->player, extra=0x0
    VulnerabilityUp = 2347, // Necrohaze/ClaretDragonHelper/ClaretDragon->player, extra=0x1/0x2/0x3
    Zombification = 2305, // Necrohaze/ClaretDragonHelper->player, extra=0x0
    UnknownStatus = 2056, // ClaretDragon->ClaretDragon, extra=0x164
    Heavy = 1796, // none->Necrohaze, extra=0x32
    DirectionalInvincibility = 1125, // none->AetherialWard, extra=0x0
    CacheMeIfYouCan = 1531, // none->player, extra=0x0
}

sealed class HowlingDarkness(BossModule module) : Components.RaidwideCast(module, (uint)AID.HowlingDarkness);
sealed class SnakingNecrobreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SnakingNecrobreath, new AOEShapeCone(60f, 135f.Degrees()));
sealed class GraveMold(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GraveMold, 8f);
sealed class Cauterize(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Cauterize, new AOEShapeRect(40f, 5f));
sealed class Catching(BossModule module) : Components.GenericAOEs(module)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return [];
    }
}
sealed class BreathInThrees(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BreathInThrees, (uint)AID.BreathInThrees1], new AOEShapeCone(60f, 60f.Degrees()));

[SkipLocalsInit]
sealed class CE205CursedResurgenceStates : StateMachineBuilder
{
    public CE205CursedResurgenceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(CE205CursedResurgenceStates),
    ConfigType = null, // replace null with typeof(CursedResurgenceConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.ClaretDragon,
    Contributors = "The Combat Reborn Team (LTS)",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 53u,
    SortOrder = 5,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE205CursedResurgence(WorldState ws, Actor primary) : BossModule(ws, primary, new(-688f, 150f), new ArenaBoundsSquare(20f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InSquare(Arena.Center, 20f);
}
