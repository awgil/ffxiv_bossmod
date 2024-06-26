namespace BossMod.RealmReborn.Alliance.A33Cerberus;

public enum OID : uint
{
    Boss = 0xDF6, // R10.800, x?
    CerberusHelper1 = 0x1B2, // R0.500, x?, 523 type
    CerberusHelper2 = 0x8EE, // R0.500, x?, mixed types
    CerberusHelper3 = 0x19A, // R0.500, x?
    GastricJuice = 0xDF9, // R1.000, x?
    StomachWall = 0xDFA, // R1.500, x?
    Wolfsbane = 0xDF8, // R0.800, x?
    Unknown = 0xDF7, // R1.800, x?
    Electron = 0xDFB, // R1.000, x?
    OpenFetter1 = 0x1E9735, // R0.500, x?, EventObj type
    OpenFetter2 = 0x1E9733, // R0.500, x?, EventObj type
    MagickedFetter1 = 0x1E9736, // R2.000, x?, EventObj type
    MagickedFetter2 = 0x1E9737, // R2.000, x?, EventObj type
}

public enum AID : uint
{
    AutoAttack1 = 3509, // Boss->player, no cast, single-target
    PredatorClaws = 3245, // Boss->self, no cast, range 9+R ?-degree cone
    TailBlow = 3246, // Boss->self, 2.0s cast, range 9+R 90-degree cone
    Innerspace = 3248, // Boss->player, no cast, single-target
    Slabber = 3241, // Boss->location, 3.0s cast, range 8 circle
    Mini = 3249, // GastricJuice->self, 3.0s cast, range 8+R circle
    Unknown = 3376, // CerberusHelper2->self, no cast, range 8 circle
    Engorge = 3243, // Boss->location, no cast, range 8 circle
    AutoAttack2 = 872, // Wolfsbane/Unknown->player, no cast, single-target
    Seedvolley = 344, // Wolfsbane->player, no cast, single-target
    DeathRay = 1913, // Unknown->player, no cast, single-target
    SulphurousBreath1 = 3250, // Boss->self, 1.8s cast, range 25+R width 6 rect
    SulphurousBreath2 = 3251, // CerberusHelper1->self, 2.8s cast, range 40+R width 6 rect
    Spew = 3244, // CerberusHelper2->self, no cast, range 60+R circle
    LightningBolt1 = 3252, // Boss->location, no cast, range 8 circle
    LightningBolt2 = 3253, // Electron->Electron, 2.0s cast, width 4 rect charge
    HoundOutOfHell = 3247, // Boss->CerberusHelper3, 3.5s cast, width 14 rect charge
    Reawakening = 3507, // Boss->self, 50.0s cast, single-target
    SourSough = 3510, // Wolfsbane->self, no cast, range 6+R ?-degree cone
    Ululation = 3254, // Boss->self, 4.2s cast, range 80+R circle
    Devour = 3242, // Boss->location, no cast, range 8 circle
    HexEye = 1914, // Unknown->self, 2.5s cast, range 3+R circle

}

public enum SID : uint
{
    SlashingResistanceDown = 572, // Boss->player, extra=0x1
    Minimum = 438, // GastricJuice->player, extra=0xA
    Seized = 609, // none->player, extra=0x0
    Haste = 480, // none->Boss/CerberusHelper1/CerberusHelper2, extra=0x1/0x2/0x3
    BluntResistanceDown = 573, // Boss->player, extra=0x1/0x2
    Digesting = 645, // none->player, extra=0x1/0x2
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Transporting = 404, // none->player, extra=0xE
    Electrocution = 271, // Electron->player, extra=0x0
    Heavy = 14, // Boss->player, extra=0x32
    Poison = 18, // Wolfsbane->player, extra=0x0
    Abandonment = 646, // Boss->player, extra=0x0
    Hysteria = 296, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    Icon3 = 3, // player
}

public enum TetherID : uint
{
    Tether34 = 34, // CerberusHelper3->player
    Tether33 = 33, // CerberusHelper3->Boss
    Tether32 = 32, // CerberusHelper3->Boss
}
