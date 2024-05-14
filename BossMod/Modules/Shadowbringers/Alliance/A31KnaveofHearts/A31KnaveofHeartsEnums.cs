namespace BossMod.Shadowbringers.Alliance.A31KnaveofHearts;

public enum OID : uint
{
    Spheroid = 0x322D, // R1.000, x15
    Ally2B = 0x31A8, // R0.512, x1
    Boss = 0x31B8, // R30.000, x1
    CopiedKnave = 0x322C, // R30.000, x2
    Helper = 0x233C, // R0.500, x15, 523 type
    Energy = 0x322E, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    BladeFlurry1 = 23788, // Ally2B->Boss, no cast, single-target
    BladeFlurry2 = 23789, // Ally2B->Boss, no cast, single-target
    BossAutoAttack = 24668, // Boss->player, no cast, single-target
    DancingBlade = 23790, // Ally2B->Boss, no cast, width 2 rect charge
    Roar = 24245, // Boss->self, 5.0s cast, range 80 circle
    BalancedEdge = 23791, // Ally2B->self, 2.0s cast, range 5 circle
    WhirlingAssault = 23792, // Ally2B->self, 2.0s cast, range 40 width 4 rect

    ColossalImpact1 = 24231, // Boss/CopiedKnave->self, 8.0s cast, range 61 width 20 rect

    ColossalImpact2 = 24228, // Boss/CopiedKnave->self, 7.0s cast, single-target
    ColossalImpact3 = 24229, // Helper->self, 8.0s cast, range 61 width 20 rect
    ColossalImpact4 = 24230, // Helper->self, 8.0s cast, range 61 width 20 rect

    ColossalImpact5 = 23805, // Boss->self, 8.5s cast, single-target

    ColossalImpact6 = 24774, // Helper->self, 9.5s cast, range 61 width 20 rect
    ColossalImpact7 = 24775, // Helper->self, 9.5s cast, range 61 width 20 rect
    ColossalImpact8 = 24776, // Boss->self, 9.5s cast, range 61 width 20 rect

    MagicArtilleryBeta1 = 24242, // Boss->self, 3.0s cast, single-target
    MagicArtilleryBeta2 = 24243, // Helper->player, 5.0s cast, range 3 circle
    Replicate = 24233, // Boss->self, 3.0s cast, single-target
    StackingTheDeck1 = 24816, // CopiedKnave->self, 6.0s cast, single-target
    StackingTheDeck2 = 23801, // Boss->self, 6.0s cast, single-target
    Spheroids = 24232, // Boss->self, 4.0s cast, single-target
    KnavishBullets = 24237, // Spheroid->self, no cast, single-target
    MagicArtilleryAlpha1 = 24234, // Boss->self, 3.0s cast, single-target
    Burst = 24244, // Energy->player, no cast, single-target
    MagicArtilleryAlpha2 = 24235, // Helper->players, 6.0s cast, range 5 circle
    LightLeap1 = 24238, // Boss->self, 7.0s cast, single-target
    LightLeap2 = 24239, // Helper->location, 8.5s cast, range 40 circle
    UnknownAbiltiy = 24240, // Helper->self, 4.0s cast, range 8 width 8 rect
    Lunge = 24241, // Boss/CopiedKnave->self, 8.0s cast, range 61 width 60 rect
    MagicBarrage = 24236, // Spheroid->self, 6.0s cast, range 61 width 5 rect
}

public enum SID : uint
{
    TheHeatOfBattle = 365, // none->player, extra=0xA
    MeatAndMead = 360, // none->player, extra=0xA
    Invincibility = 1570, // none->player, extra=0x0
    VulnerabilityUp = 1789, // Boss/Helper/CopiedKnave/Energy/Spheroid->player, extra=0x1/0x2/0x3
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 218, // player
    Icon_279 = 279, // CopiedKnave
    Icon_278 = 278, // Boss
    Icon_169 = 169, // player
    Icon_280 = 280, // CopiedKnave
}

public enum TetherID : uint
{
    Tether_152 = 152, // CopiedKnave->Boss
}
