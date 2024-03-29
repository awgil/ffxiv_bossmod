namespace BossMod.Endwalker.Ultimate.DSW2;

public enum OID : uint
{
    ArenaFeatures = 0x1EA1A1, // R2.000, x1, EventObj type

    BossP2 = 0x313C, // R5.001, x1 - king thordan - p2
    SerZephirin = 0x3130, // R4.002, x1 - ??
    SerAdelphel = 0x3139, // R4.002, x1 - p1, p2
    SerGrinnaux = 0x313A, // R4.002, x1 - p1, p2
    SerCharibert = 0x313B, // R4.002, x1 - ??
    SerJanlenoux = 0x3158, // R4.002, x1 - p2
    SerVellguine = 0x3159, // R4.002, x1 - p2
    SerPaulecrain = 0x315A, // R4.002, x1 - p2
    SerIgnasse = 0x315B, // R4.002, x1 - p2
    SerHermenost = 0x315C, // R4.002, x1 - p2
    SerGuerrique = 0x315D, // R4.002, x1 - p2
    SerHaumeric = 0x315E, // R4.002, x1 - p2
    SerNoudenet = 0x315F, // R4.002, x1 - p2
    //_Gen_Actor_3316 = 0x3316, // R3.800, x1
    Helper = 0x233C, // R0.500, x24
    Brightsphere = 0x330E, // R1.000, spawn during fight by Shining Blade
    HolyComet = 0x312F, // R1.440, spawn during fight by Holy Comet
    VoidzoneFire = 0x1EB686, // R0.500, EventObj type, spawn during fight - fire voidzones spawned on intercards during meteors
    VoidzoneIce = 0x1EB682, // R0.500, EventObj type, spawn during fight - ice voidzones spawned on hiemal storm targets during meteors

    BossP3 = 0x313D, // R8.019, nidhogg - p3
    NidhoggDrake = 0x313E, // R8.019, x8 - p3, x2 - p4
    Background1 = 0x34FB, // R3.600, x3 - p3/p4 (these are fighting in background, outside the arena)
    Background2 = 0x34FC, // R5.000, x3 - p3
    Background3 = 0x34FD, // R0.500, x1 - p3
    Background4 = 0x34FE, // R0.500, x1 - p3
    Background5 = 0x34FF, // R0.500, x1 - p3
    Background6 = 0x3500, // R12.000, x3 - p3
    Background7 = 0x3501, // R6.000, x1 - p3
    Background8 = 0x3502, // R0.500, x1 - p3

    LeftEye = 0x3141, // R4.000, x1 - p4
    RightEye = 0x3142, // R4.000, x1 - p4
    NidhoggP4 = 0x3147, // R12.000, x1 - p4
    Alphinaud = 0x35C9, // R0.500, x1 - p4
    Haurchefant = 0x333D, // R0.500, x1 - p4
    Ysayle = 0x333E, // R0.500, x1 - p4
    Estinien = 0x333F, // R0.500, x1 - p4
    AzurePrice = 0x313F, // R1.000-2.000, x4 spawn during p4 (blue orb)
    GildedPrice = 0x3140, // R1.000-2.000, x2 spawn during p4 (yellow orb)

    SpearOfTheFury = 0x2E22, // R2.000, x1 - intermission
    //_Gen_Actorfd8a1 = 0xFD8A1, // R1.000, EventNpc type, x1 - intermission
    //_Gen_Actor1eb681 = 0x1EB681, // R0.500, EventObj type, x1 - intermission

    BossP5 = 0x3143, // R5.001, king thordan, x1 - p5
    Vedrfolnir = 0x3166, // R12.000, x1 - p5
    DarkscaleVisual = 0x3167, // R7.000, x1 - p5
    VidofnirVisual = 0x3168, // R6.000, x1 - p5
    Darkscale = 0x3156, // R16.800, x1 - p5
    Vidofnir = 0x3157, // R14.400, x1 - p5
    VoidzoneTwister = 0x1E8910, // R0.500, EventObj type, x8 spawn during p5
    VoidzoneLiquidHeaven = 0x1EB684, // R0.500, EventObj type, x5 spawn during p5
    VoidzoneCleanse = 0x1EB685, // R0.500, EventObj type, x4 spawn during p5, where wings of salvations aoes were cast (cleanses dooms)
    MeteorVisual = 0x1EB687, // R0.500, EventObj type, x8 - p5
    MeteorCircle = 0x3660, // R3.000, x8 spawn during p5

    NidhoggP6 = 0x3144, // R19.000, x1 - p6
    HraesvelgrP6 = 0x3145, // R19.000, x1 - p6
    ScarletPrice = 0x33B6, // R1.000, spawn during p6
    VoidzoneAhkMorn = 0x1EB683, // R0.500, EventObj type, spawn during p6

    DragonKingThordan = 0x3148, // R8.000, x1 - p7
};

public enum AID : uint
{
    AutoAttackBackground = 870, // Background1/Background2/Background3/Background4/Background5/Background6/Background7->location/Background1/Background2/Background3/Background4/Background5/Background6/Background7, no cast, single-target

    // phase 2
    AutoAttackP2 = 25531, // BossP2->mt, no cast, range 10 ?-degree cone
    TeleportP2 = 25540, // BossP2->location, no cast
    Reappear = 25532, // BossP2->self, no cast
    WalkTo = 25535, // BossP2->location, no cast

    AscalonsMercyConcealed = 25544, // BossP2->self, 3.0s cast, visual
    AscalonsMercyConcealedAOE = 25545, // Helper->self, 1.5s cast, range 50 30-degree (?) cone
    AscalonsMight = 25541, // BossP2->self, no cast, range 50 60-degree (?) cone tankbuster

    KnightsOfTheRound = 25581, // BossP2->self, no cast, visual
    StrengthOfTheWard = 25555, // BossP2->self, 4.0s cast, visual
    LightningStorm = 25548, // BossP2->self, 5.7s cast, visual
    LightningStormAOE = 25549, // Helper->player, no cast, range 5 aoe
    SpiralThrust = 25556, // SerIgnasse/SerVellguine/SerPaulecrain->self, 6.0s cast, range 52 width 16 rect aoe
    HeavyImpact = 25557, // SerGuerrique->self, 4.3s cast, visual
    HeavyImpactHit1 = 25558, // Helper->self, 6.0s cast, range 6 aoe
    HeavyImpactHit2 = 25559, // Helper->self, no cast, range 6-12 donut
    HeavyImpactHit3 = 25560, // Helper->self, no cast, range 12-18 donut
    HeavyImpactHit4 = 25561, // Helper->self, no cast, range 18-24 donut
    HeavyImpactHit5 = 25562, // Helper->self, no cast, range 24-30 donut

    DragonsRage = 25550, // BossP2->self, 4.7s cast, visual
    DragonsRageAOE = 25551, // Helper->players, no cast, range 8 shared aoe
    DimensionalCollapse = 25563, // SerGrinnaux->self, 8.0s cast, visual (growing void zones)
    DimensionalCollapseAOE = 25564, // Helper->location, 9.0s cast, range 3+6 aoe
    SkywardLeapP2 = 25565, // SerIgnasse/SerVellguine/SerPaulecrain->player, no cast, range 24 aoe on player with blue mark
    Conviction1 = 25566, // SerHermenost->self, 8.2s cast, visual towers
    Conviction1AOE = 25567, // Helper->location, 11.0s cast, range 3 aoe, soaked towers
    EternalConviction = 25568, // Helper->self, no cast, raidwide from unsoaked towers
    HolyShieldBash = 25297, // SerJanlenoux/SerAdelphel->tethered player, no cast, width 8 rect tankbuster
    HolyShieldBashAOE = 25579, // Helper->self, no cast, range 6 ??-degree cone, ??
    HolyBladedanceVisual = 25298, // SerJanlenoux/SerAdelphel->self, no cast, visual
    HolyBladedanceAOE = 25299, // Helper->self, no cast, range 16 90-degree cone aoe (follows tankbuster, multiple hits)

    AncientQuaga = 25542, // BossP2->self, 6.0s cast, raidwide
    HeavenlyHeel = 25543, // BossP2->player, 4.0s cast, tankbuster forcing tankswap

    SanctityofTheWard = 25569, // BossP2->self, 4.0s cast, visual
    DragonsGaze = 25552, // BossP2->self, 4.0s cast, visual
    DragonsGazeAOE = 25553, // Helper->self, no cast, face away from caster
    DragonsGlory = 25554, // Helper->self, no cast, face away from caster
    ShiningBlade = 25570, // SerAdelphel/SerJanlenoux->location, no cast, half-width 3 rect (?) charge
    SacredSever = 25571, // SerZephirin->players, no cast, range 6 shared aoe
    BrightFlare = 25295, // Brightsphere->self, 1.0s cast, range 9 aoe

    HiemalStorm = 25574, // SerHaumeric->self, 7.0s cast, visual
    HiemalStormAOE = 25575, // Helper->players, no cast, range 7 aoe, baited at 4 dd or tanks/healers
    HeavensStake = 28590, // SerCharibert->self, 7.0s cast, visual
    HeavensStakeAOE = 28591, // Helper->location, 7.5s cast, range 7 aoe (at four intercardinals)
    HeavensStakeDonut = 28592, // Helper->self, 7.5s cast, range 15?-30 donut aoe
    Conviction2 = 29563, // SerHermenost->self, 9.2s cast, visual
    Conviction2AOE = 29564, // Helper->location, 12.0s cast, range 3 aoe, soaked towers
    HolyComet = 25576, // SerNoudenet->self, 12.0s cast, visual
    HolyCometAOE = 25577, // HolyComet->self, no cast, range 20 aoe on comet drop
    HolyImpact = 25578, // HolyComet->self, no cast, raidwide on comet fail (link range ~5)
    FaithUnmoving = 25308, // SerGrinnaux->self, 4.0s cast, knockback 16
    Conviction3 = 28650, // SerHermenost->self, 8.2s cast, visual
    Conviction3AOE = 28651, // Helper->location, 11.0s cast, range 3 aoe, soaked towers

    UltimateEnd = 25533, // BossP2->self, no cast, visual
    UltimateEndAOE = 25534, // Helper->self, no cast, raidwide

    BroadSwingRL = 25536, // BossP2->self, 3.0s cast, visual
    BroadSwingLR = 25537, // BossP2->self, 3.0s cast, visual
    BroadSwingAOE = 25538, // Helper->self, no cast, range 40 120-degree cone
    AethericBurstP2 = 25539, // BossP2->self, 6.0s cast, enrage

    // phase 3
    FinalChorus = 26376, // BossP3->self, no cast, visual
    FinalChorusAOE = 26377, // Helper->self, no cast, raidwide
    AutoAttackP3 = 26416, // BossP3->player, no cast, range 60 half-width width 3 rect aoe

    DiveFromGrace = 26381, // BossP3->self, 5.0s cast, visual
    DarkHighJump = 26382, // NidhoggDrake->players, no cast, range 5 aoe
    DarkSpineshatterDive = 26383, // NidhoggDrake->player, no cast, range 5 aoe
    DarkElusiveJump = 26384, // NidhoggDrake->player, no cast, range 5 aoe
    DarkdragonDive = 26385, // NidhoggDrake->self, 2.5s cast, range 5 tower aoe
    DarkdragonDiveFail = 26395, // Helper->self, no cast, raidwide if tower is not soaked
    GnashAndLash = 26386, // BossP3->self, 7.6s cast, visual
    LashAndGnash = 26387, // BossP3->self, 7.6s cast, visual
    EyeOfTheTyrant = 26388, // BossP3->player, no cast, range 6 shared aoe
    GnashingWheel = 26389, // BossP3->self, no cast, range 8 aoe
    LashingWheel = 26390, // BossP3->self, no cast, range 8-40 donut aoe
    Geirskogul = 26378, // NidhoggDrake->self, 4.5s cast, range 62 width 8 baited rect aoe

    Drachenlance = 26379, // BossP3->self, 2.9s cast, visual
    DrachenlanceAOE = 26380, // Helper->self, 3.5s cast, range 13 90-degree cone

    DarkdragonDive1 = 26391,// NidhoggDrake->self, 2.5s cast, range 5 tower aoe, should be soaked by 1 person
    DarkdragonDive2 = 26392,// NidhoggDrake->self, 2.5s cast, range 5 tower aoe, should be soaked by 2 persons
    DarkdragonDive3 = 26393,// NidhoggDrake->self, 2.5s cast, range 5 tower aoe, should be soaked by 3 persons
    DarkdragonDive4 = 26394,// NidhoggDrake->self, 2.5s cast, range 5 tower aoe, should be soaked by 4 persons
    SoulTether = 26396, // BossP3/NidhoggDrake->player, no cast, range 5 aoe tankbuster on tether targets
    RevengeOfTheHordeP3 = 29750, // BossP3->self, 11.0s cast, enrage

    // phase 4
    AlphinaudVisual1 = 25584, // Alphinaud->LeftEye, 2.0s cast, single-target, visual
    AlphinaudVisual2 = 25585, // Alphinaud->LeftEye, 2.0s cast, single-target, visual
    AlphinaudVisual3 = 25586, // Alphinaud->LeftEye, no cast, single-target, visual
    AutoAttackEye = 26811, // RightEye/LeftEye->player, no cast, single-target
    TeleportEye = 29050, // RightEye/LeftEye->location, no cast, single-target, teleport
    SoulOfFriendship = 26821, // Haurchefant->players, no cast, range 5 circle, applies buff
    SoulOfDevotion = 26822, // Ysayle->Alphinaud, no cast, range 5 circle, applies buff
    Resentment = 26810, // Estinien->self, no cast, range 40 circle, raidwide with bleed
    Hatebound = 26814, // RightEye/LeftEye->self, 3.0s cast, single-target, visual (tethers)
    FlareStar = 26815, // AzurePrice->self, no cast, range 6 circle
    FlareStarFail = 26816, // AzurePrice->self, no cast, range 65 circle (raidwide + damage down if unsoaked)
    FlareNova = 26817, // GildedPrice->self, no cast, range 6 circle
    FlareNovaFail = 26818, // GildedPrice->self, no cast, range 65 circle (raidwide + damage down if unsoaked)
    MirageDive = 26819, // LeftEye/RightEye->self, 3.0s cast, single-target, visual
    MirageDiveAOE = 26820, // NidhoggDrake->player, no cast, range 4 circle
    SteepInRage = 26813, // RightEye/LeftEye->self, 6.0s cast, range 60 circle, raidwide
    CreepingShadows = 26823, // RightEye/LeftEye->Estinien, no cast, single-target, visual (enrage sequence)
    RevengeOfTheHordeP4 = 26402, // NidhoggP4->self, 6.0s cast, range 80 circle, enrage

    // intermission
    PlanarPrison = 25313, // SerGrinnaux->self, no cast, range 70 circle, applies small damage, pulls to grinnaux and stuns for a second the whole raid right before pure-of-heart phase
    PlanarPrisonAOE = 25580, // Helper->self, no cast, range ?-70 donut, prevent moving outside small range of (89,100)
    BrightwingedFlight = 25366, // SerAdelphel->self, no cast, range 8 ?-degree cone, ??? (applies two buffs on ser charibert)
    SpearOfTheFuryIntermission = 25314, // SerZephirin->self, 10.0s cast, range 22 width 10 rect, visual (hits haurchefant??)
    Shockwave = 25315, // SpearOfTheFury->self, no cast, raidwide, multiple casts every ~1.1s (if not mitigated by lb3)
    PureOfHeart = 25316, // SerCharibert->self, 35.5s cast, raidwide, damage depends on caster hp %?
    ShockwaveMitigated = 25368, // SpearOfTheFury->self, no cast, raidwide, multiple casts every ~1.1s (if mitigated by lb3)
    Brightwing = 25369, // Helper->self, no cast, range 18 ?-degree cone, baited on 2 closest targets
    Skyblind = 25370, // Helper->location, 2.5s cast, range 3 puddle
    Pierce = 26971, // SpearOfTheFury->Haurchefant, 11.0s cast, single-target, kill target (enrage)
    PierceSuccess = 25367, // Haurchefant->self, no cast, single-target, visual

    // phase 5
    Incarnation = 27526, // BossP5->self, 4.0s cast, single-target, visual (tether to dragons)
    DragonsEye = 27527, // BossP5->self, 3.0s cast, single-target, visual (resurrect dragons)
    WrathOfTheHeavens = 27529, // BossP5->self, 4.0s cast, single-target, visual (trio 1 start)
    SkywardLeapP5 = 29346, // SerPaulecrain->player, no cast, range 24 circle spread
    SpiralPierce = 27530, // SerVellguine/SerIgnasse->player, no cast, width 16 rect charge
    TwistingDive = 27531, // Vedrfolnir->self, 6.0s cast, range 60 width 10 rect
    Twister = 27532, // Helper->self, no cast, ???, (range 8 knockback 50 if touched)
    Cauterize1 = 27533, // Darkscale->self, 6.0s cast, range 48 width 20 rect
    Cauterize2 = 27534, // Vidofnir->self, 6.0s cast, range 48 width 20 rect
    ChainLightning = 27535, // Darkscale->self, no cast, apply debuffs to 2 players
    ChainLightningAOE = 27536, // Helper->self, no cast, range 5? spread
    AscalonsMercyRevealed = 25546, // BossP5->self, 3.3s cast, single-target, visual (proteans)
    AscalonsMercyRevealedAOE = 25547, // Helper->self, no cast, range 50 30?-degree cone
    LiquidHeaven = 27537, // Vedrfolnir->location, no cast, range 6 circle voidzones (x5)
    AltarFlare = 25572, // SerCharibert->self, 3.5s cast, single-target, visual
    AltarFlareAOE = 25573, // Helper->location, 4.0s cast, range 8 circle puddle (x4)
    EmptyDimension = 25306, // SerGrinnaux->self, 5.0s cast, range 6-70 donut

    DeathOfTheHeavens = 27538, // BossP5->self, 4.0s cast, single-target, visual (trio 2 start)
    SpearOfTheFuryP5 = 27539, // SerZephirin->self, 6.0s cast, range 50 width 10 rect
    Deathstorm = 27540, // Darkscale->self, no cast, applies dooms
    WingsOfSalvation = 27541, // Vidofnir->self, 5.3s cast, single-target, visual (cleansing puddles)
    WingsOfSalvationAOE = 27542, // Helper->location, 10.0s cast, range 4 circle (cleansing puddle visual)
    Heavensflame = 25310, // SerCharibert->self, 7.0s cast, single-target, visual (playstation)
    HeavensflameAOE = 25311, // Helper->players, no cast, range 10 circle spread
    HolyChain = 25312, // Helper->self, no cast, heavy hit and damage down on unbroken chains
    HolyMeteor = 27543, // SerNoudenet->self, 8.0s cast, single-target, visual (spawn meteors)
    MeteorImpact = 27544, // MeteorCircle->self, no cast, range 60 circle (meteor enrage)
    Surrender = 26215, // BossP5->self, no cast, single-target, visual (surrender at 3%)
    AethericBurstP5 = 27528, // BossP5->self, 6.0s cast, range 50 circle, enrage

    // phase 6
    AutoAttackYsayle = 872, // Ysayle->Background1, no cast, single-target
    YsayleBlizzard = 29751, // Ysayle->Background1, 1.0s cast, single-target
    WyrmclawN = 27946, // NidhoggP6->player, no cast, single-target (autoattack)
    WyrmclawH = 27938, // HraesvelgrP6->player, no cast, single-target (autoattack)
    DreadWyrmsbreathNormal = 27954, // NidhoggP6->self, 6.3s cast, single-target, visual (non-glowing)
    DreadWyrmsbreathGlow = 27955, // NidhoggP6->self, 6.3s cast, single-target, visual (glowing)
    GreatWyrmsbreathNormal = 27956, // HraesvelgrP6->self, 6.3s cast, single-target, visual (non-glowing)
    GreatWyrmsbreathGlow = 27957, // HraesvelgrP6->self, 6.3s cast, single-target, visual (glowing)
    FlameBreath = 27958, // Helper->self, no cast, range 100 20?-degree cone
    IceBreath = 27959, // Helper->self, no cast, range 100 20?-degree cone
    SwirlingBlizzard = 27960, // Helper->self, 7.0s cast, range 20-35 donut
    DarkOrb = 27961, // Helper->players, no cast, range 6 circle shared tankbuster
    HolyOrb = 27962, // Helper->players, no cast, range 6 circle shared tankbuster
    DarkBreath = 27963, // Helper->self, no cast, range 50 ?-degree cone (fixed direction)
    HolyBreath = 27964, // Helper->self, no cast, range 50 ?-degree cone (fixed direction)
    StaggeringBreath = 27965, // Helper->player, no cast, range 15 circle solo tankbuster
    //_Ability_ = 27951, // Helper->self, no cast, single-target
    MortalVowApply = 27952, // NidhoggP6->player, no cast, range 5 circle, apply debuff
    MortalVowPass = 27953, // Helper->self, no cast, range 5 circle, pass debuff
    AkhAfahN = 27971, // NidhoggP6->self, 8.0s cast, single-target
    AkhAfahH = 27969, // HraesvelgrP6->self, 8.0s cast, single-target
    AkhAfahNAOE = 27972, // NidhoggP6->player, no cast, range 4 circle stack
    AkhAfahHAOE = 27970, // HraesvelgrP6->player, no cast, range 4 circle stack
    HallowedWingsLN = 27939, // HraesvelgrP6->self, 7.5s cast, single-target (left side, near tankbuster)
    HallowedWingsLF = 27940, // HraesvelgrP6->self, 7.5s cast, single-target (left side, far tankbuster)
    HallowedWingsRN = 27942, // HraesvelgrP6->self, 7.5s cast, single-target (right side, near tankbuster)
    HallowedWingsRF = 27943, // HraesvelgrP6->self, 7.5s cast, single-target (right side, far tankbuster)
    HallowedWingsAOELeft = 27941, // Helper->self, no cast, range 50 width 22 rect (hit left side)
    HallowedWingsAOERight = 27944, // Helper->self, no cast, range 50 width 22 rect (hit right side)
    HallowedPlume = 27945, // Helper->players, no cast, range 10 circle tankbuster
    CauterizeN = 27966, // NidhoggP6->self, 5.0s cast, range 80 width 22 rect

    WrothFlames = 27973, // NidhoggP6->self, 2.5s cast, single-target, visual (orbs mechanic start)
    CauterizeH = 27967, // HraesvelgrP6->self, 5.0s cast, range 80 width 22 rect
    AkhMornFirst = 27974, // NidhoggP6->players, 8.0s cast, range 6 circle stack
    AkhMornRest = 27975, // NidhoggP6->player, no cast, range 6 circle stack
    FlameBlast = 26409, // ScarletPrice->self, 5.0s cast, range 44 width 6 cross
    HotWing = 27947, // NidhoggP6->self, 5.5s cast, single-target, visual (side cleaves)
    HotWingAOE = 27948, // Helper->self, 6.5s cast, range 50 width 21 rect
    HotTail = 27949, // NidhoggP6->self, 5.5s cast, single-target, visual (center cleave)
    HotTailAOE = 27950, // Helper->self, 6.5s cast, range 50 width 16 rect
    SpreadingFlames = 29739, // Helper->self, no cast, range 5 circle spread (knockback 15 on others)
    EntangledFlames = 29740, // Helper->self, no cast, range 4 circle two-man stack
    EntangledPyre = 29741, // Helper->self, no cast, range 100 circle two-man stack fail?

    Touchdown = 27968, // NidhoggP6/HraesvelgrP6->self, no cast, single-target, visual (proximity)
    TouchdownAOE = 28903, // Helper->self, no cast, range 80 circle with ? falloff
    RevengeOfTheHordeP6 = 27937, // NidhoggP6/HraesvelgrP6->self, 25.0s cast, range 80 circle, enrage

    // phase 7
    ShockwaveP7 = 29156, // Helper->self, no cast, range 100 circle, raidwide
    TransitionP71 = 25587, // LeftEye/RightEye->BossP5, no cast, single-target, visual (absorb eye)
    TransitionP72 = 29365, // LeftEye->BossP5, no cast, single-target, visual (???)
    TransitionP73 = 25588, // LeftEye->BossP5, no cast, single-target, visual (???)
    AlternativeEnd = 29752, // Helper->self, no cast, range 100 circle, raidwide
    FlamesOfAscalon = 28049, // Helper->self, no cast, range 8 circle
    IceOfAscalon = 28050, // Helper->self, no cast, range 8-50 donut
    ExaflaresEdge = 28059, // DragonKingThordan->self, 6.0s cast, single-target, visual (exaflares)
    ExaflaresEdgeFirst = 28060, // Helper->self, 6.9s cast, range 6 circle
    ExaflaresEdgeRest = 28061, // Helper->location, no cast, range 6 circle
    Trinity = 28062, // DragonKingThordan->self, no cast, single-target, visual
    TrinityAOE1 = 28063, // Helper->players, no cast, range 3 circle (dark, mt)
    TrinityAOE2 = 28064, // Helper->players, no cast, range 3 circle (light, ot)
    TrinityAOE3 = 28065, // Helper->players, no cast, range 3 circle (dark+light, closest)
    AkhMornsEdge = 28051, // DragonKingThordan->self, 6.0s cast, single-target, visual (towers)
    AkhMornsEdgeVisual1 = 28052, // DragonKingThordan->self, no cast, single-target
    AkhMornsEdgeVisual2 = 28053, // DragonKingThordan->self, no cast, single-target
    AkhMornsEdgeAOEFirstNormal1 = 29452, // Helper->self, 6.7s cast, range 4 circle tower
    AkhMornsEdgeAOEFirstNormal2 = 29453, // Helper->self, 6.7s cast, range 4 circle tower
    AkhMornsEdgeAOEFirstTanks = 29454, // Helper->self, 6.7s cast, range 4 circle tower (for tanks)
    AkhMornsEdgeAOERestNormal = 28054, // Helper->self, no cast, range 4 circle tower secondary
    AkhMornsEdgeAOERestTanks = 28055, // Helper->self, no cast, range 4 circle tower secondary (for tanks)
    AkhMornsEdgeAOEFail = 28056, // Helper->self, no cast, range 60 circle tower fail
    GigaflaresEdge = 28057, // DragonKingThordan->self, 8.0s cast, single-target, visual (proximity)
    GigaflaresEdgeAOE1 = 28058, // Helper->self, 9.0s cast, range 50 circle with 20? falloff
    GigaflaresEdgeAOE2 = 28114, // Helper->self, 13.0s cast, range 50 circle with 20? falloff
    GigaflaresEdgeAOE3 = 28115, // Helper->self, 17.0s cast, range 50 circle with 20? falloff
    MornAfahsEdge = 28206, // DragonKingThordan->self, 10.0s cast, single-target, visual (enrage)
    MornAfahsEdgeFirst1 = 29455, // Helper->self, 10.7s cast, range 4 circle tower
    MornAfahsEdgeFirst2 = 29456, // Helper->self, 10.7s cast, range 4 circle tower
    MornAfahsEdgeFirst3 = 29457, // Helper->self, 10.7s cast, range 4 circle tower
    MornAfahsEdgeVisual = 28207, // DragonKingThordan->self, no cast, single-target, visual (subsequent)
    MornAfahsEdgeRest = 28208, // Helper->self, no cast, range 4 circle tower
    MornAfahsEdgeFail = 28209, // Helper->self, no cast, range 60 circle unsoaked tower
};

public enum SID : uint
{
    None = 0,
    Prey = 562, // none->player, extra=0x0
    Discomposed = 2733, // none->BossP2, extra=0x0
    Jump1 = 3004, // none->player, extra=0x0, 'First in Line'
    Jump2 = 3005, // none->player, extra=0x0, 'Second in Line'
    Jump3 = 3006, // none->player, extra=0x0, 'Third in Line'
    JumpCenter = 2755, // none->player, extra=0x0, 'High Jump Target'
    JumpForward = 2756, // none->player, extra=0x0, 'Spineshatter Dive Target'
    JumpBackward = 2757, // none->player, extra=0x0, 'Elusive Jump Target'
    Clawbound = 2775, // none->player, extra=0x0, red tether
    Fangbound = 2776, // none->player, extra=0x0, blue tether
    BoundAndDetermined = 2777, // none->player, extra=0x0, prevents swap for next 3s
    PiercingResistanceDown = 3131, // NidhoggDrake->player, extra=0x0
    Doom = 2976, // Darkscale->player, extra=0x0
    BurningChains = 769, // none->player, extra=0x0
    MortalVow = 2896, // NidhoggP6/Helper->player, extra=0xFFA1
    MortalAtonement = 2897, // none->player, extra=0x0
    Suppuration = 3133, // Helper->player, extra=0x0 (extra targets for mortal vow pass)
    SpreadingFlames = 2758, // none->player, extra=0x0
    EntangledFlames = 2759, // none->player, extra=0x0
    Boiling = 2898, // Helper->player, extra=0x0
    Freezing = 2899, // Helper->player, extra=0x0
    GenericMechanic = 2056, // none->SerAdelphel/Haurchefant/DragonKingThordan, extra=0x1B6/0x127/0x12A (fire sword)/0x12B (ice sword)
}

public enum TetherID : uint
{
    None = 0,
    HolyShieldBash = 84, // SerJanlenoux/SerAdelphel/NidhoggDrake/BossP3->player
    LeftEye = 178, // LeftEye->Estinien
    RightEye = 179, // RightEye->Estinien
    SoulOfFriendshipDevotion = 12, // Haurchefant/Ysayle->player/Alphinaud
    Fangbound = 51, // player->RightEye, blue tether
    Clawbound = 52, // player->LeftEye, red tether
    Incarnation = 165, // Vedrfolnir/VidofnirVisual/DarkscaleVisual->BossP5
    SpiralPierce = 5, // SerVellguine/SerIgnasse->player
    //_Gen_Tether_53 = 53, // player->SerGrinnaux
    Heavensflame = 9, // player->player
    FlameBreath = 194, // player->NidhoggP6
    IceBreath = 195, // player->HraesvelgrP6
    FlameIceBreathNear = 196, // player->NidhoggP6
    //_Gen_Tether_1 = 1, // NidhoggP6->HraesvelgrP6
    //_Gen_Tether_2 = 2, // NidhoggP6->HraesvelgrP6
}

public enum IconID : uint
{
    None = 0,
    SacredSever1 = 50, // player
    SacredSever2 = 51, // player
    SkywardLeapP2 = 330, // player
    Prey = 285, // player
    Jump1 = 319, // player
    Jump2 = 320, // player
    Jump3 = 321, // player
    SoulOfFriendship = 286, // player
    SoulOfDevotion = 287, // Alphinaud
    SkywardLeapP5 = 14, // player
    Cauterize = 20, // player
    HeavensflameCircle = 281,
    HeavensflameTriangle = 282,
    HeavensflameSquare = 283,
    HeavensflameCross = 284,
}
