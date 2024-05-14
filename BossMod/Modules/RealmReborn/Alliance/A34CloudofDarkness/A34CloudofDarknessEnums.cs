namespace BossMod.RealmReborn.Alliance.A34CloudofDarkness;
public enum OID : uint
{
    Boss = 0xCFA, // R14.000, x?
    CloudOfDarknessHelper1 = 0x1B2, // R0.500, x?
    CloudOfDarknessHelper2 = 0x8EE, // R0.500, x?
    DarkCloud = 0xE1C, // R1.000, x?
    DarkStorm = 0xE1D, // R2.000, x?
    HyperchargedCloud = 0xE03, // R1.000, x?
    Actor1e9750 = 0x1E9750, // R2.000, x?, EventObj type
    Actor1e9752 = 0x1E9752, // R2.000, x?, EventObj type
    Actor1e9751 = 0x1E9751, // R2.000, x?, EventObj type
    Actor1e974e = 0x1E974E, // R0.500, x?, EventObj type
}

public enum AID : uint
{
    Attack = 3306, // Boss->player, no cast, single-target

    FeintParticleBeam1 = 3298, // Boss->location, 3.5s cast, range 8 circle
    FeintParticleBeam2 = 3299, // CloudOfDarknessHelper1->location, no cast, range 3 circle

    FloodOfDarkness = 3305, // Boss->location, no cast, range 60 circle

    ParticleBeam1 = 3301, // CloudOfDarknessHelper1->location, no cast, range 5 circle
    ParticleBeam2 = 3302, // HyperchargedCloud->self, 20.0s cast, range 60 circle
    ParticleBeam3 = 3300, // CloudOfDarknessHelper1->location, no cast, range 60 circle

    Unknown = 2369, // CloudOfDarknessHelper2->self, no cast, ???
    ZeroFormParticleBeam = 3297, // Boss->self, 2.5s cast, range 60+R width 24 rect
}

public enum SID : uint
{
    Bleeding = 642, // Boss->player, extra=0x0
    VulnerabilityUp = 202, // CloudOfDarknessHelper1/Boss->player, extra=0x1/0x2
    StoneskinPhysical = 152, // none->CloudOfDarknessHelper1/Boss, extra=0x0
    StoneskinMagical = 153, // none->CloudOfDarknessHelper1/Boss, extra=0x0
    Heavy = 240, // none->DarkCloud/DarkStorm, extra=0x5A
    Invincibility = 1570, // none->player, extra=0x0
    DamageUp = 443, // none->CloudOfDarknessHelper1/Boss, extra=0x3
}

public enum IconID : uint
{
    FeintParticleBeam = 197, // player chasing AOE icon
}
