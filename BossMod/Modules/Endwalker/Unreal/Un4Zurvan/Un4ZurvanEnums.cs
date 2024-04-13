namespace BossMod.Endwalker.Unreal.Un4Zurvan;

public enum OID : uint
{
    BossP1 = 0x3F66, // R7.440, x1
    BossP2 = 0x3F67, // R5.270, x1
    SoarHelper = 0x3F68, // R5.270, x3
    Helper = 0x3F6D, // R0.500, x10
    ExecratedWitHelper = 0x3F93, // R0.500, x5
    ExecratedWill = 0x3F69, // R3.600, spawn during fight
    ExecratedWit = 0x3F6A, // R3.000, spawn during fight
    ExecratedWile = 0x3F6B, // R3.000, spawn during fight
    ExecratedThew = 0x3F6C, // R2.100, spawn during fight
    //_Gen_Actorf8fd0 = 0xF8FD0, // R0.500-3.000, x1, EventNpc type
    //_Gen_Actorf8fcf = 0xF8FCF, // R0.500-3.000, x1, EventNpc type
    //_Gen_Actorf8fce = 0xF8FCE, // R0.500-3.000, x1, EventNpc type
    //_Gen_Actorf8fd1 = 0xF8FD1, // R0.500-3.000, x1, EventNpc type
    PlatformE = 0x1EA28A, // R2.000, x1, EventObj type
    PlatformN = 0x1EA28B, // R2.000, x1, EventObj type
    PlatformW = 0x1EA28C, // R2.000, x1, EventObj type
    //_Gen_Actor1ea28d = 0x1EA28D, // R2.000, x1, EventObj type
    //_Gen_Actor1ea28e = 0x1EA28E, // R2.000, x1, EventObj type
    //_Gen_Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type
    //_Gen_Actor1ea289 = 0x1EA289, // R2.000, x1, EventObj type
    FlamingHalberdVoidzone = 0x1EA2A4, // R0.500, EventObj type, spawn during fight
    IcyVoidzone = 0x1EA2A5, // R0.500, EventObj type, spawn during fight
    SouthernCrossVoidzone = 0x1EA2A6, // R0.500, EventObj type, spawn during fight
    FireTower = 0x1EA2A7, // R0.500, EventObj type, spawn during fight
    IceTower = 0x1EA2A8, // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttackBossP2 = 872, // BossP2->player, no cast, single-target
    AutoAttackAdds = 6498, // ExecratedThew/ExecratedWill->player, no cast, single-target
    MetalCutterP1 = 34175, // BossP1->self, no cast, range 30+R 90-degree cone
    FlareStar = 34135, // BossP1->self, 3.0s cast, single-target, visual (puddles)
    FlareStarAOE = 34136, // Helper->location, 3.6s cast, range 6 circle
    Purge = 34137, // BossP1->self, no cast, range 100 circle, raidwide
    Eidos1 = 34138, // BossP2->self, no cast, single-target, phase change
    MetalCutterP2 = 34169, // BossP2->self, no cast, range 30+R 90-degree cone
    Soar = 34174, // BossP2->self, 5.0s cast, single-target, visual (mechanic start)
    TwinSpiritFirst = 34139, // SoarHelper->location, 5.0s cast, width 10 rect charge
    TwinSpiritSecond = 34140, // SoarHelper->location, 1.0s cast, width 10 rect charge
    FlamingHalberd = 34141, // Helper->location, no cast, range 12 circle spread
    DemonicDive = 34142, // BossP2->location, no cast, range 7 circle stack
    CoolFlame = 34143, // Helper->location, no cast, range 8 circle spread
    DemonsClaw = 34170, // BossP2->player, 3.0s cast, single-target, knockback 17
    WaveCannonShared = 34171, // BossP2->self, 5.0s cast, range 50+R width 10 rect
    Eidos2 = 34144, // BossP2->self, no cast, single-target, phase change
    IceAndFire = 34145, // BossP2->self, no cast, single-target, visual (spawns icy voidzone)
    BitingHalberd = 34146, // BossP2->self, 5.0s cast, range 50+R 270-degree cone
    TailEnd = 34147, // BossP2->self, 5.0s cast, range 15 circle
    Ciclicle = 34148, // BossP2->self, 5.0s cast, range ?-20 donut
    SouthernCross = 34149, // BossP2->self, 3.0s cast, single-target, visual (baited puddles)
    SouthernCrossAOE = 34150, // Helper->location, 3.5s cast, range 6 circle
    AddPhaseStart = 34151, // BossP2->self, no cast, single-target, visual (add phase start)
    HardThrust = 34154, // ExecratedWill->player, no cast, single-target, mini tankbuster
    Berserk = 34155, // ExecratedWill->self, no cast, single-target, damage up stack
    MeracydianMeteor = 34156, // ExecratedWit->location, 20.0s cast, range 100 circle
    Comet = 34157, // ExecratedWitHelper->location, 2.0s cast, range 4 circle
    MeracydianFire = 34158, // ExecratedWile->player, no cast, single-target, autoattack
    MeracydianFear = 34159, // ExecratedWile->self, 5.0s cast, range 100 circle, gaze
    Eidos3 = 34152, // BossP2->self, no cast, single-target, phase change
    AhuraMazda = 34153, // BossP2->self, no cast, range 100 circle, raidwide
    InfiniteFire = 34160, // Helper->player, no cast, single-target, apply color
    InfiniteIce = 34161, // Helper->player, no cast, single-target, apply color
    WaveCannonSolo = 34172, // BossP2->self/players, 5.0s cast, range 50+R width 10 rect
    Tyrfing = 34166, // BossP2->player, 3.0s cast, single-target, first hit
    TyrfingAOE = 34167, // BossP2->player, no cast, single-target, remaining hits
    TyrfingFire = 34168, // BossP2->player, no cast, range 5 circle, tyrfing finisher
    BrokenSeal = 34173, // BossP2->self, 3.0s cast, single-target, visual
    SouthStar = 34162, // Helper->self, no cast, range 2 circle (fire tower)
    NorthStar = 34163, // Helper->self, no cast, range 2 circle (ice tower)
    SouthStarUnsoaked = 34164, // Helper->self, no cast, range 100 circle (aoe if fire tower is not soaked)
    NorthStarUnsoaked = 34165, // Helper->self, no cast, range 100 circle (aoe if ice tower is not soaked)
    SouthStarWrong = 34178, // Helper->self, no cast, range 2 circle (oneshot if fire tower is soaked by wrong debuff)
    NorthStarWrong = 34179, // Helper->self, no cast, range 2 circle (oneshot if fire tower is soaked by wrong debuff)
    Enrage = 34177, // BossP2->self, 10.0s cast, single-target, visual (enrage)
    EnrageAOE = 34176, // Helper->location, no cast, range 12 circle, enrage
}

public enum IconID : uint
{
    FlamingHalberd = 44, // player
    DemonicDive = 62, // player
    CoolFlame = 23, // player
    WaveCannon = 14, // player
}

public enum TetherID : uint
{
    InfiniteAnguish = 1, // player->player (tether stretched too far, > ~12)
    InfiniteFire = 5, // player->player
    InfiniteIce = 8, // player->player
}
