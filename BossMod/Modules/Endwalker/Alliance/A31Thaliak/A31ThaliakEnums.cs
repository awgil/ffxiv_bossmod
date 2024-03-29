using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A31Thaliak

{

    public enum OID : uint
{
    Thaliak = 0x404C, // R9.496, x1
    ThaliakClone = 0x404D, // R9.496, x1
    ThaliakHelper = 0x233C, // R0.500, x44, 523 type
    WindWreathedPortal = 0x1EB91D, // R0.500, x1, EventObj type
    HieroglyphikaIndicator = 0x40AA, // R0.500, x1 // Rotation Indicator
    UnknownActor = 0x400E, // R0.500, x1
};

  
    public enum AID : uint
{
    AutoAttack = 35036, // Thaliak->player, no cast, single-target
    Ability1_1 = 35035, // Thaliak->location, no cast, single-target
    
    Katarraktes = 35025, // Thaliak->self, 5.0s cast, single-target // Raidwide damage and bleeding damage-over-time.
    KatarraktesHelper = 35034, // ThaliakHelper->self, 5.7s cast, range 70 circle 
    
    Hieroglyphika = 35023, // Thaliak->self, 5.0s cast, single-target // Covers all but two tiles of the arena with a green AoE telegraph
    HieroglyphikaHelper = 35024, // ThaliakHelper->self, 3.0s cast, range 12 width 12 rect // 2 safe spots
    
    Thlipsis = 35032, // Thaliak->self, 4.0s cast, single-target // Stack marker on random player
    ThlipsisHelper = 35033, // ThaliakHelper->players, 6.0s cast, range 6 circle
    
    Hydroptosis = 35028, // Thaliak->self, 4.0s cast, single-target // Spread markers on random players. Inflicts Water resistance down making overlap lethal.
    HydroptosisHelper = 35029, // ThaliakHelper->player, 5.0s cast, range 6 circle
    
    Rhyton = 35030, // Thaliak->self, 5.0s cast, single-target // Line AoE tankbusters targeting all 3 tanks, or whoever is top enmity if not all tanks are alive.
    RhytonHelper = 35031, // ThaliakHelper->players, no cast, range 70 width 6 rect

    Rheognosis = 35012, // Thaliak->self, 5.0s cast, single-target // Castbar Indicator
    RheognosisPetrine = 35013, // Thaliak->self, 5.0s cast, single-target // Castbar Indicator
    RheognosisPetrineHelper = 35014, // ThaliakClone->self, no cast, single-target
    RheognosisKnockback = 35015, // ThaliakHelper->self, 3.0s cast, range 48 width 48 rect // Summons the same knockback clone as in Rheognosis, but two spheres of water
    RheognosisCrashExaflare = 35016, // ThaliakHelper->self, no cast, range 10 width 24 rect // 5 helpers
    
    Tetraktys = 35017, // Thaliak->self, 6.0s cast, single-target // Transforms the arena into a triangle, with a dangerous AoE surrounding it.
    TetraBlueTriangles = 35018, // ThaliakHelper->self, 1.8s cast, ??? // Blue triangles?
    TetraGreenTriangles = 35019, // ThaliakHelper->self, 1.8s cast, ??? // Green triangles?
    
    TetraktuosKosmos = 35020, // Thaliak->self, 4.0s cast, single-target // Summons a triangular tower on a panel
    TetraktuosKosmosHelper = 35022, // ThaliakHelper->self, 2.9s cast, ??? // telegraphed line AoEs
    TetraktuosKosmosHelper2 = 35021, // ThaliakHelper->self, 2.9s cast, range 30 width 16 rect // This attack repeats, but now two towers will spawn

    LeftBank = 35026, // Thaliak->self, 5.0s cast, range 60 180-degree cone // A half-room cleave
    LeftBank2 = 35884, // Thaliak->self, 22.0s cast, range 60 180-degree cone // A half-room cleave
    RightBank = 35027, // Thaliak->self, 5.0s cast, range 60 180-degree cone // A half-room cleave
    RightBank2 = 35885, // Thaliak->self, 22.0s cast, range 60 180-degree cone // A half-room cleave
};

    public enum SID : uint
{
    Weakness = 43, // none->player, extra=0x0
    Bleeding = 2088, // ThaliakHelper->player, extra=0x0
    VulnerabilityUp = 1789, // Thaliak/ThaliakHelper->player, extra=0x1
    WaterResistanceDownII = 1025, // ThaliakHelper->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    SustainedDamage = 2935, // ThaliakHelper->player, extra=0x0
    DownForTheCount = 783, // ThaliakHelper->player, extra=0xEC7
    Inscribed = 3732, // none->player, extra=0x0
    Bind = 2518, // none->player, extra=0x0
    BrinkOfDeath = 44, // none->player, extra=0x0

};


    public enum IconID : uint
{
    Icon_318 = 318, // player
    HydroptosisSpread = 139, // player
    RhytonBuster = 471, // player
    ClockwiseHieroglyphika = 487, // HieroglyphikaIndicator
    CounterClockwiseHieroglyphika = 490, // HieroglyphikaIndicator
};

public enum TetherID : uint
{
};

  
}
