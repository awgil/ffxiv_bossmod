namespace BossMod.SAM;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    DoomOfTheLiving = 7861, // LB3, 4.5s cast, range 8, single-target, targets=Hostile, animLock=3.700s?
    Hakaze = 7477, // L1, instant, GCD, range 3, single-target, targets=Hostile
    Jinpu = 7478, // L4, instant, GCD, range 3, single-target, targets=Hostile
    ThirdEye = 7498, // L6, instant, 15.0s CD (group 6), range 0, single-target, targets=Self
    Enpi = 7486, // L15, instant, GCD, range 20, single-target, targets=Hostile
    Shifu = 7479, // L18, instant, GCD, range 3, single-target, targets=Hostile
    Fuga = 7483, // L26, instant, GCD, range 8, AOE 8+R ?-degree cone, targets=Hostile
    Iaijutsu = 7867, // L30, 1.8s cast, GCD, range 0, single-target, targets=Self
    Gekko = 7481, // L30, instant, GCD, range 3, single-target, targets=Hostile
    Higanbana = 7489, // L30, 1.8s cast, GCD, range 6, single-target, targets=Hostile, animLock=0.100s?
    Mangetsu = 7484, // L35, instant, GCD, range 0, AOE 5 circle, targets=Self
    Kasha = 7482, // L40, instant, GCD, range 3, single-target, targets=Hostile
    TenkaGoken = 7488, // L40, 1.8s cast, GCD, range 0, AOE 8 circle, targets=Self
    Oka = 7485, // L45, instant, GCD, range 0, AOE 5 circle, targets=Self
    MidareSetsugekka = 7487, // L50, 1.8s cast, GCD, range 6, single-target, targets=Hostile
    MeikyoShisui = 7499, // L50, instant, 55.0s CD (group 18/70) (1-2 charges), range 0, single-target, targets=Self
    Yukikaze = 7480, // L50, instant, GCD, range 3, single-target, targets=Hostile
    HissatsuShinten = 7490, // L52, instant, 1.0s CD (group 1), range 3, single-target, targets=Hostile
    HissatsuGyoten = 7492, // L54, instant, 10.0s CD (group 4), range 20, single-target, targets=Hostile
    HissatsuYaten = 7493, // L56, instant, 10.0s CD (group 5), range 5, single-target, targets=Hostile, animLock=0.800
    Meditate = 7497, // L60, instant, 60.0s CD (group 12/57), range 0, single-target, targets=Self
    HissatsuKyuten = 7491, // L62, instant, 1.0s CD (group 0), range 0, AOE 5 circle, targets=Self
    Hagakure = 7495, // L68, instant, 5.0s CD (group 3), range 0, single-target, targets=Self
    Ikishoten = 16482, // L68, instant, 120.0s CD (group 19), range 0, single-target, targets=Self
    HissatsuGuren = 7496, // L70, instant, 120.0s CD (group 21), range 10, AOE 10+R width 4 rect, targets=Hostile
    HissatsuSenei = 16481, // L72, instant, 120.0s CD (group 21), range 3, single-target, targets=Hostile
    TsubameGaeshi = 16483, // L76, instant, GCD (1-2 charges), range 0, single-target, targets=Self
    KaeshiSetsugekka = 16486, // L76, instant, GCD (1-2 charges), range 6, single-target, targets=Hostile
    KaeshiGoken = 16485, // L76, instant, GCD (1-2 charges), range 0, AOE 8 circle, targets=Self
    Shoha = 16487, // L80, instant, 15.0s CD (group 7), range 10, AOE 10+R width 4 rect, targets=Hostile
    Tengentsu = 36962, // L82, instant, 15.0s CD (group 6), range 0, single-target, targets=Self
    Fuko = 25780, // L86, instant, GCD, range 0, AOE 5 circle, targets=Self
    OgiNamikiri = 25781, // L90, 1.8s cast, GCD, range 8, AOE 8+R ?-degree cone, targets=Hostile
    KaeshiNamikiri = 25782, // L90, instant, GCD, range 8, AOE 8+R ?-degree cone, targets=Hostile
    Gyofu = 36963, // L92, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    Zanshin = 36964, // L96, instant, 1.0s CD (group 2), range 8, AOE 8+R ?-degree cone, targets=Hostile, animLock=???
    TendoGoken = 36965, // L100, 1.8s cast, GCD, range 0, AOE 8 circle, targets=Self, animLock=???
    TendoSetsugekka = 36966, // L100, 1.8s cast, GCD, range 6, single-target, targets=Hostile, animLock=???
    TendoKaeshiGoken = 36967, // L100, instant, GCD, range 0, AOE 8 circle, targets=Self, animLock=???
    TendoKaeshiSetsugekka = 36968, // L100, instant, GCD, range 6, single-target, targets=Hostile, animLock=???

    // Shared
    Braver = ClassShared.AID.Braver, // LB1, 2.0s cast, range 8, single-target, targets=Hostile, animLock=3.860s?
    Bladedance = ClassShared.AID.Bladedance, // LB2, 3.0s cast, range 8, single-target, targets=Hostile, animLock=3.860s?
    SecondWind = ClassShared.AID.SecondWind, // L8, instant, 120.0s CD (group 49), range 0, single-target, targets=Self
    LegSweep = ClassShared.AID.LegSweep, // L10, instant, 40.0s CD (group 43), range 3, single-target, targets=Hostile
    Bloodbath = ClassShared.AID.Bloodbath, // L12, instant, 90.0s CD (group 46), range 0, single-target, targets=Self
    Feint = ClassShared.AID.Feint, // L22, instant, 90.0s CD (group 47), range 10, single-target, targets=Hostile
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=Self
    TrueNorth = ClassShared.AID.TrueNorth, // L50, instant, 45.0s CD (group 45/50) (2 charges), range 0, single-target, targets=Self
}

public enum TraitID : uint
{
    None = 0,
    KenkiMastery = 215, // L52
    KenkiMastery2 = 208, // L62
    WayoftheSamurai = 520, // L66
    EnhancedIaijutsu = 277, // L74
    EnhancedMeikyoShisui = 443, // L76
    EnhancedFufu = 278, // L78
    ThirdEyeMastery = 589, // L82
    WayoftheSamurai2 = 521, // L84
    FugaMastery = 519, // L86
    EnhancedIkishoten = 514, // L90
    HakazeMastery = 590, // L92
    EnhancedSecondWind = 642, // L94
    EnhancedHissatsu = 591, // L94
    WayOfTheSamuraiIII = 655, // L94
    EnhancedIkishotenII = 592, // L96
    EnhancedFeint = 641, // L98
    EnhancedMeikyoShisuiII = 593, // L100
}

public enum SID : uint
{
    None = 0,
    Fugetsu = 1298, // applied by Jinpu, Gekko to self
    Higanbana = 1228, // applied by Higanbana to target
    Fuka = 1299, // applied by Kasha to self
    MeikyoShisui = 1233, // applied by Meikyo Shisui to self
    KaeshiGoken = 3852, // applied by Tenka Goken to self
    KaeshiSetsugekka = 4216, // applied by Midare Setsugekka to self
    TendoKaeshiGoken = 4217, // applied by Tendo Goken to self
    TendoKaeshiSetsugekka = 4218, // applied by Tendo Setsugekka to self
    EnhancedEnpi = 1236, // applied by Hissatsu: Yaten to self
    Meditate = 1231, // applied by Meditate to self
    OgiNamikiriReady = 2959, // applied by Ikishoten to self
    Tengentsu = 3853, // applied by Tengentsu to self
    ZanshinReady = 3855, // applied by Ikishoten to self
    Tendo = 3856, // applied by Meikyo Shisui to self
    TsubameReady = 4216, // applied by Midare Setsugekka to self

    //Shared
    Feint = ClassShared.SID.Feint, // applied by Feint to target
    TrueNorth = ClassShared.SID.TrueNorth, // applied by True North to self
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.DoomOfTheLiving, castAnimLock: 3.70f); // animLock=3.700s?
        d.RegisterSpell(AID.Hakaze);
        d.RegisterSpell(AID.Jinpu);
        d.RegisterSpell(AID.ThirdEye);
        d.RegisterSpell(AID.Enpi);
        d.RegisterSpell(AID.Shifu);
        d.RegisterSpell(AID.Fuga);
        d.RegisterSpell(AID.Iaijutsu);
        d.RegisterSpell(AID.Gekko);
        d.RegisterSpell(AID.Higanbana, instantAnimLock: 0.10f); // animLock=0.100s?
        d.RegisterSpell(AID.Mangetsu);
        d.RegisterSpell(AID.Kasha);
        d.RegisterSpell(AID.TenkaGoken);
        d.RegisterSpell(AID.Oka);
        d.RegisterSpell(AID.MidareSetsugekka); // animLock=???
        d.RegisterSpell(AID.MeikyoShisui);
        d.RegisterSpell(AID.Yukikaze);
        d.RegisterSpell(AID.HissatsuShinten);
        d.RegisterSpell(AID.HissatsuGyoten);
        d.RegisterSpell(AID.HissatsuYaten, instantAnimLock: 0.80f); // animLock=0.800
        d.RegisterSpell(AID.Meditate);
        d.RegisterSpell(AID.HissatsuKyuten);
        d.RegisterSpell(AID.Hagakure);
        d.RegisterSpell(AID.Ikishoten);
        d.RegisterSpell(AID.HissatsuGuren);
        d.RegisterSpell(AID.HissatsuSenei);
        d.RegisterSpell(AID.TsubameGaeshi);
        d.RegisterSpell(AID.KaeshiSetsugekka);
        d.RegisterSpell(AID.KaeshiGoken);
        d.RegisterSpell(AID.Shoha);
        d.RegisterSpell(AID.Tengentsu);
        d.RegisterSpell(AID.Fuko);
        d.RegisterSpell(AID.OgiNamikiri);
        d.RegisterSpell(AID.KaeshiNamikiri);
        d.RegisterSpell(AID.Gyofu); // animLock=???
        d.RegisterSpell(AID.Zanshin); // animLock=???
        d.RegisterSpell(AID.TendoGoken); // animLock=???
        d.RegisterSpell(AID.TendoSetsugekka); // animLock=???
        d.RegisterSpell(AID.TendoKaeshiGoken); // animLock=???
        d.RegisterSpell(AID.TendoKaeshiSetsugekka); // animLock=???

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // hardcoded mechanics
        d.RegisterChargeIncreaseTrait(AID.MeikyoShisui, TraitID.EnhancedMeikyoShisui);

        // upgrades (TODO: don't think we actually care...)
        //d.Spell(AID.Iaijutsu)!.TransformAction = () => ActionID.MakeSpell(_state.BestIai);
        //d.Spell(AID.MeikyoShisui)!.Condition = _ => _state.MeikyoLeft == 0;

        d.Spell(AID.HissatsuGyoten)!.ForbidExecute = ActionDefinitions.DashToTargetCheck;
        d.Spell(AID.HissatsuYaten)!.ForbidExecute = ActionDefinitions.BackdashCheck(10);

        // dont want accidental double meikyo
        d.Spell(AID.MeikyoShisui)!.ForbidExecute = (_, player, _, _) => player.FindStatus(SID.MeikyoShisui) != null;
    }
}
