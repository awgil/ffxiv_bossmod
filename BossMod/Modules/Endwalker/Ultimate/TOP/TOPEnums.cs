namespace BossMod.Endwalker.Ultimate.TOP
{
    public enum OID : uint
    {
        Boss = 0x3D5C, // R12.006, x1
        Helper = 0x233C, // R0.500, x36
        OpticalUnit = 0x3D64, // R0.500, x1
        Tower1 = 0x1EB83C, // R0.500, EventObj type, spawn during fight (unlike Tower2, doesn't get eobjstate events on enter/exit)
        Tower2 = 0x1EB83D, // R0.500, EventObj type, spawn during fight
        //_Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x1, EventObj type
        //_Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type
    };

    public enum AID : uint
    {
        AutoAttack = 31741, // Boss->player, no cast, single-target
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
    };

    public enum SID : uint
    {
        InLine1 = 3004, // none->player, extra=0x0
        InLine2 = 3005, // none->player, extra=0x0
        InLine3 = 3006, // none->player, extra=0x0
        InLine4 = 3451, // none->player, extra=0x0
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
    };

    public enum IconID : uint
    {
        WaveCannonKyrios = 23, // player
    };

    public enum TetherID : uint
    {
        Blaster = 89, // player->Boss
    };
}
