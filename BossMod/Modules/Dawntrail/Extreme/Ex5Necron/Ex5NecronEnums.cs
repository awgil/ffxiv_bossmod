namespace BossMod.Dawntrail.Extreme.Ex5Necron;

public enum OID : uint
{
    Boss = 0x490B, // R20.000, x1
    AutoAttacker = 0x49B2, // R0.000-0.500, x1, Part type
    HelperUnknown = 0x4945, // R1.000, x4
    LoomingSpecter1 = 0x49DD, // R3.000, x1-3
    LoomingSpecter2 = 0x4910, // R15.750, x2-3
    Helper = 0x233C, // R0.500, x12-18 (spawn during fight), Helper type
    IcyHandsAdds = 0x4911, // R3.300, x0 (spawn during fight)
    BeckoningHands = 0x4912, // R5.500, x0 (spawn during fight)
    IcyHandsP1 = 0x490C, // R3.575, x0 (spawn during fight)
    AzureAether1 = 0x49B0, // R1.000, x0 (spawn during fight)
    AzureAether2 = 0x4914, // R1.000, x0 (spawn during fight)
    AzureAether3 = 0x4949, // R1.000, x0-1 (spawn during fight)
    IcyHandsTower = 0x4913, // R3.575, x0 (spawn during fight)
    IcyHandsDPSJail = 0x490D, // R3.575, x0 (spawn during fight)
    IcyHandsHealerJail = 0x490E, // R3.575, x0 (spawn during fight)
    IcyHandsTankJail = 0x490F, // R3.575, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 44615, // AutoAttacker->player, no cast, single-target
    BlueShockwaveCast = 44592, // Boss->self, 6.0+1.0s cast, single-target
    BlueShockwaveAOE = 44593, // Helper->self, no cast, range 100 100-degree cone
    BlueShockwaveRepeat = 44594, // Boss->self, no cast, single-target
    FearOfDeathRaidwide = 44550, // Boss->self, 5.0s cast, range 100 circle
    FearOfDeathPuddle = 44551, // Helper->location, 3.0s cast, range 3 circle
    ColdGripLeftSafe = 44553, // Boss->self, 5.0+1.0s cast, single-target
    ColdGripRightSafe = 44554, // Boss->self, 5.0+1.0s cast, single-target
    ColdGripAOE = 44612, // Helper->self, 6.0s cast, range 100 width 12 rect
    ExistentialDread = 44555, // Helper->self, 1.0s cast, range 100 width 24 rect
    MementoMoriDarkRight = 44565, // Boss->self, 5.0s cast, range 100 width 12 rect
    MementoMoriDarkLeft = 44566, // Boss->self, 5.0s cast, range 100 width 12 rect
    ChokingGraspInstant = 44552, // IcyHandsMiddleP1->self, no cast, range 24 width 6 rect
    ChokingGraspCast1 = 44567, // IcyHandsMiddleP1->self, 3.0s cast, range 24 width 6 rect
    SmiteOfGloom = 44602, // Helper->player, 4.0s cast, range 10 circle
    SmiteOfGloomCast = 44601, // Boss->self, 4.0s cast, single-target
    SoulReaping = 44556, // Boss->self, 4.0s cast, single-target
    TwofoldBlight = 44557, // Boss->self, 5.0s cast, single-target
    FourfoldBlight = 44558, // Boss->self, 5.0s cast, single-target
    AetherblightSides = 44608, // Helper->self, 1.0s cast, range 100 width 12 rect
    AetherblightMiddle = 45185, // Helper->self, 1.0s cast, range 100 width 12 rect
    AetherblightCircle = 45183, // Helper->self, 1.0s cast, range 20 circle
    AetherblightDonut = 45184, // Helper->self, 1.0s cast, range 16-60 donut - vfx is 15.8y but donut aoes are occasionally visually inaccurate
    ShockwaveParties = 44559, // Helper->self, no cast, range 100 20-degree cone
    ShockwavePairs = 44560, // Helper->self, no cast, range 100 20-degree cone
    TheEndsEmbraceCast = 44597, // Boss->self, 4.0s cast, single-target
    TheEndsEmbraceSpread = 44598, // Helper->player, no cast, range 3 circle
    AetherblightBoss1 = 44607, // Boss->self, no cast, single-target
    AetherblightBoss2 = 44563, // Boss->self, no cast, single-target
    AetherblightBoss3 = 44561, // Boss->self, no cast, single-target
    AetherblightBoss4 = 44562, // Boss->self, no cast, single-target
    GrandCrossRaidwide = 44568, // Boss->location, 7.0s cast, range 50 circle
    GrandCrossArenaChange = 44604, // Helper->location, 7.0s cast, range 9-60 donut
    GrandCrossLaser = 44569, // Helper->self, 0.5s cast, range 100 width 4 rect
    GrandCrossProximity = 44570, // Helper->self, 4.0s cast, range 100 width 100 rect
    GrandCrossPuddle = 44571, // Helper->location, 3.0s cast, range 3 circle
    GrandCrossSpread = 44572, // Helper->players, 5.0s cast, range 3 circle
    Shock = 44573, // Helper->self, 5.0s cast, range 3 circle
    Electrify = 44574, // Helper->self, no cast, range 50 circle, shock tower fail
    NeutronRingCast = 44575, // Boss->location, 7.0s cast, single-target
    NeutronRing = 44576, // Helper->self, no cast, range 50 circle
    FearOfDeathPuddleAdds = 44577, // Helper->location, 3.0s cast, range 3 circle
    ChokingGraspAdds = 44579, // IcyHandsAdds->self, 3.0s cast, range 24 width 6 rect
    MutedStruggle = 44578, // BeckoningHands->self/player, 3.0s cast, range 24 width 6 rect
    DarknessOfEternityCast = 44580, // Boss->self, 10.0s cast, single-target
    DarknessOfEternityRaidwide = 44581, // Helper->location, no cast, range 50 circle
    Inevitability = 44583, // Helper->self, no cast, range 50 circle, i think this teleports the player to the jail arena
    SpecterOfDeath = 44606, // Boss->self, 5.0s cast, single-target
    InvitationVisual = 44591, // LoomingSpecter2->self, 4.3+0.7s cast, range 36 width 10 rect
    Invitation = 44818, // Helper->self, 5.0s cast, range 36 width 10 rect
    RelentlessReaping = 44564, // Boss->self, 15.0s cast, single-target
    CropRotation = 44610, // Boss->self, 3.0s cast, single-target
    TheSecondSeason = 45167, // Boss->self, 8.0s cast, single-target
    TheFourthSeason = 45168, // Boss->self, 8.0s cast, single-target
    CircleOfLivesCast = 44599, // Boss->self, 4.0s cast, single-target
    CircleOfLives = 44600, // AzureAether1->self, 7.0s cast, range 3-50 donut
    MassMacabre = 44595, // Boss->self, 4.0s cast, single-target
    MacabreMark = 44819, // Helper->location, no cast, range 3 circle
    SpreadingFearTower = 44596, // IcyHandsTower->self, no cast, range 50 circle
    ChokingGraspDPSJail = 44584, // IcyHandsDPSJail->self, 3.0s cast, range 24 width 6 rect
    SpreadingFearDPSJail = 44585, // IcyHandsDPSJail->self, 10.0s cast, range 50 circle
    SpreadingFearTankJail = 44586, // IcyHandsTankJail->self, 5.0s cast, range 50 circle
    ChokingGraspTankJail = 44587, // IcyHandsTankJail->self/player, 5.0s cast, range 24 width 6 rect
    NecroticPulse = 44588, // IcyHandsHealerJail->self, no cast, range 24 width 6 rect, applies bleed
    ChillingFingers = 44589, // IcyHandsHealerJail->self/player, 5.0s cast, range 24 width 6 rect, applies slow
    ChokingGraspHealerJail = 44590, // IcyHandsHealerJail->self/player, 4.0s cast, range 24 width 6 rect, unavoidable damage
    DarknessOfEternityAddsEnrage = 44582, // Helper->location, no cast, range 50 circle

    DarknessOfEternityEnrage = 44613, // Boss->self, 10.0s cast, range 50 circle
}

public enum IconID : uint
{
    StoreCircle = 604, // Boss->self
    StoreDonut = 605, // Boss->self
    StoreInverseRect = 606, // Boss->self
    StoreRect = 607, // Boss->self
    GrandCrossSpread = 611, // player->self
    SmiteOfGloom = 612, // player->self
    EndsEmbrace = 614, // player->self
    BlueShockwave = 615, // Boss->player
    Tankbuster = 381, // player->self
}

public enum SID : uint
{
    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
}
