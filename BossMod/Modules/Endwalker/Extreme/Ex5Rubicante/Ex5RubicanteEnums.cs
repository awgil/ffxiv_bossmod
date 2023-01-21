namespace BossMod.Endwalker.Extreme.Ex5Rubicante
{
    public enum OID : uint
    {
        Boss = 0x3D8C, // R10.000, x1
        Helper = 0x233C, // R0.500, x26
        RubicanteMirage = 0x3D8E, // R10.000, x8
        CircleOfPurgatoryInner = 0x3D95, // R1.000, x1
        CircleOfPurgatoryMiddle = 0x3D96, // R1.000, x1
        CircleOfPurgatoryOuter = 0x3D97, // R1.000, x1
        CircleOfPurgatoryFireball = 0x3D98, // R1.000, x2
        CircleOfPurgatoryTriange = 0x3D8F, // R1.000, x8
        CircleOfPurgatorySquare = 0x3D90, // R1.000, x8
        GreaterFlamesent = 0x3D91, // R5.000, spawn during fight
        FlamesentNS = 0x3D92, // R2.600, spawn during fight (northern side adds)
        FlamesentSS = 0x3D93, // R2.600, spawn during fight (sourthern side adds)
        FlamesentNC = 0x3D94, // R2.600, spawn during fight (northern center add)
        //_Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x1, EventObj type
        //_Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type
    };

    public enum AID : uint
    {
        AutoAttack = 32621, // Boss->player, no cast, single-target
        Teleport1 = 31932, // Boss->location, no cast, single-target, teleport
        Teleport2 = 31933, // Boss->location, no cast, single-target, teleport (in second form)

        InfernoRaidwide = 32044, // Boss->self, 5.0s cast, range 60 circle, raidwide
        ShatteringHeatBoss = 32045, // Boss->player, 5.0s cast, range 4 circle, tankbuster

        HopeAbandonYe = 32551, // Boss->self, no cast, single-target, visual (start transition to ordeals pair)
        ArcaneRevelation = 31934, // Boss->self, no cast, single-target, visual (transition to ordeals pair: show concentric circles)
        NinthCircle = 32802, // Boss->self, no cast, single-target, visual (start new ordeals instance - show patterns)
        CircleRotationStart = 32804, // CircleOfPurgatoryInner/CircleOfPurgatoryMiddle/CircleOfPurgatoryOuter->self, no cast, single-target, visual (start rotating, sets model state to 5)
        CircleRotationEnd = 32805, // CircleOfPurgatoryInner/CircleOfPurgatoryMiddle/CircleOfPurgatoryOuter->self, no cast, single-target, visual (sets model state to 0)
        CircleRotationCW = 31935, // CircleOfPurgatoryInner/CircleOfPurgatoryMiddle/CircleOfPurgatoryOuter->self, no cast, single-target, visual (rotate CW)
        CircleRotationCCW = 31936, // CircleOfPurgatoryInner/CircleOfPurgatoryMiddle/CircleOfPurgatoryOuter->self, no cast, single-target, visual (rotate CCW)
        OrdealOfPurgation = 33001, // Boss->self, 12.0s cast, single-target, visual (ordeal mechanic)
        SymbolRotateCW = 32506, // CircleOfPurgatoryTriange/CircleOfPurgatorySquare->self, no cast, single-target, visual (rotate CW)
        SymbolRotateCCW = 32507, // CircleOfPurgatoryTriange/CircleOfPurgatorySquare->self, no cast, single-target, visual (rotate CCW)
        FieryExpiationTri = 31983, // CircleOfPurgatoryTriange->self, 2.0s cast, range 60 60-degree cone
        FieryExpiationSq = 31984, // CircleOfPurgatorySquare->self, 2.0s cast, range 20 width 80 rect

        ArchInferno = 31993, // Boss->self, 6.0s cast, range 5 circle aoe (in center)
        ArchInfernoRepeat = 31994, // Helper->self, no cast, range 5 circle aoe (in center, hitting every second)
        InfernoDevilFirst = 31995, // Helper->self, 6.0s cast, range 10 circle
        InfernoDevilRest = 31996, // Helper->self, 5.0s cast, range 10 circle
        Conflagration = 31997, // Boss->self, 3.0s cast, range 40 width 10 rect aoe
        RadialFlagration = 31998, // Boss->self, 7.0s cast, single-target, visual (protean)
        RadialFlagrationAOE = 31999, // Helper->self, no cast, range 21 ?-degree cone, protean
        InfernalSlaughter = 32000, // Boss->self, no cast, single-target, visual (prepare for one of three stack/spread mechanics)
        SpikeOfFlame = 32002, // Helper->player, 5.0s cast, range 5 circle spread
        FourfoldFlame = 32003, // Helper->players, 5.0s cast, range 6 circle party stack
        TwinfoldFlame = 32004, // Helper->players, 5.0s cast, range 4 circle pair stack

        StartAddPhase = 32005, // Boss->self, no cast, single-target, visual (untargetable, change model state and apply status)
        BlazingRapture = 32006, // Boss->self, 4.0s cast, single-target, visual (phase change)
        BlazingRaptureAOE = 32007, // Helper->self, 13.7s cast, range 60 circle raidwide
        GhastlyTorch = 32009, // GreaterFlamesent->self, 4.0s cast, range 60 circle, raidwide with stacking dot
        ShatteringHeatAdd = 32010, // FlamesentNS->player, no cast, range 3 circle tankbuster at tethered target
        GhastlyWind = 32011, // FlamesentSS->self, no cast, range 40 ?-degree cone aoe at tethered target
        GhastlyFlame = 32012, // FlamesentNC->self, 4.0s cast, single-target, visual (puddles)
        GhastlyFlameAOE = 32013, // Helper->location, 4.0s cast, range 5 circle puddle

        InfernoSpread = 32015, // Boss->self, 4.0s cast, single-target, visual (spread)
        InfernoSpreadAOE = 32016, // Helper->players, 5.0s cast, range 5 circle spread
        ExplosivePyre = 32017, // Boss->player, no cast, range 2 circle, cleaving autoattack in second form?

        FlamespireBrand = 32019, // Boss->self, 4.0s cast, single-target, visual (apply spread/stack/flare debuffs)
        BloomingWelt = 32020, // Helper->players, no cast, range 40 circle with ? falloff
        FuriousWelt = 32021, // Helper->players, no cast, range 6 circle stack
        StingingWelt = 32022, // Helper->players, no cast, range 6 circle spread
        Flamerake = 32023, // Boss->self, 6.0s cast, single-target, visual (sequence of aoes)
        FlamerakeVisual1 = 32024, // Boss->self, 0.5s cast, single-target, visual (first aoe telegraph)
        FlamerakeVisual2 = 32026, // RubicanteMirage->self, 0.5s cast, single-target, visual (first aoe telegraph)
        FlamerakeAOE11 = 32025, // Helper->self, 1.8s cast, range 40 width 12 rect
        FlamerakeAOE12 = 32027, // Helper->self, 1.8s cast, range 40 width 12 rect
        FlamerakeAOE21 = 32028, // Helper->self, 3.8s cast, range 8 width 40 rect
        FlamerakeAOE22 = 32030, // Helper->self, 3.8s cast, range 8 width 40 rect
        FlamerakeAOE31 = 32029, // Helper->self, 6.3s cast, range 8 width 40 rect
        FlamerakeAOE32 = 32031, // Helper->self, 6.3s cast, range 8 width 40 rect

        SweepingImmolationSpread = 32032, // Boss->self, 7.0s cast, range 20 180-degree cone aoe
        SweepingImmolationStack = 32033, // Boss->self, 7.0s cast, range 20 180-degree cone aoe
        PartialImmolation = 32034, // Helper->players, 7.5s cast, range 5 circle spread
        TotalImmolation = 32035, // Helper->players, 7.5s cast, range 6 circle stack
        ScaldingSignal = 32036, // Boss->self, 5.0s cast, range 10 circle
        ScaldingRing = 32037, // Boss->self, 5.0s cast, range 10-20 donut
        ScaldingFleetFirst = 32038, // RubicanteMirage->location, no cast, range 40 width 6 rect baits
        ScaldingFleetSecond = 32039, // RubicanteMirage->location, 6.5s cast, range 60 width 6 rect

        Dualfire = 32046, // Boss->self, 5.0s cast, single-target, visual (tankbuster cones)
        DualfireAOE = 32047, // Helper->self, no cast, range 60 120?-degree cone tankbuster (on both tanks)

        FlamespireClaw = 32040, // Boss->self, 6.0s cast, single-target, visual (limit cut)
        FlamespireClawVisual = 32041, // Boss->self, no cast, single-target, visual (1s before each hit)
        FlamespireClawAOE = 32371, // Helper->self, no cast, range 20 ?-degree cone
        Flamespire = 32042, // Helper->self, no cast, range 60 circle raidwide (wipe if flamespire reaches 5 stacks)

        Enrage = 32043, // Boss->self, 10.0s cast, range 60 circle
    };

    public enum SID : uint
    {
        Pattern = 2056, // Boss->CircleOfPurgatoryMiddle/CircleOfPurgatoryOuter/CircleOfPurgatoryInner/Boss, extra=0x21E (inner single)/0x21F (inner ortho pair)/0x220 (inner 180deg pair)/0x221 (middle)/0x222 (outer)/0x224 (arch inferno)/0x241 (add phase)/0x225 (second form)
        CircleRotation = 2193, // none->CircleOfPurgatoryOuter/CircleOfPurgatoryInner/CircleOfPurgatoryMiddle, extra=0x232 (inner)/0x233 (middle)/0x234 (outer)
        Penance = 3477, // none->player, extra=0x0 (forbid movement on expiration)
        PenitentsShackles = 3476, // none->player, extra=0x0 (forbid movement)
        BloomingWelt = 3483, // none->player, extra=0x0 (flare)
        FuriousWelt = 3484, // none->player, extra=0x0 (stack)
        StingingWelt = 3485, // none->player, extra=0x0 (spread)

        //_Gen_Burns = 2199, // GreaterFlamesent->player, extra=0x1
        //_Gen_Burns = 3537, // Boss->player, extra=0x0
        //_Gen_ = 2273, // Boss->Boss, extra=0x1F1/0x1F2
        //_Gen_Bleeding = 2088, // Helper->player, extra=0x0
        //_Gen_Flamespire = 3487, // none->player, extra=0x1/0x2/0x3/0x4/0x5
        //_Gen_Flamespire = 3486, // none->player, extra=0x1/0x2/0x3
        //_Gen_SlashingResistanceDown = 1693, // Helper->player, extra=0x0
    };

    public enum TetherID : uint
    {
        ShatteringHeatAdd = 84, // FlamesentNS->player
        GhastlyWind = 192, // FlamesentSS->player
        FlamespireClaw = 89, // player->Boss
    };

    public enum IconID : uint
    {
        Dualfire = 230, // player
        FlamespireClaw1 = 79, // player
        FlamespireClaw2 = 80, // player
        FlamespireClaw3 = 81, // player
        FlamespireClaw4 = 82, // player
        FlamespireClaw5 = 83, // player
        FlamespireClaw6 = 84, // player
        FlamespireClaw7 = 85, // player
        FlamespireClaw8 = 86, // player

        //_Gen_Icon_342 = 342, // player
        //_Gen_Icon_23 = 23, // player
        //_Gen_Icon_211 = 211, // player
        //_Gen_Icon_93 = 93, // player
    };
}
