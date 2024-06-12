namespace BossMod.BLM;

public enum CDGroup : int
{
    Transpose = 0, // 5.0 max
    BetweenTheLines = 1, // 3.0 max
    AetherialManipulation = 4, // 10.0 max
    Triplecast = 9, // 2*60.0 max
    LeyLines = 14, // 120.0 max
    Sharpcast = 19, // 60.0 max
    Amplifier = 20, // 120.0 max
    Manaward = 21, // 120.0 max
    Manafont = 23, // 180.0 max
    Swiftcast = 44, // 60.0 max
    LucidDreaming = 45, // 60.0 max
    Addle = 46, // 90.0 max
    Surecast = 48, // 120.0 max
}

public enum SID : uint
{
    None = 0,
    Thunder1 = 161, // applied by Thunder1 to target, dot
    Thunder2 = 162, // applied by Thunder2 to target, dot
    Thunder3 = 163, // applied by Thunder3 to target, dot
    Thundercloud = 164, // proc
    Firestarter = 165, // applied by Fire to self, next fire3 is free and instant
    Addle = 1203, // applied by Addle to target, -5% phys and -10% magic damage dealt
    LucidDreaming = 1204, // applied by Lucid Dreaming to self, MP restore
    Manaward = 168, // applied by Manaward to self, shield
    Swiftcast = 167, // applied by Swiftcast to self, next cast is instant
    Surecast = 160, // applied by Surecast to self, knockback immune
    Sleep = 3, // applied by Sleep to target
}
