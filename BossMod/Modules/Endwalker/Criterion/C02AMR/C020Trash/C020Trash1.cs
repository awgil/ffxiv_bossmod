namespace BossMod.Endwalker.Criterion.C02AMR.C020Trash1;

public enum OID : uint
{
    NRaiko = 0x3F94, // R4.500, x1
    NFurutsubaki = 0x3F95, // R1.440, x2
    NFuko = 0x3F96, // R4.500, x1
    NPenghou = 0x3F97, // R1.680, x2
    NYuki = 0x3FA0, // R2.000, x1

    SRaiko = 0x3F9A, // R4.500, x1
    SFurutsubaki = 0x3F9B, // R1.440, x2
    SFuko = 0x3F9C, // R4.500, x1
    SPenghou = 0x3F9D, // R1.680, x2
    SYuki = 0x3FA2, // R2.000, x1
}

public enum AID : uint
{
    AutoAttack = 31320, // *Raiko/*Furutsubaki/*Fuko/*Penghou/*Yuki->player, no cast, single-target
    // raiko
    NBarrelingSmash = 34387, // NRaiko->player, 4.0s cast, width 7 rect charge
    NHowl = 34388, // NRaiko->self, 4.0s cast, range 60 circle
    NMasterOfLevin = 34389, // NRaiko->self, 4.0s cast, range 5-30 donut
    NDisciplesOfLevin = 34390, // NRaiko->self, 4.0s cast, range 10 circle
    NBloodyCaress = 34391, // NFurutsubaki->self, 3.0s cast, range 12 120-degree cone
    SBarrelingSmash = 34405, // SRaiko->player, 4.0s cast, width 7 rect charge
    SHowl = 34406, // SRaiko->self, 4.0s cast, range 60 circle
    SMasterOfLevin = 34407, // SRaiko->self, 4.0s cast, range 5-30 donut
    SDisciplesOfLevin = 34408, // SRaiko->self, 4.0s cast, range 10 circle
    SBloodyCaress = 34409, // SFurutsubaki->self, 3.0s cast, range 12 120-degree cone
    // fuko
    NTwister = 34392, // NFuko->players, 5.0s cast, range 8 circle stack
    NCrosswind = 34393, // NFuko->self, 4.0s cast, range 60 circle knockback 25
    NScytheTail = 34394, // NFuko->self, 4.0s cast, range 10 circle
    NTornado = 34395, // NPenghou->location, 3.0s cast, range 6 circle
    STwister = 34410, // SFuko->players, 5.0s cast, range 8 circle stack
    SCrosswind = 34411, // SFuko->self, 4.0s cast, range 60 circle knockback 25
    SScytheTail = 34412, // SFuko->self, 4.0s cast, range 10 circle
    STornado = 34413, // SPenghou->location, 3.0s cast, range 6 circle
    // yuki
    NRightSwipe = 34437, // NYuki->self, 4.0s cast, range 60 180-degree cone
    NLeftSwipe = 34438, // NYuki->self, 4.0s cast, range 60 180-degree cone
    SRightSwipe = 34440, // SYuki->self, 4.0s cast, range 60 180-degree cone
    SLeftSwipe = 34441, // SYuki->self, 4.0s cast, range 60 180-degree cone
}

public abstract class C020Trash1(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsSquare(20));
