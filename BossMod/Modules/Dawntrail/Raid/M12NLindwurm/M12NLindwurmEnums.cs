namespace BossMod.Dawntrail.Raid.M12NLindwurm;

public enum OID : uint
{
    Lindwurm = 0x4AF7, // R13.8
    BurstBlob = 0x1EBF29, // R0.5
    _Gen_Lindwurm1 = 0x4AE4, // R1.000, x?
    _Gen_Lindwurm2 = 0x4AF9, // R1.000, x?, Helper type
    _Gen_Lindwurm3 = 0x4AF8, // R4.000, x?, Helper type
    _Gen_Lindwurm4 = 0x4AFA, // R0.000, x?, Part type
    Helper = 0x233C
}

public enum AID : uint
{
    _AutoAttack_ = 46225, // 4AFA->player, no cast, single-target
    TheFixer = 46228, // Lindwurm->self, 5.0s cast, range 60 circle
    BloodshedLeft = 46181, // Lindwurm->self, 0.5+6.0s cast, single-target
    BloodshedRight = 46182, // Lindwurm->self, 0.5+6.0s cast, single-target
    SerpentineScourgeLeft = 46183, // Lindwurm->self, 1.0+1.0s cast, single-target, left
    SerpentineScourgeRight = 46184, // Lindwurm->self, 1.0+1.0s cast, single-target, right
    SerpentineScourge = 47547, // 233C->self, 2.0s cast, range 30 width 20 rect
    _Weaponskill_RavenousReach = 46185, // Lindwurm->self, 1.0+10.7s cast, single-target
    _Weaponskill_ = 46186, // 4AF8->self, no cast, single-target
    _Weaponskill_RavenousReach1 = 46390, // 4AF8->self, no cast, single-target
    _Weaponskill_1 = 46190, // Lindwurm->self, no cast, single-target
    RavenousReach = 46189, // 233C->self, 10.6s cast, range 35 120.000-degree cone (assuming players stack middle, say risky at 5s?)
    Burst = 46191, // 233C->location, 2.5s cast, range 12 circle
    _Weaponskill_CruelCoil = 45339, // 4AF7->location, 3.0s cast, single-target
    _Spell_ = 46194, // 233C->self, no cast, range 60 circle
    _Weaponskill_9 = 48332, // Lindwurm->self, no cast, single-target
    SkinsplitterVisual = 46195, // 4AF7->self, no cast, single-target (spin + reset)
    Skinsplitter = 46396, // 233C->self, no cast, range ?-13 donut
    _Weaponskill_Constrictor = 46397, // 4AF7->location, no cast, single-target
    Constrictor = 46199, // 233C->self, 1.0s cast, range 13 circle
    VisceralBurstVisual = 46226, // 4AF7->self, 4.0+1.0s cast, single-target
    VisceralBurst = 46227, // 233C->player, no cast, range 6 circle
    Grotesquerie = 46209, // 4AF7->self, 3.0s cast, single-target
    _Weaponskill_2 = 46187, // 4AF8->self, no cast, single-target
    _Weaponskill_RavenousReach3 = 46952, // 4AF8->self, no cast, single-target
    DramaticLysis = 46211, // 233C->player, no cast, range 6 circle (Bursting Grotesquerie)
    Splattershed1Visual = 47552, // 4AF7->self, 3.0+2.1s cast, single-target
    Splattershed = 47557, // 233C->self, no cast, range 60 circle
    _Weaponskill_3 = 47046, // 4AF7->self, no cast, single-target
    FeralFissionVisual = 46200, // 4AF7->self, 3.0s cast, single-target
    GrandEntrance1 = 46202, // 4AF9->self, 3.0s cast, range 2 circle (small circles leading up to arena break)
    GrandEntrance2 = 46203, // 233C->location, 3.5s cast, range 2 circle
    BringDownTheHouseVisual = 46204, // 4AF7->self, 3.0+1.0s cast, single-target
    _Weaponskill_4 = 46206, // 4AF9->self, no cast, single-target
    BringDownTheHouse = 46205, // 233C->self, 4.0s cast, range 15 width 10 rect
    SplitScourge = 46207, // 4AF8->self, 5.0s cast, range 30 width 10 rect
    _Weaponskill_5 = 46201, // 4AF7->self, no cast, single-target
    VenomousScourge = 46208, // 233C->player, 5.0s cast, range 5 circle
    Shockwave = 46210, // 233C->player, no cast, single-target (Fleshforward/back tp cast)
    FourthWallFusion = 46212, // 233C->players, no cast, range 6 circle (SharedGrotesquerie)
    HemorrhagicProjection = 46213, // 233C->self, no cast, range 60 ?-degree cone (DirectedGrotesquerie cone) (30 degrees?)
    Splattershed2Visual = 47549, // 4AF7->self, 3.0+2.1s cast, single-target
    _Weaponskill_6 = 46224, // 4AF7->self, no cast, single-target
    _Weaponskill_CruelCoil1 = 45340, // 4AF7->location, 3.0s cast, single-target
    _Weaponskill_Constrictor2 = 48628, // 4AF7->location, no cast, single-target
    _Weaponskill_7 = 48085, // 4AF7->self, no cast, single-target
    Unknown = 48086, // 4AF7->self, no cast, single-target, boss "death" animation?
    MindlessFleshVisual = 48088, // 4AF7->self, 3.0s cast, single-target
    MindlessFlesh1 = 48090, // 233C->self, 4.0s cast, range 30 width 8 rect
    MindlessFlesh2 = 48091, // 233C->self, 5.5s cast, range 30 width 8 rect
    MindlessFlesh3 = 48092, // 233C->self, 7.5s cast, range 30 width 8 rect
    MindlessFlesh4 = 48093, // 233C->self, 8.5s cast, range 30 width 8 rect
    MindlessFlesh5 = 48094, // 233C->self, 10.0s cast, range 30 width 8 rect
    MindlessFleshBig = 48095, // 233C->self, 16.1s cast, range 30 width 35 rect
    DramaticLysis1 = 48371, // 233C->player, 5.0s cast, range 6 circle (phase 2 spread icon AOE, no grotesquerie)
    _Weaponskill_Splattershed3 = 48096, // 4AF7->self, 5.0s cast, single-target
    _Weaponskill_Splattershed4 = 48097, // 233C->self, 2.3s cast, range 60 circle
    _Weaponskill_MindlessFlesh7 = 48089, // 4AF7->self, 3.0s cast, single-target
    _Weaponskill_8 = 46198, // 233C->self, 40.0s cast, range 13 circle (for cruel coil mechanic)
}

public enum SID : uint
{
    _Gen_1 = 3913, // Boss->Boss, extra=0x444/0x447
    _Gen_Bind = 2518, // none->player, extra=0x0
    _Gen_2 = 4956, // none->player, extra=0x441 (unknown but applied to players at same time as grotesquerie)
    BurstingGrotesquerie = 4749, // none->player, extra=0x0 (aoe around players after timer)
    SharedGrotesquerie = 4750, // none->player, extra=0x0 (stack after timer?) (only 1 player gets this?)
    DirectedGrotesquerie = 4751, // none->player, extra=0x0 (cone AOE after timer)
    FateOfTheWurm = 4772, // none->player, extra=0x0 (no jump)
    FleshForward = 4747, // none->player, extra=0x0 (tp forward after timer) 15f
    FleshBack = 4748, // none->player, extra=0x0 (tp back after timer)
    _Gen_Direction = 2056, // none->player, extra=0x40B/0x409/0x408/0x40A (408 = forward, 409 = right, 40A = back, 40B = left) (30 degrees?) 
}

public enum IconID : uint
{
    TankBait = 344, // player->self Visceral Burst
    SpreadBurstingGrotesquerie = 139, // player->self (spread)
    VenomousScourge = 376, // player->self (spread)
    FleshTimer = 654, // player->self (5s before tp)
    SharedGrotesquerie = 93, // player->self
    Countdown = 354, // player->self
}
