namespace BossMod.Stormblood.Alliance.A34UltimaP1;

public enum OID : uint
{
    Boss = 0x2604, // R10.400, x?
    Helper = 0x233C, // R0.500, x?
    Agrias1 = 0x262B, // R0.500, x0 (spawn during fight)
    Agrias2 = 0x267D, // R0.500, x3
    Aspersory = 0x2635, // R2.400, x0 (spawn during fight)
    AuraciteShard = 0x2639, // R1.250, x0 (spawn during fight)
    DemiBelias = 0x2636, // R4.590, x1
    DemiFamfrit = 0x2634, // R5.940, x1
    DemiHashmal = 0x2637, // R5.400, x1
    Mustadio = 0x262A, // R0.500, x0 (spawn during fight)
    Ramza = 0x2638, // R0.650, x0 (spawn during fight)
    Ruination = 0x263A, // R3.900, x0 (spawn during fight), Part type
    TheThunderGod = 0x262C, // R0.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 14499, // Boss->player, no cast, single-target

    Auralight1 = 14487, // Helper->self, 6.0s cast, range 70 width 10 rect
    Auralight2 = 14488, // Helper->self, 6.0s cast, range 20 circle
    Auralight3 = 14570, // Boss->self, 6.0s cast, single-target

    ControlTower1 = 14548, // DemiHashmal->self, 3.0s cast, range 80 circle
    ControlTower2 = 14549, // Helper->self, 4.0s cast, range 6 circle
    CrushWeapon = 14515, // Agrias2->location, 3.0s cast, range 6 circle
    DarkCannonade1 = 14541, // DemiFamfrit->self, 5.0s cast, single-target
    DarkCannonade2 = 14542, // Helper->player, no cast, single-target
    DarkEwer = 14538, // DemiFamfrit->self, 3.0s cast, single-target
    DemiAquarius = 14527, // Boss->self, 3.0s cast, single-target
    DemiAries = 14528, // Boss->self, 3.0s cast, single-target
    DemiLeo = 14529, // Boss->self, 3.0s cast, single-target
    EarthHammer = 14552, // DemiHashmal->self, 5.0s cast, single-target
    Eruption1 = 15479, // DemiBelias->self, 3.0s cast, single-target
    Eruption2 = 15480, // Helper->location, 3.0s cast, range 8 circle

    ExtremeEdge1 = 14554, // DemiHashmal->self, 6.0s cast, range 60+R width 36 rect
    ExtremeEdge2 = 14555, // DemiHashmal->self, 6.0s cast, range 60+R width 36 rect

    GrandCross1 = 14508, // Boss->self, 3.0s cast, single-target
    GrandCross2 = 14510, // AuraciteShard->self, 4.0s cast, range 60 width 15 cross
    HallowedBolt = 14514, // Agrias2->location, 3.0s cast, range 6 circle
    Hammerfall = 14553, // Helper->self, 1.0s cast, range 40 circle
    Hellfire = 14673, // DemiBelias->self, 15.0s cast, single-target

    HolyIV1 = 14489, // Boss->self, 3.0s cast, single-target
    HolyIV2 = 14490, // Helper->location, 3.0s cast, range 6 circle
    HolyIV3 = 14491, // Helper->players, 5.5s cast, range 6 circle
    HolyIV4 = 15367, // Boss->self, 5.0s cast, single-target

    Landwaster = 14674, // DemiHashmal->self, 15.0s cast, single-target
    Materialize = 14539, // Aspersory->self, 2.0s cast, range 6 circle
    Plummet = 14509, // AuraciteShard->self, 3.5s cast, range 15 width 15 rect
    PrevailingCurrent = 14540, // Helper->self, no cast, range 6 circle
    Sanction = 14550, // DemiHashmal->self, no cast, range 80 circle
    Searchlight = 14513, // Agrias2->location, 3.0s cast, range 6 circle

    TimeEruption1 = 14543, // DemiBelias->self, 3.0s cast, single-target
    TimeEruption2 = 14544, // Helper->self, 6.0s cast, range 20 width 20 rect
    TimeEruption3 = 14545, // Helper->self, 9.0s cast, range 20 width 20 rect

    Towerfall = 14551, // Helper->self, no cast, range 40+R width 10 rect
    Tsunami = 14672, // DemiFamfrit->self, 15.0s cast, single-target

    UltimateIllusion1 = 14485, // Boss->self, 22.0s cast, single-target
    UltimateIllusion2 = 14486, // Helper->self, no cast, range 80 circle
    UltimateIllusion3 = 15003, // Helper->self, no cast, range 80 circle

    Unknown1 = 14482, // Boss->self, no cast, single-target
    Unknown2 = 14483, // Boss->self, no cast, single-target
    Unknown3 = 14517, // Ramza->self, no cast, single-target
    Unknown4 = 14556, // Boss->self, no cast, single-target
    UnknownAbility = 14969, // Mustadio/Agrias1/TheThunderGod->self, no cast, single-target
}

public enum SID : uint
{
    BrinkOfDeath = 44, // none->player, extra=0x0
    FleshWound = 264, // DemiHashmal->player, extra=0x0
    GuardiansAegis = 1748, // none->player, extra=0x0
    GuardianSpirit = 1692, // none->Mustadio/Agrias1/TheThunderGod, extra=0x0
    Invincibility = 1570, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    VulnerabilityUp = 202, // AuraciteShard/Helper->player, extra=0x1/0x2
    Weakness = 43, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon_43 = 43, // player
    Icon_55 = 55, // player
    Icon_90 = 90, // player
    Icon_102 = 102, // player
    Icon_127 = 127, // player
    Icon_139 = 139, // player
}