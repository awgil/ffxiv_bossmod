namespace BossMod.GNB;

public enum CDGroup : int
{
    EyeGouge = 0, // 1.0 max, shared by Eye Gouge, Jugular Rip, Continuation, Abdomen Tear, Hypervelocity
    JugularRip = 0, // 1.0 max, shared by Eye Gouge, Jugular Rip, Continuation, Abdomen Tear, Hypervelocity
    Continuation = 0, // 1.0 max, shared by Eye Gouge, Jugular Rip, Continuation, Abdomen Tear, Hypervelocity
    AbdomenTear = 0, // 1.0 max, shared by Eye Gouge, Jugular Rip, Continuation, Abdomen Tear, Hypervelocity
    Hypervelocity = 0, // 1.0 max, shared by Eye Gouge, Jugular Rip, Continuation, Abdomen Tear, Hypervelocity
    RoyalGuard = 1, // variable max, shared by Royal Guard, Release Royal Guard
    ReleaseRoyalGuard = 1, // variable max, shared by Royal Guard, Release Royal Guard
    HeartOfStone = 3, // 25.0 max, shared by Heart of Stone, Heart of Corundum
    HeartOfCorundum = 3, // 25.0 max, shared by Heart of Stone, Heart of Corundum
    DangerZone = 4, // 30.0 max, shared by Danger Zone, Blasting Zone
    BlastingZone = 4, // 30.0 max, shared by Danger Zone, Blasting Zone
    GnashingFang = 5, // 30.0 max
    RoughDivide = 9, // 2*30.0 max
    NoMercy = 10, // 60.0 max
    BowShock = 11, // 60.0 max
    DoubleDown = 12, // 60.0 max
    SonicBreak = 13, // 60.0 max
    Bloodfest = 14, // 120.0 max
    Camouflage = 15, // 90.0 max
    HeartOfLight = 16, // 90.0 max
    Aurora = 19, // 2*60.0 max
    Nebula = 21, // 120.0 max
    Superbolide = 24, // 360.0 max
    LowBlow = 41, // 25.0 max
    Provoke = 42, // 30.0 max
    Interject = 43, // 30.0 max
    Reprisal = 44, // 60.0 max
    Rampart = 46, // 90.0 max
    ArmsLength = 48, // 120.0 max
    Shirk = 49, // 120.0 max
    LimitBreak = 71, // special/fake (TODO: remove need for it?)
}

public enum SID : uint
{
    None = 0,
    BrutalShell = 1898, // applied by Brutal Shell to self
    NoMercy = 1831, // applied by No Mercy to self
    ReadyToRip = 1842, // applied by Gnashing Fang to self
    SonicBreak = 1837, // applied by Sonic Break to target
    BowShock = 1838, // applied by Bow Shock to target
    ReadyToTear = 1843, // applied by Savage Claw to self
    ReadyToGouge = 1844, // applied by Wicked Talon to self
    ReadyToBlast = 2686, // applied by Burst Strike to self
    Nebula = 1834, // applied by Nebula to self
    Rampart = 1191, // applied by Rampart to self
    Reprisal = 1193, // applied by Reprisal to target
    Camouflage = 1832, // applied by Camouflage to self
    ArmsLength = 1209, // applied by Arm's Length to self
    HeartOfLight = 1839, // applied by Heart of Light to self
    Aurora = 1835, // applied by Aurora to self
    Superbolide = 1836, // applied by Superbolide to self
    HeartOfCorundum = 2683, // applied by Heart of Corundum to self
    ClarityOfCorundum = 2684, // applied by Heart of Corundum to self
    CatharsisOfCorundum = 2685, // applied by Heart of Corundum to self
    RoyalGuard = 1833, // applied by Royal Guard to self
    Stun = 2, // applied by Low Blow to target
}
