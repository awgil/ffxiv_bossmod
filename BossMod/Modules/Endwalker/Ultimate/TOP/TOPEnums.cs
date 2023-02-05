namespace BossMod.Endwalker.Ultimate.TOP
{
    public enum OID : uint
    {
        Boss = 0x3D5C, // R12.006, x1
        Helper = 0x233C, // R0.500, x36
        OpticalUnit = 0x3D64, // R0.500, x1
        Tower1 = 0x1EB83C, // R0.500, EventObj type, spawn during fight (unlike Tower2, doesn't get eobjstate events on enter/exit)
        Tower2 = 0x1EB83D, // R0.500, EventObj type, spawn during fight
        OmegaM = 0x3D60, // R3.000-5.010, spawn during fight (starts as M, turns into F)
        OmegaF = 0x3D61, // R3.000-5.010, spawn during fight (starts as F, turns into M)
        OmegaMHelper = 0x3D62, // R5.010, spawn during fight (never targetable)
        OmegaFHelper = 0x3D63, // R5.010, spawn during fight (never targetable)
        //_Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x1, EventObj type
        //_Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type
    };

    public enum AID : uint
    {
        AutoAttackP1 = 31741, // Boss->player, no cast, single-target
        ProgramLoop = 31491, // Boss->self, 4.0s cast, single-target, visual (first mechanic start)
        StorageViolation = 31492, // Helper->self, no cast, range 3 circle tower
        StorageViolationObliteration = 31494, // Helper->self, no cast, range 100 circle if tower is unsoaked
        Blaster = 31495, // Boss->self, 7.9s cast, single-target, visual (first tethers explosion)
        BlasterRepeat = 31496, // Boss->self, no cast, single-target, visual (1-3 tether explosion)
        BlasterLast = 31497, // Boss->self, no cast, single-target, visual (4 tether explosion)
        BlasterAOE = 31498, // Helper->players, no cast, range 15 circle aoe around tether target
        Pantokrator = 31499, // Boss->self, 5.0s cast, single-target, visual (second mechanic start)
        BallisticImpact = 31500, // Helper->location, 3.0s cast, range 5 circle baited puddle
        FlameThrowerFirst = 31501, // Helper->self, 4.0s cast, range 65 60-degree cone
        FlameThrowerRest = 32368, // Helper->self, 4.0s cast, range 65 60-degree cone
        GuidedMissileKyrios = 31502, // Helper->player, no cast, range 5 circle aoe
        CondensedWaveCannonKyrios = 31503, // Helper->self, no cast, range 50 width 6 rect shared aoe
        DiffuseWaveCannonKyrios = 31504, // Helper->self, no cast, range 60 ?-degree cone tankbuster baited on 2 furthest targets
        WaveCannonKyrios = 31505, // Helper->self, no cast, range 50 width 6 rect aoe
        AtomicRay = 31480, // Boss->self, 5.0s cast, range 100 circle, enrage

        AutoAttackM = 31742, // OmegaM->player, no cast, single-target
        AutoAttackF = 31743, // OmegaF->player, no cast, single-target
        FirewallM = 31552, // OmegaM->self, 3.0s cast, range 100 circle, visual (filter preventing damage from furthest 4 players)
        FirewallF = 31553, // OmegaF->self, 3.0s cast, range 100 circle, visual (filter preventing damage from furthest 4 players)
        SolarRayM = 32362, // OmegaM->player, 5.0s cast, range 5 circle, tankbuster
        SolarRayF = 32363, // OmegaF->player, 5.0s cast, range 5 circle, tankbuster
        PartySynergyM = 31550, // OmegaM->self, 3.0s cast, single-target, visual (big mechanic)
        PartySynergyF = 31551, // OmegaF->self, 3.0s cast, single-target, visual (big mechanic)
        SubjectSimulationF = 31515, // OmegaM->self, no cast, single-target, applies 'superfluid' buff
        SubjectSimulationM = 31516, // OmegaF->self, no cast, single-target, applies 'superfluid' buff
        BeyondStrength = 31525, // OmegaMHelper->self, 1.5s cast, range 10-40 donut aoe
        EfficientBladework = 31526, // OmegaMHelper/OmegaF->self, 1.5s cast, range 10 circle aoe
        SuperliminalSteel = 31530, // OmegaFHelper->self, 1.5s cast, single-target, visual (side cleaves)
        SuperliminalSteelL = 31531, // Helper->self, 1.5s cast, range 80 width 36 rect aoe (left side cleave)
        SuperliminalSteelR = 31532, // Helper->self, 1.5s cast, range 80 width 36 rect aoe (right side cleave)
        OptimizedBlizzard = 31533, // OmegaFHelper->self, 1.5s cast, range 100 width 10 cross aoe
        OptimizedFire = 31535, // Helper->players, no cast, range 7 circle spread
        OpticalLaser = 31521, // OpticalUnit->self, 1.3s cast, range 100 width 16 rect
        Discharger = 31534, // OmegaM->self, no cast, range 100 circle knockback 13
        Spotlight = 31536, // Helper->players, no cast, range 6 circle stack
        AnimationGrayishM = 31508, // Helper->self, no cast, single-target, visual (become 'grayish', model state 6)
        AnimationGrayishF = 31509, // Helper->self, no cast, single-target, visual (become 'grayish', model state 6)
        AnimationDisappearM = 31510, // OmegaM->self, no cast, single-target, visual (sink down, model state 11)
        AnimationDisappearF = 31511, // OmegaF->self, no cast, single-target, visual (sink down, model state 11)
        AnimationSwapFM = 31517, // OmegaF->self, no cast, single-target, visual (F becomes M)
        AnimationSwapMF = 31518, // OmegaM->self, no cast, single-target, visual (M becomes F)
        AnimationReappearM = 31519, // OmegaF->self, no cast, single-target, visual (reappear from ground, model state 11)
        AnimationReappearF = 31520, // OmegaM->self, no cast, single-target, visual (reappear from ground, model state 11)
    };

    public enum SID : uint
    {
        InLine1 = 3004, // none->player, extra=0x0
        InLine2 = 3005, // none->player, extra=0x0
        InLine3 = 3006, // none->player, extra=0x0
        InLine4 = 3451, // none->player, extra=0x0
        MidGlitch = 3427, // none->player, extra=0x0
        RemoteGlitch = 3428, // none->player, extra=0x0
        //_Gen_Looper = 3456, // none->player, extra=0x0
        //_Gen_TwiceComeRuin = 2534, // Helper->player, extra=0x1
        //_Gen_HPPenalty = 3401, // Helper->player, extra=0x0
        //_Gen_Doom = 2519, // Helper->player, extra=0x0
        //_Gen_MemoryLoss = 1626, // none->player, extra=0x0
        //_Gen_GuidedMissileKyriosIncoming = 3497, // none->player, extra=0x0
        //_Gen_CondensedWaveCannonKyrios = 3508, // none->player, extra=0x0
        //_Gen_GuidedMissileKyriosIncoming = 3495, // none->player, extra=0x0
        //_Gen_CondensedWaveCannonKyrios = 3510, // none->player, extra=0x0
        //_Gen_GuidedMissileKyriosIncoming = 3424, // none->player, extra=0x0
        //_Gen_CondensedWaveCannonKyrios = 3509, // none->player, extra=0x0
        //_Gen_GuidedMissileKyriosIncoming = 3496, // none->player, extra=0x0
        //_Gen_CondensedWaveCannonKyrios = 3507, // none->player, extra=0x0
        //_Gen_OmegaM = 3454, // OmegaF->OmegaM/OmegaF, extra=0x1EA
        //_Gen_OmegaF = 1675, // OmegaM->OmegaF/OmegaM, extra=0x1EB
        //_Gen_PacketFilterF = 3500, // none->player, extra=0x0
        //_Gen_PacketFilterM = 3499, // none->player, extra=0x0
        //_Gen_VulnerabilityUp = 3366, // none->player, extra=0x0
        //_Gen_Superfluid = 1676, // OmegaM/OmegaF->OmegaM/OmegaF, extra=0x1ED
        //_Gen_RadiantAegis = 2702, // none->player, extra=0x0
    };

    public enum IconID : uint
    {
        WaveCannonKyrios = 23, // player
        SolarRay = 343, // player
        PartySynergyCircle = 416, // player (TODO: verify)
        PartySynergyTriangle = 417, // player (TODO: verify)
        PartySynergySquare = 418, // player (TODO: verify)
        PartySynergyCross = 419, // player (TODO: verify)
        Spotlight = 100, // player
    };

    public enum TetherID : uint
    {
        Blaster = 89, // player->Boss
        PartySynergy = 222, // player->player
    };
}
