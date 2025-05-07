namespace BossMod.PCT;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    ChromaticFantasy = 34867, // LB3, 4.5s cast, range 25, AOE 15 circle, targets=Area
    FireInRed = 34650, // L1, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    AeroInGreen = 34651, // L5, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    TemperaCoat = 34685, // L10, instant, 120.0s CD (group 21), range 0, single-target, targets=Self
    WaterInBlue = 34652, // L15, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    Smudge = 34684, // L20, instant, 20.0s CD (group 5), range 0, single-target, targets=Self, animLock=0.800
    FireIIInRed = 34656, // L25, 1.5s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    WingedMuse = 34671, // L30, instant, 40.0s CD (group 18/70) (2-3 charges), range 25, AOE 5 circle, targets=Hostile
    LivingMuse = 35347, // L30, instant, 40.0s CD (group 18/70) (2-3 charges), range 0, single-target, targets=Self
    CreatureMotif = 34689, // L30, 3.0s cast, GCD, range 0, single-target, targets=Self
    WingMotif = 34665, // L30, 3.0s cast, GCD, range 0, single-target, targets=Self
    MogOfTheAges = 34676, // L30, instant, 30.0s CD (group 6), range 25, AOE 25+R width 4 rect, targets=Hostile
    PomMotif = 34664, // L30, 3.0s cast, GCD, range 0, single-target, targets=Self
    PomMuse = 34670, // L30, instant, 40.0s CD (group 18/70) (2-3 charges), range 25, AOE 5 circle, targets=Hostile
    AeroIIInGreen = 34657, // L35, 1.5s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    WaterIIInBlue = 34658, // L45, 1.5s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    WeaponMotif = 34690, // L50, 3.0s cast, GCD, range 0, single-target, targets=Self
    HammerStamp = 34678, // L50, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    SteelMuse = 35348, // L50, instant, 60.0s CD (group 19/71), range 0, single-target, targets=Self
    StrikingMuse = 34674, // L50, instant, 60.0s CD (group 19/71), range 0, single-target, targets=Self
    HammerMotif = 34668, // L50, 3.0s cast, GCD, range 0, single-target, targets=Self
    ThunderIIInMagenta = 34661, // L60, 2.3s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    StoneInYellow = 34654, // L60, 2.3s cast, GCD, range 25, single-target, targets=Hostile
    BlizzardIIInCyan = 34659, // L60, 2.3s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    ThunderInMagenta = 34655, // L60, 2.3s cast, GCD, range 25, single-target, targets=Hostile
    BlizzardInCyan = 34653, // L60, 2.3s cast, GCD, range 25, single-target, targets=Hostile
    StoneIIInYellow = 34660, // L60, 2.3s cast, GCD, range 25, AOE 5 circle, targets=Hostile
    SubtractivePalette = 34683, // L60, instant, 1.0s CD (group 0), range 0, single-target, targets=Self
    StarrySkyMotif = 34669, // L70, 3.0s cast, GCD, range 0, single-target, targets=Self
    StarryMuse = 34675, // L70, instant, 120.0s CD (group 20), range 0, AOE 30 circle, targets=Area
    LandscapeMotif = 34691, // L70, 3.0s cast, GCD, range 0, single-target, targets=Self
    ScenicMuse = 35349, // L70, instant, 120.0s CD (group 20), range 0, single-target, targets=Self
    HolyInWhite = 34662, // L80, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    HammerBrush = 34679, // L86, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    PolishingHammer = 34680, // L86, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    TemperaGrassa = 34686, // L88, instant, 1.0s CD (group 1), range 0, AOE 30 circle, targets=Self
    CometInBlack = 34663, // L90, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    RainbowDrip = 34688, // L92, 4.0s cast, GCD, range 25, AOE 25+R width 4 rect, targets=Hostile
    RetributionOfTheMadeen = 34677, // L96, instant, 30.0s CD (group 6), range 25, AOE 25+R width 4 rect, targets=Hostile
    FangedMuse = 34673, // L96, instant, 40.0s CD (group 18/70) (2-3 charges), range 25, AOE 5 circle, targets=Hostile
    ClawedMuse = 34672, // L96, instant, 40.0s CD (group 18/70) (2-3 charges), range 25, AOE 5 circle, targets=Hostile
    MawMotif = 34667, // L96, 3.0s cast, GCD, range 0, single-target, targets=Self
    ClawMotif = 34666, // L96, 3.0s cast, GCD, range 0, single-target, targets=Self
    StarPrism = 34681, // L100, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    StarPrism2 = 34682, // L100, instant, range 100, AOE 30 circle, targets=Hostile

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
    MaimAndMend = 670, // L20
    MaimAndMendII = 671, // L40
    PictomancyMastery = 672, // L54
    PaletteGauge = 537, // L60
    PictomancyMasteryII = 676, // L74
    EnhancedArtistry = 538, // L80
    EnhancedPictomancy = 546, // L82
    EnhancedSmudge = 539, // L84
    PictomancyMasteryIII = 677, // L84
    EnhancedPictomancyII = 540, // L86
    EnhancedTempera = 541, // L88
    EnhancedPalette = 542, // L90
    EnhancedPictomancyIII = 543, // L92
    PictomancyMasteryIV = 678, // L94
    EnhancedSwiftcast = 644, // L94
    EnhancedPictomancyIV = 545, // L96
    EnhancedAddle = 643, // L98
    EnhancedPictomancyV = 544, // L100
}

public enum SID : uint
{
    None = 0,
    Aetherhues = 3675, // applied by Fire in Red, Fire II in Red, Blizzard II in Cyan, Blizzard in Cyan to self
    AetherhuesII = 3676, // applied by Aero in Green, Aero II in Green, Stone in Yellow to self
    TemperaCoat = 3686, // applied by Tempera Coat to self
    TemperaGrassa = 3687, // applied by Tempera Grassa to self
    Smudge = 3684, // applied by Smudge to self
    HammerTime = 3680, // applied by Striking Muse to self
    SubtractivePalette = 3674, // applied by Subtractive Palette to self
    MonochromeTones = 3691, // applied by Subtractive Palette to self
    StarryMuse = 3685, // applied by Starry Muse to self/target
    SubtractiveSpectrum = 3690, // applied by Starry Muse to self
    Hyperphantasia = 3688, // applied by Starry Muse to self
    Starstruck = 3681, // applied by Starry Muse to self
    RainbowBright = 3679,

    //Shared
    Addle = ClassShared.SID.Addle, // applied by Addle to target
    Surecast = ClassShared.SID.Surecast, // applied by Surecast to self
    LucidDreaming = ClassShared.SID.LucidDreaming, // applied by Lucid Dreaming to self
    Swiftcast = ClassShared.SID.Swiftcast, // applied by Swiftcast to self
}

public sealed class Definitions : IDisposable
{
    private readonly PCTConfig _config = Service.Config.Get<PCTConfig>();
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.ChromaticFantasy);
        d.RegisterSpell(AID.FireInRed);
        d.RegisterSpell(AID.AeroInGreen);
        d.RegisterSpell(AID.TemperaCoat);
        d.RegisterSpell(AID.WaterInBlue);
        d.RegisterSpell(AID.Smudge, instantAnimLock: 0.80f); // animLock=0.800
        d.RegisterSpell(AID.FireIIInRed);
        d.RegisterSpell(AID.WingedMuse);
        d.RegisterSpell(AID.LivingMuse);
        d.RegisterSpell(AID.CreatureMotif);
        d.RegisterSpell(AID.WingMotif);
        d.RegisterSpell(AID.MogOfTheAges);
        d.RegisterSpell(AID.PomMotif);
        d.RegisterSpell(AID.PomMuse);
        d.RegisterSpell(AID.AeroIIInGreen);
        d.RegisterSpell(AID.WaterIIInBlue);
        d.RegisterSpell(AID.WeaponMotif);
        d.RegisterSpell(AID.HammerStamp);
        d.RegisterSpell(AID.SteelMuse);
        d.RegisterSpell(AID.StrikingMuse);
        d.RegisterSpell(AID.HammerMotif);
        d.RegisterSpell(AID.ThunderIIInMagenta);
        d.RegisterSpell(AID.StoneInYellow);
        d.RegisterSpell(AID.BlizzardIIInCyan);
        d.RegisterSpell(AID.ThunderInMagenta);
        d.RegisterSpell(AID.BlizzardInCyan);
        d.RegisterSpell(AID.StoneIIInYellow);
        d.RegisterSpell(AID.SubtractivePalette);
        d.RegisterSpell(AID.StarrySkyMotif);
        d.RegisterSpell(AID.StarryMuse);
        d.RegisterSpell(AID.LandscapeMotif);
        d.RegisterSpell(AID.ScenicMuse);
        d.RegisterSpell(AID.HolyInWhite);
        d.RegisterSpell(AID.HammerBrush);
        d.RegisterSpell(AID.PolishingHammer);
        d.RegisterSpell(AID.TemperaGrassa);
        d.RegisterSpell(AID.CometInBlack);
        d.RegisterSpell(AID.RainbowDrip);
        d.RegisterSpell(AID.RetributionOfTheMadeen);
        d.RegisterSpell(AID.FangedMuse);
        d.RegisterSpell(AID.ClawedMuse);
        d.RegisterSpell(AID.MawMotif);
        d.RegisterSpell(AID.ClawMotif);
        d.RegisterSpell(AID.StarPrism);
        d.RegisterSpell(AID.StarPrism2); // AOE heal effect

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.RegisterChargeIncreaseTrait(AID.StrikingMuse, TraitID.EnhancedPictomancyII);
        foreach (var creature in new AID[] { AID.LivingMuse, AID.PomMuse, AID.WingedMuse, AID.ClawedMuse, AID.FangedMuse })
            d.RegisterChargeIncreaseTrait(creature, TraitID.EnhancedPictomancyIV);

        d.Spell(AID.Smudge)!.TransformAngle = (ws, _, _, _) => _config.AlignDashToCamera
            ? ws.Client.CameraAzimuth + 180.Degrees()
            : null;
        d.Spell(AID.Smudge)!.ForbidExecute = ActionDefinitions.DashFixedDistanceCheck(15);
    }
}

