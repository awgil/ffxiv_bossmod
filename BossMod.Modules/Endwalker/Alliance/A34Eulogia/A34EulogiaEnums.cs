namespace BossMod.Endwalker.Alliance.A34Eulogia;

public enum OID : uint
{
    Boss = 0x4086, // R11.000, x1
    Helper = 0x233C, // R0.500, x14, 523 type
    Avatar = 0x4087, // R11.000, spawn during fight
    HydrostasisQuick = 0x408A, // R1.000, spawn during fight
    WardensFlame = 0x408B, // R2.400, spawn during fight
    FistOfWrath = 0x408C, // R3.600, spawn during fight
    FistOfJudgment = 0x408D, // R3.600, spawn during fight
    Trident = 0x408E, // R3.000, spawn during fight
    Hieroglyphika = 0x408F, // R1.000, spawn during fight
    EudaimonEorzeaVisualHelper = 0x4028, // R1.000, spawn during fight
    MatronsBreathHelper = 0x4090, // R1.000, spawn during fight, related somehow to MatronsBreath
    BlueSafeZone = 0x1EB845, // R0.500, EventObj type, spawn during fight
    GoldSafeZone = 0x1EB846, // R0.500, EventObj type, spawn during fight
    BlueFlowers = 0x1EB843, // R0.500, EventObj type, spawn during fight
    GoldFlowers = 0x1EB844, // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttackVisual = 35326, // Boss->self, no cast, single-target, hits all 3 main tanks
    AutoAttackAOE = 35327, // Helper->player, no cast, single-target
    Sunbeam = 35328, // Boss->self, 5.0s cast, single-target, visual (tankbusters)
    SunbeamAOE = 35329, // Helper->players, 5.0s cast, range 6 circle, aoe tankbuster
    Teleport = 35330, // Boss->location, no cast, single-target
    DawnOfTime = 35331, // Boss->self, 5.0s cast, range 70 circle, raidwide
    Whorl = 35375, // Boss->self, 7.0s cast, range 40 circle, raidwide and arena transition

    QuintessencePrepare = 35336, // Boss->self, no cast, single-target, visual (before first form / eudaimon)
    FirstFormRight = 35338, // Boss->self, 7.0s cast, single-target, visual
    FirstFormLeft = 35341, // Boss->self, 7.0s cast, single-target, visual
    FirstFormDonut = 35344, // Boss->self, 7.0s cast, single-target, visual
    SecondFormRight = 35339, // Boss->self, 7.0s cast, single-target, visual
    SecondFormLeft = 35342, // Boss->self, 7.0s cast, single-target, visual
    SecondFormDonut = 35345, // Boss->self, 7.0s cast, single-target, visual
    ThirdFormRight = 35340, // Boss->self, 7.0s cast, single-target, visual
    ThirdFormLeft = 35343, // Boss->self, 7.0s cast, single-target, visual
    ThirdFormDonut = 35346, // Boss->self, 7.0s cast, single-target, visual
    Quintessence = 35350, // Boss->self, 4.0s cast, single-target, visual (execute prepared forms)
    QuintessenceMove1 = 35351, // Boss->location, no cast, single-target, teleport (not sure what the difference is)
    QuintessenceMove2 = 35352, // Boss->location, no cast, single-target, teleport (not sure what the difference is)
    QuintessenceMove3 = 35353, // Boss->location, no cast, single-target, teleport (not sure what the difference is)
    QuintessenceVisual1 = 35357, // Avatar->self, 0.5s cast, single-target, ??? (right after move)
    QuintessenceVisual2 = 35358, // Avatar->self, 0.5s cast, single-target, ??? (right after move)
    QuintessenceVisual3 = 35359, // Avatar->self, 0.5s cast, single-target, ??? (right after move)
    QuintessenceVisual4 = 35360, // Avatar->self, 0.5s cast, single-target, ??? (right after move)
    QuintessenceVisual5 = 35361, // Avatar->self, 0.5s cast, single-target, ??? (right after move)
    QuintessenceAOE1Right = 35354, // Helper->self, 4.8s cast, range 50 180-degree cone
    QuintessenceAOE1Left = 35355, // Helper->self, 4.8s cast, range 50 180-degree cone
    QuintessenceAOE1Donut = 35356, // Helper->self, 4.8s cast, range 8-50 donut
    QuintessenceAOE2Right = 36069, // Helper->self, 8.3s cast, range 50 180-degree cone
    QuintessenceAOE2Left = 36070, // Helper->self, 8.3s cast, range 50 180-degree cone
    QuintessenceAOE2Donut = 36071, // Helper->self, 8.3s cast, range 8-50 donut
    QuintessenceAOE3Right = 36072, // Helper->self, 11.9s cast, range 50 180-degree cone
    QuintessenceAOE3Left = 36073, // Helper->self, 11.9s cast, range 50 180-degree cone
    QuintessenceAOE3Donut = 36074, // Helper->self, 11.9s cast, range 8-50 donut
    QuintessenceFinish1 = 35337, // Boss->self, no cast, single-target, visual (after mechanic resolve)
    QuintessenceFinish2 = 36066, // Boss->self, no cast, single-target, visual (after mechanic resolve)

    LovesLight = 35376, // Boss->self, 4.0s cast, single-target, visual (menphina mechanic)
    FullBright = 35377, // Boss->self, 3.0s cast, single-target, visual (moons)
    FirstBlush1 = 35379, // Helper->self, 10.3s cast, range 80 width 25 rect
    FirstBlush2 = 35380, // Helper->self, 12.3s cast, range 80 width 25 rect
    FirstBlush3 = 35381, // Helper->self, 14.3s cast, range 80 width 25 rect
    FirstBlush4 = 35382, // Helper->self, 16.3s cast, range 80 width 25 rect

    SolarFans = 35387, // Boss->self, 3.0s cast, single-target, visual (azeyma mechanic)
    SolarFansAOE = 35388, // WardensFlame->location, 4.0s cast, width 10 rect charge
    RadiantRhythm = 35389, // Boss->self, no cast, range 100 circle, visual (move flames)
    TeleportFlame = 35390, // WardensFlame->location, no cast, single-target
    RadiantFlight = 35391, // Helper->self, 0.5s cast, range 20-30 donut 90-degree cone
    RadiantFinish = 35392, // Boss->self, 3.0s cast, single-target, visual (final explosions)
    RadiantFlourish = 35393, // WardensFlame->self, 3.0s cast, range 25 circle

    Hydrostasis = 35383, // Boss->self, 4.0s cast, single-target, 4.0s cast, single-target, visual (nymeia mechanic)
    TimeAndTide = 35378, // Boss->self, 6.0s cast, single-target, single-target, visual (althyk mechanic)
    HydrostasisAOE1 = 35384, // Helper->self, 7.0s cast, range 72 circle, knockback 28
    HydrostasisAOE2 = 35385, // Helper->self, 10.0s cast, range 72 circle, knockback 28
    HydrostasisAOE3 = 35386, // Helper->self, 13.0s cast, range 72 circle, knockback 28

    DestructiveBolt = 36076, // Boss->self, 6.0s cast, single-target, visual (stack)
    DestructiveBoltAOE = 36093, // Helper->players, 7.0s cast, range 6 circle, stack

    Hieroglyphika = 35395, // Boss->self, 5.0s cast, single-target (thaliak mechanic)
    HieroglyphikaAOE = 35396, // Helper->self, 3.0s cast, range 12 width 12 rect
    FistOfWrathAppear = 35397, // FistOfWrath->self, no cast, single-target, visual (portal)
    FistOfJudgmentAppear = 35398, // FistOfJudgment->self, no cast, single-target, visual (portal)
    HandOfTheDestroyerWrath = 35399, // Boss->self, 7.5s cast, single-target, visual (rhalgr mechanic)
    HandOfTheDestroyerJudgment = 35400, // Boss->self, 7.5s cast, single-target, visual (rhalgr mechanic)
    HandOfTheDestroyerWrathAOE = 35401, // FistOfWrath->self, 8.0s cast, range 90 width 40 rect
    HandOfTheDestroyerJudgmentAOE = 35402, // FistOfJudgment->self, 8.0s cast, range 90 width 40 rect

    MatronsBreath = 35403, // Boss->self, 3.0s cast, single-target, visual (nophica mechanic)
    Blueblossoms = 35404, // Helper->self, no cast, range 100 circle, gold tower explosion
    Giltblossoms = 35405, // Helper->self, no cast, range 100 circle, blue tower explosion

    TorrentialTridents = 35406, // Boss->self, 2.0s cast, single-target, visual (llymlaen mechanic)
    TorrentialTridentLanding = 35407, // Trident->self, no cast, range 80 circle, raidwide x6
    LightningBolt = 35408, // Trident->self, 5.0s cast, range 18 circle

    ByregotStrike = 35410, // Boss->location, 6.0s cast, range 8 circle aoe (byregot mechanic)
    ByregotStrikeKnockback = 35411, // Helper->self, 6.7s cast, range 45 circle, knockback 20
    ByregotStrikeAOE = 35412, // Helper->self, 6.7s cast, range 90 30-degree cone

    ThousandfoldThrustR = 35415, // Boss->self, 5.0s cast, single-target, visual (halone mechanic, right cleave)
    ThousandfoldThrustL = 35416, // Boss->self, 5.0s cast, single-target, visual (halone mechanic, left cleave)
    ThousandfoldThrustAOEFirst = 35417, // Helper->self, 6.3s cast, range 60 180-degree cone
    ThousandfoldThrustAOERest = 35418, // Helper->self, no cast, range 60 180-degree cone

    AsAboveSoBelowNald = 35419, // Boss->self, 5.0s cast, range 40 circle, visual (nald mechanic - orange)
    AsAboveSoBelowThal = 35420, // Boss->self, 5.0s cast, range 40 circle, visual (thal mechanic - blue)
    OnceBurnedFake = 36094, // Helper->self, 9.0s cast, range 6 circle, orange
    EverFireFake = 36095, // Helper->self, 9.0s cast, range 6 circle, blue
    OnceBurnedFirst = 36096, // Helper->self, 9.0s cast, range 6 circle, orange
    EverfireFirst = 36097, // Helper->self, 9.0s cast, range 6 circle, blue
    OnceBurnedRest = 36098, // Helper->self, no cast, range 6 circle, orange
    EverfireRest = 36099, // Helper->self, no cast, range 6 circle, blue
    ClimbingShotNald = 36106, // Boss->self, 8.0s cast, range 40 circle, visual (oschon mechanic after nald)
    ClimbingShotThal = 36107, // Boss->self, 8.0s cast, range 40 circle, visual (oschon mechanic after thal)
    ClimbingShotAOE1 = 35429, // Boss->self, no cast, range 40 circle, knockback 20
    ClimbingShotAOE2 = 35430, // Boss->self, no cast, range 40 circle, knockback 20
    ClimbingShotAOE3 = 35431, // Boss->self, no cast, range 40 circle, knockback 20
    ClimbingShotAOE4 = 35432, // Boss->self, no cast, range 40 circle, knockback 20
    SoaringMinuet = 35433, // Boss->self, 7.0s cast, range 40 270-degree cone

    EudaimonEorzea = 35372, // Boss->self, 22.2s cast, single-target, visual (raidwide x13)
    BuildersArt = 35362, // Helper->self, no cast, range 80 circle, raidwide
    DestroyersMight = 35363, // Helper->self, no cast, range 80 circle, raidwide
    WardensRadiance = 35364, // Helper->self, no cast, range 80 circle, raidwide
    TradersEquity = 35365, // Helper->self, no cast, range 80 circle, raidwide
    MatronsPlenty = 35366, // Helper->self, no cast, range 80 circle, raidwide
    KeepersGravity = 35367, // Helper->self, no cast, range 80 circle, raidwide
    FurysAmbition = 35368, // Helper->self, no cast, range 80 circle, raidwide
    LoversDevotion = 35369, // Helper->self, no cast, range 80 circle, raidwide
    ScholarsWisdom = 35370, // Helper->self, no cast, range 80 circle, raidwide
    SpinnersCunning = 35371, // Helper->self, no cast, range 80 circle, raidwide
    NavigatorsCommand = 35373, // Helper->self, no cast, range 80 circle, raidwide
    WanderersWhimsy = 35374, // Helper->self, no cast, range 80 circle, raidwide
    EudaimonEorzeaAOE = 36091, // Helper->self, 24.9s cast, range 40 circle, raidwide
}

public enum SID : uint
{
    Glow = 2056, // WardensFlame->WardensFlame/EudaimonEorzeaVisualHelper, extra=0x195/0x29E/0x29F/0x2A0/0x2A1/0x2A2/0x2A3/0x2A4/0x2A5/0x2A6/0x2A7/0x2A8/0x2A9
    Inscribed = 3732, // none->player, extra=0x0
    Bind = 2518, // none->player, extra=0x0
    BloomingGold = 3460, // none->player, extra=0x0
    BloomingBlue = 3459, // none->player, extra=0x0
}

public enum TetherID : uint
{
    HydrostasisQuick = 219, // HydrostasisQuick->Boss
}

public enum IconID : uint
{
    Sunbeam = 344, // player
    DestructiveBolt = 317, // player
    HieroglyphikaCW = 487, // HieroglyphikaIndicator
    HieroglyphikaCCW = 490, // HieroglyphikaIndicator
    Order1 = 398, // MatronsBreathHelper
    Order2 = 399, // MatronsBreathHelper
    Order3 = 400, // MatronsBreathHelper
    Order4 = 401, // MatronsBreathHelper

    ThousandfoldThrust1 = 388, // Boss
    ThousandfoldThrust2 = 389, // Boss
}
