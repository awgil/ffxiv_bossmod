namespace BossMod.Endwalker.Alliance.A31Thaliak;

public enum OID : uint
{
    Boss = 0x404C, // R9.496, x1
    Clone = 0x404D, // R9.496, x1
    HieroglyphikaIndicator = 0x40AA, // R0.500, x1
    Helper = 0x233C, // R0.500, x44, 523 type
}

public enum AID : uint
{
    AutoAttack = 35036, // Boss->player, no cast, single-target
    Teleport = 35035, // Boss->location, no cast, single-target
    Katarraktes = 35025, // Boss->self, 5.0s cast, single-target, visual (raidwide)
    KatarraktesAOE = 35034, // Helper->self, 5.7s cast, range 70 circle, raidwide
    Thlipsis = 35032, // Boss->self, 4.0s cast, single-target, visual (stack)
    ThlipsisAOE = 35033, // Helper->players, 6.0s cast, range 6 circle stack
    Hydroptosis = 35028, // Boss->self, 4.0s cast, single-target, visual (spread)
    HydroptosisAOE = 35029, // Helper->player, 5.0s cast, range 6 circle spread
    Rhyton = 35030, // Boss->self, 5.0s cast, single-target, visual (baited line tankbusters)
    RhytonAOE = 35031, // Helper->players, no cast, range 70 width 6 rect, tankbuster
    LeftBank = 35026, // Boss->self, 5.0s cast, range 60 180-degree cone
    RightBank = 35027, // Boss->self, 5.0s cast, range 60 180-degree cone

    Rheognosis = 35012, // Boss->self, 5.0s cast, single-target, visual (knockback across arena)
    RheognosisPetrine = 35013, // Boss->self, 5.0s cast, single-target, visual (knockback across arena + aoes)
    RheognosisCloneVisual = 35014, // Clone->self, no cast, single-target, visual (some clone vfx)
    RheognosisKnockback = 35015, // Helper->self, 3.0s cast, range 48 width 48 rect, knockback 25, dir forward
    RheognosisCrash = 35016, // Helper->self, no cast, range 10 width 24 rect 'exaflare'

    Tetraktys = 35017, // Boss->self, 6.0s cast, single-target, visual (triangles)
    TetraktysAOESmall = 35018, // Helper->self, 1.8s cast, triangle 16
    TetraktysAOELarge = 35019, // Helper->self, 1.8s cast, triangle 32
    TetraktuosKosmos = 35020, // Boss->self, 4.0s cast, single-target, visual (splitting triangle)
    TetraktuosKosmosAOERect = 35021, // Helper->self, 2.9s cast, range 30 width 16 rect
    TetraktuosKosmosAOETri = 35022, // Helper->self, 2.9s cast, triangle 16

    Hieroglyphika = 35023, // Boss->self, 5.0s cast, single-target, visual (rotating safespots)
    HieroglyphikaAOE = 35024, // Helper->self, 3.0s cast, range 12 width 12 rect
    HieroglyphikaLeftBank = 35884, // Boss->self, 22.0s cast, range 60 180-degree cone
    HieroglyphikaRightBank = 35885, // Boss->self, 22.0s cast, range 60 180-degree cone
}

public enum SID : uint
{
    Inscribed = 3732, // none->player, extra=0x0
    Bind = 2518, // none->player, extra=0x0
}

public enum IconID : uint
{
    Thlipsis = 318, // player
    Hydroptosis = 139, // player
    Rhyton = 471, // player
    HieroglyphikaCW = 487, // HieroglyphikaIndicator
    HieroglyphikaCCW = 490, // HieroglyphikaIndicator
}
