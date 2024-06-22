namespace BossMod.SAM;

public enum CDGroup : int
{
    HissatsuKyuten = 0, // 1.0 max
    HissatsuShinten = 1, // 1.0 max
    KaeshiNamikiri = 2, // 1.0 max
    Hagakure = 4, // 5.0 max
    HissatsuGyoten = 5, // 10.0 max
    HissatsuYaten = 6, // 10.0 max
    ThirdEye = 7, // 15.0 max
    Shoha = 8, // 15.0 max, shared by Shoha, Shoha II
    TsubameGaeshi = 10, // 2*60.0 max, shared by Tsubame-gaeshi, Kaeshi: Higanbana, Kaeshi: Goken, Kaeshi: Setsugekka
    Meditate = 11, // 60.0 max
    Ikishoten = 19, // 120.0 max
    MeikyoShisui = 20, // 2*55.0 max
    HissatsuGuren = 21, // 120.0 max, shared by Hissatsu: Guren, Hissatsu: Senei
    LegSweep = 41, // 40.0 max
    TrueNorth = 45, // 2*45.0 max
    Bloodbath = 46, // 90.0 max
    Feint = 47, // 90.0 max
    ArmsLength = 48, // 120.0 max
    SecondWind = 49, // 120.0 max
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
