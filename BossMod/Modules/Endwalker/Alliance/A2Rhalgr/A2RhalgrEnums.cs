namespace BossMod.Endwalker.Alliance.A2Rhalgr
{
    public enum OID : uint
    {
        Boss = 0x38D6,
        FistOfWrath = 0x38D7, // x1, red portal
        FistOfJudgment = 0x38D8, // x1, blue portal
        LightningOrb = 0x38D9, // spawn during fight
        Helper = 0x233C, // x19
    };

    public enum AID : uint
    {
        AutoAttack = 28835,
        Teleport = 28846, // Boss->location, no cast

        LightningReign = 28837, // Boss->self, 5.0s cast, raidwide
        DestructiveBolt = 28836, // Boss->self, 4.0s cast, tankbuster visual
        DestructiveBoltAOE = 28852, // Helper->players, 5.0s cast, range 3 aoe on 3 tanks

        AdventOfTheEighth = 28839, // Boss->self, 4.0s cast, visual
        PortalWrath = 29347, // FistOfWrath->self, no cast, visual (portal appears)
        PortalJudgment = 29348, // FistOfJudgment->self, no cast, visual (portal appears)
        PortalWrathBroken = 29349, // FistOfWrath->self, no cast, visual (portal appears, combo with broken world)
        HandOfTheDestroyerWrath = 28840, // Boss->self, 9.0s cast, visual, standalone
        HandOfTheDestroyerJudgment = 28841, // Boss->self, 9.0s cast, visual, standalone
        HandOfTheDestroyerWrathBroken = 28844, // Boss->self, 9.0s cast, visual, combo with broken world
        HandOfTheDestroyerWrathAOE = 28847, // FistOfWrath->self, 9.4s cast, range 90 half-width 20 rect
        HandOfTheDestroyerJudgmentAOE = 28848, // FistOfJudgment->self, 9.4s cast, range 90 half-width 20 rect
        HandOfTheDestroyerWrathBrokenVisual = 29148, // FistOfWrath->self, no cast, visual (fist hitting comet)
        BrokenWorld = 28838, // Boss->self, 3.0s cast, visual
        BrokenWorldAOE = 28854, // Helper->self, 10.6s cast, range 96 aoe with ? falloff
        BrokenShards = 29147, // Helper->location, 14.7s cast, visual (large comet hit location, before it is split)
        BrokenShardsVisual = 28855, // Helper->self, no cast, visual (show aoe cicles where comet shards hit)
        BrokenShardsAOE = 29149, // Helper->self, no cast, range 20 aoe

        RhalgrsBeacon = 28842, // Boss->self, 9.3s cast, visual
        RhalgrsBeaconKnockback = 28856, // Helper->self, 10.0s cast, knockback 50
        RhalgrsBeaconAOE = 29460, // Helper->self, 10.3s cast, range 10 aoe

        //_Gen_Hand_of_the_Destroyer = 28849, // FistOfWrath->self, no cast
        //_Gen_ = 29152, // Helper->location, 14.7s cast
        //_Gen_Bronze_Work = 28843, // Boss->self, 6.5s cast
        //_Gen_Bronze_Lightning = 28857, // Helper->self, 7.0s cast
        //_Gen_Striking_Meteor = 28859, // Helper->location, 3.0s cast
        //_Gen_Lightning_Storm = 28858, // Helper->players, 8.0s cast
        //_Gen_Hell_of_Lightning = 28845, // Boss->self, 3.0s cast
        //_Gen_Shock = 28851, // LightningOrb->self, 6.0s cast
    };

    public enum SID : uint
    {
        None = 0,
    }

    public enum TetherID : uint
    {
        None = 0,
    }

    public enum IconID : uint
    {
        None = 0,
    }
}
