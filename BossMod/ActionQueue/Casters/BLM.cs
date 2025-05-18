namespace BossMod.BLM;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    Meteor = 205, // LB3, 4.5s cast, range 25, AOE 15 circle, targets=Area, animLock=8.100s?
    Blizzard1 = 142, // L1, 2.5s cast, GCD, range 25, single-target, targets=Hostile
    Fire1 = 141, // L2, 2.5s cast, GCD, range 25, single-target, targets=Hostile
    Transpose = 149, // L4, instant, 5.0s CD (group 1), range 0, single-target, targets=Self
    Thunder1 = 144, // L6, instant, GCD, range 25, single-target, targets=Hostile
    Blizzard2 = 25793, // L12, 3.0s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    Scathe = 156, // L15, instant, GCD, range 25, single-target, targets=Hostile
    Fire2 = 147, // L18, 3.0s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    Thunder2 = 7447, // L26, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    Manaward = 157, // L30, instant, 120.0s CD (group 21), range 0, single-target, targets=Self
    Manafont = 158, // L30, instant, 120.0s CD (group 23), range 0, single-target, targets=Self
    UmbralSoul = 16506, // L35, instant, GCD, range 0, single-target, targets=Self
    Fire3 = 152, // L35, 3.5s cast, GCD, range 25, single-target, targets=Hostile
    Blizzard3 = 154, // L35, 3.5s cast, GCD, range 25, single-target, targets=Hostile
    Freeze = 159, // L40, 2.8s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    Thunder3 = 153, // L45, instant, GCD, range 25, single-target, targets=Hostile
    Flare = 162, // L50, 4.0s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    AetherialManipulation = 155, // L50, instant, 10.0s CD (group 2), range 25, single-target, targets=Party, animLock=0.800s?
    LeyLines = 3573, // L52, instant, 120.0s CD (group 19), range 0, ???, targets=Area
    Blizzard4 = 3576, // L58, 2.5s cast, GCD, range 25, single-target, targets=Hostile
    Fire4 = 3577, // L60, 2.8s cast, GCD, range 25, single-target, targets=Hostile
    BetweenTheLines = 7419, // L62, instant, 3.0s CD (group 0), range 25, ???, targets=Area, animLock=0.800
    Thunder4 = 7420, // L64, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    Triplecast = 7421, // L66, instant, 60.0s CD (group 18/71) (2 charges), range 0, single-target, targets=Self
    Foul = 7422, // L70, 2.5s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    Despair = 16505, // L72, 3.0s cast, GCD, range 25, single-target, targets=Hostile
    Xenoglossy = 16507, // L80, instant, GCD, range 25, single-target, targets=Hostile
    HighBlizzard2 = 25795, // L82, 3.0s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    HighFire2 = 25794, // L82, 3.0s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    Amplifier = 25796, // L86, instant, 120.0s CD (group 20), range 0, single-target, targets=Self
    Paradox = 25797, // L90, instant, GCD, range 25, single-target, targets=Hostile
    HighThunder = 36986, // L92, instant, GCD, range 25, single-target, targets=Hostile, animLock=???
    HighThunder2 = 36987, // L92, instant, GCD, range 25, AOE 5 circle, targets=Hostile, animLock=???
    Retrace = 36988, // L96, instant, 40.0s CD (group 8), range 0, ???, targets=Area, animLock=???
    FlareStar = 36989, // L100, 3.0s cast, GCD, range 25, AOE 5 circle, targets=Hostile, animLock=???

    // Shared
    Skyshard = ClassShared.AID.Skyshard, // LB1, 2.0s cast, range 25, AOE 8 circle, targets=Area, animLock=3.100s?
    Starstorm = ClassShared.AID.Starstorm, // LB2, 3.0s cast, range 25, AOE 10 circle, targets=Area, animLock=5.100s?
    Addle = ClassShared.AID.Addle, // L8, instant, 90.0s CD (group 46), range 25, single-target, targets=Hostile
    Sleep = ClassShared.AID.Sleep, // L10, 2.5s cast, GCD, range 30, AOE 5 circle, targets=Hostile
    LucidDreaming = ClassShared.AID.LucidDreaming, // L14, instant, 60.0s CD (group 44), range 0, single-target, targets=Self
    Swiftcast = ClassShared.AID.Swiftcast, // L18, instant, 60.0s CD (group 43), range 0, single-target, targets=Self
    Surecast = ClassShared.AID.Surecast, // L44, instant, 120.0s CD (group 48), range 0, single-target, targets=Self
}

public enum TraitID : uint
{
    None = 0,
    AspectMastery1 = 296, // L1
    MaimAndMend1 = 29, // L20
    AspectMastery2 = 458, // L20
    AspectMastery3 = 459, // L35
    MaimAndMend2 = 31, // L40
    Firestarter = 32, // L42
    ThunderMastery1 = 171, // L45
    Enochian = 460, // L56
    UmbralHeart = 295, // L58
    ThunderMastery2 = 172, // L64
    EnhancedEnochian1 = 174, // L70
    EnhancedEnochian2 = 322, // L78
    EnhancedPolyglot = 297, // L80
    EnhancedFoul = 461, // L80
    AspectMastery4 = 462, // L82
    EnhancedManafont = 463, // L84
    EnhancedEnochian3 = 509, // L86
    AspectMastery5 = 465, // L90
    ThunderMasteryIII = 613, // L92
    EnhancedSwiftcast = 644, // L94
    EnhancedLeyLines = 614, // L96
    EnhancedEnochianIV = 659, // L96
    EnhancedPolyglotII = 615, // L98
    EnhancedAddle = 643, // L98
    EnhancedAstralFire = 616, // L100
}

public enum SID : uint
{
    None = 0,
    Manaward = 168, // applied by Manaward to self
    Thunder = 161, // applied by Thunder to target
    ThunderII = 162, // applied by Thunder II to target
    ThunderIII = 163, // applied by Thunder III to target
    ThunderIV = 1210, // applied by Thunder IV to target
    LeyLines = 737, // applied by Ley Lines to self
    CircleOfPower = 738, // applied by Ley Lines to self
    Triplecast = 1211, // applied by Triplecast to self
    Firestarter = 165, // applied by Paradox to self
    Thunderhead = 3870,
    HighThunder = 3871, // applied by High Thunder to target
    HighThunderII = 3872, // applied by High Thunder II to target

    //Shared
    Addle = ClassShared.SID.Addle, // applied by Addle to target
    Surecast = ClassShared.SID.Surecast, // applied by Surecast to self
    LucidDreaming = ClassShared.SID.LucidDreaming, // applied by Lucid Dreaming to self
    Swiftcast = ClassShared.SID.Swiftcast, // applied by Swiftcast to self
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.Meteor, castAnimLock: 8.10f); // animLock=8.100s?
        d.RegisterSpell(AID.Blizzard1);
        d.RegisterSpell(AID.Fire1);
        d.RegisterSpell(AID.Transpose);
        d.RegisterSpell(AID.Thunder1);
        d.RegisterSpell(AID.Blizzard2);
        d.RegisterSpell(AID.Scathe);
        d.RegisterSpell(AID.Fire2);
        d.RegisterSpell(AID.Thunder2);
        d.RegisterSpell(AID.Manaward);
        d.RegisterSpell(AID.Manafont);
        d.RegisterSpell(AID.UmbralSoul);
        d.RegisterSpell(AID.Fire3);
        d.RegisterSpell(AID.Blizzard3);
        d.RegisterSpell(AID.Freeze);
        d.RegisterSpell(AID.Thunder3);
        d.RegisterSpell(AID.Flare);
        d.RegisterSpell(AID.AetherialManipulation, instantAnimLock: 0.80f); // animLock=0.800s?
        d.RegisterSpell(AID.LeyLines);
        d.RegisterSpell(AID.Blizzard4);
        d.RegisterSpell(AID.Fire4);
        d.RegisterSpell(AID.BetweenTheLines, instantAnimLock: 0.80f); // animLock=0.800
        d.RegisterSpell(AID.Thunder4);
        d.RegisterSpell(AID.Triplecast);
        d.RegisterSpell(AID.Foul);
        d.RegisterSpell(AID.Despair);
        d.RegisterSpell(AID.Xenoglossy);
        d.RegisterSpell(AID.HighBlizzard2);
        d.RegisterSpell(AID.HighFire2);
        d.RegisterSpell(AID.Amplifier);
        d.RegisterSpell(AID.Paradox);
        d.RegisterSpell(AID.HighThunder); // animLock=???
        d.RegisterSpell(AID.HighThunder2); // animLock=???
        d.RegisterSpell(AID.Retrace); // animLock=???
        d.RegisterSpell(AID.FlareStar); // animLock=???

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // *** add any properties that can't be autogenerated here ***
        d.Spell(AID.AetherialManipulation)!.ForbidExecute = ActionDefinitions.DashToTargetCheck;
        d.Spell(AID.BetweenTheLines)!.ForbidExecute = ActionDefinitions.DashToPositionCheck;

        d.Spell(AID.Triplecast)!.ForbidExecute = (_, player, _, _) => player.FindStatus(SID.Triplecast) != null;
    }
}
