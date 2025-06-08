namespace BossMod.Dawntrail.Foray.ForkedTower.FT02DeadStars;

public enum OID : uint
{
    DeathWallHelper = 0x4866,
    Helper = 0x233C,
    Triton = 0x4786, // R4.000, x1 (spawn during fight)
    Nereid = 0x4787, // R4.000, x1 (spawn during fight)
    Phobos = 0x4788, // R4.000, x1 (spawn during fight)
    LiquifiedTriton = 0x478B, // R4.000, x0 (spawn during fight)
    LiquifiedNereid = 0x478C, // R4.000, x0 (spawn during fight)
    DeadStars = 0x478D, // R15.000, x0 (spawn during fight)
    GaseousNereid = 0x4814, // R5.000, x0 (spawn during fight)
    GaseousPhobos = 0x4815, // R5.000, x0 (spawn during fight)
    FrozenTriton = 0x4816, // R5.000, x0 (spawn during fight)
    FrozenPhobos = 0x4817, // R5.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Triton/Phobos/Nereid->player, no cast, single-target
    BossJump = 42420, // Phobos/Nereid/Triton->location, no cast, single-target
    DecisiveBattle1 = 42490, // Triton->self, 6.0s cast, range 35 circle
    DecisiveBattle2 = 42491, // Nereid->self, 6.0s cast, range 35 circle
    DecisiveBattle3 = 42492, // Phobos->self, 6.0s cast, range 35 circle
    DeathWall = 42504, // Boss->self, no cast, range ?-40 donut
    SliceNDice = 42498, // Helper->player, 5.0s cast, range 70 90-degree cone
    SliceNDiceCast = 42497, // Phobos/Nereid/Triton->player, 5.0s cast, single-target
    ThreeBodyProbl = 42421, // Phobos/Nereid/Triton->location, 5.3+0.7s cast, single-target
    ThreeBodyProblemN = 43453, // Nereid->self, 6.5s cast, single-target
    ThreeBodyProblemPN = 42423, // Phobos/Nereid->self, 6.5s cast, single-target
    ThreeBodyProblemPT = 42424, // Phobos/Triton->self, 6.5s cast, single-target
    ThreeBodyProblemT = 42425, // Triton->self, 6.5s cast, single-target
    NoisomeNuisance = 42551, // Helper->self, 7.0s cast, range 6 circle
    PrimordialChaosCast = 42457, // Phobos->self, 5.0s cast, single-target
    PrimordialChaosVisual = 42458, // LiquifiedNereid/LiquifiedTriton->self, no cast, single-target
    PrimordialChaos = 42460, // Helper->self, no cast, ???
    FrozenFalloutFireCast = 42463, // Helper->self, 2.5s cast, range 22 circle
    FrozenFalloutIceCast = 42464, // Helper->self, 2.5s cast, range 22 circle
    FrozenFalloutVisual = 42461, // Phobos->self, 10.0s cast, single-target
    FrozenFalloutN = 42466, // LiquifiedNereid->location, no cast, single-target
    FrozenFalloutT = 42465, // LiquifiedTriton->location, no cast, single-target
    FrozenFalloutIndicator = 42459, // Helper->self, 1.4s cast, range 22 circle
    FrozenFalloutFireAOE = 42467, // Helper->self, 1.4s cast, ???
    FrozenFalloutIceAOE = 42468, // Helper->self, 1.4s cast, ???
    FrozenFalloutVisual2 = 42462, // Phobos->self, no cast, single-target
    NoxiousNovaCast = 42469, // Phobos->self, 5.0s cast, single-target
    NoxiousNova = 42470, // Helper->self, 5.8s cast, ???
    Unk1 = 42426, // Phobos/Nereid/Triton->self, no cast, single-target
    Unk2 = 42427, // Nereid/Triton/Phobos->self, no cast, single-target
    Unk3 = 42428, // Nereid/Triton/Phobos->self, no cast, single-target
    VengefulFireIII = 42429, // Triton->self, 5.5s cast, range 60 120-degree cone
    VengefulBlizzardIII = 42430, // Nereid->self, 5.5s cast, range 60 120-degree cone
    VengefulBioIII = 42431, // Phobos->self, 5.5s cast, range 60 120-degree cone
    ExcruciatingEquilibriumInstant = 42488, // Phobos/Nereid/Triton->self, no cast, single-target
    ExcruciatingEquilibrium = 42489, // Helper->Nereid/Triton/Phobos, 0.5s cast, single-target
    DeltaAttackHelper = 42495, // Helper->self, 5.0s cast, single-target
    DeltaAttackCast = 42493, // Phobos/Nereid/Triton->self, 5.0s cast, single-target
    DeltaAttack = 42558, // Helper->self, 5.5s cast, ???
    DeltaAttackUnk1 = 42496, // Helper->self, no cast, single-target
    DeltaAttackUnk2 = 42559, // Helper->self, 0.5s cast, ???
    DeltaAttackInstant = 42494, // Phobos/Nereid/Triton->self, no cast, single-target
    FirestrikeTarget = 42500, // Helper->player, no cast, single-target
    FirestrikeTarget2 = 42503, // Helper->player, no cast, single-target
    FirestrikeCast = 42499, // Phobos/Nereid/Triton->self, 5.0s cast, single-target
    FirestrikeRect = 42502, // Phobos/Triton/Nereid->players, no cast, range 70 width 10 rect
    IceboundBuffoon = 42550, // Helper->self, 7.0s cast, range 6 circle
    SnowBoulderCast = 42447, // Helper->location, 7.0s cast, width 10 rect charge
    SnowballFlight = 42446, // Nereid->self, 7.0s cast, single-target
    SnowBoulder = 42448, // FrozenTriton/FrozenPhobos->location, no cast, width 10 rect charge
    ChillingCollisionIndicator = 42451, // Helper->self, 5.0s cast, range 40 circle
    ChillingCollisionCast = 42422, // Nereid->self, 5.0s cast, single-target
    ChillingCollisionKB = 42452, // Helper->self, no cast, ???
    AvalaunchCast = 43162, // FrozenTriton/FrozenPhobos->self, 5.0s cast, single-target
    AvalaunchJump = 42450, // FrozenTriton/FrozenPhobos->location, no cast, range 8 circle
    AvalaunchStack = 42449, // Helper->player, 8.0s cast, single-target
    ToTheWinds1 = 42453, // Nereid->self, 13.0s cast, single-target
    ToTheWinds2 = 42437, // Triton->self, 7.0s cast, single-target
    SliceNStrike = 42501, // Phobos/Nereid/Triton->player, 5.0s cast, single-target
    BlazingBelligerent = 42549, // Helper->self, 7.0s cast, range 6 circle
    ElementalImpact1 = 42432, // GaseousPhobos/GaseousNereid->self, 3.0s cast, range 5 circle
    ElementalImpact2 = 42433, // GaseousPhobos/GaseousNereid->self, 3.0s cast, range 5 circle
    ElementalImpact3 = 42434, // GaseousPhobos/GaseousNereid->self, 5.0s cast, range 5 circle
    FireSpread = 43272, // Helper->players, no cast, range 60 90-degree cone
    GeothermalRuptureCast = 42441, // Triton->self, 5.0s cast, single-target
    FlameThrowerTarget = 42444, // Helper->player, no cast, single-target
    GeothermalRupture = 42442, // Helper->location, 3.0s cast, range 8 circle
    FlameThrowerCast = 42443, // Triton->self, 5.0s cast, single-target
    FlameThrower = 42445, // Helper->self, no cast, range 40 width 8 rect
    SixHandedFistfight = 42471, // Phobos/Nereid/Triton->location, 9.4+0.6s cast, single-target
    SixHandedFistfightAOE = 42472, // Helper->self, 10.0s cast, range 12 circle
    SixHandedRaidwide = 42473, // Helper->self, 10.5s cast, ???
    Bodied = 42474, // Helper->self, no cast, range 12 circle
    Unk5 = 42475, // Triton/Phobos/Nereid->self, no cast, single-target
    Unk6 = 42476, // Nereid/Phobos/Triton->self, no cast, single-target
    CollateralHeatJet = 42478, // Helper->self, 5.0s cast, range 40 60-degree cone
    CollateralColdJet = 42479, // Helper->self, 5.0s cast, range 40 60-degree cone
    CollateralGasJet = 42480, // Helper->self, 5.0s cast, range 40 60-degree cone
    CollateralDamage = 42477, // DeadStars->self, 5.0s cast, single-target
    CollateralFireball = 42481, // Helper->player, no cast, range 4 circle
    CollateralIceball = 42482, // Helper->player, no cast, range 4 circle
    CollateralBioball = 42483, // Helper->player, no cast, range 4 circle
    Unk7 = 42486, // Phobos/Nereid/Triton->location, no cast, single-target
    Return = 42487, // Phobos/Nereid/Triton->self, 6.0s cast, single-target
    SelfDestructIce = 42454, // FrozenTriton/FrozenPhobos->self, 13.0s cast, single-target
    SelfDestructFire = 42438, // GaseousPhobos->self, 7.0s cast, single-target
}

public enum SID : uint
{
    TritonicGravity = 4438, // none->player, extra=0x0
    NereidicGravity = 4439, // none->player, extra=0x0
    PhobosicGravity = 4440, // none->player, extra=0x0
    Triton = 4505, // none->Triton, extra=0x334
    Nereid = 4506, // none->Nereid, extra=0x37E
    Phobos = 4507, // none->Phobos, extra=0x37F
    NovaOoze = 4441, // none->player, extra=0x2/0x3/0x1
    IceOoze = 4442, // none->player, extra=0x2/0x1/0x3
    MagicVulnerabilityUp = 2941, // Helper/Triton/Phobos/Nereid/GaseousPhobos/GaseousNereid->player, extra=0x0
    IceboundBuffoonery = 4443, // none->FrozenTriton/FrozenPhobos, extra=0x4/0x3/0x2/0x1
    PhysicalVulnerabilityUp = 2940, // FrozenPhobos/FrozenTriton->player, extra=0x0
}

public enum TetherID : uint
{
    DecisiveBattle = 249, // player->Nereid/Triton/Phobos
    StarTether = 310, // Nereid/Triton/Phobos->Phobos/Nereid/Triton
    ShortTether = 246, // FrozenPhobos/FrozenTriton->player
    LongTether = 1, // FrozenPhobos/FrozenTriton->player
}
