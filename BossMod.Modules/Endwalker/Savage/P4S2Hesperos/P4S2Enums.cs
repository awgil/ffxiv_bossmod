namespace BossMod.Endwalker.Savage.P4S2Hesperos;

public enum OID : uint
{
    Boss = 0x3600, // second phase boss (exists from start, but recreated on checkpoint)
    Akantha = 0x3601, // ?? 'akantha', 12 exist at start
    Helper = 0x233C, // 38 exist at start
}

public enum AID : uint
{
    AkanthaiAct1 = 27148, // Boss->Boss
    AkanthaiExplodeAOE = 27149, // Helper->Helper
    AkanthaiExplodeTower = 27150, // Helper->Helper
    AkanthaiExplodeKnockback = 27152, // Helper->Helper
    AkanthaiVisualTower = 27153, // Akantha->Akantha
    AkanthaiVisualAOE = 27154, // Akantha->Akantha
    AkanthaiVisualKnockback = 27155, // Akantha->Akantha
    AkanthaiWaterBreakAOE = 27156, // Helper->targets, no cast
    AkanthaiDarkBreakAOE = 27158, // Helper->targets, no cast
    AkanthaiFireBreakAOE = 27160, // Helper->targets, no cast
    AkanthaiWindBreakAOE = 27162, // Helper->targets, no cast
    FleetingImpulseAOE = 27164, // Helper->target, no cast
    HellsSting = 27166, // Boss->Boss
    HellsStingSecond = 27167, // Boss->Boss, no cast
    HellsStingAOE1 = 27168, // Helper->Helper
    HellsStingAOE2 = 27169, // Helper->Helper
    KothornosKock = 27170, // Boss->Boss
    KothornosKickJump = 27171, // Boss->target, no cast
    KothornosQuake2 = 27172, // Boss->Boss, no cast
    KothornosQuakeAOE = 27173, // Helper->target, no cast
    Nearsight = 27174, // Boss->Boss
    Farsight = 27175, // Boss->Boss
    NearsightAOE = 27176, // Helper->target, no cast
    DarkDesign = 27177, // Boss->Boss
    DarkDesignAOE = 27178, // Helper->location
    HeartStake = 27179, // Boss->target
    UltimateImpulse = 27180, // Boss->Boss
    SearingStream = 27181, // Boss->Boss
    WreathOfThorns1 = 27183, // Boss->Boss
    WreathOfThorns2 = 27184, // Boss->Boss
    WreathOfThorns3 = 27185, // Boss->Boss
    WreathOfThorns4 = 27186, // Boss->Boss
    WreathOfThorns5 = 27188, // Boss->Boss
    WreathOfThorns6 = 27189, // Boss->Boss
    AkanthaiCurtainCall = 27190, // Boss->Boss
    Enrage = 27191, // Boss->Boss
    FarsightAOE = 28123, // Helper->target, no cast
    KothornosQuake1 = 28276, // Boss->Boss, no cast
    HeartStakeSecond = 28279, // Boss->target, no cast
    DemigodDouble = 28280, // Boss->target
    AkanthaiAct2 = 28340, // Boss->Boss
    AkanthaiAct3 = 28341, // Boss->Boss
    AkanthaiAct4 = 28342, // Boss->Boss
    AkanthaiFinale = 28343, // Boss->Boss
    FleetingImpulse = 28344, // Boss->Boss
}

public enum SID : uint
{
    Thornpricked = 2804,
}

public enum TetherID : uint
{
    WreathOfThornsPairsClose = 172,
    WreathOfThorns = 173, // also used when pairs are about to break
}

public enum IconID : uint
{
    None = 0,
    AkanthaiWater = 300, // act 4
    AkanthaiDark = 301, // acts 2 & 4 & 6
    AkanthaiWind = 302, // acts 2 & 5
    AkanthaiFire = 303, // act 2
}
