namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

public enum OID : uint
{
    Boss = 0x3AC9, // R10.000, x1
    Helper = 0x233C, // R0.500, x42
    IllusoryHephaistosIntermission = 0x3ACD, // R10.000, x4 (during intermission between first snakes/centaurs)
    IllusoryHephaistosSnakes = 0x3ACE, // R10.000, x5 (during second snakes)
    Suneater = 0x3AD0, // R7.000, x3 (chtonic vent snake)
    Gorgon = 0x3ACC, // R1.200, spawn during fight
    CthonicVent = 0x1EB703, // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 31047, // Boss->player, no cast, single-target
    AutoAttackSnake = 31049, // Boss->player, no cast, single-target
    Teleport = 28936, // Boss->location, no cast, single-target
    GenesisOfFlame = 31044, // Boss->self, 5.0s cast, raidwide
    Flameviper = 31045, // Boss->self, 5.0s cast, range 60 width 5 rect tankbuster
    FlameviperSecond = 31046, // Boss->self, no cast, range 60 width 5 rect tankbuster second hit

    VolcanicTorches = 30967, // Boss->self, 3.0s cast, single-target, visual
    TorchFlame = 31015, // Helper->self, 10.0s cast, range 10 width 10 rect aoe
    SunforgeCenter = 30992, // Boss->self, 7.0s cast, single-target
    SunforgeSides = 30993, // Boss->self, 7.0s cast, single-target, visual
    ScorchingFang = 30994, // Helper->self, 8.0s cast, range 42 width 14 rect aoe (sunforge center)
    SunsPinion = 30995, // Helper->self, 8.0s cast, range 14 width 42 rect (sunforge sides)
    ConceptualOctaflare = 30996, // Boss->self, 3.0s cast, single-target, visual (determines mechanic)
    ConceptualTetraflare = 30997, // Boss->self, 3.0s cast, single-target, visual (determines mechanic)
    ConceptualTetraflareCentaur = 30998, // Boss->self, 3.0s cast, single-target, visual (determines mechanic)
    ConceptualDiflare = 30999, // Boss->self, 3.0s cast, single-target, visual (determines mechanic)
    EmergentOctaflare = 31000, // Helper->players, no cast, range 6 circle spread
    EmergentTetraflare = 31001, // Helper->players, no cast, range 3 circle 2-man stack
    EmergentDiflare = 31003, // Helper->players, no cast, range 6 circle 4-man stack
    Octaflare = 31005, // Boss->self, 5.0s cast, single-target, visual
    Tetraflare = 31006, // Boss->self, 5.0s cast, single-target, visual

    ReforgedReflectionCentaur = 31051, // Boss->self, 3.0s cast, single-target
    Footprint = 28937, // Helper->location, no cast, raidwide knockback 20
    RearingRampage = 31027, // Boss->self, 5.0s cast, raidwide
    RearingRampageSecond = 31028, // Boss->self, no cast, raidwide
    RearingRampageLast = 31061, // Boss->self, no cast, raidwide
    Uplift = 31029, // Helper->players, no cast, range 6 circle spread
    StompDead = 31030, // Boss->self, 5.0s cast, single-target
    StompDeadAOE = 31031, // Boss->players, no cast, range 6 circle pair stack

    ReforgedReflectionSnake = 31052, // Boss->self, 3.0s cast, single-target
    SnakingKick = 31017, // Boss->self, no cast, range 10 circle aoe
    Gorgomanteia = 31002, // Boss->self, 3.0s cast, single-target, visual (debuffs are applied right after this cast)
    IntoTheShadows = 31018, // Boss->self, 3.0s cast, single-target, visual (gorgons are created right after cast start)
    Petrifaction = 31019, // Gorgon->self, 8.0s cast, single-target, visual
    PetrifactionAOE = 26404, // Helper->self, no cast, raidwide gaze (petrifies anyone looking at gorgon)
    Gorgoneion = 31020, // Gorgon->self, 3.0s cast, raidwide (if gorgon wasn't petrified in time)
    EyeOfTheGorgon = 31021, // Helper->self, no cast, range 25 45-degree cone (petrification)
    BloodOfTheGorgon = 31023, // Helper->location, no cast, range 5 circle aoe (when debuff expires, oneshots gorgons)
    Ektothermos = 31210, // Boss->self, 5.0s cast, raidwide

    IllusoryCreation = 31004, // Boss->self, 3.0s cast, single-target, visual
    CreationOnCommand = 31055, // Boss->self, 3.0s cast, single-target, visual
    SunforgeCenterIntermission = 31056, // IllusoryHephaistosIntermission->self, 7.0s cast, single-target, visual
    SunforgeSidesIntermission = 31057, // IllusoryHephaistosIntermission->self, 7.0s cast, single-target, visual
    ScorchingFangIntermission = 31058, // Helper->self, 8.0s cast, range 42 width 14 rect aoe (sunforge center)
    ScorchedPinion = 31059, // Helper->self, 8.0s cast, range 14 width 84 rect aoe (sunforge sides)
    ManifoldFlames = 31009, // Boss->self, 5.0s cast, single-target, visual
    ManifoldFlamesSecond = 31010, // Boss->self, no cast, single-target, visual
    HemitheosFlare = 29390, // Helper->player, no cast, range 6 circle spread (after manifold flames)
    NestOfFlamevipers = 31007, // Boss->self, 5.0s cast, single-target, visual
    NestOfFlamevipersAOE = 31008, // Helper->self, no cast, range 60 width 5 rect baited aoe (used both after manifold flames on 4 closest and later on everyone)

    FourfoldFires = 30962, // Boss->self, 3.0s cast, single-target, visual
    AbyssalFires = 31060, // Helper->location, 5.0s cast, raidwide with ? falloff
    CthonicVent = 30960, // Boss->self, 3.0s cast, single-target, visual
    CthonicVentMoveDiag = 31011, // Suneater->self, no cast, single-target, visual
    CthonicVentMoveNear = 31012, // Suneater->self, no cast, single-target, visual
    CthonicVentAOE1 = 31013, // Helper->self, 5.0s cast, range 23 circle aoe
    CthonicVentAOE2 = 31054, // Helper->self, 2.0s cast, range 23 circle aoe
    CthonicVentAOE3 = 31014, // Suneater->self, 2.0s cast, range 23 circle aoe

    QuadrupedalImpact = 31236, // Boss->location, 5.0s cast, single-target, visual/teleport
    QuadrupedalImpactAOE = 28932, // Helper->location, 0.8s cast, raidwide knockback 30
    QuadrupedalCrush = 31237, // Boss->location, 5.0s cast, single-target, visual/teleport
    QuadrupedalCrushAOE = 28933, // Helper->location, 0.8s cast, range 30 circle aoe

    BlazingFootfalls = 31032, // Boss->location, 12.0s cast, single-target, visual/teleport
    BlazingFootfallsKnocksideVisual = 31035, // Helper->self, 2.0s cast, range 42 width 42 rect, visual
    BlazingFootfallsImpactVisual = 31036, // Helper->location, 2.0s cast, range 60 circle, visual
    BlazingFootfallsCrushVisual = 31037, // Helper->location, 2.0s cast, range 30 circle, visual
    BlazingFootfallsTrailblaze = 31038, // Helper->self, 0.6s cast, range 42 width 42 rect knockback 10, deadly aoe in the middle
    BlazingFootfallsImpactTeleport = 31034, // Boss->location, no cast, single-target, visual
    BlazingFootfallsCrushTeleport = 31039, // Boss->location, no cast, single-target, visual
    BlazingFootfallsImpact = 28934, // Helper->location, 0.8s cast, range 60 circle, knockback 30
    BlazingFootfallsCrush = 28935, // Helper->location, 0.8s cast, range 30 circle aoe with knockback 35
    BlazingFootfallsTrailblazeTeleport = 31033, // Boss->location, no cast, single-target, visual

    Gorgospit = 31026, // IllusoryHephaistosSnakes->self, 6.0s cast, range 60 width 10 rect
    IllusoryCreationSnakes = 31025, // Boss->self, 3.0s cast, single-target, visual
    CrownOfTheGorgon = 31022, // Helper->self, no cast, raidwide petrify
    BreathOfTheGorgon = 31024, // Helper->location, no cast, range 6 circle stack

    Enrage = 31050, // Boss->self, 5.0s cast, range 60 circle
}

public enum SID : uint
{
    FirstInLine = 3004, // none->player, extra=0x0
    SecondInLine = 3005, // none->player, extra=0x0
    EyeOfTheGorgon = 3351, // none->player, extra=0x0 (petrifying cone gaze)
    CrownOfTheGorgon = 3352, // none->player, extra=0x0 (petrifying circle gaze)
    BloodOfTheGorgon = 3326, // none->player, extra=0x0 (poison aoe)
    BreathOfTheGorgon = 3327, // none->player, extra=0x0 (poison stack aoe)
}
