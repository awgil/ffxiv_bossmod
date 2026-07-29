namespace BossMod.Dawntrail.Raid.M12NLindwurm;

public enum OID : uint
{
    Lindwurm = 0x4AF7, // R13.8
    BurstBlob = 0x1EBF29, // R0.5
    Lindwurm1 = 0x4AE4, // R1.000, x?
    Lindwurm2 = 0x4AF9, // R1.000, x?, Helper type
    Lindwurm3 = 0x4AF8, // R4.000, x?, Helper type
    Lindwurm4 = 0x4AFA, // R0.000, x?, Part type
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 46225, // Lindwurm4->player, no cast, single-target

    TheFixer = 46228, // Lindwurm->self, 5.0s cast, range 60 circle
    BloodshedLeft = 46181, // Lindwurm->self, 0.5+6.0s cast, single-target
    BloodshedRight = 46182, // Lindwurm->self, 0.5+6.0s cast, single-target
    SerpentineScourgeLeft = 46183, // Lindwurm->self, 1.0+1.0s cast, single-target, left
    SerpentineScourgeRight = 46184, // Lindwurm->self, 1.0+1.0s cast, single-target, right
    SerpentineScourge = 47547, // Helper->self, 2.0s cast, range 30 width 20 rect
    RavenousReachVisual1 = 46185, // Lindwurm->self, 1.0+10.7s cast, single-target
    RavenousReachVisual2 = 46390, // Lindwurm3->self, no cast, single-target
    RavenousReachVisual3 = 46952, // Lindwurm3->self, no cast, single-target

    RavenousReach = 46189, // Helper->self, 10.6s cast, range 35 120.000-degree cone (assuming players stack middle, say risky at 5s?)
    Burst = 46191, // Helper->location, 2.5s cast, range 12 circle

    SkinsplitterVisual = 46195, // Lindwurm->self, no cast, single-target (spin + reset)
    Skinsplitter = 46396, // Helper->self, no cast, range ?-13 donut
    CruelCoil1 = 45339, // Lindwurm->location, 3.0s cast, single-target
    CruelCoil2 = 45340, // Lindwurm->location, 3.0s cast, single-target
    ConstrictorVisual1 = 46397, // Lindwurm->location, no cast, single-target
    ConstrictorVisual2 = 48628, // Lindwurm->location, no cast, single-target
    ConstrictorVisual3 = 46198, // Helper->self, 40.0s cast, range 13 circle (for cruel coil mechanic)
    Constrictor = 46199, // Helper->self, 1.0s cast, range 13 circle

    VisceralBurstVisual = 46226, // Lindwurm->self, 4.0+1.0s cast, single-target
    VisceralBurst = 46227, // Helper->player, no cast, range 6 circle
    Grotesquerie = 46209, // Lindwurm->self, 3.0s cast, single-target

    DramaticLysis = 46211, // Helper->player, no cast, range 6 circle (Bursting Grotesquerie)
    Splattershed1Visual1 = 47552, // Lindwurm->self, 3.0+2.1s cast, single-target
    Splattershed1Visual2 = 48096, // Lindwurm->self, 5.0s cast, single-target
    Splattershed1 = 47557, // Helper->self, no cast, range 60 circle
    Splattershed2 = 48097, // Helper->self, 2.3s cast, range 60 circle

    FeralFissionVisual = 46200, // Lindwurm->self, 3.0s cast, single-target
    GrandEntrance1 = 46202, // Lindwurm2->self, 3.0s cast, range 2 circle (small circles leading up to arena break)
    GrandEntrance2 = 46203, // Helper->location, 3.5s cast, range 2 circle
    BringDownTheHouseVisual = 46204, // Lindwurm->self, 3.0+1.0s cast, single-target
    BringDownTheHouse = 46205, // Helper->self, 4.0s cast, range 15 width 10 rect
    SplitScourge = 46207, // Lindwurm3->self, 5.0s cast, range 30 width 10 rect

    VenomousScourge = 46208, // Helper->player, 5.0s cast, range 5 circle
    Shockwave = 46210, // Helper->player, no cast, single-target (Fleshforward/back tp cast)
    FourthWallFusion = 46212, // Helper->players, no cast, range 6 circle (SharedGrotesquerie)
    HemorrhagicProjection = 46213, // Helper->self, no cast, range 60 ?-degree cone (DirectedGrotesquerie cone) (30 degrees?)
    Splattershed2Visual = 47549, // Lindwurm->self, 3.0+2.1s cast, single-target

    MindlessFleshVisual1 = 48088, // Lindwurm->self, 3.0s cast, single-target
    MindlessFleshVisual2 = 48089, // Lindwurm->self, 3.0s cast, single-target
    MindlessFlesh1 = 48090, // Helper->self, 4.0s cast, range 30 width 8 rect
    MindlessFlesh2 = 48091, // Helper->self, 5.5s cast, range 30 width 8 rect
    MindlessFlesh3 = 48092, // Helper->self, 7.5s cast, range 30 width 8 rect
    MindlessFlesh4 = 48093, // Helper->self, 8.5s cast, range 30 width 8 rect
    MindlessFlesh5 = 48094, // Helper->self, 10.0s cast, range 30 width 8 rect
    MindlessFleshBig = 48095, // Helper->self, 16.1s cast, range 30 width 35 rect
    DramaticLysis1 = 48371, // Helper->player, 5.0s cast, range 6 circle (phase 2 spread icon AOE, no grotesquerie)

    Unknown = 48086, // Lindwurm->self, no cast, single-target, boss "death" animation?
    _Weaponskill_ = 46186, // Lindwurm3->self, no cast, single-target
    _Weaponskill_1 = 46190, // Lindwurm->self, no cast, single-target
    _Weaponskill_3 = 47046, // Lindwurm->self, no cast, single-target
    _Weaponskill_6 = 46224, // Lindwurm->self, no cast, single-target
    _Weaponskill_2 = 46187, // Lindwurm3->self, no cast, single-target
    _Weaponskill_4 = 46206, // Lindwurm2->self, no cast, single-target
    _Weaponskill_5 = 46201, // Lindwurm->self, no cast, single-target
    _Weaponskill_7 = 48085, // Lindwurm->self, no cast, single-target
    _Spell_ = 46194, // Helper->self, no cast, range 60 circle
    _Weaponskill_9 = 48332, // Lindwurm->self, no cast, single-target
}

public enum SID : uint
{
    Bind = 2518, // none->player, extra=0x0
    BurstingGrotesquerie = 4749, // none->player, extra=0x0 (aoe around players after timer)
    SharedGrotesquerie = 4750, // none->player, extra=0x0 (stack after timer?) (only 1 player gets this?)
    DirectedGrotesquerie = 4751, // none->player, extra=0x0 (cone AOE after timer)
    FateOfTheWurm = 4772, // none->player, extra=0x0 (no jump)
    FleshForward = 4747, // none->player, extra=0x0 (tp forward after timer) 15f
    FleshBack = 4748, // none->player, extra=0x0 (tp back after timer)
    Direction = 2056 // none->player, extra=0x40B/0x409/0x408/0x40A (408 = forward, 409 = right, 40A = back, 40B = left) (30 degrees?) 
}

public enum IconID : uint
{
    TankBait = 344, // player->self Visceral Burst
    SpreadBurstingGrotesquerie = 139, // player->self (spread)
    VenomousScourge = 376, // player->self (spread)
    FleshTimer = 654, // player->self (5s before tp)
    SharedGrotesquerie = 93, // player->self
    Countdown = 354 // player->self
}
