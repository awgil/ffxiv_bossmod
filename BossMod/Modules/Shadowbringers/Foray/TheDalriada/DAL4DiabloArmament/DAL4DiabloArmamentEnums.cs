namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL4DiabloArmament;

public enum OID : uint
{
    Boss = 0x31B3, // R28.500, x1
    Helper = 0x233C, // R0.500, x33, mixed types
    DiabolicBit = 0x31B4, // R1.200, x0 (spawn during fight)
    Aether = 0x31B5, // R1.500, x0 (spawn during fight)
    Actor1eb1d8 = 0x1EB1D8, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eb1d7 = 0x1EB1D7, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eb1d6 = 0x1EB1D6, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eb1d9 = 0x1EB1D9, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AdvancedDeathIV = 23727, // Boss->self, 4.0s cast, single-target
    AdvancedDeathIVAOE = 23728, // Helper->location, 7.0s cast, range 1 circle

    AdvancedDeathRay = 23748, // Boss->self, 5.0s cast, single-target
    AdvancedDeathRayAOE = 23749, // Helper->player, no cast, range 70 width 8 rect

    AdvancedNox = 23743, // Boss->self, 4.0s cast, single-target
    AdvancedNoxAOEFirst = 23744, // Helper->self, 10.0s cast, range 10 circle
    AdvancedNoxAOERest = 23745, // Helper->self, no cast, range 10 circle

    AethericBoom1 = 23731, // Boss->self, 5.0s cast, single-target
    AethericBoom2 = 23732, // Helper->self, 5.0s cast, ???

    AethericExplosion1 = 23750, // Boss->self, 5.0s cast, single-target
    AethericExplosion2 = 23751, // Helper->self, 5.0s cast, ???

    Aetheroplasm = 23733, // Aether->self, no cast, range 6 circle
    AssaultCannon = 23726, // DiabolicBit->self, 7.0s cast, range 100 width 6 rect
    BrutalCamisado = 24899, // Boss->player, no cast, single-target

    DeadlyDealingAOE = 23746, // Boss->location, 7.0s cast, range 6 circle
    DeadlyDealing = 23747, // Helper->self, 7.5s cast, ???

    DiabolicGate1 = 23711, // Boss->self, 4.0s cast, single-target
    DiabolicGate2 = 25028, // Helper->self, 5.0s cast, ???

    AetherochemicalLaserAOE1 = 23716, // Boss->self, no cast, range 60 width 22 rect
    AetherochemicalLaserAOE2 = 23717, // Boss->self, no cast, range 60 width 60 rect

    Explosion1 = 23718, // Helper->self, 10.0s cast, range 60 width 22 rect
    Explosion2 = 23719, // Helper->self, 10.0s cast, range 60 width 22 rect
    Explosion3 = 24721, // Helper->self, 10.0s cast, range 60 width 22 rect

    Explosion4 = 23720, // Helper->self, 8.0s cast, range 60 width 22 rect
    Explosion5 = 23721, // Helper->self, 8.0s cast, range 60 width 22 rect
    Explosion6 = 24722, // Helper->self, 8.0s cast, range 60 width 22 rect

    Explosion7 = 23722, // Helper->self, 6.0s cast, range 60 width 22 rect
    Explosion8 = 23723, // Helper->self, 6.0s cast, range 60 width 22 rect
    Explosion9 = 24723, // Helper->self, 6.0s cast, range 60 width 22 rect

    FusionBurst = 23734, // Aether->self, no cast, range 100 circle

    LightPseudopillar = 23729, // Boss->self, 3.0s cast, single-target
    LightPseudopillarAOE = 23730, // Helper->location, 4.0s cast, range 10 circle

    MagitekBit = 23724, // Boss->self, 4.0s cast, single-target

    PillarOfShamash1 = 23737, // Helper->self, 8.0s cast, range 70 20-degree cone
    PillarOfShamash2 = 23738, // Helper->self, 9.5s cast, range 70 20-degree cone
    PillarOfShamash3 = 23739, // Helper->self, 11.0s cast, range 70 20-degree cone
    PillarOfShamash4 = 23740, // Helper->player, no cast, range 70 width 4 rect
    PillarOfShamash5 = 23741, // Helper->player, no cast, single-target
    PillarOfShamash6 = 23742, // Helper->player, no cast, range 70 width 8 rect

    RuinousPseudomen1 = 23712, // Boss->self, 15.0s cast, single-target
    RuinousPseudomen2 = 23713, // Helper->self, 1.0s cast, single-target
    RuinousPseudomen3 = 23714, // Boss->self, no cast, single-target
    RuinousPseudomen4 = 24908, // Helper->self, 1.5s cast, range 100 width 24 rect
    RuinousPseudomen5 = 24911, // Helper->self, 4.5s cast, range 80 width 24 rect
    RuinousPseudomen6 = 24995, // Helper->self, 1.5s cast, range 80 width 24 rect

    UltimatePseudoterror = 23715, // Boss->self, 4.0s cast, range ?-70 donut

    UnknownAbility1 = 23725, // DiabolicBit->location, no cast, single-target
    UnknownAbility2 = 24994, // Helper->self, no cast, range ?-60 donut

    VoidSystemsOverload1 = 23735, // Boss->self, 5.0s cast, single-target
    VoidSystemsOverload2 = 23736, // Helper->self, 5.0s cast, ???
    VoidSystemsOverload3 = 25364, // Boss->self, 5.0s cast, single-target
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper/Aether/Boss->player, extra=0x1/0x2/0x3/0x4
    UnknownStatus = 2056, // none->DiabolicBit, extra=0xE1
    AreaOfInfluenceUp = 1749, // none->Helper, extra=0x9
    AccelerationBomb = 2657, // none->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0

}
public enum IconID : uint
{
    Nox = 230, // player
    AccelerationBomb = 267, // player
    Spreadmarker = 23, // player
}
public enum TetherID : uint
{
    Tether_1 = 1, // Aether->Aether
}
