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

// TODO: regenerate
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

public sealed class Definitions : IDisposable
{
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
