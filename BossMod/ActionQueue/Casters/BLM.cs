namespace BossMod.BLM;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    Meteor = 205, // LB3, 4.5s cast, range 25, AOE 15 circle, targets=area, animLock=???, castAnimLock=8.100
    Blizzard1 = 142, // L1, 2.5s cast, GCD, range 25, single-target, targets=hostile
    Fire1 = 141, // L2, 2.5s cast, GCD, range 25, single-target, targets=hostile
    Transpose = 149, // L4, instant, 5.0s CD (group 0), range 0, single-target, targets=self
    Thunder1 = 144, // L6, 2.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Blizzard2 = 25793, // L12, 3.0s cast, GCD, range 25, AOE 5 circle, targets=hostile, animLock=???
    Scathe = 156, // L15, instant, GCD, range 25, single-target, targets=hostile
    Fire2 = 147, // L18, 3.0s cast, GCD, range 25, AOE 5 circle, targets=hostile, animLock=???
    Thunder2 = 7447, // L26, 2.5s cast, GCD, range 25, AOE 5 circle, targets=hostile, animLock=???
    Manaward = 157, // L30, instant, 120.0s CD (group 21), range 0, single-target, targets=self
    Manafont = 158, // L30, instant, 180.0s CD (group 23), range 0, single-target, targets=self
    Fire3 = 152, // L35, 3.5s cast, GCD, range 25, single-target, targets=hostile
    Blizzard3 = 154, // L35, 3.5s cast, GCD, range 25, single-target, targets=hostile
    Freeze = 159, // L40, 2.8s cast, GCD, range 25, AOE 5 circle, targets=hostile
    Thunder3 = 153, // L45, 2.5s cast, GCD, range 25, single-target, targets=hostile
    Flare = 162, // L50, 4.0s cast, GCD, range 25, AOE 5 circle, targets=hostile
    AetherialManipulation = 155, // L50, instant, 10.0s CD (group 4), range 25, single-target, targets=party, animLock=0.800
    LeyLines = 3573, // L52, instant, 120.0s CD (group 14), range 0, ???, targets=area
    Sharpcast = 3574, // L54, instant, 60.0s CD (group 19/70), range 0, single-target, targets=self
    Blizzard4 = 3576, // L58, 2.5s cast, GCD, range 25, single-target, targets=hostile
    Fire4 = 3577, // L60, 2.8s cast, GCD, range 25, single-target, targets=hostile
    BetweenTheLines = 7419, // L62, instant, 3.0s CD (group 1), range 25, ???, targets=area, animLock=0.800
    Thunder4 = 7420, // L64, 2.5s cast, GCD, range 25, AOE 5 circle, targets=hostile
    Triplecast = 7421, // L66, instant, 60.0s CD (group 9/71) (2? charges), range 0, single-target, targets=self
    Foul = 7422, // L70, 2.5s cast, GCD, range 25, AOE 5 circle, targets=hostile
    Despair = 16505, // L72, 3.0s cast, GCD, range 25, single-target, targets=hostile
    UmbralSoul = 16506, // L76, instant, GCD, range 0, single-target, targets=self
    Xenoglossy = 16507, // L80, instant, GCD, range 25, single-target, targets=hostile
    HighFire2 = 25794, // L82, 3.0s cast, GCD, range 25, AOE 5 circle, targets=hostile
    HighBlizzard2 = 25795, // L82, 3.0s cast, GCD, range 25, AOE 5 circle, targets=hostile
    Amplifier = 25796, // L86, instant, 120.0s CD (group 20), range 0, single-target, targets=self
    Paradox = 25797, // L90, 2.5s cast, GCD, range 25, single-target, targets=hostile

    // Shared
    Skyshard = ClassShared.AID.Skyshard, // LB1, 2.0s cast, range 25, AOE 8 circle, targets=area, castAnimLock=3.100
    Starstorm = ClassShared.AID.Starstorm, // LB2, 3.0s cast, range 25, AOE 10 circle, targets=area, castAnimLock=5.100
    Addle = ClassShared.AID.Addle, // L8, instant, 90.0s CD (group 46), range 25, single-target, targets=hostile
    Sleep = ClassShared.AID.Sleep, // L10, 2.5s cast, GCD, range 30, AOE 5 circle, targets=hostile
    LucidDreaming = ClassShared.AID.LucidDreaming, // L14, instant, 60.0s CD (group 45), range 0, single-target, targets=self
    Swiftcast = ClassShared.AID.Swiftcast, // L18, instant, 60.0s CD (group 44), range 0, single-target, targets=self
    Surecast = ClassShared.AID.Surecast, // L44, instant, 120.0s CD (group 48), range 0, single-target, targets=self
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

public sealed class Definitions : IDisposable
{
    public static readonly uint[] UnlockQuests = [65886, 65889, 66609, 66610, 66611, 66612, 66614, 67215, 67216, 67217, 67218, 67219, 68128];

    public static bool Unlocked(AID id, int level, int questProgress) => id switch
    {
        AID.Fire1 => level >= 2,
        AID.Transpose => level >= 4,
        AID.Thunder1 => level >= 6,
        AID.Addle => level >= 8,
        AID.Sleep => level >= 10,
        AID.Blizzard2 => level >= 12,
        AID.LucidDreaming => level >= 14,
        AID.Scathe => level >= 15 && questProgress > 0,
        AID.Swiftcast => level >= 18,
        AID.Fire2 => level >= 18,
        AID.Thunder2 => level >= 26,
        AID.Manaward => level >= 30 && questProgress > 1,
        AID.Manafont => level >= 30 && questProgress > 2,
        AID.Fire3 => level >= 35,
        AID.Blizzard3 => level >= 35 && questProgress > 3,
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
        _ => true
    };

    public static bool Unlocked(TraitID id, int level, int questProgress) => id switch
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
        _ => true
    };

    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.Meteor, castAnimLock: 8.10f); // animLock=???, castAnimLock=8.100
        d.RegisterSpell(AID.Blizzard1);
        d.RegisterSpell(AID.Fire1);
        d.RegisterSpell(AID.Transpose);
        d.RegisterSpell(AID.Thunder1); // animLock=???
        d.RegisterSpell(AID.Blizzard2); // animLock=???
        d.RegisterSpell(AID.Scathe);
        d.RegisterSpell(AID.Fire2); // animLock=???
        d.RegisterSpell(AID.Thunder2); // animLock=???
        d.RegisterSpell(AID.Manaward);
        d.RegisterSpell(AID.Manafont);
        d.RegisterSpell(AID.Fire3);
        d.RegisterSpell(AID.Blizzard3);
        d.RegisterSpell(AID.Freeze);
        d.RegisterSpell(AID.Thunder3);
        d.RegisterSpell(AID.Flare);
        d.RegisterSpell(AID.AetherialManipulation, instantAnimLock: 0.80f); // animLock=0.800
        d.RegisterSpell(AID.LeyLines);
        d.RegisterSpell(AID.Sharpcast);
        d.RegisterSpell(AID.Blizzard4);
        d.RegisterSpell(AID.Fire4);
        d.RegisterSpell(AID.BetweenTheLines, instantAnimLock: 0.80f); // animLock=0.800
        d.RegisterSpell(AID.Thunder4);
        d.RegisterSpell(AID.Triplecast, maxCharges: 2);
        d.RegisterSpell(AID.Foul);
        d.RegisterSpell(AID.Despair);
        d.RegisterSpell(AID.UmbralSoul);
        d.RegisterSpell(AID.Xenoglossy);
        d.RegisterSpell(AID.HighFire2);
        d.RegisterSpell(AID.HighBlizzard2);
        d.RegisterSpell(AID.Amplifier);
        d.RegisterSpell(AID.Paradox);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // *** add any properties that can't be autogenerated here ***
    }
}
