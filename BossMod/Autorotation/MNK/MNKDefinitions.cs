namespace BossMod.MNK;

public enum CDGroup : int
{
    SteelPeak = 0, // 1.0 max, shared by Steel Peak, Howling Fist, the Forbidden Chakra, Enlightenment
    Thunderclap = 9, // 2*30.0 max
    PerfectBalance = 10, // 2*40.0 max
    RiddleOfFire = 11, // 60.0 max
    Anatman = 12, // 60.0 max
    RiddleOfEarth = 14, // 120.0 max
    Mantra = 15, // 90.0 max
    RiddleOfWind = 16, // 90.0 max
    Brotherhood = 19, // 120.0 max
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
    OpoOpoForm = 107, // applied by Snap Punch to self
    RaptorForm = 108, // applied by Bootshine, Arm of the Destroyer to self
    CoeurlForm = 109, // applied by True Strike, Twin Snakes to self
    RiddleOfFire = 1181, // applied by Riddle of Fire to self
    RiddleOfWind = 2687, // applied by Riddle of Wind to self
    LeadenFist = 1861, // applied by Dragon Kick to self
    DisciplinedFist = 3001, // applied by Twin Snakes to self, damage buff
    PerfectBalance = 110, // applied by Perfect Balance to self, ignore form requirements
    Demolish = 246, // applied by Demolish to target, dot
    Bloodbath = 84, // applied by Bloodbath to self, lifesteal
    Feint = 1195, // applied by Feint to target, -10% phys and -5% magic damage dealt
    Mantra = 102, // applied by Mantra to targets, +10% healing taken
    TrueNorth = 1250, // applied by True North to self, ignore positionals
    Stun = 2, // applied by Leg Sweep to target
    FormlessFist = 2513, // applied by Form Shift to self
    SixSidedStar = 2514, // applied by Six-Sided Star to self

    LostFontofPower = 2346,
    BannerHonoredSacrifice = 2327,
    LostExcellence = 2564,
    Memorable = 2565,
}
