using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A33Oschon

{

    public enum OID : uint
{
    Oschon = 0x406D, // R8.000, x1
    OschonBig = 0x406F, // R24.990, spawn during fight
    OschonsAvatar = 0x406E, // R8.000, x4
    OschonHelper = 0x233C, // R0.500, x40, 523 type
    Unknown = 0x400E, // R0.500, x1
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    PedestalOfPassage = 0x1EB91A, // R0.500, x1, EventObj type
    ExitToTheOmphalos = 0x1EB91E, // R0.500, x1, EventObj type
};

    public enum AID : uint
{
    AutoAttack = 35906, // Oschon->player, no cast, single-target
    AutoAttackBig = 35907, // OschonBig->player, no cast, single-target

    Ability1 = 35224, // Oschon->location, no cast, single-target
    Ability2 = 34863, // OschonHelper->self, no cast, single-target
    Ability3 = 34864, // OschonHelper->self, no cast, single-target
    Ability4 = 34862, // OschonHelper->self, no cast, single-target
    Ability5 = 34865, // OschonHelper->self, no cast, single-target

    TrekShot1 = 35214, // Oschon->self, 6.0s cast, single-target
    TrekShot2 = 35908, // OschonHelper->self, 9.5s cast, range 65 ?-degree cone // wide frontal cone AoE
    TrekShot3 = 35213, // Oschon->self, 6.0s cast, single-target
    TrekShot4 = 35215, // OschonHelper->self, 9.5s cast, range 65 ?-degree cone // wide frontal cone AoE

    SwingingDraw1 = 35210, // OschonsAvatar->self, 7.0s cast, single-target
    SwingingDraw2 = 35212, // OschonsAvatar->self, 2.0s cast, range 65 120-degree cone // wide frontal cone AoE
    SwingingDraw3 = 35211, // OschonsAvatar->self, 7.0s cast, single-target
    Reproduce = 35209, // Oschon->self, 3.0s cast, single-target // Summons an OschonsAvatar add; The add will use Swinging Draw
    
    SuddenDownpour1 = 35225, // Oschon->self, 4.0s cast, single-target
    SuddenDownpour2 = 36026, // OschonHelper->self, 5.0s cast, range 60 circle // Raidwide 

    Downhill1 = 35231, // Oschon->self, 3.0s cast, single-target
    Downhill2 = 35233, // OschonHelper->location, 8.5s cast, range 6 circle // Summons several circle AoE telegraphs. This is used with Climbing Shot.
    ClimbingShot = 35217, // Oschon->self, 5.0s cast, range 80 circle // knockback; Soaring Minuet immediately follows
    ClimbingShot2 = 35216, // Oschon->self, 5.0s cast, range 80 circle // knockback; Soaring Minuet immediately follows

    SoaringMinuet1 = 36110, // Oschon->self, 5.0s cast, range 65 270-degree cone // 270 degree frontal cleave from the boss. Only has a brief AoE indicator.
    SoaringMinuet2 = 35220, // Oschon->self, 5.0s cast, range 65 270-degree cone // 270 degree frontal cleave from the boss. Only has a brief AoE indicator.
    
    FlintedFoehn1 = 35235, // Oschon->self, 4.5s cast, single-target
    FlintedFoehnStack = 35237, // OschonHelper->players, no cast, range 6 circle // Multi-hit stack AoE
    
    TheArrow1 = 35227, // Oschon->self, 4.0s cast, single-target
    TheArrow2 = 35229, // OschonHelper->players, 5.0s cast, range 6 circle // Telegraphed AoE tankbusters on all three tanks.
    
    LoftyPeaks = 35239, // Oschon->self, 5.0s cast, single-target // Phase Change
    MovingMountains = 36067, // OschonHelper->self, no cast, range 60 circle // Raidwide
    PeakPeril = 36068, // OschonHelper->self, no cast, range 60 circle // Raidwide
    Shockwave = 35240, // OschonHelper->self, 8.4s cast, range 60 circle // Raidwide
};

    public enum SID : uint
{
    Windburn1 = 3069, // none->player, extra=0x0
    Windburn2 = 3070, // none->player, extra=0x0
    Unknown = 2970, // Oschon->Oschon, extra=0x294
    Weakness = 43, // none->player, extra=0x0
    VulnerabilityUp = 1789, // OschonsAvatar/Oschon/OschonHelper->player, extra=0x1/0x2
    Transcendent = 418, // none->player, extra=0x0
    TheRoadTo80 = 1411, // none->player, extra=0x0
    Invincibility = 1570, // none->player, extra=0x0

};
    
    public enum IconID : uint
{
    FlintedFoehnStack = 316, // player
    Icon_344 = 344, // player
};


}
