namespace BossMod.WHM;

public enum CDGroup : int
{
    LiturgyOfTheBellEnd = 0, // 1.0 max
    Assize = 5, // 40.0 max
    DivineBenison = 9, // 2*30.0 max
    PlenaryIndulgence = 10, // 60.0 max
    Aquaveil = 11, // 60.0 max
    Tetragrammaton = 13, // 60.0 max
    Asylum = 17, // 90.0 max
    ThinAir = 20, // 2*60.0 max
    Temperance = 21, // 120.0 max
    PresenceOfMind = 22, // 120.0 max
    Benediction = 23, // 180.0 max
    LiturgyOfTheBell = 24, // 180.0 max
    Swiftcast = 44, // 60.0 max
    LucidDreaming = 45, // 60.0 max
    Surecast = 48, // 120.0 max
    Rescue = 49, // 120.0 max
}

public enum SID : uint
{
    None = 0,
    Aero1 = 143, // applied by Aero1 to target, dot
    Aero2 = 144, // applied by Aero2 to target, dot
    Dia = 1871, // applied by Dia to target, dot
    Medica2 = 150, // applied by Medica2 to targets, hot
    Freecure = 155, // applied by Cure1 to self, next cure2 is free
    Swiftcast = 167, // applied by Swiftcast to self, next gcd is instant
    ThinAir = 1217, // applied by Thin Air to self, next gcd costs no mp
    LucidDreaming = 1204, // applied by Lucid Dreaming to self, mp regen
    DivineBenison = 1218, // applied by Divine Benison to target, shield
    Confession = 1219, // applied by Plenary Indulgence to self, heal buff
    Temperance = 1872, // applied by Temperance to self, heal and mitigate buff
    Surecast = 160, // applied by Surecast to self, knockback immune
    PresenceOfMind = 157, // applied by Presence of Mind to self, damage buff
    Regen = 158, // applied by Regen to target, hp regen
    Asylum = 1911, // applied by Asylum to target, hp regen
    Aquaveil = 2708, // applied by Aquaveil to target, mitigate
    LiturgyOfTheBell = 2709, // applied by Liturgy of the Bell to target, heal on hit
    Sleep = 3, // applied by Repose to target
}
