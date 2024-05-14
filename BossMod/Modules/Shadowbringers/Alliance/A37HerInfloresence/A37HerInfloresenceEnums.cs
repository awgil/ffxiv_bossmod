namespace BossMod.Shadowbringers.Alliance.A37HerInfloresence;

public enum OID : uint
{
    Boss = 0x3190, // R5.999, x1
    Energy = 0x3192, // R1.000, x0 (spawn during fight)
    RedGirl = 0x3191, // R3.450, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x21, 523 type
    Ally2B = 0x31A8, // R0.512, x1
}

public enum AID : uint
{
    BladeFlurry1 = 23788, // Ally2B->Boss, no cast, single-target
    BladeFlurry2 = 23789, // Ally2B->Boss, no cast, single-target
    BossAutoAttack = 24575, // Boss->player, no cast, single-target
    DancingBlade = 23790, // Ally2B->Boss, no cast, width 2 rect charge
    Pervasion = 23520, // Boss->self, 3.0s cast, single-target
    BalancedEdge = 23791, // Ally2B->self, 2.0s cast, range 5 circle
    RecreateStructure = 23521, // Boss->self, 3.0s cast, single-target
    UnknownAbility1 = 18683, // Ally2B->location, no cast, single-target
    UnevenFooting = 23522, // Helper->self, 1.9s cast, range 80 width 30 rect
    RecreateSignal = 23523, // Boss->self, 3.0s cast, single-target
    MixedSignals = 23524, // Boss->self, 3.0s cast, single-target
    Crash = 23525, // Helper->self, 0.8s cast, range 50 width 10 rect
    LighterNote1 = 23564, // Boss->self, 3.0s cast, single-target
    LighterNote2 = 23513, // Helper->location, no cast, range 6 circle
    LighterNote3 = 23514, // Helper->location, no cast, range 6 circle
    ScreamingScore = 23541, // Boss->self, 5.0s cast, range 71 circle
    WhirlingAssault = 23792, // Ally2B->self, 2.0s cast, range 40 width 4 rect
    DarkerNote1 = 23516, // Helper->player, 5.0s cast, range 6 circle
    DarkerNote2 = 23562, // Boss->self, 5.0s cast, single-target
    HeavyArms1 = 23535, // Helper->self, 7.0s cast, range 44 width 100 rect
    HeavyArms2 = 23534, // Boss->self, 7.0s cast, single-target
    HeavyArms3 = 23533, // Boss->self, 7.0s cast, range 100 width 12 rect
    Distortion1 = 23529, // Boss->self, 3.0s cast, range 60 circle
    TheFinalSong = 23530, // Boss->self, 3.0s cast, single-target
    PlaceOfPower = 23565, // Helper->location, 3.0s cast, range 6 circle
    WhiteDissonance = 23531, // Helper->self, no cast, range 60 circle
    BlackDissonance = 23532, // Helper->self, no cast, range 60 circle
    PillarImpact1 = 23536, // Boss->self, 10.0s cast, single-target
    Shockwave1 = 23538, // Helper->self, 6.5s cast, range 71 circle
    Shockwave2 = 23537, // Helper->self, 6.5s cast, range 7 circle
    PillarImpact2 = 23566, // Boss->self, no cast, single-target
    Towerfall1 = 23539, // Boss->self, 3.0s cast, single-target
    Towerfall2 = 23540, // Helper->self, 3.0s cast, range 70 width 14 rect
    UnknownAbility2 = 23526, // RedGirl->self, no cast, single-target
    Distortion2 = 24664, // Boss->self, 3.0s cast, range 60 circle
    ScatteredMagic = 23528, // Energy->player, no cast, single-target
    UnknownAbility3 = 23527, // RedGirl->self, no cast, single-target
    RhythmRings = 23563, // Boss->self, 3.0s cast, single-target
    MagicalInterference = 23509, // Helper->self, no cast, range 50 width 10 rect
}

public enum SID : uint
{
    UnknownStatus = 2056, // none->Boss, extra=0xE1
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    VulnerabilityUp = 1789, // Helper/Boss/Energy->player, extra=0x1/0x2/0x3/0x4/0x5/0x6
    BrinkOfDeath = 44, // none->player, extra=0x0
    Distorted = 2535, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    Icon_1 = 1, // player
    Icon_139 = 139, // player
}

public enum TetherID : uint
{
    Tether_54 = 54, // Helper/Boss->Boss/Helper
}
