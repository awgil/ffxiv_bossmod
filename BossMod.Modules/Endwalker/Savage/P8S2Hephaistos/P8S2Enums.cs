namespace BossMod.Endwalker.Savage.P8S2;

public enum OID : uint
{
    Boss = 0x3AE5, // R24.960, x1
    Helper = 0x233C, // R0.500, x42
    IllusoryHephaistosMovable = 0x3AE6, // R5.760, x4
    IllusoryHephaistosLanes = 0x3AE7, // R5.760, x8
}

public enum AID : uint
{
    AutoAttack = 29378, // Boss->players, no cast, range 4 circle shared damage
    AshingBlazeL = 31191, // Boss->self, 6.0s cast, range 46 width 21 rect aoe on left (-X) half of the arena
    AshingBlazeR = 31192, // Boss->self, 6.0s cast, range 46 width 21 rect aoe on right (+X) half of the arena
    TyrantsUnholyDarkness = 31197, // Boss->self, 5.0s cast, single-target, visual
    TyrantsUnholyDarknessAOE = 31198, // Helper->player, no cast, range 6 circle aoe
    Aioniopyr = 31199, // Boss->self, 5.0s cast, raidwide

    NaturalAlignment = 31163, // Boss->self, 5.0s cast, single-target, visual
    TwistNature = 31164, // Boss->self, 3.0s cast, single-target, visual
    ForcibleTrifire = 31165, // Helper->location, no cast, range 6 circle stack on 3 farthest targets
    ForcibleDifreeze = 31166, // Helper->location, no cast, range 5 circle stack on 2 closest targets
    ForcibleFireStack = 31167, // Helper->location, no cast, range 6 circle stack
    ForcibleFireSpread = 31168, // Helper->location, no cast, range 6 circle spread
    TyrantsFlare = 31369, // Boss->self, 3.0s cast, single-target, visual
    TyrantsFlareAOE = 31370, // Helper->location, 3.0s cast, range 6 circle puddle
    EndOfDays = 31371, // IllusoryHephaistosLanes->self, 6.0s cast, range 60 width 10 rect aoe
    InverseMagicks = 31170, // Boss->self, 3.0s cast, single-target, visual
    ForcibleFailure = 30188, // Helper->self, no cast, wipe if natural alignment target takes damage

    HighConcept = 31148, // Boss->self, 5.0s cast, raidwide, visual ?
    HighConceptAOE = 28938, // Helper->self, no cast, raidwide
    ArcaneControl = 31158, // Boss->self, 3.0s cast, single-target, visual
    ConceptualShiftAlpha = 31149, // Helper->location, no cast, range 20 circle aoe (applies perfection & inconceivable debuffs)
    ConceptualShiftBeta = 31150, // Helper->location, no cast, range 20 circle aoe (applies perfection & inconceivable debuffs)
    ConceptualShiftGamma = 31151, // Helper->location, no cast, range 20 circle aoe (applies perfection & inconceivable debuffs)
    FailureOfImagination = 31153, // Helper->location, no cast, wipe on HC failure (? exact conditions)
    Splicer1 = 31154, // Helper->location, no cast, range 6 circle 1-person stack
    Splicer2 = 31155, // Helper->location, no cast, range 6 circle 2-person stack
    Splicer3 = 31156, // Helper->location, no cast, range 6 circle 3-person stack
    Conception = 31152, // Helper->self, no cast, visual (merge)
    ArcaneChannel = 31159, // Helper->self, no cast, range 3 circle tower
    ArcaneWave = 31160, // Helper->self, no cast, raidwide from unsoaked tower
    Deconceptualize = 31374, // Boss->self, 3.0s cast, single-target, visual (clear colors)
    EndOfDaysMovable = 31161, // IllusoryHephaistosMovable->self, no cast, range 60 width 10 rect aoe

    LimitlessDesolation = 30189, // Boss->self, 5.0s cast, single-target, visual
    TyrantsFire = 30192, // Helper->players, no cast, range 6 circle aoe
    TyrantsFlareLimitless = 31368, // Helper->location, 3.0s cast, range 8 circle puddle
    Burst = 31189, // Helper->self, no cast, range 4 circle tower
    BigBurst = 31190, // Helper->self, no cast, raidwide from unsoaked tower

    Everburn = 31157, // Helper->self, no cast, raidwide (set hp to 1 and apply buff if full -or- kill if not)
    EgoDeath = 31162, // Boss->self, 10.0s cast, single-target, visual
    EgoDeathAOE = 31366, // Boss->self, no cast, raidwide visual?
    EgoDeathWipe = 31367, // Boss->self, no cast, raidwide wipe (if there are no phoenixes?)
    EgoDeathKill = 31392, // Helper->self, 3.0s cast, raidwide (kill everyone during cutscene)
    EgoDeathResurrect = 31216, // Helper->self, no cast, raidwide (resurrect everyone who had the buff during cutscene)

    Aionagonia = 31266, // Boss->self, 8.0s cast, raidwide
    Dominion = 31193, // Boss->self, 7.0s cast, raidwide
    OrogenicDeformation = 31195, // Helper->player, no cast, range 6 circle spread on 4 targets
    OrogenicShift = 31196, // Helper->self, 7.0s cast, range 3 circle tower
    OrogenicAnnihilation = 31194, // Helper->self, no cast, raidwide from unsoaked tower

    Enrage = 31204, // Boss->self, 16.0s cast
}

public enum SID : uint
{
    NaturalAlignment = 3412, // none->player, extra=0x0
    InverseMagicks = 3349, // none->player, extra=0x0
    NaturalAlignmentMechanic = 2552, // none->player, extra=0x209/0x1E0/0x1E1/0x1E2/0x1E3/0x1DC/0x1DD/0x1DE/0x1DF

    ImperfectionAlpha = 3330, // none->player, extra=0x0 (HC letter)
    ImperfectionBeta = 3331, // none->player, extra=0x0 (HC letter)
    ImperfectionGamma = 3332, // none->player, extra=0x0 (HC letter)
    PerfectionAlpha = 3333, // Helper->player, extra=0x0 (HC red/blue/green)
    PerfectionBeta = 3334, // Helper->player, extra=0x0 (HC yellow/purple/green)
    PerfectionGamma = 3335, // Helper->player, extra=0x0 (HC brown/blue/purple)
    Inconceivable = 3336, // Helper->player, extra=0x0 (HC can't merge)
    WingedConception = 3337, // none->player, extra=0x0 (HC green)
    AquaticConception = 3338, // none->player, extra=0x0 (HC blue)
    ShockingConception = 3339, // none->player, extra=0x0 (HC purple)
    FieryConception = 3340, // none->player, extra=0x0 (HC red)
    ToxicConception = 3341, // none->player, extra=0x0 (HC snake)
    GrowingConception = 3342, // none->player, extra=0x0 (HC tree)
    ImmortalSpark = 3343, // none->player, extra=0x0 (HC phoenix)
    ImmortalConception = 3344, // Helper->player, extra=0x0 (HC phoenix all raid)
    Solosplice = 3345, // none->player, extra=0x0 (HC 1-person stack)
    Multisplice = 3346, // none->player, extra=0x0 (HC 2-person stack)
    Supersplice = 3347, // none->player, extra=0x0 (HC 3-person stack)
    Everburn = 3406, // Helper->player, extra=0x0 (damage up after resurrection)
    InEvent = 2999, // none->player, extra=0x0
}
