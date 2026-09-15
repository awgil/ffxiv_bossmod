namespace BossMod.Endwalker.Alliance.A12Rhalgr;

public enum OID : uint
{
    Boss = 0x38D6, // R7.200, x1
    FistOfWrath = 0x38D7, // R3.600, x1, red portal
    FistOfJudgment = 0x38D8, // R3.600, x1, blue portal
    LightningOrb = 0x38D9, // R2.000, spawn during fight
    Helper = 0x233C, // R0.500, x19
}

public enum AID : uint
{
    AutoAttack = 28835,
    Teleport = 28846, // Boss->location, no cast

    LightningReign = 28837, // Boss->self, 5.0s cast, raidwide
    DestructiveBolt = 28836, // Boss->self, 4.0s cast, tankbuster visual
    DestructiveBoltAOE = 28852, // Helper->players, 5.0s cast, range 3 aoe on 3 tanks
    StrikingMeteor = 28859, // Helper->location, 3.0s cast, range 6 aoe (puddle)
    LightningStorm = 28858, // Helper->players, 8.0s cast, range 5 aoe (spread)

    AdventOfTheEighth = 28839, // Boss->self, 4.0s cast, visual
    PortalWrath = 29347, // FistOfWrath->self, no cast, visual (portal appears)
    PortalJudgment = 29348, // FistOfJudgment->self, no cast, visual (portal appears)
    PortalCometWrath = 29349, // FistOfWrath->self, no cast, visual (portal + comet appears)
    HandOfTheDestroyerWrath = 28840, // Boss->self, 9.0s cast, visual, standalone
    HandOfTheDestroyerJudgment = 28841, // Boss->self, 9.0s cast, visual, standalone
    HandOfTheDestroyerWrathBroken = 28844, // Boss->self, 9.0s cast, visual, combo with broken world
    //HandOfTheDestroyerJudgmentBroken = 28853, // Boss->self, 9.0s cast, visual, combo with broken world (note: never actually seen...)
    HandOfTheDestroyerWrathAOE = 28847, // FistOfWrath->self, 9.4s cast, range 90 half-width 20 rect
    HandOfTheDestroyerJudgmentAOE = 28848, // FistOfJudgment->self, 9.4s cast, range 90 half-width 20 rect
    HandOfTheDestroyerWrathBrokenVisualE = 29148, // FistOfWrath->self, no cast, visual (fist hitting comet on E side)
    HandOfTheDestroyerWrathBrokenVisualW = 28849, // FistOfWrath->self, no cast, visual (fist hitting comet on W side)
    // 29149 / 29151 - judgment broken visual?
    BrokenWorld = 28838, // Boss->self, 3.0s cast, visual
    BrokenWorldAOE = 28854, // Helper->self, 10.6s cast, range 96 aoe with ? falloff
    BrokenShardsE = 29147, // Helper->location, 14.7s cast, visual (large comet hit location on E side, before it is split)
    BrokenShardsW = 29152, // Helper->location, 14.7s cast, visual (large comet hit location on W side, before it is split)
    BrokenShardsVisual = 28855, // Helper->self, no cast, visual (show aoe cicles where comet shards hit)
    BrokenShardsAOE = 29149, // Helper->self, no cast, range 20 aoe with ~10 falloff

    RhalgrsBeacon = 28842, // Boss->self, 9.3s cast, visual
    RhalgrsBeaconKnockback = 28856, // Helper->self, 10.0s cast, knockback 50
    RhalgrsBeaconAOE = 29460, // Helper->self, 10.3s cast, range 10 aoe
    HellOfLightning = 28845, // Boss->self, 3.0s cast, visual
    Shock = 28851, // LightningOrb->self, 6.0s cast, range 8 aoe

    BronzeWork = 28843, // Boss->self, 6.5s cast, visual (two sets of cones)
    BronzeLightning = 28857, // Helper->self, 7.0s cast, range 50 45-degree cone, 4 casts then 4 more casts
}
