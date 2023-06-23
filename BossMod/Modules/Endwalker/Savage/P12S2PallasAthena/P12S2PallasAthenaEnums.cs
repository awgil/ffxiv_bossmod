namespace BossMod.Endwalker.Savage.P12S2PallasAthena
{
    public enum OID : uint
    {
        Boss = 0x3F35, // R22.500-35.000, x1
        Helper = 0x233C, // R0.500, x35
        Hemitheos = 0x3F36, // R1.700, spawn during fight
        ConceptOfFire = 0x3F37, // R1.000, spawn during fight (triangle)
        ConceptOfWater = 0x3F38, // R1.000, spawn during fight (hexagon)
        ConceptOfEarth = 0x3F39, // R1.000, spawn during fight (square)
        ForbiddenFactor = 0x3F3A, // R1.000, spawn during fight (pangenesis slime)
    };

    public enum AID : uint
    {
        AutoAttackNormal = 34424, // Boss->player, no cast, single-target
        AutoAttackSmall = 34549, // Boss->player, no cast, single-target
        UltimaNormal = 34434, // Boss->self, 5.0s cast, range 60 circle, raidwide
        PalladianGrasp1 = 33562, // Boss->self, 5.0s cast, single-target, visual (half-arena tankbuster)
        PalladianGrasp2 = 33563, // Boss->self, 1.0s cast, single-target, visual (second hit)
        PalladianGraspL = 33564, // Boss->self, no cast, left half-arena tankbuster
        PalladianGraspR = 33565, // Boss->self, no cast, right half-arena tankbuster
        CrushHelm = 33559, // Boss->self, 5.0s cast, single-target, visual (multihit tankbuster with dispellable vuln)
        CrushHelmAOEFirst = 33560, // Helper->player, no cast, single-target, small hit applying dispellable vuln
        CrushHelmAOERest = 33561, // Helper->player, no cast, single-target, large hit

        Gaiaochos = 33574, // Boss->self, 7.0s cast, range 60 circle, raidwide + transition to small models
        GaiaochosTransition = 33613, // Helper->location, 7.0s cast, range 7-30 donut
        GaiaochosVisual = 34719, // Helper->self, no cast, single-target, visual (post transition)
        SummonDarkness = 33583, // Boss->self, 3.0s cast, single-target, visual (spawn adds)
        UltimaRay = 33584, // Hemitheos->self, 7.0s cast, range 20 width 6 rect
        MissingLink = 34182, // Helper->self, no cast, kill anyone who didn't break tether
        DemiParhelion = 33575, // Boss->self, 3.0s cast, single-target, visual (circle aoes)
        DemiParhelionAOE = 33576, // Helper->location, 9.5s cast, range 2 circle
        GeocentrismV = 33577, // Boss->self, 7.0s cast, single-target, visual (vertical lines)
        GeocentrismC = 33578, // Boss->self, 7.0s cast, single-target, visual (circle + donut)
        GeocentrismH = 33579, // Boss->self, 7.0s cast, single-target, visual (horizontal lines)
        DemiParhelionGeoLine = 33580, // Helper->self, no cast, range 20 width 4 rect
        DemiParhelionGeoDonut = 33581, // Helper->self, no cast, range 3-7 donut
        DemiParhelionGeoCircle = 33620, // Helper->location, no cast, range 2 circle
        DivineExcoriation = 33582, // Helper->players, no cast, range 1 circle spread
        UltimaBlow = 33608, // Hemitheos->self, no cast, range 20 width 6 rect wild charge
        UltimaGaiaochos = 34550, // Boss->self, 5.0s cast, range 60 circle, raidwide
        UltimaTransition = 34720, // Helper->self, no cast, single-target, visual (transition to normal state)

        ClassicalConcepts = 33585, // Boss->self, 7.0s cast, range 60 circle, raidwide + playstation
        FusionBurst = 33586, // ConceptOfWater->self, no cast, range 60 circle, wipe if tether not intercepted
        Implode = 33587, // ConceptOfEarth/ConceptOfWater/ConceptOfFire->self, 3.0s cast, range 4 circle
        AspectEffect = 33588, // ConceptOfFire/ConceptOfEarth->player, no cast, single-target, intercepted tether
        TiltedBalance = 33589, // Helper->player, no cast, single-target, kill if tether is stretched
        PalladianRay = 33571, // Boss->self, 2.0s cast, single-target, visual (baited cones)
        PalladianRayAOEFirst = 33572, // Helper->self, no cast, range 100 30-degree cone
        PalladianRayAOERest = 33573, // Helper->self, no cast, range 100 30-degree cone
        PantaRhei = 33590, // Boss->self, 10.0s cast, single-target, visual (mirror locations)
        PantaRheiTeleport = 33591, // ConceptOfWater/ConceptOfEarth/ConceptOfFire->location, no cast, single-target, teleport

        CaloricTheory = 33592, // Boss->self, 8.0s cast, range 60 circle, raidwide + mechanic start
        CaloricTheory1InitialFire = 33597, // Helper->players, 8.0s cast, range 4 circle, initial hit (2-man stack?)
        CaloricTheory2InitialWind = 33598, // Helper->player, 8.0s cast, range 7 circle
        CaloricTheory2InitialFire = 34706, // Helper->player, 8.0s cast, range 7 circle
        UnmitigatedExplosionMovement = 33593, // Helper->players, no cast, range 60 circle, wipe if someone moved too far after getting 4 close caloric stacks
        PyrePulse = 34557, // Boss->self, no cast, single-target, visual (aoe explode)
        PyrePulseAOE = 33594, // Helper->players, no cast, range 4 circle stack
        DynamicAtmosphere = 33595, // Helper->player, no cast, range 7 circle spread
        EntropicExcess = 33596, // Helper->location, 3.0s cast, range 7 circle

        Ekpyrosis = 33566, // Boss->self, 4.0s cast, single-target, visual (proximity + exaflare)
        EkpyrosisExaflareFirst = 33567, // Helper->self, 6.0s cast, range 6 circle exaflare
        EkpyrosisExaflareRest = 33568, // Helper->self, no cast, range 6 circle exaflare
        EkpyrosisProximityV = 33569, // Helper->location, 7.0s cast, range 60 circle with ? falloff
        EkpyrosisProximityH = 34435, // Helper->self, 7.0s cast, range 60 circle with ? falloff
        EkpyrosisSpread = 33570, // Helper->player, no cast, range 6 circle spread

        Pangenesis = 33599, // Boss->self, 7.0s cast, range 60 circle, raidwide + towers
        Pantheos = 33602, // Boss->self, 4.0s cast, single-target, visual (create towers)
        UmbralAdvent = 33603, // Hemitheos->self, 5.0s cast, range 3 circle (light tower)
        AstralAdvent = 33604, // Hemitheos->self, 5.0s cast, range 3 circle (dark tower)
        UnmitigatedExplosionUmbral = 33605, // Helper->location, no cast, range 60 circle, wipe if incorrectly soaked
        UnmitigatedExplosionAstral = 33606, // Helper->location, no cast, range 60 circle, wipe if incorrectly soaked
        BiochemicalFactor = 33600, // Helper->player, no cast, single-target (merge & create slimes)
        FactorIn = 33607, // ForbiddenFactor->players, no cast, range 20 circle spread (slime tethers)
        ExFactor = 33601, // Helper->players, no cast, range 60 circle, wipe if not enough slimes were spawned

        Ignorabimus = 33609, // Boss->self, 15.0s cast, range 100 circle, enrage
    };

    public enum SID : uint
    {
        AlphaTarget = 3560, // none->player, extra=0x0
        BetaTarget = 3561, // none->player, extra=0x0
        CloseCaloric = 3589, // none->player, extra=0x1/0x2/0x3/0x4 (movement increases stacks, wipe on reaching 5)
        Pyrefaction = 3590, // none->player, extra=0x0 (fire)
        Atmosfaction = 3591, // none->player, extra=0x0 (air)
        Entropifaction = 3592, // none->player, extra=0x8/0x7/0x6/0x5/0x4/0x3/0x2/0x1 (hot potato)
        StableSystem = 3618, // none->player, extra=0x0 (prevents early merging)
        UmbralTilt = 3576, // none->player, extra=0x0 (light guy)
        AstralTilt = 3577, // none->player, extra=0x0 (dark guy)
        UnstableFactor = 3593, // none->player, extra=0x2/0x1 (allow merging)
        CriticalFactor = 3594, // none->player, extra=0x0 (vuln to slimes tethers)
        //_Gen_FourfoldComeRuin = 3695, // ForbiddenFactor->player, extra=0x3
        //_Gen_Ascended = 3586, // none->player, extra=0x46
        //_Gen_ = 2536, // none->Boss, extra=0x1BA
        //_Gen_MissingLink = 3587, // none->player, extra=0x0
        //_Gen_ShackledTogether = 3588, // none->player, extra=0x0
        //_Gen_EntropyResistance = 3617, // none->player, extra=0x0
    };

    public enum IconID : uint
    {
        PalladianGrasp = 468, // player
        MissingLink = 97, // player
        DivineExcoriation = 22, // player
        ClassicalConceptsCircle = 367, // player 'en'
        ClassicalConceptsTriangle = 368, // player 'sitasankaku'
        ClassicalConceptsSquare = 369, // player 'sikaku'
        ClassicalConceptsCross = 370, // player 'batu'
        CaloricTheory1InitialFire = 303, // player
        CaloricTheory2InitialWind = 469, // player
        CaloricTheory2InitialFire = 470, // player
    };

    public enum TetherID : uint
    {
        MissingLink = 9, // player->player
        ClassicalConceptsPlayers = 129, // player->player
        ClassicalConceptsShapes = 1, // ConceptOfEarth/ConceptOfFire/Hemitheos->player/ConceptOfWater
        BiochemicalFactor = 213, // player->player
        FactorIn = 84, // ForbiddenFactor->player
    };
}
