namespace BossMod.Stormblood.Alliance.A23Construct7;

public enum OID : uint
{
    Boss = 0x225D, // R4.200, x?
    Helper = 0x18D6, // R0.500, x?
    RamzaBasLexentale = 0x23E, // R0.500, x?
    CrudeScrawling = 0x1EA9ED, // R0.500, x?, EventObj type
    Montblanc = 0x239D, // R0.300, x?
    LooseCog = 0x225E, // R8.700, x?
    Actorb84 = 0xB84, // R0.500, x?
    Missile = 0x225F, // R1.300, x?
    Construct71 = 0x22A3, // R3.150, x?
    Construct72 = 0x22A2, // R3.150, x?
    Construct73 = 0x2260, // R3.150, x?
    Construct7 = 0x0, // R0.500, x?, None type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Accelerate = 11365, // Boss->players, 8.0s cast, range 6 circle
    AnnihilationMode = 11353, // Boss->self, 10.0s cast, single-target
    Compress1 = 11356, // Boss->self, 2.5s cast, range 100+R width 7 rect
    Compress2 = 11357, // Boss->self, 2.5s cast, range 100 width 7 cross

    ComputationMode = 11351, // Boss->self, 5.0s cast, single-target

    Destroy1 = 11354, // Boss->player, 5.0s cast, single-target
    Destroy2 = 11377, // Boss->player, 5.0s cast, single-target

    Dispose1 = 11359, // Boss->self, 7.0s cast, range 100+R ?-degree cone
    Dispose2 = 11360, // Boss->self, no cast, range 100+R ?-degree cone
    Dispose3 = 11497, // Boss->self, 7.0s cast, range 100+R ?-degree cone
    Dispose4 = 11498, // Boss->self, no cast, range 100+R ?-degree cone

    DivideByThree = 11466, // Boss->self, 10.0s cast, range 100 circle
    DivideByFour = 11468, // Boss->self, 10.0s cast, range 100 circle
    DivideByFive = 11469, // Boss->self, 10.0s cast, range 100 circle

    Ignite1 = 11366, // Boss->self, 3.0s cast, range 100+R width 20 rect
    Ignite2 = 11367, // Helper/player->players, no cast, range 10 circle

    Incinerate1 = 11364, // Boss->self, 5.0s cast, range 100 circle
    Incinerate2 = 11464, // Boss->self, 10.0s cast, range 100 circle
    Incinerate3 = 11612, // Boss->self, 5.0s cast, range 100 circle
    Incinerate4 = 11613, // Boss->self, 10.0s cast, range 100 circle

    Indivisible = 11470, // Boss->self, 10.0s cast, range 100 circle
    Lithobrake1 = 11368, // Boss->self, 5.0s cast, range 100 circle
    Lithobrake2 = 11550, // Helper/player->self, 5.0s cast, range 100 circle

    Pulverize1 = 11361, // Boss->self, 7.0s cast, range 15 circle
    Pulverize2 = 11362, // Helper/player->location, 4.0s cast, range 12 circle

    Subtract = 11372, // Boss->self, 5.0s cast, range 100 circle
    Tartarus = 11363, // Boss->Helper, no cast, range 100 circle
    TartarusMode = 11352, // Boss->self, 5.0s cast, single-target

    Triboelectricity = 11373, // LooseCog->self, no cast, range 8 circle
}

public enum SID : uint
{
    AnnihilationMode = 1564, // Boss->player/Boss, extra=0x0
    BrinkOfDeath = 44, // none->player, extra=0x0
    ComputationBoost = 1565, // none->player, extra=0x1/0x2/0x3/0x4
    ComputationError = 1566, // none->player, extra=0x1/0x2
    ComputationMode = 1562, // Boss->Helper/Boss/player, extra=0x0
    DamageDown = 696, // Boss/player->player, extra=0x0
    DownForTheCount = 783, // Boss->player, extra=0xEC7
    Hover = 1515, // none->LooseCog, extra=0x1F4
    HPBoost1 = 1558, // none->player, extra=0x0
    HPBoost2 = 1559, // none->player, extra=0x0
    HPBoost3 = 1560, // none->player, extra=0x0
    HPBoost4 = 1561, // none->player, extra=0x0
    HPPenalty = 1557, // Boss->player, extra=0x9
    Invincibility = 1570, // none->player, extra=0x0
    Minimum = 438, // Boss->player, extra=0xF
    Paralysis = 17, // LooseCog->player, extra=0x0
    TartarusMode = 1563, // Boss->Helper/Boss, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0

}

public enum IconID : uint
{
    Icon100 = 100, // player
    Icon136 = 136, // player
    Icon137 = 137, // player
    Icon138 = 138, // player
    Tankbuster = 218, // player
}
