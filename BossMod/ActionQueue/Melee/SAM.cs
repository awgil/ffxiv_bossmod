namespace BossMod.SAM;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    DoomOfTheLiving = 7861, // LB3, 4.5s cast, range 8, single-target, targets=hostile, animLock=???, castAnimLock=3.700
    Hakaze = 7477, // L1, instant, GCD, range 3, single-target, targets=hostile
    Jinpu = 7478, // L4, instant, GCD, range 3, single-target, targets=hostile
    ThirdEye = 7498, // L6, instant, 15.0s CD (group 7), range 0, single-target, targets=self
    Enpi = 7486, // L15, instant, GCD, range 20, single-target, targets=hostile
    Shifu = 7479, // L18, instant, GCD, range 3, single-target, targets=hostile
    Fuga = 7483, // L26, instant, GCD, range 8, AOE 8+R ?-degree cone, targets=hostile
    Iaijutsu = 7867, // L30, 1.8s cast, GCD, range 0, single-target, targets=self, animLock=???
    Gekko = 7481, // L30, instant, GCD, range 3, single-target, targets=hostile
    Higanbana = 7489, // L30, 1.8s cast, GCD, range 6, single-target, targets=hostile, animLock=0.100
    Mangetsu = 7484, // L35, instant, GCD, range 0, AOE 5 circle, targets=self
    Kasha = 7482, // L40, instant, GCD, range 3, single-target, targets=hostile
    TenkaGoken = 7488, // L40, 1.8s cast, GCD, range 0, AOE 8 circle, targets=self, animLock=???
    Oka = 7485, // L45, instant, GCD, range 0, AOE 5 circle, targets=self
    MidareSetsugekka = 7487, // L50, 1.8s cast, GCD, range 6, single-target, targets=hostile, animLock=???
    MeikyoShisui = 7499, // L50, instant, 55.0s CD (group 20/70), range 0, single-target, targets=self
    Yukikaze = 7480, // L50, instant, GCD, range 3, single-target, targets=hostile
    HissatsuShinten = 7490, // L52, instant, 1.0s CD (group 1), range 3, single-target, targets=hostile
    HissatsuGyoten = 7492, // L54, instant, 10.0s CD (group 5), range 20, single-target, targets=hostile
    HissatsuYaten = 7493, // L56, instant, 10.0s CD (group 6), range 5, single-target, targets=hostile, animLock=0.800
    Meditate = 7497, // L60, instant, 60.0s CD (group 11/57), range 0, single-target, targets=self
    HissatsuKyuten = 7491, // L62, instant, 1.0s CD (group 0), range 0, AOE 5 circle, targets=self
    Hagakure = 7495, // L68, instant, 5.0s CD (group 4), range 0, single-target, targets=self
    Ikishoten = 16482, // L68, instant, 120.0s CD (group 19), range 0, single-target, targets=self
    HissatsuGuren = 7496, // L70, instant, 120.0s CD (group 21), range 10, AOE 10+R width 4 rect, targets=hostile
    HissatsuSenei = 16481, // L72, instant, 120.0s CD (group 21), range 3, single-target, targets=hostile
    TsubameGaeshi = 16483, // L76, instant, 60.0s CD (group 10/57), range 0, single-target, targets=self, animLock=???
    KaeshiHiganbana = 16484, // L76, instant, 60.0s CD (group 10/57), range 6, single-target, targets=hostile
    KaeshiSetsugekka = 16486, // L76, instant, 60.0s CD (group 10/57), range 6, single-target, targets=hostile
    KaeshiGoken = 16485, // L76, instant, 60.0s CD (group 10/57), range 0, AOE 8 circle, targets=self
    Shoha = 16487, // L80, instant, 15.0s CD (group 8), range 3, single-target, targets=hostile
    Shoha2 = 25779, // L82, instant, 15.0s CD (group 8), range 0, AOE 5 circle, targets=self
    Fuko = 25780, // L86, instant, GCD, range 0, AOE 5 circle, targets=self
    OgiNamikiri = 25781, // L90, 1.8s cast, GCD, range 8, AOE 8+R ?-degree cone, targets=hostile, animLock=???
    KaeshiNamikiri = 25782, // L90, instant, 1.0s CD (group 2/57), range 8, AOE 8+R ?-degree cone, targets=hostile

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
    KenkiMastery = 215, // L52
    KenkiMastery2 = 208, // L62
    WayoftheSamurai = 520, // L66
    EnhancedIaijutsu = 277, // L74
    EnhancedFufu = 278, // L78
    EnhancedTsubame = 442, // L84
    WayoftheSamurai2 = 521, // L84
    FugaMastery = 519, // L86
    EnhancedMeikyoShisui = 443, // L88
    EnhancedIkishoten = 514, // L90
}

// TODO: regenerate
public enum SID : uint
{
    None = 0,
    ThirdEye = 1232, // applied by Third Eye to self
    EnhancedEnpi = 1236, // applied by Hissatsu: Yaten to self
    Fugetsu = 1298, // applied by Jinpu or Mangetsu to self, damage buff
    Fuka = 1299, // applied by Shifu or Oka to self, haste
    Meditate = 1231, // applied by Meditate to self, increases gauge
    MeikyoShisui = 1233, // applied by Meikyo Shisui to self, perform combo actions without prerequisites
    Higanbana = 1228, // applied by Higanbana to target, dot
    OgiNamikiriReady = 2959, // applied by Ikishoten to self
    TrueNorth = 1250, // applied by True North to self, ignore positionals
    Bloodbath = 84, // applied by Bloodbath to self, lifesteal
    Feint = 1195, // applied by Feint to target, 30% phys and -5% magic damage dealt
    Stun = 2, // applied by Leg Sweep to target

    LostFontofPower = 2346,
    BannerHonoredSacrifice = 2327,
    LostExcellence = 2564,
    Memorable = 2565,
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.DoomOfTheLiving, castAnimLock: 3.70f); // animLock=???, castAnimLock=3.700
        d.RegisterSpell(AID.Hakaze);
        d.RegisterSpell(AID.Jinpu);
        d.RegisterSpell(AID.ThirdEye);
        d.RegisterSpell(AID.Enpi);
        d.RegisterSpell(AID.Shifu);
        d.RegisterSpell(AID.Fuga);
        d.RegisterSpell(AID.Iaijutsu); // animLock=???
        d.RegisterSpell(AID.Gekko);
        d.RegisterSpell(AID.Higanbana, instantAnimLock: 0.10f); // animLock=0.100
        d.RegisterSpell(AID.Mangetsu);
        d.RegisterSpell(AID.Kasha);
        d.RegisterSpell(AID.TenkaGoken); // animLock=???
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
        d.RegisterSpell(AID.TsubameGaeshi); // animLock=???
        d.RegisterSpell(AID.KaeshiHiganbana);
        d.RegisterSpell(AID.KaeshiSetsugekka);
        d.RegisterSpell(AID.KaeshiGoken);
        d.RegisterSpell(AID.Shoha);
        d.RegisterSpell(AID.Shoha2);
        d.RegisterSpell(AID.Fuko);
        d.RegisterSpell(AID.OgiNamikiri); // animLock=???
        d.RegisterSpell(AID.KaeshiNamikiri);

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
    }
}
