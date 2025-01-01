namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

public enum OID : uint
{
    Boss = 0x461E, // R23.000, x1
    Helper = 0x233C, // R0.500, x24, Helper type
    StygianShadow = 0x461F, // R4.000, x0 (spawn during fight), big add
    Atomos = 0x4620, // R2.800, x0 (spawn during fight), small add
    DeathsHand = 0x4621, // R2.000, x6, grim embrace hand
    StygianTendrils = 0x4622, // R1.200, x0 (spawn during fight), evil seed
    CloudletOfDarkness = 0x4623, // R3.000, x0 (spawn during fight), criss-cross source
    BallOfNaught = 0x4624, // R1.500, x1, (en)death sphere
    //_Gen_DreadGale = 0x4625, // R1.200, x1
    SinisterEye = 0x4626, // R2.800, x2, break gaze source
    AtomosSpawnPoint = 0x1EBD7B, // R0.500, x0 (spawn during fight), EventObj type
    EvilSeed = 0x1E9B3B, // R0.500, x0 (spawn during fight), EventObj type
    //_Gen_Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type
    //_Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    AutoAttackNormalPrimary = 40441, // Boss->player, no cast, single-target
    AutoAttackNormalSecondary = 40442, // Helper->player, no cast, single-target
    AutoAttackVulnPrimary = 41348, // Boss->player, no cast, single-target
    AutoAttackVulnSecondary = 41349, // Helper->player, no cast, single-target
    Teleport1 = 40534, // Boss->location, no cast, single-target
    BladeOfDarknessL = 40443, // Boss->self, 7.0+0.7s cast, single-target, visual (left hand safe)
    BladeOfDarknessLAOE = 40444, // Helper->self, 7.7s cast, range 12-60 150-degree cone donut
    BladeOfDarknessR = 40445, // Boss->self, 7.0+0.7s cast, single-target, visual (right hand safe)
    BladeOfDarknessRAOE = 40446, // Helper->self, 7.7s cast, range 12-60 150-degree cone donut
    BladeOfDarknessC = 40447, // Boss->self, 7.0+0.7s cast, single-target, visual (out safe)
    BladeOfDarknessCAOE = 40448, // Helper->self, 7.7s cast, range 30 180-degree cone

    DelugeOfDarkness1 = 40509, // Boss->location, 8.0s cast, range 60 circle, raidwide + arena transition
    GrimEmbraceForward = 40505, // Boss->self, 5.0s cast, tethered players spawn aoe forward
    GrimEmbraceBackward = 40506, // Boss->self, 5.0s cast, tethered players spawn aoe backward
    GrimEmbraceVisual = 40507, // DeathsHand->self, no cast, single-target, visual (hand spawn)
    GrimEmbraceAOE = 40508, // Helper->self, 1.0s cast, range 8 width 8 rect
    RazingVolleyParticleBeam = 40511, // CloudletOfDarkness->self, 8.0s cast, range 45 width 8 rect, criss-cross
    RapidSequenceParticleBeam = 40512, // Boss->self, 7.0+0.7s cast, single-target, visual (party line stacks)
    RapidSequenceParticleBeamRepeat = 40513, // Boss->self, no cast, single-target, visual (repeat)
    RapidSequenceParticleBeamAOE = 40514, // Helper->self, no cast, range 50 width 6 rect
    Death = 40515, // Boss->self, 5.6+0.7s cast, single-target, visual (attract to out/in)
    DeathVortex = 40516, // Helper->self, 2.0s cast, range 40 circle, attract 15
    DeathAOE1 = 40517, // Helper->self, 4.0s cast, range 6 circle
    DeathAOE2 = 40518, // BallOfNaught->self, 6.0s cast, range 6-40 donut
    Endeath = 40531, // Boss->self, 5.0s cast, single-target, visual (delayed attract)
    EndeathVortex = 40519, // Helper->self, 1.0s cast, range 40 circle, attract 15
    EndeathAOE1 = 40520, // Helper->self, 3.0s cast, range 6 circle
    EndeathAOE2 = 40521, // BallOfNaught->self, 5.0s cast, range 6-40 donut
    Aero = 40524, // Boss->self, 5.6+0.7s cast, single-target, visual (knockback)
    AeroKnockback = 40522, // Helper->self, 2.0s cast, range 40 circle, knockback 15
    AeroAOE = 40523, // 4625->self, 2.0s cast, range 8 circle
    Enaero = 40532, // Boss->self, 5.0s cast, single-target, visual (delayed knockback)
    EnaeroKnockback = 40525, // Helper->self, 1.0s cast, range 40 circle, knockback 15
    EnaeroAOE = 40526, // 4625->self, 1.0s cast, range 8 circle
    BreakBoss = 40527, // Boss->self, 5.0+1.0s cast, single-target, visual (gazes)
    BreakBossAOE = 40528, // Helper->self, 6.0s cast, range 60 circle, gaze
    BreakEye = 40529, // SinisterEye->self, 3.0+1.0s cast, single-target, visual (gaze)
    BreakEyeAOE = 40530, // Helper->self, 4.0s cast, range 60 circle, gaze
    Flare = 40536, // Boss->self, 4.0s cast, single-target, visual (flares)
    FlareAOE = 40537, // Helper->players, no cast, range 60 circle with 25 falloff
    UnholyDarkness = 41261, // Boss->self, 5.0s cast, single-target, visual (4-man stacks on healers)
    UnholyDarknessAOE = 41262, // Helper->players, no cast, range 6 circle, 4-man stack
    FloodOfDarkness1 = 40510, // Boss->location, 7.0s cast, range 60 circle, raidwide + arena transition to normal

    DelugeOfDarkness2 = 40449, // Boss->location, 8.0s cast, range 60 circle, raidwide + arena transition
    Teleport2 = 40450, // Boss->location, no cast, single-target
    AutoAttackAdd = 40501, // StygianShadow->player, no cast, single-target
    DarkDominion = 40456, // Boss->self, 5.0s cast, range 60 circle, raidwide
    TeleportAdd = 40494, // StygianShadow->location, no cast, single-target
    Excruciate = 40502, // StygianShadow->player, 5.0s cast, range 4 circle, tankbuster
    FloodOfDarknessAdd = 40503, // StygianShadow->self, 6.0s cast, range 40 circle, interruptible raidwide

    ThirdArtOfDarknessR = 40480, // StygianShadow->self, 10.0+0.4s cast, single-target, visual (right first)
    ThirdArtOfDarknessL = 40483, // StygianShadow->self, 10.0+0.4s cast, single-target, visual (left first)
    ArtOfDarknessNextR = 40481, // StygianShadow->self, no cast, single-target, visual (next right)
    ArtOfDarknessAOER = 40482, // Helper->self, no cast, range 15 180-degree cone
    ArtOfDarknessNextL = 40484, // StygianShadow->self, no cast, single-target, visual (next left)
    ArtOfDarknessAOEL = 40485, // Helper->self, no cast, range 15 ?-degree cone
    HyperFocusedParticleBeamNext = 40486, // StygianShadow->self, no cast, single-target, visual (next spread)
    HyperFocusedParticleBeamAOE = 40487, // Helper->self, no cast, range 22 width 5 rect protean
    MultiProngedParticleBeamNext = 40488, // StygianShadow->self, no cast, single-target, visual (next pairs)
    MultiProngedParticleBeamAOE = 40489, // Helper->players, no cast, range 3 circle, 2-man stack

    ParticleConcentration = 40472, // Boss->self, 6.0s cast, single-target, visual (towers)
    ParticleBeam1 = 40474, // Helper->location, no cast, range 3 circle, 1-man tower
    ParticleBeam2 = 40475, // Helper->location, no cast, range 3 circle, 2-man tower
    ParticleBeam3 = 40476, // Helper->location, no cast, range 3 circle, 3-man tower
    ParticleBeam1Fail = 40473, // Helper->self, no cast, range 80 circle, raidwide if 1-man tower is not soaked
    ParticleBeam2Fail = 41346, // Helper->self, no cast, range 80 circle, raidwide if 2-man tower is not soaked
    ParticleBeam3Fail = 41347, // Helper->self, no cast, range 80 circle, raidwide if 3-man tower is not soaked

    GhastlyGloomCross = 40457, // Boss->self, 7.8+0.7s cast, single-target, visual (cross)
    GhastlyGloomCrossAOE = 40458, // Helper->self, 8.5s cast, range 40 width 30 cross
    GhastlyGloomDonut = 40459, // Boss->self, 7.8+0.7s cast, single-target, visual (donut)
    GhastlyGloomDonutAOE = 40460, // Helper->self, 8.5s cast, range 21-40 donut

    CurseOfDarkness = 40498, // StygianShadow->self, 2.0s cast, single-target, visual (raidwide with debuff that causes dark energy particle beam on expire)
    CurseOfDarknessAOE = 40499, // Helper->self, 3.0s cast, range 40 circle, raidwide
    DarkEnergyParticleBeam = 40500, // Helper->self, no cast, range 25 15?-degree cone

    EvilSeed = 40490, // StygianShadow->self, 7.0s cast, single-target, visual (seeds plant)
    EvilSeedAOE = 40491, // Helper->location, 5.0s cast, range 5 circle, puddle when seed is planted
    ThornyVine = 40492, // StygianShadow->self, 8.0s cast, single-target, visual (seeds tether)
    ThornyVineAOE = 40493, // Helper->self, no cast, ??? (if tethers weren't broken?)

    ChaosCondensedParticleBeam = 40461, // Boss->self, 8.0+0.7s cast, single-target, visual (wild charges)
    ChaosCondensedParticleBeamAOE1 = 40462, // Helper->self, no cast, range 50 width 6 rect, 6-man wild charge on platforms
    ChaosCondensedParticleBeamAOE2 = 40463, // Helper->self, no cast, range 50 width 6 rect, 3-man wild charge on mid
    DiffusiveForceParticleBeam = 40464, // Boss->self, 8.0+0.7s cast, single-target, visual (spread)
    DiffusiveForceParticleBeamAOE1 = 40465, // Helper->players, no cast, range 7 circle, first wave (any specific targeting?)
    DiffusiveForceParticleBeamAOE2 = 40466, // Helper->players, no cast, range 5 circle, second wave

    LateralCorePhaser = 40495, // StygianShadow->self, 6.0+2.0s cast, single-target, visual (sides > front)
    CoreLateralPhaser = 40496, // StygianShadow->self, 6.0+2.0s cast, single-target, visual (front > sides)
    Phaser = 40497, // Helper->self, 8.0s cast, range 23 ?-degree cone

    ActivePivotParticleBeamCW = 40467, // Boss->self, 14.0+0.5s cast, single-target, visual (cw rotation)
    ActivePivotParticleBeamCWRepeat = 40468, // Boss->self, no cast, single-target
    ActivePivotParticleBeamCCW = 40469, // Boss->self, 14.0+0.5s cast, single-target, visual (ccw rotation)
    ActivePivotParticleBeamCCWRepeat = 40470, // Boss->self, no cast, single-target
    ActivePivotParticleBeamAOE = 40471, // Helper->self, no cast, range 80 width 18 rect

    LoomingChaosAdd = 41673, // StygianShadow->self, 7.0s cast, single-target, visual (position swaps)
    LoomingChaosBoss = 41674, // Boss->self, 7.0s cast, single-target, visual (position swaps)
    LoomingChaosAOE = 41675, // Helper->self, 7.7s cast, range 50 circle, raidwide + position swaps

    FeintParticleBeam = 40477, // Boss->self, 6.0+0.7s cast, single-target, visual (chasers)
    FeintParticleBeamAOEFirst = 40478, // Helper->location, 4.0s cast, range 3 circle
    FeintParticleBeamAOERest = 40479, // Helper->location, no cast, range 3 circle

    Evaporation = 40454, // StygianShadow->Boss, 2.0s cast, single-target, destroy add and transfer damage done to boss
    FloodOfDarkness2 = 40455, // Boss->location, 7.0s cast, range 60 circle, raidwide + arena transition to normal

    Enrage = 40533, // Boss->location, 12.0s cast, range 100 circle, enrage
}

public enum SID : uint
{
    //_Gen_ArcaneDesign = 4180, // Boss->Boss, extra=0x0
    //_Gen_VeilOfDarkness = 4179, // Boss->Boss, extra=0x0
    //_Gen_LightningResistanceDown = 4386, // Helper/Boss->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9/0xA/0xB/0xC/0xD/0xE/0xF/0x10
    DeadlyEmbrace = 4181, // none->player, extra=0x0
    AbyssalEdge = 4182, // Boss->Boss, extra=0x0 (endeath/enaero stored)
    //_Gen_CloyingCondensation = 2532, // none->player, extra=0x0, prevent jumps?
    //_Gen_ = 4388, // none->StygianShadow, extra=0x1052
    //_Gen_ = 4387, // none->Boss, extra=0x1051
    InnerDarkness = 4177, // none->player, extra=0x0, on main platform
    OuterDarkness = 4178, // none->player, extra=0x0, on side platform
    //_Gen_Rehabilitation = 4191, // none->Boss, extra=0x1/0x4/0x3/0x2
    //_Gen_LifeDrain = 1377, // none->player, extra=0x0
    CurseOfDarkness = 2387, // none->player, extra=0x0
    //_Gen_StabWound = 3061, // none->player, extra=0x0
    //_Gen_StabWound = 3062, // none->player, extra=0x0
    //_Gen_ThornyVine = 445, // none->player, extra=0x0
    //_Gen_ForwardWithThee = 2240, // none->player, extra=0x33F
    //_Gen_Stun = 149, // none->player, extra=0x0
    //_Gen_BackWithThee = 2241, // none->player, extra=0x340
    //_Gen_LeftWithThee = 2242, // none->player, extra=0x341
    //_Gen_Stun = 2656, // none->player, extra=0x0
    //_Gen_RightWithThee = 2243, // none->player, extra=0x342
}

public enum IconID : uint
{
    GrimEmbraceCountdown = 552, // player->self
    Flare = 346, // player->self
    UnholyDarkness = 100, // player->self
    ThirdArtOfDarknessLeft = 239, // StygianShadow->self
    ThirdArtOfDarknessRight = 240, // StygianShadow->self
    ThirdArtOfDarknessStack = 241, // StygianShadow->self
    ThirdArtOfDarknessSpread = 242, // StygianShadow->self
    EvilSeed = 551, // player->self
    ThornyVineHatch = 569, // StygianTendrils->self
    ThornyVineBait = 12, // player->self
    RotateCW = 564, // Boss->self
    RotateCCW = 565, // Boss->self
    Excruciate = 342, // player->self
    FeintParticleBeam = 197, // player->self
}

public enum TetherID : uint
{
    GrimEmbraceForward = 300, // player->Boss
    GrimEmbraceBackward = 301, // player->Boss
    //_Gen_Tether_14 = 14, // Boss/StygianShadow->StygianShadow/Boss
    //_Gen_Tether_165 = 165, // Atomos->player
    ThornyVine = 18, // StygianTendrils/player->player
    LoomingChaos = 38, // player->player
    //_Gen_Tether_1 = 1, // StygianTendrils->StygianTendrils
}
