namespace BossMod.RealmReborn.Extreme.Ex4Ifrit;

public enum OID : uint
{
    Boss = 0xD3, // R5.000, x1, and more spawn during fight
    Helper = 0x908, // R0.500, x20
    InfernalNailSmall = 0xD4, // R1.000, spawn during fight
    InfernalNailLarge = 0x91B, // R2.000, spawn during fight
};

public enum AID : uint
{
    AutoAttack = 451, // Boss->player, no cast, range 8+R 120-degree cone cleave, autoattack
    VulcanBurst = 1531, // Boss->self, no cast, range 16+R circle knockback 10
    Incinerate = 1528, // Boss->self, no cast, range 10+R 120-degree cone cleave, tankbuster applying debuff stack
    InfernoHowl = 1529, // Boss->player, 2.0s cast, single-target, applies searing wind on healer
    SearingWind = 1530, // Helper->location, no cast, range 14 circle around player with searing wind debuff, not including player himself
    Eruption = 1676, // Boss->self, 2.2s cast, single-target, visual
    EruptionAOE = 1677, // Helper->location, 3.0s cast, range 8 circle baited aoe (on 3 dps or on OT)
    InfernalSurge = 1535, // InfernalNailSmall->self, no cast, raidwide applying vuln up on death
    Hellfire = 1536, // Boss->self, 2.0s cast, raidwide on nail death (~45s/75s/115s time limit)
    RadiantPlume = 1356, // Boss->self, 2.2s cast, single-target, visual
    RadiantPlumeAOE = 1359, // Helper->location, 3.0s cast, range 8 circle aoe
    CrimsonCyclone = 1532, // Boss->self, 3.0s cast, range 44+R width 18 rect aoe
    InfernalFetters = 1534, // Helper->player, no cast, single-target, visual
};

public enum SID : uint
{
    Suppuration = 375, // Boss->player, extra=0x1/0x2/0x3/0x4/0x5
    SearingWind = 376, // Boss->player, extra=0x0
    VulnerabilityUp = 202, // InfernalNailSmall->player, extra=0x1/0x2/...
    Invincibility = 775, // none->Helper/Boss, extra=0x0
    InfernalFetters = 377, // none->player, extra=0x1/0x3/0x5
};
