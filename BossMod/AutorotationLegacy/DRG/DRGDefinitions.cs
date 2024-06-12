namespace BossMod.DRG;

public enum CDGroup : int
{
    MirageDive = 0, // 1.0 max
    WyrmwindThrust = 1, // 10.0 max
    Nastrond = 2, // 10.0 max
    Jump = 4, // 30.0 max
    ElusiveJump = 5, // 30.0 max
    Stardiver = 6, // 30.0 max
    Geirskogul = 7, // 30.0 max
    HighJump = 8, // 30.0 max
    LanceCharge = 9, // 60.0 max
    LifeSurge = 14, // 2*40.0 max
    SpineshatterDive = 19, // 2*60.0 max
    DragonfireDive = 20, // 120.0 max
    DragonSight = 21, // 120.0 max
    BattleLitany = 23, // 120.0 max
    LegSweep = 41, // 40.0 max
    TrueNorth = 45, // 2*45.0 max
    Bloodbath = 46, // 90.0 max
    Feint = 47, // 90.0 max
    ArmsLength = 48, // 120.0 max
    SecondWind = 49, // 120.0 max
}

public enum SID : uint
{
    None = 0,
    LanceCharge = 1864, // applied by Lance Charge to self, damage buff
    LifeSurge = 116, // applied by Life Surge to self, forced crit for next gcd
    BattleLitany = 786, // applied by Battle Litany to self
    PowerSurge = 2720, // applied by Disembowel & Sonic Thrust to self, damage buff
    ChaosThrust = 118, // applied by Chaos Thrust to target, dot
    ChaoticSpring = 2719, // applied by Chaotic Spring to target, dot
    FangAndClawBared = 802, // applied by Full Thrust to self
    WheelInMotion = 803, // applied by Chaos Thrust to self
    DraconianFire = 1863, // applied by Fang and Claw, Wheeling Thrust to self
    RightEye = 1910, // applied by Dragon Sight to self
    DiveReady = 1243, // applied by Jump to self
    TrueNorth = 1250, // applied by True North to self, ignore positionals
    Bloodbath = 84, // applied by Bloodbath to self, lifesteal
    Feint = 1195, // applied by Feint to target, -10% phys and -5% magic damage dealt
    Stun = 2, // applied by Leg Sweep to target
}
