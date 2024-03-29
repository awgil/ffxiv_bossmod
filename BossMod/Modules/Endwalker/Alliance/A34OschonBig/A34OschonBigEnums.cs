using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A34OschonBig

{
    public enum OID : uint
{
    OschonBig  = 0x406F, // R24.990, x1
    OschonSmall = 0x406D, // R8.000, x1
    OschonsAvatar = 0x406E, // R8.000, x4
    OschonHelper = 0x233C, // R0.500, x40, 523 type
    PedestalOfPassage = 0x1EB91A, // R0.500, x1, EventObj type
    ExitToTheOmphalos = 0x1EB91E, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Unknown = 0x400E, // R0.500, spawn during fight
};
    public enum AID : uint
{
    _AutoAttack_ = 35907, // OschonBig->player, no cast, single-target
    PitonPull1Visual = 35241, // OschonBig->self, 8.0s cast, single-target // NW and ES Visual
    PitonPull3Visual2 = 35242, // OschonBig->self, 8.0s cast, single-target // NE and SW Visual
    PitonPullAOE = 35243, // OschonHelper->location, 8.5s cast, range 22 circle // Massive AOEs
    
    WeaponskillAOE = 35248, // OschonHelper->location, 2.0s cast, range 6 circle
    
    AltitudeVisual = 35247, // OschonBig->location, 6.0s cast, single-target // Visual
    AltitudeAOE = 35249, // OschonHelper->location, 7.0s cast, range 6 circle // Multiple AOEs

    //For the life of me couldnt figure out why this would not appear
    FlintedFoehnVisual = 35236, // OschonBig->self, 4.5s cast, single-target // Visual
    FlintedFoehnStack = 35238, // OschonHelper->players, no cast, range 8 circle // Multihit party stack
    
    WanderingShotVisual = 36087, // OschonBig->self, 7.0s cast, range 40 width 40 rect
    WanderingShot2 = 36086, // OschonBig->self, 7.0s cast, range 40 width 40 rect
    
    GreatWhirlwindAOE = 35246, // OschonHelper->location, 3.6s cast, range 23 circle // Massive AOE
    
    TheArrowVisual = 35228, // OschonBig->self, 6.0s cast, single-target
    TheArrowTankbuster = 35230, // OschonHelper->player, 7.0s cast, range 10 circle // Tankbuster


    //Not finished
    ArrowTrailVisual = 35250, // OschonBig->self, 3.0s cast, single-target
    ArrowTrailAOE = 35252, // OschonHelper->self, no cast, range 10 width 10 rect // Arrows will travel down several columns in the arena, telegraphed by red areas
    ArrowTrailRectAOE = 35251, // OschonHelper->self, 2.0s cast, range 40 width 10 rect // telegraph for ArrowTrailAOE

    DownhillVisual = 35232, // OschonBig->self, 3.0s cast, single-target
    DownhillSmallAOE = 35909, // OschonHelper->location, 3.0s cast, range 6 circle
    DownhillBigAOE = 35234, // OschonHelper->location, 14.0s cast, range 8 circle

    WanderingVolley = 35245, // OschonBig->self, 10.0s cast, range 40 width 40 rect
    WanderingVolley2 = 35244, // OschonBig->self, 10.0s cast, range 40 width 40 rect


};

  public enum SID : uint
{
    VulnerabilityUp = 1789, // OschonHelper->player, extra=0x1/0x2/0x3
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    SustainedDamage = 2935, // OschonHelper->player, extra=0x0

};
  
  public enum IconID : uint
{
    FlintedFoehnMarker = 316, // player
    Arrowbuster = 500, // player
};

}
