namespace BossMod.Endwalker.Variant.V02MR.V021Yozakura;

public enum OID : uint
{
    Boss = 0x3EDB, // R3.450, x1
    Helper = 0x233C, // R0.500, x13, 523 type
    Helper2 = 0x3F53, // R0.500, x4
    ShishuYamabiko = 0x3F00, // R0.800, x1
    MirroredYozakura = 0x3EDC, // R3.450, x4
    MudBubble = 0x3EDD, // R4.000, x0 (spawn during fight)
    Kuromaru = 0x3EDF, // R0.400, x1 (spawn during fight)
    Shiromaru = 0x3EE0, // R0.400, x1 (spawn during fight)
    Shibamaru = 0x3EDE, // R0.400, x1 (spawn during fight)
    AccursedSeedling = 0x3EE1, // R0.750, x0 (spawn during fight)
    AutumnalTempest = 0x3EE3, // R0.800, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    GloryNeverlasting = 33705, // Boss->player, 5.0s cast, single-target //tankbuster

    ArtOfTheFireblossom = 33640, // Boss->self, 3.0s cast, range 9 circle
    ArtOfTheWindblossom = 33641, // Boss->self, 5.0s cast, range 6-60 donut

    KugeRantsui = 33706, // Boss->self, 5.0s cast, range 60 circle //raidwide
    OkaRanman = 33646, // Boss->self, 5.0s cast, range 60 circle //raidwide

    SealOfRiotousBloom = 33652, // Boss->self, 5.0s cast, single-target

    SealOfTheWindblossom1 = 33654, // Helper->self, 2.0s cast, range 6-60 donut //Donut AoE centered on Yozakura
    SealOfTheWindblossom2 = 33659, // Boss->location, no cast, single-target

    SealOfTheFireblossom1 = 33653, // Helper->self, 2.0s cast, range 9 circle //Point-blank AoE centered on Yozakura
    SealOfTheFireblossom2 = 33658, // Boss->location, no cast, single-target

    SealOfTheRainblossom1 = 33655, // Helper->self, 2.0s cast, range 70 45-degree cone //Four cone AoEs from Yozakura's hitbox in the intercardinal directions
    SealOfTheRainblossom2 = 33661, // Boss->location, no cast, single-target

    SealOfTheLevinblossom1 = 33656, // Helper->self, 1.8s cast, range 70 45-degree cone //Four cone AoEs from Yozakura's hitbox in the cardinal directions
    SealOfTheLevinblossom2 = 33660, // Boss->location, no cast, single-target

    SealOfTheFleeting = 33657, // Boss->self, 3.0s cast, single-target //Yozakura tethers to the petal piles

    SeasonsOfTheFleeting1 = 33665, // Boss->self, 10.0s cast, single-target //telegraph four sequential AoEs

    SeasonsOfTheFleeting2 = 33666, // Boss->self, no cast, single-target
    FireAndWaterVisual = 33667, // Helper->self, 2.0s cast, range 46 width 5 rect
    EarthAndLightningVisual = 33668, // Helper->self, 2.0s cast, range 60 ?-degree cone

    SeasonOfFire = 33669, // Helper->self, 0.8s cast, range 46 width 5 rect
    SeasonOfWater = 33670, // Helper->self, 0.8s cast, range 46 width 5 rect
    SeasonOfLightning = 33671, // Helper->self, 0.8s cast, range 70 ?-degree cone
    SeasonOfEarth = 33672, // Helper->self, 0.8s cast, range 70 ?-degree cone

    UnknownAbility = 34322, // Boss->location, no cast, single-target

    //Left Windy
    WindblossomWhirl1 = 33679, // Boss->self, 3.0s cast, single-target
    WindblossomWhirl2 = 33680, // Helper->self, 5.0s cast, range ?-60 donut
    WindblossomWhirl3 = 34544, // Helper->self, 3.0s cast, range 5.5-60 donut
    LevinblossomStrike1 = 33681, // Boss->self, 2.3s cast, single-target
    LevinblossomStrike2 = 33682, // Helper->location, 3.0s cast, range 3 circle
    DriftingPetals = 33683, // Boss->self, 5.0s cast, range 60 circle //knockback

    //Left Rainy
    Bunshin = 33662, // Boss->self, 5.0s cast, single-target
    Shadowflight = 33663, // Boss->self, 3.0s cast, single-target
    ShadowflightAOE = 33664, // MirroredYozakura->self, 2.5s cast, range 10 width 6 rect
    Mudrain = 33673, // Boss->self, 3.0s cast, single-target
    MudrainAOE = 33674, // Helper->location, 3.8s cast, range 5 circle
    IcebloomFirst = 33675, // Boss->self, 3.0s cast, single-target
    IcebloomRest = 33676, // Helper->location, 3.0s cast, range 6 circle
    MudPie = 33677, // Boss->self, 3.0s cast, single-target
    MudPieAOE = 33678, // MudBubble->self, 4.0s cast, range 60 width 6 rect

    //Middle Rope Pulled
    ArtOfTheFluff1 = 33693, // Shibamaru/Kuromaru->self, 6.5s cast, range 60 circle //gaze
    ArtOfTheFluff2 = 33694, // Shiromaru->self, 6.5s cast, range 60 circle //gaze

    FireblossomFlare1 = 33695, // Boss->self, 3.0s cast, single-target
    FireblossomFlare2 = 33696, // Helper->location, 3.0s cast, range 6 circle

    DondenGaeshi = 33692, // Boss->self, 3.0s cast, single-target // Indicates platforms
    SilentWhistle = 33691, // Boss->self, 3.0s cast, single-target //Dog summons

    //Middle Rope Unpulled
    LevinblossomLance1 = 33687, // Boss->self, 5.0s cast, single-target
    LevinblossomLance2 = 33688, // Boss->self, 5.0s cast, single-target
    LevinblossomLanceFirst = 33689, // Helper->self, 5.8s cast, range 60 width 7 rect
    LevinblossomLanceRest = 33690, // Helper->self, no cast, range 60 width 7 rect

    TatamiTrap = 33684, // Boss->self, 3.0s cast, single-target
    TatamiGaeshi = 33685, // Boss->self, 3.0s cast, single-target
    TatamiGaeshiAOE = 33686, // Helper->self, 3.8s cast, range 40 width 10 rect

    //Right No Dogu
    RockMebuki = 33697, // Boss->self, 3.0s cast, single-target
    RockRootArrangementVisual = 33700, // Boss->self, 5.0s cast, single-target
    RockRootArrangementFirst = 33701, // Helper->location, 3.0s cast, range 4 circle
    RockRootArrangementRest = 33702, // Helper->location, no cast, range 4 circle

    //Right Dogu
    Witherwind = 33703, // Boss->self, 3.0s cast, single-target

}
public enum SID : uint
{
    SeasonsOfTheFleeting = 3623, // none->Boss, extra=0x0
    Unknown = 2056, // none->3EE1, extra=0x243
}

public enum IconID : uint
{
    Tankbuster = 218, // player
    Icon374 = 374, // Shibamaru/Shiromaru/Kuromaru
    RotateCW = 167, // Boss
    RotateCCW = 168, // Boss
    RootArrangement = 197, // player
}

public enum TetherID : uint
{
    Tether6 = 6, // Helper2->Boss
    Tether4 = 4, // Helper2->Boss
    Tether3 = 3, // Helper2->Boss
    Tether5 = 5, // Helper2->Boss
    Tether79 = 79, // Helper2->Boss
}
