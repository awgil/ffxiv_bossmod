namespace BossMod.Stormblood.Alliance.A11Mateus;

public enum OID : uint
{
    Boss = 0x1F9B, // R4.500, x?
    MateusTheCorrupt = 0x18D6, // R0.500, x?
    LinaMewrilah = 0x23E, // R0.500, x?
    IceAzer = 0x1F9C, // R1.040, x?
    Totema = 0x1F9D, // R1.000, x?
    BlizzardIII = 0x1FA0, // R1.500, x?
    AquaSphere = 0x1F9F, // R1.800, x?
    AquaBubble = 0x1F9E, // R1.000, x?
    IceSlave = 0x2062, // R1.000, x?
    BlizzardIV = 0x1FA4, // R0.700, x?
    Icicle = 0x1FA2, // R1.000, x?
    FlumeToad = 0x1FA1, // R1.920, x?
    Froth = 0x1FA3, // R0.700-1.400, x?
    BlizzardSphere = 0x1FA6, // R1.000, x?
    AzureGuard = 0x1FA5, // R2.800, x?
}

public enum AID : uint
{
    Blizzard = 9784, // IceAzer->player, 1.0s cast, single-target
    AutoAttack = 872, // Boss/IceSlave/FlumeToad/AzureGuard->player, no cast, single-target
    FlashFreeze = 9799, // Boss->self, no cast, range 12+R ?-degree cone
    HypothermalCombustion = 9785, // IceAzer->self, 5.0s cast, range 8+R circle
    Unbind = 9779, // Boss->self, 5.0s cast, single-target
    Unknown1 = 10004, // Totema->self, no cast, single-target
    Unknown2 = 9890, // Boss->self, no cast, single-target
    DreadTide = 10046, // AquaBubble->self, no cast, range 4 circle
    BallOfIce = 9780, // MateusTheCorrupt->self, no cast, range 2 circle
    RTI11First = 9783, // MateusTheCorrupt->self, no cast, single-target
    DarkBlizzardIII = 10045, // IceSlave->players, 3.0s cast, range 5 circle
    Rebind = 9781, // Boss->self, 5.0s cast, single-target
    RTI11Second = 9782, // Totema->Boss, no cast, single-target
    Dualcast = 9788, // Boss->self, 5.0s cast, single-target
    FF12RTI11 = 9787, // Boss->self, no cast, single-target
    BlizzardIV1 = 9789, // Boss->self, 10.0s cast, single-target
    BlizzardIV2 = 9790, // BlizzardIV->location, 10.0s cast, range 100 circle
    Froth = 9791, // FlumeToad->self, no cast, single-target
    Snowpierce = 9792, // Icicle->players, 10.0s cast, width 3 rect charge
    Dendrite = 9797, // Boss->self, no cast, single-target
    Chill = 9798, // BlizzardSphere->self, 5.0s cast, range 40+R ?-degree cone
    Unknown3 = 9836, // Boss->self, no cast, single-target
    FinRays = 9794, // AzureGuard->self, no cast, range 9+R ?-degree cone
    Frostwave = 9793, // Boss->self, no cast, range 100 circle
    TheWhiteWhisper = 10030, // BlizzardIII->self, 15.0s cast, single-target
}

public enum SID : uint
{
    Sleep = 3, // IceAzer->player, extra=0x0
    Heavy = 1141, // none->Totema, extra=0x4
    Drenched = 1427, // AquaBubble->player, extra=0x0
    DeepFreeze = 1254, // MateusTheCorrupt->player, extra=0x0
    Dualcast = 1378, // Boss->MateusTheCorrupt/Boss, extra=0x0
    Breathless = 1429, // none->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    VulnerabilityDown = 350, // none->AzureGuard, extra=0x0

}

public enum TetherID : uint
{
    Tether_8 = 8, // player/BlizzardSphere->BlizzardIII/player
    Tether_1 = 1, // AzureGuard->AzureGuard
}
