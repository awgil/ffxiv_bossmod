namespace BossMod.RealmReborn.Alliance.A31AngraMainyu;

public enum OID : uint
{
    Boss = 0xE00, // R3.200, x?
    AngraMainyuHelper = 0x1B2, // R0.500, x?, 523 type
    FinalHourglass = 0xE02, // R1.500, x?
    GrimReaper = 0xE01, // R2.000, x?
    AngraMainyusDaewa = 0xE4E, // R1.800, x?
}

public enum AID : uint
{
    AutoAttack = 3508, // Boss->player, no cast, single-target
    DoubleVision = 3272, // Boss->self, 2.5s cast, range 60 circle
    IrefulGaze = 3274, // AngraMainyuHelper->self, no cast, ???
    SullenGaze = 3273, // AngraMainyuHelper->self, no cast, ???
    Stare = 3280, // Boss->self, no cast, range 60+R width 8 rect
    Level100Flare1 = 3275, // Boss->location, 4.5s cast, ???
    Level100Flare2 = 3276, // AngraMainyuHelper->player, no cast, ???
    MortalGaze1 = 3281, // Boss->self, 3.0s cast, range 60 circle
    MortalGaze2 = 3499, // AngraMainyuHelper->self, 4.5s cast, range 60 circle
    Death = 3279, // GrimReaper->self, no cast, ???
    Thunder = 968, // AngraMainyusDaewa->player, 1.0s cast, single-target
    Paralyze = 1118, // AngraMainyusDaewa->player, 4.0s cast, single-target
    Level150Death1 = 3277, // Boss->location, 4.5s cast, ???
    Level150Death2 = 3278, // AngraMainyuHelper->player, no cast, ???
    EyesOnMe = 3358, // AngraMainyusDaewa->self, 4.0s cast, range 30+R circle
}

public enum SID : uint
{
    BrandOfTheIreful = 637, // AngraMainyuHelper->player, extra=0x1/0x2
    BrandOfTheSullen = 636, // AngraMainyuHelper->player, extra=0x1
    Bind = 280, // none->player, extra=0x0
    Doom = 210, // AngraMainyuHelper->player, extra=0x0
    Stun = 149, // none->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Paralysis = 17, // AngraMainyusDaewa->player, extra=0x0
    BrinkOfDeath = 44, // none->player, extra=0x0
    Suppuration = 375, // AngraMainyuHelper->player, extra=0x1
}

public enum IconID : uint
{
    Icon44 = 44, // player
    Icon45 = 45, // player
}

public enum TetherID : uint
{
    Tether5 = 5, // player->player
    Tether1 = 1, // player->player
}
