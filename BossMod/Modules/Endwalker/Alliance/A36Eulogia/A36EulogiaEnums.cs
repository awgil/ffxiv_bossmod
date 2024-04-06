namespace BossMod.Endwalker.Alliance.A36Eulogia;

public enum OID : uint
{
    Boss = 0x4086, // R11.000, x1
    Helper = 0x233C, // R0.500, x14, 523 type
    Avatar = 0x4087, // R11.000, spawn during fight
    Trident = 0x408E, // R3.000, spawn during fight
    FistOfWrath = 0x408C, // R3.600, spawn during fight
    FistOfJudgment = 0x408D, // R3.600, spawn during fight
    WardensFlame = 0x408B, // R2.400, spawn during fight
    HydrostasisQuick = 0x408A, // R1.000, spawn during fight
    Hieroglyphika = 0x408F, // R1.000, spawn during fight
    EudaimonEorzeaVisualHelper = 0x4028, // R1.000, spawn during fight
    MatronsBreathHelper = 0x4090, // R1.000, spawn during fight, related somehow to MatronsBreath
    GoldSafeZone = 0x1EB846, // R0.500, EventObj type, spawn during fight
    BlueSafeZone = 0x1EB845, // R0.500, EventObj type, spawn during fight
    GoldFlowers = 0x1EB844, // R0.500, EventObj type, spawn during fight
    BlueFlowers = 0x1EB843, // R0.500, EventObj type, spawn during fight
};

public enum AID : uint
{
    AutoAttackStarter = 35326, // Eulogia->self, no cast, single-target, hits all 3 main tanks
    AutoAttack = 35327, // Helper->player, no cast, single-target

    Teleport = 35330, // Eulogia->location, no cast, single-target
    Visual = 35336, // Eulogia->self, no cast, single-target
    Visual2 = 35337, // Eulogia->self, no cast, single-target
    Visual3 = 36066, // Eulogia->self, no cast, single-target

    QuintessenceVisual1 = 35360, // Avatar->self, 0.5s cast, single-target
    QuintessenceVisual2 = 35361, // Avatar->self, 0.5s cast, single-target
    QuintessenceVisual3 = 35358, // Avatar->self, 0.5s cast, single-target
    QuintessenceVisual4 = 35359, // Avatar->self, 0.5s cast, single-target
    QuintessenceVisual5 = 35357, // Avatar->self, 0.5s cast, single-target
    FirstFormRight = 35338, // Eulogia->self, 7.0s cast, single-target
    FirstFormLeft = 35341, // Eulogia->self, 7.0s cast, single-target
    FirstFormAOE = 35344, // Eulogia->self, 7.0s cast, single-target
    SecondFormRight = 35339, // Eulogia->self, 7.0s cast, single-target
    SecondFormLeft = 35342, // Eulogia->self, 7.0s cast, single-target
    SecondFormAOE = 35345, // Eulogia->self, 7.0s cast, single-target
    ThirdFormRight = 35340, // Eulogia->self, 7.0s cast, single-target
    ThirdFormLeft = 35343, // Eulogia->self, 7.0s cast, single-target
    ThirdFormAOE = 35346, // Eulogia->self, 7.0s cast, single-target
    QuintessenceSetup = 35350, // Eulogia->self, 4.0s cast, single-target
    Quintessence1stSpot = 35351, // Eulogia->location, no cast, single-target, teleport to 1st spot
    Quintessence2ndSpot = 35352, // Eulogia->location, no cast, single-target, teleport to 2st spot
    Quintessence3rdSpot = 35353, // Eulogia->location, no cast, single-target, teleport to 3rd spot
    QuintessenceFirstRight = 35354, // Helper->self, 4.8s cast, range 50 180-degree cone
    QuintessenceFirstLeft = 35355, // Helper->self, 4.8s cast, range 50 180-degree cone
    QuintessenceFirstAOE = 35356, // Helper->self, 4.8s cast, range 8-50 donut
    QuintessenceSecondRight = 36069, // Helper->self, 8.3s cast, range 50 180-degree cone
    QuintessenceSecondLeft = 36070, // Helper->self, 8.3s cast, range 50 180-degree cone
    QuintessenceSecondAOE = 36071, // Helper->self, 8.3s cast, range 8-50 donut
    QuintessenceThirdRight = 36072, // Helper->self, 11.9s cast, range 50 180-degree cone
    QuintessenceThirdLeft = 36073, // Helper->self, 11.9s cast, range 50 180-degree cone
    QuintessenceThirdAOE = 36074, // Helper->self, 11.9s cast, range 8-50 donut

    SunbeamSelf = 35328, // Eulogia->self, 5.0s cast, single-target, visual
    SunbeamTankBuster = 35329, // Helper->players, 5.0s cast, range 6 circle, tankbusters

    DawnOfTime = 35331, // Eulogia->self, 5.0s cast, range 70 circle
    TheWhorl = 35375, // Eulogia->self, 7.0s cast, range 40 circle, raidwide

    LovesLight = 35376, // Eulogia->self, 4.0s cast, single-target
    FullBright = 35377, // Eulogia->self, 3.0s cast, single-target
    FirstBlush1 = 35379, // Helper->self, 10.3s cast, range 80 width 25 rect
    FirstBlush2 = 35380, // Helper->self, 12.3s cast, range 80 width 25 rect
    FirstBlush3 = 35381, // Helper->self, 14.3s cast, range 80 width 25 rect
    FirstBlush4 = 35382, // Helper->self, 16.3s cast, range 80 width 25 rect

    SolarFans = 35387, // Eulogia->self, 3.0s cast, single-target
    SolarFansAOE = 35388, // WardensFlame->location, 4.0s cast, width 10 rect charge
    RadiantRhythm = 35389, // Eulogia->self, no cast, range 100 circle
    TeleportFlame = 35390, // WardensFlame->location, no cast, single-target
    RadiantFlight = 35391, // Helper->self, 0.5s cast, range 20-30 donut 90-degree cone
    RadiantFlourish = 35393, // WardensFlame->self, 3.0s cast, range 25 circle
    RadiantFinish = 35392, // Eulogia->self, 3.0s cast, single-target

    TimeAndTide = 35378, // Eulogia->self, 6.0s cast, single-target, single-target, visual (speed up time)
    Hydrostasis = 35383, // Eulogia->self, 4.0s cast, single-target, 4.0s cast, single-target, visual (knockbacks)
    HydrostasisAOE1 = 35384, // Helper->self, 7.0s cast, range 72 circle, knockback 28, away from source
    HydrostasisAOE2 = 35385, // Helper->self, 10.0s cast, range 72 circle, knockback 28, away from source
    HydrostasisAOE3 = 35386, // Helper->self, 13.0s cast, range 72 circle, knockback 28, away from source

    DestructiveBolt = 36076, // Eulogia->self, 6.0s cast, single-target, tankbuster visual
    DestructiveBoltStack = 36093, // Helper->players, 7.0s cast, range 6 circle, stack marker

    HieroglyphikaVisual = 35395, // Eulogia->self, 5.0s cast, single-target
    HieroglyphikaRect = 35396, // Helper->self, 3.0s cast, range 12 width 12 rect

    FistOfWrathVisual = 35397, // FistOfWrath->self, no cast, single-target
    FistOfJudgmentVisual = 35398, // FistOfJudgment->self, no cast, single-target
    HandOfTheDestroyerWrath = 35399, // Eulogia->self, 7.5s cast, single-target
    HandOfTheDestroyerJudgment = 35400, // Eulogia->self, 7.5s cast, single-target
    HandOfTheDestroyerWrathAOE = 35401, // FistOfWrath->self, 8.0s cast, range 90 width 40 rect
    HandOfTheDestroyerJudgmentAOE = 35402, // FistOfJudgment->self, 8.0s cast, range 90 width 40 rect

    MatronsBreath = 35403, // Eulogia->self, 3.0s cast, single-target, visual (color towers)
    Giltblossoms = 35405, // Helper->self, no cast, range 100 circle, blue tower explosion
    Blueblossoms = 35404, // Helper->self, no cast, range 100 circle, gold tower explosion

    TorrentialTridents = 35406, // Eulogia->self, 2.0s cast, single-target
    Landing = 35407, // Trident->self, no cast, range 80 circle
    LightningBolt = 35408, // Trident->self, 5.0s cast, range 18 circle
    ByregotStrikeJump = 35410, // Eulogia->location, 6.0s cast, range 8 circle
    ByregotStrikeKnockback = 35411, // Helper->self, 6.7s cast, range 45 circle, knockback 20, away from source
    ByregotStrikeCone = 35412, // Helper->self, 6.7s cast, range 90 30-degree cone

    ThousandfoldThrustVisual1 = 35415, // Eulogia->self, 5.0s cast, single-target
    ThousandfoldThrustVisual2 = 35416, // Eulogia->self, 5.0s cast, single-target
    ThousandfoldThrustAOEFirst = 35417, // Helper->self, 6.3s cast, range 60 180-degree cone
    ThousandfoldThrustAOERest = 35418, // Helper->self, no cast, range 60 180-degree cone

    AsAboveSoBelow = 35419, // Eulogia->self, 5.0s cast, range 40 circle
    AsAboveSoBelowAlt = 35420, // Eulogia->self, 5.0s cast, range 40 circle

    ClimbingShotVisual = 36106, // Eulogia->self, 8.0s cast, range 40 circle, visual
    ClimbingShotVisual2 = 36107, // Eulogia->self, 8.0s cast, range 40 circle, visual
    ClimbingShot1 = 35431, // Eulogia->self, no cast, range 40 circle, knockback 20, away from source
    ClimbingShot2 = 35429, // Eulogia->self, no cast, range 40 circle, knockback 20, away from source
    ClimbingShot3 = 35430, // Eulogia->self, no cast, range 40 circle, knockback 20, away from source
    ClimbingShot4 = 35432, // Eulogia->self, no cast, range 40 circle, knockback 20, away from source

    OnceBurnedFake = 36094, // Helper->self, 9.0s cast, range 6 circle
    EverFireFake = 36095, // Helper->self, 9.0s cast, range 6 circle
    OnceBurnedFirst = 36096, // Helper->self, 9.0s cast, range 6 circle
    EverfireFirst = 36097, // Helper->self, 9.0s cast, range 6 circle
    OnceBurnedRest = 36098, // Helper->self, no cast, range 6 circle
    EverfireRest = 36099, // Helper->self, no cast, range 6 circle

    SoaringMinuet = 35433, // Eulogia->self, 7.0s cast, range 40 270-degree cone

    TheBuildersArt = 35362, // Helper->self, no cast, range 80 circle, raidwide
    TheDestroyersMight = 35363, // Helper->self, no cast, range 80 circle, raidwide
    TheWardensRadiance = 35364, // Helper->self, no cast, range 80 circle, raidwide
    TheTradersEquity = 35365, // Helper->self, no cast, range 80 circle, raidwide
    TheMatronsPlenty = 35366, // Helper->self, no cast, range 80 circle, raidwide
    TheKeepersGravity = 35367, // Helper->self, no cast, range 80 circle, raidwide
    TheFurysAmbition = 35368, // Helper->self, no cast, range 80 circle, raidwide
    TheLoversDevotion = 35369, // Helper->self, no cast, range 80 circle, raidwide
    TheScholarsWisdom = 35370, // Helper->self, no cast, range 80 circle, raidwide
    TheSpinnersCunning = 35371, // Helper->self, no cast, range 80 circle, raidwide
    TheNavigatorsCommand = 35373, // Helper->self, no cast, range 80 circle, raidwide
    TheWanderersWhimsy = 35374, // Helper->self, no cast, range 80 circle, raidwide
    EudaimonEorzea1 = 35372, // Eulogia->self, 22.2s cast, single-target
    EudaimonEorzea2 = 36091, // Helper->self, 24.9s cast, range 40 circle    
};

public enum SID : uint
{
    Glow = 2056, // WardensFlame->WardensFlame/EudaimonEorzeaVisualHelper, extra=0x195/0x29E/0x29F/0x2A0/0x2A1/0x2A2/0x2A3/0x2A4/0x2A5/0x2A6/0x2A7/0x2A8/0x2A9
    Inscribed = 3732, // none->player, extra=0x0
    Bleeding = 3077, // none->player, extra=0x0
    Bleeding2 = 3078, // none->player, extra=0x0
    Bind = 2518, // none->player, extra=0x0
    BloomingGold = 3460, // none->player, extra=0x0
    BloomingBlue = 3459, // none->player, extra=0x0
};

public enum TetherID : uint
{
    HydrostasisQuick = 219, // HydrostasisQuick->Eulogia
};

public enum IconID : uint
{
    Stackmarker = 317, // player
    Sunbeam = 344, // player
    ClockwiseHieroglyphika = 487, // HieroglyphikaIndicator
    CounterClockwiseHieroglyphika = 490, // HieroglyphikaIndicator
    ThousandfoldThrust1 = 388, // Eulogia
    ThousandfoldThrust2 = 389, // Eulogia
    Order1 = 398, // MatronsBreathHelper
    Order2 = 399, // MatronsBreathHelper
    Order3 = 400, // MatronsBreathHelper
    Order4 = 401, // MatronsBreathHelper
};
