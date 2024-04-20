namespace BossMod.BLM;

public enum AID : uint
{
    None = 0,
    Sprint = 3,

    // single target GCDs
    Blizzard1 = 142, // L1, 2.5s cast, range 25, single-target 0/0, targets=hostile
    Fire1 = 141, // L2, 2.5s cast, range 25, single-target 0/0, targets=hostile
    Thunder1 = 144, // L6, 2.5s cast, range 25, single-target 0/0, targets=hostile
    Scathe = 156, // L15, instant, range 25, single-target 0/0, targets=hostile
    Blizzard3 = 154, // L35, 3.5s cast, range 25, single-target 0/0, targets=hostile
    Fire3 = 152, // L35, 3.5s cast, range 25, single-target 0/0, targets=hostile
    Thunder3 = 153, // L45, 2.5s cast, range 25, single-target 0/0, targets=hostile
    Blizzard4 = 3576, // L58, 2.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
    Fire4 = 3577, // L60, 2.8s cast, range 25, single-target 0/0, targets=hostile, animLock=???
    Foul = 7422, // L70, 2.5s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
    Despair = 16505, // L72, 3.0s cast, range 25, single-target 0/0, targets=hostile, animLock=???
    Xenoglossy = 16507, // L80, instant, range 25, single-target 0/0, targets=hostile, animLock=???
    Paradox = 25797, // L90, 2.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???

    // aoe GCDs
    Blizzard2 = 25793, // L12, 3.0s cast, range 25, AOE circle 5/0, targets=hostile
    Fire2 = 147, // L18, 3.0s cast, range 25, AOE circle 5/0, targets=hostile
    Thunder2 = 7447, // L26, 2.5s cast, range 25, AOE circle 5/0, targets=hostile
    Freeze = 159, // L40, 2.8s cast, range 25, AOE circle 5/0, targets=hostile
    Flare = 162, // L50, 4.0s cast, range 25, AOE circle 5/0, targets=hostile
    Thunder4 = 7420, // L64, 2.5s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
    HighBlizzard2 = 25795, // L82, 3.0s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
    HighFire2 = 25794, // L82, 3.0s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???

    // oGCDs
    Transpose = 149, // L4, instant, 5.0s CD (group 0), range 0, single-target 0/0, targets=self
    Sharpcast = 3574, // L54, instant, 60.0s CD (group 19), range 0, single-target 0/0, targets=self, animLock=???

    // offsensive CDs
    Swiftcast = 7561, // L18, instant, 60.0s CD (group 42), range 0, single-target 0/0, targets=self
    Manafont = 158, // L30, instant, 180.0s CD (group 23), range 0, single-target 0/0, targets=self
    LeyLines = 3573, // L52, instant, 120.0s CD (group 14), range 0, Ground circle 3/0, targets=area, animLock=???
    Triplecast = 7421, // L66, instant, 60.0s CD (group 9) (2 charges), range 0, single-target 0/0, targets=self, animLock=???
    Amplifier = 25796, // L86, instant, 120.0s CD (group 20), range 0, single-target 0/0, targets=self, animLock=???
    LucidDreaming = 7562, // L14, instant, 60.0s CD (group 41), range 0, single-target 0/0, targets=self

    // defensive CDs
    Addle = 7560, // L8, instant, 90.0s CD (group 40), range 25, single-target 0/0, targets=hostile
    Manaward = 157, // L30, instant, 120.0s CD (group 21), range 0, single-target 0/0, targets=self
    Surecast = 7559, // L44, instant, 120.0s CD (group 43), range 0, single-target 0/0, targets=self

    // misc
    AetherialManipulation = 155, // L50, instant, 10.0s CD (group 4), range 25, single-target 0/0, targets=party, animLock=???
    BetweenTheLines = 7419, // L62, instant, 3.0s CD (group 1), range 25, Ground circle 0/0, targets=area, animLock=???
    UmbralSoul = 16506, // L76, instant, range 0, single-target 0/0, targets=self, animLock=???
    Sleep = 25880, // L10, 2.5s cast, range 30, AOE circle 5/0, targets=hostile
}

public enum TraitID : uint
{
    None = 0,
    AspectMastery1 = 296, // L1, first elemental stack
    MaimAndMend1 = 29, // L20, damage increase
    AspectMastery2 = 458, // L20, second elemental stack
    Thundercloud = 33, // L28, thunder proc
    AspectMastery3 = 459, // L35, third elemental stack
    MaimAndMend2 = 31, // L40, damage increase
    Firestarter = 32, // L42, fire proc
    ThunderMastery1 = 171, // L45, T1->T3 upgrade
    Enochian = 460, // L56, damage increase under elemental stacks
    EnhancedFreeze = 295, // L58, gauge by freeze
    ThunderMastery2 = 172, // L64, T2->T4 upgrade
    EnhancedEnochian1 = 174, // L70, polyglot and effect increase
    EnhancedSharpcast = 321, // L74, reduce cd
    EnhancedEnochian2 = 322, // L78, effect increase
    EnhancedPolyglot = 297, // L80, second stack
    EnhancedFoul = 461, // L80, instant
    AspectMastery4 = 462, // L82, B2/F2->HB2/HF2 upgrade
    EnhancedManafont = 463, // L84, reduce cd
    EnhancedEnochian3 = 509, // L86, effect increase
    EnhancedSharpcast2 = 464, // L88, second charge
    AspectMastery5 = 465, // L90, paradox
}

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

public static class Definitions
{
    public static readonly uint[] UnlockQuests = [65886, 65889, 66609, 66610, 66611, 66612, 66614, 67215, 67216, 67217, 67218, 67219, 68128];

    public static bool Unlocked(AID aid, int level, int questProgress)
    {
        return aid switch
        {
            AID.Fire1 => level >= 2,
            AID.Transpose => level >= 4,
            AID.Thunder1 => level >= 6,
            AID.Addle => level >= 8,
            AID.Sleep => level >= 10,
            AID.Blizzard2 => level >= 12,
            AID.LucidDreaming => level >= 14,
            AID.Scathe => level >= 15 && questProgress > 0,
            AID.Fire2 => level >= 18,
            AID.Swiftcast => level >= 18,
            AID.Thunder2 => level >= 26,
            AID.Manaward => level >= 30 && questProgress > 1,
            AID.Manafont => level >= 30 && questProgress > 2,
            AID.Blizzard3 => level >= 35 && questProgress > 3,
            AID.Fire3 => level >= 35,
            AID.Freeze => level >= 40 && questProgress > 4,
            AID.Surecast => level >= 44,
            AID.Thunder3 => level >= 45 && questProgress > 5,
            AID.Flare => level >= 50 && questProgress > 6,
            AID.AetherialManipulation => level >= 50,
            AID.LeyLines => level >= 52 && questProgress > 7,
            AID.Sharpcast => level >= 54 && questProgress > 8,
            AID.Blizzard4 => level >= 58 && questProgress > 10,
            AID.Fire4 => level >= 60 && questProgress > 11,
            AID.BetweenTheLines => level >= 62,
            AID.Thunder4 => level >= 64,
            AID.Triplecast => level >= 66,
            AID.Foul => level >= 70 && questProgress > 12,
            AID.Despair => level >= 72,
            AID.UmbralSoul => level >= 76,
            AID.Xenoglossy => level >= 80,
            AID.HighFire2 => level >= 82,
            AID.HighBlizzard2 => level >= 82,
            AID.Amplifier => level >= 86,
            AID.Paradox => level >= 90,
            _ => true,
        };
    }

    public static bool Unlocked(TraitID tid, int level, int questProgress)
    {
        return tid switch
        {
            TraitID.MaimAndMend1 => level >= 20,
            TraitID.AspectMastery2 => level >= 20,
            TraitID.Thundercloud => level >= 28,
            TraitID.AspectMastery3 => level >= 35,
            TraitID.MaimAndMend2 => level >= 40,
            TraitID.Firestarter => level >= 42,
            TraitID.ThunderMastery1 => level >= 45 && questProgress > 5,
            TraitID.Enochian => level >= 56 && questProgress > 9,
            TraitID.EnhancedFreeze => level >= 58,
            TraitID.ThunderMastery2 => level >= 64,
            TraitID.EnhancedEnochian1 => level >= 70 && questProgress > 12,
            TraitID.EnhancedSharpcast => level >= 74,
            TraitID.EnhancedEnochian2 => level >= 78,
            TraitID.EnhancedPolyglot => level >= 80,
            TraitID.EnhancedFoul => level >= 80,
            TraitID.AspectMastery4 => level >= 82,
            TraitID.EnhancedManafont => level >= 84,
            TraitID.EnhancedEnochian3 => level >= 86,
            TraitID.EnhancedSharpcast2 => level >= 88,
            TraitID.AspectMastery5 => level >= 90,
            _ => true,
        };
    }

    public static readonly Dictionary<ActionID, ActionDefinition> SupportedActions = BuildSupportedActions();
    private static Dictionary<ActionID, ActionDefinition> BuildSupportedActions()
    {
        var res = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionInt);
        res.GCDCast(AID.Blizzard1, 25, 2.5f);
        res.GCDCast(AID.Fire1, 25, 2.5f);
        res.GCDCast(AID.Thunder1, 25, 2.5f); // note: animation lock is 0.1 if cast or 0.6 if instant (under thundercloud)
        res.GCD(AID.Scathe, 25);
        res.GCDCast(AID.Blizzard3, 25, 3.5f);
        res.GCDCast(AID.Fire3, 25, 3.5f);
        res.GCDCast(AID.Thunder3, 25, 2.5f);
        res.GCDCast(AID.Blizzard4, 25, 2.5f);
        res.GCDCast(AID.Fire4, 25, 2.8f);
        res.GCDCast(AID.Foul, 25, 2.5f);
        res.GCDCast(AID.Despair, 25, 3.0f);
        res.GCD(AID.Xenoglossy, 25);
        res.GCDCast(AID.Paradox, 25, 2.5f);
        res.GCDCast(AID.Blizzard2, 25, 3.0f);
        res.GCDCast(AID.Fire2, 25, 3.0f);
        res.GCDCast(AID.Thunder2, 25, 2.5f);
        res.GCDCast(AID.Freeze, 25, 2.8f);
        res.GCDCast(AID.Flare, 25, 4.0f);
        res.GCDCast(AID.Thunder4, 25, 2.5f);
        res.GCDCast(AID.HighBlizzard2, 25, 3.0f);
        res.GCDCast(AID.HighFire2, 25, 3.0f);
        res.OGCD(AID.Transpose, 0, CDGroup.Transpose, 5.0f);
        res.OGCD(AID.Sharpcast, 0, CDGroup.Sharpcast, 60.0f);
        res.OGCD(AID.Swiftcast, 0, CDGroup.Swiftcast, 60.0f);
        res.OGCD(AID.Manafont, 0, CDGroup.Manafont, 180.0f);
        res.OGCD(AID.LeyLines, 0, CDGroup.LeyLines, 120.0f);
        res.OGCDWithCharges(AID.Triplecast, 0, CDGroup.Triplecast, 60.0f, 2);
        res.OGCD(AID.Amplifier, 0, CDGroup.Amplifier, 120.0f);
        res.OGCD(AID.LucidDreaming, 0, CDGroup.LucidDreaming, 60.0f);
        res.OGCD(AID.Addle, 25, CDGroup.Addle, 90.0f);
        res.OGCD(AID.Manaward, 0, CDGroup.Manaward, 120.0f);
        res.OGCD(AID.Surecast, 0, CDGroup.Surecast, 120.0f);
        res.OGCD(AID.AetherialManipulation, 25, CDGroup.AetherialManipulation, 10.0f);
        res.OGCD(AID.BetweenTheLines, 25, CDGroup.BetweenTheLines, 3.0f);
        res.GCD(AID.UmbralSoul, 0);
        res.GCDCast(AID.Sleep, 30, 2.5f);
        return res;
    }
}
