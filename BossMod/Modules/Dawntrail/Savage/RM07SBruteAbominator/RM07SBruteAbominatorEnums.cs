namespace BossMod.Dawntrail.Savage.RM07SBruteAbominator;

public enum OID : uint
{
    Boss = 0x4783, // R7.000-19.712, x1
    Helper = 0x233C, // R0.500, x32, Helper type
    Building = 0x4785, // R7.000, x5
    BruteAbombinatorPart = 0x481C, // R0.000-0.500, x1, Part type
    BloomingAbomination = 0x4784, // R3.400, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 42330, // Boss->player, no cast, single-target
    AutoAttackPart = 43157, // _Gen_BruteAbombinator->player, no cast, single-target
    AutoAttackAdds = 872, // _Gen_BloomingAbomination->player, no cast, single-target

    BrutalImpact = 42331, // Boss->self, 5.0s cast, single-target
    BrutalImpactRepeat = 42332, // Boss->self, no cast, range 60 circle

    P1StoneringerClub = 42333, // Boss->self, 2.0+3.5s cast, single-target
    P1StoneringerSword = 42334, // Boss->self, 2.0+3.5s cast, single-target
    P2StoneringerClub = 42367, // Boss->self, 2.0+3.5s cast, single-target
    P2StoneringerSword = 42368, // Boss->self, 2.0+3.5s cast, single-target

    SmashHere = 42335, // Boss->self, 3.0+1.0s cast, single-target
    SmashThere = 42336, // Boss->self, 3.0+1.0s cast, single-target

    P1BrutishSwingOut = 42337, // Helper->self, 4.0s cast, range 12 circle
    P1BrutishSwingIn = 42338, // Helper->self, 4.0s cast, range 9-60 donut

    BrutalSmash1 = 42341, // Boss->players, no cast, range 6 circle, tankbuster
    BrutalSmash2 = 42342, // Boss->players, no cast, range 6 circle, tankbuster
    BrutalSmashLocation = 42344, // Boss->location, no cast, range 6 circle, what the hell is this

    Unk1 = 42262, // Boss->location, no cast, single-target

    SporeSacVisual = 42345, // Boss->self, 3.0s cast, single-target
    SporeSac = 42346, // Helper->location, 4.0s cast, range 8 circle

    Pollen = 42347, // Helper->location, 4.0s cast, range 8 circle

    SinisterSeedsVisual = 42349, // Boss->self, 4.0+1.0s cast, single-target
    SinisterSeedsSpread = 42350, // Helper->players, 7.0s cast, range 6 circle
    SinisterSeedsChase = 42353, // Helper->location, 3.0s cast, range 7 circle
    RootsOfEvil = 42354, // Helper->location, 3.0s cast, range 12 circle

    ImpactVisual = 42356, // Boss->self, no cast, single-target
    Impact = 42355, // Helper->players, no cast, range 6 circle

    TendrilsOfTerror1 = 42352, // Helper->self, 3.0s cast, range 60 width 4 cross
    TendrilsOfTerror2 = 42394, // Helper->self, 3.0s cast, range 60 width 4 cross
    TendrilsOfTerror3 = 42397, // Helper->self, 3.0s cast, range 60 width 4 cross
    TendrilsOfTerrorAOE1 = 42351, // Helper->self, 3.0s cast, range 10 circle, not sure what these 3 are used for, they don't deal damage
    TendrilsOfTerrorAOE2 = 42393, // Helper->self, 3.0s cast, range 10 circle
    TendrilsOfTerrorAOE3 = 42396, // Helper->self, 3.0s cast, range 10 circle

    WindingWildwinds = 43277, // _Gen_BloomingAbomination->self, 7.0s cast, range 5-60 donut
    CrossingCrosswinds = 43278, // _Gen_BloomingAbomination->self, 7.0s cast, range 50 width 10 cross
    HurricaneForce = 42348, // _Gen_BloomingAbomination->self, 6.0s cast, range 60 circle
    QuarrySwamp = 42357, // Boss->self, 4.0s cast, range 60 circle, LOS petrify

    Explosion = 42358, // Helper->location, 9.0s cast, range 60 circle
    PulpSmashVisual = 42359, // Boss->self, 3.0+2.0s cast, single-target
    PulpSmashJump = 42360, // Boss->player, no cast, single-target
    PulpSmashStack = 42361, // Helper->players, no cast, range 6 circle
    TheUnpotted = 42363, // Helper->self, no cast, range 60 30?-degree cone
    ItCameFromTheDirt = 42362, // Helper->location, 2.0s cast, range 6 circle
    NeoBombarianSpecial = 42364, // Boss->self, 8.0s cast, range 60 circle

    GrapplingIvy = 42365, // Boss->location, no cast, single-target

    P2BrutishSwingJump1 = 42380, // Boss->location, 4.0+3.8s cast, single-target
    P2BrutishSwingJump2 = 42381, // Boss->location, 4.0+3.8s cast, single-target
    P2BrutishSwingJump3 = 42382, // Boss->location, 4.0+3.8s cast, single-target
    P2BrutishSwingJump4 = 42383, // Boss->location, 4.0+3.8s cast, single-target
    P2BrutishSwingJump5 = 42384, // Boss->location, 4.0+3.8s cast, single-target
    P2BrutishSwingJump6 = 42385, // Boss->location, 4.0+3.8s cast, single-target

    P3BrutishSwingJump1 = 42402, // Boss->location, 3.0+3.8s cast, single-target
    P3BrutishSwingJump2 = 42411, // Boss->location, 3.0+3.8s cast, single-target
    P3BrutishSwingJump3 = 42412, // Boss->location, 3.0+3.8s cast, single-target

    BrutishSwingVisual1 = 42339, // Boss->self, no cast, single-target
    BrutishSwingVisual2 = 42340, // Boss->self, no cast, single-target
    BrutishSwingVisual3 = 42388, // Boss->self, no cast, single-target
    BrutishSwingVisual4 = 42389, // Boss->self, no cast, single-target
    BrutishSwingVisual5 = 42556, // Boss->self, no cast, single-target
    BrutishSwingVisual6 = 42557, // Boss->self, no cast, single-target
    BrutishSwingVisual7 = 42404, // Boss->self, no cast, single-target
    BrutishSwingVisual8 = 42406, // Boss->self, no cast, single-target

    P2BrutishSwingOut = 42386, // Helper->self, 8.1s cast, range 25 180-degree cone
    P2BrutishSwingIn = 42387, // Helper->self, 8.1s cast, range 21.7?-88 donut, assuming hitbox size+2 similar to p1

    P2GlowerPowerVisual = 42373, // Boss->self, 2.7+1.3s cast, single-target
    ElectrogeneticForce = 42374, // Helper->players, no cast, range 6 circle
    P2GlowerPower = 43340, // Helper->self, 4.0s cast, range 65 width 14 rect

    RevengeOfTheVines = 42375, // Boss->self, 5.0s cast, range 60 circle
    RevengeOfTheVinesInstant = 42553, // Boss->self, no cast, range 60 circle

    ThornyDeathmatch = 42376, // Boss->self, 3.0s cast, single-target

    AbominableBlinkVisual = 42377, // Boss->self, 5.3+1.3s cast, single-target
    AbominableBlink = 43156, // Helper->players, no cast, range 60 circle
    SporesplosionVisual = 42378, // Boss->self, 4.0s cast, single-target
    Sporesplosion = 42379, // Helper->location, 5.0s cast, range 8 circle

    DemolitionDeathmatch = 42390, // Boss->self, 3.0s cast, single-target

    P2StrangeSeedsVisual = 42391, // Boss->self, 4.0s cast, single-target
    P3StrangeSeedsVisual = 43274, // Boss->self, 4.0s cast, single-target
    StrangeSeedsSpread = 42392, // Helper->players, 5.0s cast, range 6 circle

    KillerSeeds = 42395, // Helper->players, 5.0s cast, range 6 circle
    Powerslam = 42398, // Boss->location, 6.0s cast, range 60 circle

    Stoneringer2Stoneringers1 = 42401, // Boss->self, 2.0+3.5s cast, single-target
    Stoneringer2Stoneringers2 = 42400, // Boss->self, 2.0+3.5s cast, single-target

    P3BrutishSwingOut = 42403, // Helper->self, 6.7s cast, range 25 180-degree cone
    P3BrutishSwingIn = 42405, // Helper->self, 6.7s cast, range ?-88 donut

    LashingLariatJump1 = 42407, // Boss->location, 3.5+0.5s cast, single-target
    LashingLariatJump2 = 42409, // Boss->location, 3.5+0.5s cast, single-target
    LashingLariat1 = 42408, // Helper->self, 4.0s cast, range 70 width 32 rect
    LashingLariat2 = 42410, // Helper->self, 4.0s cast, range 70 width 32 rect

    P3GlowerPowerVisual = 43338, // Boss->self, 0.7+1.3s cast, single-target
    P3GlowerPower = 43358, // Helper->self, 2.0s cast, range 65 width 14 rect
    SlaminatorVisual = 42413, // Boss->location, 4.0+1.0s cast, single-target
    Slaminator = 42414, // Helper->self, 5.0s cast, range 8 circle

    DebrisDeathmatch = 42416, // Boss->self, 3.0s cast, single-target

    SpecialBombarianSpecialVisual = 42417, // Boss->location, 10.0s cast, single-target
    SpecialBombarianSpecialJump = 42418, // Boss->location, no cast, single-target
    SpecialBombarianSpecialEnrage = 42419, // Helper->self, no cast, range 60 circle
}

public enum IconID : uint
{
    SporeSac = 375, // player->self
    SinisterSeed = 466, // player->self
    PulpSmash = 161, // player->self
    KillerSeed = 93, // player->self
    Flare = 327, // player->self
}

public enum TetherID : uint
{
    WallTether = 326, // Wall->Boss
    ThornsOfDeathTank = 338, // Boss->player
    ThornsOfDeath = 325, // Boss/Wall->player
    ThornsOfDeathPre = 84, // Wall->player
    WallTether2 = 339, // Wall->Boss
}

public enum SID : uint
{
    Unk0 = 2056, // none->Boss, extra=0x38A/0x388/0x377/0x389
    Unk1 = 4434, // none->Boss, extra=0x1
    Unk2 = 4435, // none->Boss, extra=0x16
    ThornsOfDeathI = 4499, // none->player, extra=0x0
    ThornsOfDeathII = 4500, // none->player, extra=0x0
    ThornsOfDeathIII = 4501, // none->player, extra=0x0
    ThornsOfDeathIV = 4502, // none->player, extra=0x0
    ThornsOfDeathI2 = 4466, // none->player, extra=0x0
    ThornsOfDeathII2 = 4467, // none->player, extra=0x0
    ThornsOfDeathIII2 = 4468, // none->player, extra=0x0
    ThornsOfDeathIV2 = 4469, // none->player, extra=0x0
    StoneCurse = 4537, // Boss->BloomingAbomination/player, extra=0x0

}
