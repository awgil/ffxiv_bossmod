namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN4Index;

public enum OID : uint
{
    Index = 0x4B5F, // R7.500, x1
    Index2 = 0x4B72, // R1.000, x3
    IndexHelper = 0x233C, // R0.500, x15 (spawn during fight), Helper type

    BallOfFire = 0x4B65, // R1.500, x0 (spawn during fight)
    BallOfLevin = 0x4B66, // R1.500, x0 (spawn during fight)
    ForetoldPhenomenon = 0x4B63, // R1.000, x0 (spawn during fight)
    HolyLance = 0x4B62, // R1.000, x3
    SummonedBomb = 0x4B60, // R2.100, x0 (spawn during fight)
    SwirlingOrb = 0x4B64, // R1.500, x0 (spawn during fight)
    TranscribedIndex = 0x4B6F, // R7.500, x3

    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Actor1ea1a1 = 0x1EA1A1, // R0.500, x1, EventObj type
    OmniElementFire = 0x1EC008, // R0.500, x0 (spawn during fight), EventObj type, goes across arena to both sides, created before Elementary Evocation
    OmniElementIce = 0x1EC009, // R0.500, x0 (spawn during fight), EventObj type
    OmniElementThunder = 0x1EC00A, // R0.500, x0 (spawn during fight), EventObj type
    ExpansionFire = 0x1EC00B, // R0.500, x0 (spawn during fight), EventObj type
    ExpansionIce = 0x1EC00C, // R0.500, x0 (spawn during fight), EventObj type
    ExpansionThunder = 0x1EC00D, // R0.500, x0 (spawn during fight), EventObj type
    Actor1ec00f = 0x1EC00F, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 48421, // Index->player, no cast, single-target
    Flare = 48415, // Index->self, 5.0s cast, single-target
    Flare1 = 48417, // IndexHelper->self, no cast, ???
    SealedImplements = 48384, // Index->self, 5.0+2.0s cast, single-target
    RomeosBallad = 48385, // IndexHelper->self, 7.0s cast, range 15 circle
    UnknownWeaponskill1 = 50665, // Index->self, no cast, single-target
    SealedImplements1 = 48386, // Index->self, 5.0+2.1s cast, single-target
    Aim = 48387, // IndexHelper->self, 7.1s cast, range 11 circle
    OmniElements = 48394, // Index->self, 4.0+1.0s cast, single-target
    OmniElements1 = 48395, // IndexHelper->self, no cast, ???
    ElementaryEvocation = 48400, // Index->self, 3.0s cast, single-target
    FireIV = 48396, // IndexHelper->self, no cast, range 30 ?-degree cone
    ElementaryExpansion = 48399, // Index->self, 3.0s cast, single-target
    BlizzardIV = 48397, // IndexHelper->self, no cast, range 30 ?-degree cone
    ThunderIV = 48398, // IndexHelper->self, no cast, range 30 ?-degree cone
    ElementaryChemistryCast = 48401, // Index->self, 3.9+1.1s cast, single-target
    ElementaryChemistry1 = 48402, // IndexHelper->self, no cast, ???
    ElementaryChemistry = 48905, // IndexHelper->self, 6.0s cast, range 15 width 15 rect
    PropulsiveProphecy = 48403, // Index->self, 3.0s cast, single-target
    Jump = 48404, // TranscribedIndex->self, no cast, single-target
    Shockwave = 48406, // IndexHelper->self, 5.0s cast, ???, 9f kb
    Shockwave1 = 48405, // HolyLance->self, 5.0s cast, single-target
    Summon = 48408, // Index->self, 3.0s cast, single-target
    DuologyOfImplements = 48388, // Index->self, 5.0+1.0s cast, single-target
    Iainuki = 48389, // IndexHelper->self, 6.0s cast, range 30 60.000-degree cone
    SealedImplements2 = 48904, // Index->self, no cast, single-target
    WindSlash = 48391, // IndexHelper->self, 6.0s cast, range 30 60.000-degree cone
    AllKnowingFlames = 48418, // Index->self, 5.0s cast, single-target
    AllConsumingFlames = 48420, // IndexHelper->players, no cast, range 6 circle
    Predict = 48412, // Index->self, 3.0s cast, single-target
    Cleansing = 48414, // ForetoldPhenomenon->self, 0.5s cast, range 4-15 donut
    Starfall = 48413, // ForetoldPhenomenon->self, 0.5s cast, range 10 circle
    Dualcast = 48407, // Index->self, 3.0s cast, single-target
    Flare2 = 48416, // Index->self, no cast, single-target
}

public enum SID : uint
{
    SealOfTheHarp = 5535, // none->Index, extra=0x404
    VulnerabilityUp = 2347, // IndexHelper->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7
    SealOfTheBow = 5534, // none->Index, extra=0x401
    SealOfTheBlade = 5533, // none->Index, extra=0x402
    SealOfTheBell = 5532, // none->Index, extra=0x403
    Predict = 2552, // none->ForetoldPhenomenon, extra=0x44C/0x44D, donut/circle
    Dualcast = 5438, // Index->Index, extra=0x0
}
public enum IconID : uint
{
    Spread = 466, // player->self
}
public enum TetherID : uint
{
    Predict = 88, // Index2->ForetoldPhenomenon
    Thunder = 363, // BallOfLevin->BallOfLevin
    Ice = 364, // SwirlingOrb->SwirlingOrb
    Fire = 365, // BallOfFire->BallOfFire
}
