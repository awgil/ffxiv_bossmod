namespace BossMod.Endwalker.Alliance.A21Nophica
{
    public enum OID : uint
    {
        Boss = 0x3D81, // R7.040, x1
        Helper = 0x233C, // R0.500, x48
        Tower = 0x3E73, // R1.350, spawn during fight
        BlueSafeZone = 0x1EB845, // R0.500, EventObj type, spawn during fight
        GoldSafeZone = 0x1EB846, // R0.500, EventObj type, spawn during fight
        BlueTower = 0x1EB843, // R0.500, EventObj type, spawn during fight
        GoldTower = 0x1EB844, // R0.500, EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 31782, // Boss->player, no cast, single-target
        Teleport = 31781, // Boss->location, no cast, single-target
        Abundance = 31780, // Boss->self, 5.0s cast, range 100 circle, raidwide
        MatronsPlenty = 31752, // Boss->self, 2.5s cast, single-target, visual (transition arena to flowers/grass)
        MatronsPlentyVisual = 31753, // Helper->self, 2.5s cast, range 100 circle, visual? (maybe brambles outside?)
        SeasonsPassing = 31754, // Boss->self, no cast, single-target, visual (transition to a different arena)
        GivingLandDonut = 32792, // Boss->self, 5.0s cast, single-target, visual (donut aoe)
        GivingLandCircle = 32794, // Boss->self, 5.0s cast, single-target, visual (circle aoe)
        SummerShade = 31759, // Helper->self, 5.3s cast, range 12-40 donut
        SpringFlowers = 31760, // Helper->self, 5.3s cast, range 12 circle
        MatronsHarvestDonut = 31773, // Boss->self, 8.0s cast, range 100 circle, raidwide + transition arena to normal
        MatronsHarvestCircle = 31774, // Boss->self, 8.0s cast, range 100 circle, raidwide + transition arena to normal
        ReapersGale1 = 31755, // Boss->self, 4.0s cast, single-target, visual (crisscross aoe)
        ReapersGale2 = 31756, // Boss->self, 4.0s cast, single-target, visual (crisscross aoe)
        ReapersGale3 = 31757, // Boss->self, 4.0s cast, single-target, visual (crisscross aoe)
        ReapersGaleAOE = 31758, // Helper->self, 4.5s cast, range 72 width 8 rect
        FloralHaze = 32235, // Boss->self, 3.0s cast, range 100 circle, visual (forced march debuffs)
        MatronsBreath = 31775, // Boss->self, 2.5s cast, single-target, visual (color towers)
        Blueblossoms = 31776, // Helper->self, no cast, range 100 circle, blue tower explosion
        Giltblossoms = 31777, // Helper->self, no cast, range 100 circle, gold tower explosion
        Landwaker = 31769, // Boss->self, 4.0s cast, range 100 circle, raidwide + puddles
        LandwakerAOE = 31772, // Helper->location, 8.5s cast, range 10 circle puddles
        SowingCircle = 31761, // Boss->self, 4.0s cast, single-target, visual (exaflares)
        SowingCircleFirst = 31764, // Helper->location, 5.0s cast, range 5 circle
        SowingCircleRest = 31765, // Helper->location, 1.0s cast, range 5 circle
        Furrow = 31766, // Boss->players, 6.0s cast, range 6 circle stack
        HeavensEarth = 31778, // Boss->self, 5.0s cast, single-target, visual (tankbusters)
        HeavensEarthAOE = 31779, // Helper->player, 5.0s cast, range 5 circle tankbuster
    };

    public enum SID : uint
    {
        ForwardMarch = 3538, // none->player, extra=0x0
        AboutFace = 3539, // none->player, extra=0x0
        LeftFace = 3540, // none->player, extra=0x0
        RightFace = 3541, // none->player, extra=0x0
        ForcedMarch = 1257, // none->player, extra=0x1/0x8/0x4/0x2
        BloomingBlue = 3459, // none->player, extra=0x0
        BloomingGold = 3460, // none->player, extra=0x0
    };

    public enum IconID : uint
    {
        Order1 = 398, // Tower
        Order2 = 399, // Tower
        Order3 = 400, // Tower
        Order4 = 401, // Tower
        Order5 = 402, // Tower
        Order6 = 403, // Tower
        Furrow = 318, // player
        HeavensEarth = 343, // player
    };
}
