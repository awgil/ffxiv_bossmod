namespace BossMod.Stormblood.Alliance.A22Belias;

public enum OID : uint
{
    Boss = 0x22A4, // R5.100, x2
    Gigas = 0x22A5, // R3.400, x0 (spawn during fight)
    BeliasTheGigas = 0x18D6, // R0.500, x24, 523 type
    Montblanc = 0x239D, // R0.300, x1
    RamzaBasLexentale = 0x23E, // R0.500, x1
    Actor1ea9a4 = 0x1EA9A4, // R0.500, x0 (spawn during fight), EventObj type
    Actor22a6 = 0x22A6, // R1.500, x0 (spawn during fight)
    Actor1ea9a5 = 0x1EA9A5, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    CrimsonCyclone1 = 11490, // Boss->location, 5.0s cast, single-target
    CrimsonCyclone2 = 11491, // BeliasTheGigas->self, no cast, range 40+R width 20 rect
    CrimsonCyclone3 = 11492, // BeliasTheGigas->self, no cast, range 60+R width 20 rect
    CrimsonCyclone4 = 11615, // Boss->self, no cast, range 70+R width 20 rect
    Eruption = 11485, // BeliasTheGigas->location, 3.0s cast, range 8 circle
    Fire = 11483, // Boss->player, 4.0s cast, single-target
    FireIV = 11484, // Boss->self, 5.0s cast, range 90 circle
    GigasAutoAttack = 870, // Gigas->player, no cast, single-target
    Hellfire = 11496, // Boss->self, no cast, range 60 circle
    TheHandOfTime = 11493, // Boss->self, 5.0s cast, single-target
    TimeBomb1 = 11494, // Boss->self, 3.0s cast, single-target
    TimeBomb2 = 11495, // BeliasTheGigas->self, 2.0s cast, range 60 90-degree cone
    TimeEruption1 = 11486, // Boss->self, 3.0s cast, single-target
    TimeEruption2 = 11487, // BeliasTheGigas->self, 5.0s cast, range 20 width 20 rect
    TimeEruption3 = 11488, // BeliasTheGigas->self, 8.0s cast, range 20 width 20 rect
}

public enum SID : uint
{
    Invincibility = 1570, // none->player, extra=0x0
    VulnerabilityUp = 202, // BeliasTheGigas/Boss->player, extra=0x1/0x2/0x3
    Slow1 = 1509, // BeliasTheGigas/Boss->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    Slow2 = 1568, // none->player, extra=0x2/0x3/0x1
    Burns = 530, // none->player, extra=0x2/0x3/0x1
    TemporalDisplacement = 1119, // none->player, extra=0x0
    TemporalBarrier = 1571, // none->player, extra=0x0
    VulnerabilityDown = 350, // none->Gigas, extra=0x0

}

public enum IconID : uint
{
    Icon_198 = 198, // player
    Icon_97 = 97, // player
    Icon_102 = 102, // player
}

public enum TetherID : uint
{
    Tether_5 = 5, // Actor22a6->player
    Tether_78 = 78, // Actor22a6->player
    Tether_14 = 14, // Gigas->Gigas
}
