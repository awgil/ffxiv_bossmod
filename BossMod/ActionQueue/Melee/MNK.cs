namespace BossMod.MNK;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    FinalHeaven = 202, // LB3, 4.5s cast, range 8, single-target, targets=hostile, animLock=???, castAnimLock=3.700
    Bootshine = 53, // L1, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    TrueStrike = 54, // L4, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    SnapPunch = 56, // L6, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    SteeledMeditation = 36940, // L15, instant, range 0, single-target 0/0, targets=self, animLock=???
    TwinSnakes = 61, // L18, instant, range 3, single-target 0/0, targets=hostile, animLock=0.600s
    ArmOfTheDestroyer = 62, // L26, instant, range 0, AOE circle 5/0, targets=self, animLock=???
    Demolish = 66, // L30, instant, range 3, single-target 0/0, targets=hostile, animLock=0.600s
    Rockbreaker = 70, // L30, instant, range 0, AOE circle 5/0, targets=self, animLock=???
    InspiritedMeditation = 36941, // L40, instant, range 0, single-target 0/0, targets=self, animLock=???
    FourPointFury = 16473, // L45, instant, range 0, AOE circle 5/0, targets=self, animLock=???
    DragonKick = 74, // L50, instant, range 3, single-target 0/0, targets=hostile, animLock=0.600s
    FormShift = 4262, // L52, instant, range 0, single-target 0/0, targets=self, animLock=???
    ForbiddenMeditation = 36942, // L54, instant, range 0, single-target 0/0, targets=self, animLock=???
    TornadoKick = 3543, // L60, instant, range 3, AOE circle 5/0, targets=hostile, animLock=???
    FlintStrike = 25882, // L60, instant, range 0, AOE circle 5/0, targets=self, animLock=???
    CelestialRevolution = 25765, // L60, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    MasterfulBlitz = 25764, // L60, instant, range 0, single-target 0/0, targets=self, animLock=???
    ElixirField = 3545, // L60, instant, range 0, AOE circle 5/0, targets=self, animLock=???
    EnlightenedMeditation = 36943, // L74, instant, range 0, single-target 0/0, targets=self, animLock=???
    SixSidedStar = 16476, // L80, instant, range 3, single-target 0/0, targets=hostile, animLock=0.600s
    ShadowOfTheDestroyer = 25767, // L82, instant, range 0, AOE circle 5/0, targets=self, animLock=???
    RisingPhoenix = 25768, // L86, instant, range 0, AOE circle 5/0, targets=self, animLock=0.600s
    PhantomRush = 25769, // L90, instant, range 3, AOE circle 5/0, targets=hostile, animLock=???
    LeapingOpo = 36945, // L92, instant, range 3, single-target 0/0, targets=hostile, animLock=0.600s
    RisingRaptor = 36946, // L92, instant, range 3, single-target 0/0, targets=hostile, animLock=0.600s
    PouncingCoeurl = 36947, // L92, instant, range 3, single-target 0/0, targets=hostile, animLock=0.600s
    ElixirBurst = 36948, // L92, instant, range 0, AOE circle 5/0, targets=self, animLock=0.600s
    WindsReply = 36949, // L96, instant, range 10, AOE rect 10/4, targets=hostile, animLock=0.600s
    FiresReply = 36950, // L100, instant, range 20, AOE circle 5/0, targets=hostile, animLock=0.600s

    // oGCDs
    SteelPeak = 25761, // L15, instant, 1.0s CD (group 0), range 3, single-target 0/0, targets=hostile, animLock=???
    Thunderclap = 25762, // L35, instant, 30.0s CD (group 14) (3 charges), range 20, single-target 0/0, targets=party/hostile, animLock=???
    HowlingFist = 25763, // L40, instant, 1.0s CD (group 2), range 10, AOE rect 10/2, targets=hostile, animLock=???
    Mantra = 65, // L42, instant, 90.0s CD (group 15), range 0, AOE circle 30/0, targets=self, animLock=0.600s
    PerfectBalance = 69, // L50, instant, 40.0s CD (group 13) (2 charges), range 0, single-target 0/0, targets=self, animLock=0.600s
    ForbiddenChakra = 3547, // L54, instant, 1.0s CD (group 0), range 3, single-target 0/0, targets=hostile, animLock=0.600s
    RiddleOfEarth = 7394, // L64, instant, 120.0s CD (group 20), range 0, single-target 0/0, targets=self, animLock=0.600s
    EarthsReply = 36944, // L64, instant, 1.0s CD (group 1), range 0, AOE circle 5/0, targets=self, animLock=0.600s
    RiddleOfFire = 7395, // L68, instant, 60.0s CD (group 11), range 0, single-target 0/0, targets=self, animLock=0.600s
    Brotherhood = 7396, // L70, instant, 120.0s CD (group 19), range 0, AOE circle 30/0, targets=self, animLock=0.600s
    RiddleOfWind = 25766, // L72, instant, 90.0s CD (group 16), range 0, single-target 0/0, targets=self, animLock=0.600s
    Enlightenment = 16474, // L74, instant, 1.0s CD (group 2), range 10, AOE rect 10/4, targets=hostile, animLock=???

    // Shared
    Braver = ClassShared.AID.Braver, // LB1, 2.0s cast, range 8, single-target, targets=hostile, castAnimLock=3.860
    Bladedance = ClassShared.AID.Bladedance, // LB2, 3.0s cast, range 8, single-target, targets=hostile, castAnimLock=3.860
    SecondWind = ClassShared.AID.SecondWind, // L8, instant, 120.0s CD (group 49), range 0, single-target, targets=self
    LegSweep = ClassShared.AID.LegSweep, // L10, instant, 40.0s CD (group 41), range 3, single-target, targets=hostile
    Bloodbath = ClassShared.AID.Bloodbath, // L12, instant, 90.0s CD (group 46), range 0, single-target, targets=self
    Feint = ClassShared.AID.Feint, // L22, instant, 90.0s CD (group 47), range 10, single-target, targets=hostile
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=self
    TrueNorth = ClassShared.AID.TrueNorth, // L50, instant, 45.0s CD (group 45/50) (2 charges), range 0, single-target, targets=self
}

public enum TraitID : uint
{
    None = 0,
    GreasedLightning = 364, // L1
    EnhancedGreasedLightning1 = 365, // L20
    DeepMeditation1 = 160, // L38
    EnhancedGreasedLightning2 = 366, // L40
    SteelPeakMastery = 428, // L54
    EnhancedPerfectBalance = 433, // L60
    DeepMeditation2 = 245, // L74
    HowlingFistMastery = 429, // L74
    EnhancedGreasedLightning3 = 367, // L76
    ArmOfTheDestroyerMastery = 430, // L82
    EnhancedThunderclap = 431, // L84
    MeleeMastery = 518, // L84
    FlintStrikeMastery = 512, // L86
    EnhancedBrotherhood = 432, // L88
    TornadoKickMastery = 513, // L90
    BeastChakraMastery = 577, // L92
    EnhancedSecondWind = 642, // L94
    MeleeMasteryII = 660, // L94
    EnhancedRiddleOfWind = 578, // L96
    EnhancedFeint = 641, // L98
    EnhancedRiddleOfFire = 579, // L100
}

public enum SID : uint
{
    None = 0,
    RaptorForm = 108, // applied by Dragon Kick, Leaping Opo to self
    CoeurlForm = 109, // applied by Twin Snakes, Rising Raptor to self
    MeditativeBrotherhood = 1182, // applied by Brotherhood to self
    Brotherhood = 1185, // applied by Brotherhood to self
    RiddleOfFire = 1181, // applied by Riddle of Fire to self
    FiresRumination = 3843, // applied by Riddle of Fire to self
    RiddleOfWind = 2687, // applied by Riddle of Wind to self
    WindsRumination = 3842, // applied by Riddle of Wind to self
    Mantra = 102, // applied by Mantra to self
    RiddleOfEarth = 1179, // applied by Riddle of Earth to self
    EarthsRumination = 3841, // applied by Riddle of Earth to self
    FormlessFist = 2513, // applied by Fire's Reply to self
    SixSidedStar = 2514, // applied by Six-Sided Star to self
    OpoOpoForm = 107, // applied by Demolish, Pouncing Coeurl to self
    PerfectBalance = 110, // applied by Perfect Balance to self
    Stun = 2, // applied by Leg Sweep to target

    LostFontofPower = 2346,
    BannerHonoredSacrifice = 2327,
    LostExcellence = 2564,
    Memorable = 2565,

    //Shared
    Feint = ClassShared.SID.Feint, // applied by Feint to target
    TrueNorth = ClassShared.SID.TrueNorth, // applied by True North to self
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.FinalHeaven, castAnimLock: 3.70f);
        d.RegisterSpell(AID.Bootshine);
        d.RegisterSpell(AID.TrueStrike);
        d.RegisterSpell(AID.SnapPunch);
        d.RegisterSpell(AID.SteelPeak);
        d.RegisterSpell(AID.TwinSnakes);
        d.RegisterSpell(AID.ArmOfTheDestroyer);
        d.RegisterSpell(AID.Demolish);
        d.RegisterSpell(AID.Rockbreaker);
        d.RegisterSpell(AID.Thunderclap);
        d.RegisterSpell(AID.HowlingFist);
        d.RegisterSpell(AID.Mantra);
        d.RegisterSpell(AID.FourPointFury);
        d.RegisterSpell(AID.DragonKick);
        d.RegisterSpell(AID.PerfectBalance);
        d.RegisterSpell(AID.FormShift);
        d.RegisterSpell(AID.ForbiddenChakra);
        d.RegisterSpell(AID.ElixirField);
        d.RegisterSpell(AID.MasterfulBlitz); // animLock=???
        d.RegisterSpell(AID.CelestialRevolution);
        d.RegisterSpell(AID.FlintStrike); // animLock=???
        d.RegisterSpell(AID.TornadoKick); // animLock=???
        d.RegisterSpell(AID.RiddleOfEarth);
        d.RegisterSpell(AID.RiddleOfFire);
        d.RegisterSpell(AID.Brotherhood);
        d.RegisterSpell(AID.RiddleOfWind);
        d.RegisterSpell(AID.Enlightenment);
        d.RegisterSpell(AID.SixSidedStar);
        d.RegisterSpell(AID.ShadowOfTheDestroyer);
        d.RegisterSpell(AID.RisingPhoenix);
        d.RegisterSpell(AID.PhantomRush);
        d.RegisterSpell(AID.LeapingOpo);
        d.RegisterSpell(AID.RisingRaptor);
        d.RegisterSpell(AID.PouncingCoeurl);
        d.RegisterSpell(AID.ElixirBurst);
        d.RegisterSpell(AID.WindsReply);
        d.RegisterSpell(AID.FiresReply);
        d.RegisterSpell(AID.EarthsReply);
        d.RegisterSpell(AID.EnlightenedMeditation);
        d.RegisterSpell(AID.ForbiddenMeditation);
        d.RegisterSpell(AID.InspiritedMeditation);
        d.RegisterSpell(AID.SteeledMeditation);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // hardcoded mechanics
        d.RegisterChargeIncreaseTrait(AID.Thunderclap, TraitID.EnhancedThunderclap);

        d.Spell(AID.Thunderclap)!.ForbidExecute = ActionDefinitions.DashToPositionCheck;

        // upgrades (TODO: don't think we actually care...)
        //d.Spell(AID.SteelPeak)!.TransformAction = d.Spell(AID.ForbiddenChakra)!.TransformAction = () => ActionID.MakeSpell(_state.BestForbiddenChakra);
        //d.Spell(AID.HowlingFist)!.TransformAction = d.Spell(AID.Enlightenment)!.TransformAction = () => ActionID.MakeSpell(_state.BestEnlightenment);
        //d.Spell(AID.Meditation)!.TransformAction = () => ActionID.MakeSpell(_state.Chakra == 5 ? _state.BestForbiddenChakra : AID.Meditation);
        //d.Spell(AID.ArmOfTheDestroyer)!.TransformAction = d.Spell(AID.ShadowOfTheDestroyer)!.TransformAction = () => ActionID.MakeSpell(_state.BestShadowOfTheDestroyer);
        //d.Spell(AID.MasterfulBlitz)!.TransformAction = () => ActionID.MakeSpell(_state.BestBlitz);
        //d.Spell(AID.PerfectBalance)!.Condition = _ => _state.PerfectBalanceLeft == 0;
        // combo replacement (TODO: don't think we actually care...)
        //d.Spell(AID.FourPointFury)!.TransformAction = config.AOECombos ? () => ActionID.MakeSpell(Rotation.GetNextComboAction(_state, _strategy)) : null;
    }
}
