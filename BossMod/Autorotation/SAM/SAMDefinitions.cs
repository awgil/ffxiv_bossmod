using System.Collections.Generic;

namespace BossMod.SAM
{
    public enum AID : uint
    {
        None = 0,

        // GCDs
        Hakaze = 7477, // L1, instant, range 3, single-target 0/0, targets=hostile, animLock=0.600s
        Jinpu = 7478, // L4, instant, range 3, single-target 0/0, targets=hostile, animLock=0.600s
        Enpi = 7486, // L15, instant, range 20, single-target 0/0, targets=hostile, animLock=0.600s
        Shifu = 7479, // L18, instant, range 3, single-target 0/0, targets=hostile, animLock=0.600s
        Fuga = 7483, // L26, instant, range 8, AOE cone 8/0, targets=hostile, animLock=0.600s
        Iaijutsu = 7867, // L30, 1.8s cast, range 0, single-target 0/0, targets=self, animLock=???
        Higanbana = 7489, // L30, 1.8s cast, range 6, single-target 0/0, targets=hostile, animLock=0.100s
        Gekko = 7481, // L30, instant, range 3, single-target 0/0, targets=hostile, animLock=0.600s
        Mangetsu = 7484, // L35, instant, range 0, AOE circle 5/0, targets=self, animLock=0.600s
        Kasha = 7482, // L40, instant, range 3, single-target 0/0, targets=hostile, animLock=0.600s
        TenkaGoken = 7488, // L40, 1.8s cast, range 0, AOE circle 8/0, targets=self, animLock=0.100s
        Oka = 7485, // L45, instant, range 0, AOE circle 5/0, targets=self, animLock=0.600s
        MidareSetsugekka = 7487, // L50, 1.8s cast, range 6, single-target 0/0, targets=hostile, animLock=0.100s
        Yukikaze = 7480, // L50, instant, range 3, single-target 0/0, targets=hostile, animLock=0.600s
        Fuko = 25780, // L86, instant, range 0, AOE circle 5/0, targets=self, animLock=0.600s
        OgiNamikiri = 25781, // L90, 1.8s cast, range 8, AOE cone 8/0, targets=hostile, animLock=0.100s

        // oGCDs
        ThirdEye = 7498, // L6, instant, 15.0s CD (group 7), range 0, single-target 0/0, targets=self, animLock=0.600s
        SecondWind = 7541, // L8, instant, 120.0s CD (group 49), range 0, single-target 0/0, targets=self, animLock=0.600s
        LegSweep = 7863, // L10, instant, 40.0s CD (group 41), range 3, single-target 0/0, targets=hostile, animLock=0.600s
        Bloodbath = 7542, // L12, instant, 90.0s CD (group 46), range 0, single-target 0/0, targets=self, animLock=0.600s
        Feint = 7549, // L22, instant, 90.0s CD (group 47), range 10, single-target 0/0, targets=hostile, animLock=0.600s
        ArmsLength = 7548, // L32, instant, 120.0s CD (group 48), range 0, single-target 0/0, targets=self, animLock=0.600s
        MeikyoShisui = 7499, // L50, instant, 55.0s CD (group 20) (2 charges), range 0, single-target 0/0, targets=self, animLock=0.600s
        TrueNorth = 7546, // L50, instant, 45.0s CD (group 45) (2 charges), range 0, single-target 0/0, targets=self, animLock=0.600s
        HissatsuShinten = 7490, // L52, instant, 1.0s CD (group 1), range 3, single-target 0/0, targets=hostile, animLock=0.600s
        HissatsuGyoten = 7492, // L54, instant, 10.0s CD (group 5), range 20, single-target 0/0, targets=hostile, animLock=0.600s
        HissatsuYaten = 7493, // L56, instant, 10.0s CD (group 6), range 5, single-target 0/0, targets=hostile, animLock=0.800s
        Meditate = 7497, // L60, instant, 60.0s CD (group 11), range 0, single-target 0/0, targets=self, animLock=0.600s
        HissatsuKyuten = 7491, // L62, instant, 1.0s CD (group 0), range 0, AOE circle 5/0, targets=self, animLock=0.600s
        Hagakure = 7495, // L68, instant, 5.0s CD (group 4), range 0, single-target 0/0, targets=self, animLock=0.600s
        Ikishoten = 16482, // L68, instant, 120.0s CD (group 19), range 0, single-target 0/0, targets=self, animLock=0.600s
        HissatsuGuren = 7496, // L70, instant, 120.0s CD (group 21), range 10, AOE rect 10/4, targets=hostile, animLock=0.600s
        HissatsuSenei = 16481, // L72, instant, 120.0s CD (group 21), range 3, single-target 0/0, targets=hostile, animLock=0.600s
        TsubameGaeshi = 16483, // L76, instant, 60.0s CD (group 10) (2 charges), range 0, single-target 0/0, targets=self, animLock=???
        KaeshiHiganbana = 16484, // L76, instant, 60.0s CD (group 10) (2 charges), range 6, single-target 0/0, targets=hostile, animLock=0.600s
        KaeshiGoken = 16485, // L76, instant, 60.0s CD (group 10) (2 charges), range 0, AOE circle 8/0, targets=self, animLock=0.600s
        KaeshiSetsugekka = 16486, // L76, instant, 60.0s CD (group 10) (2 charges), range 6, single-target 0/0, targets=hostile, animLock=0.600s
        Shoha = 16487, // L80, instant, 15.0s CD (group 8), range 3, single-target 0/0, targets=hostile, animLock=0.600s
        Shoha2 = 25779, // L82, instant, 15.0s CD (group 8), range 0, AOE circle 5/0, targets=self, animLock=0.600s
        KaeshiNamikiri = 25782, // L90, instant, 1.0s CD (group 2), range 8, AOE cone 8/0, targets=hostile, animLock=0.600s
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
    }

    public static class Definitions
    {
        public static uint[] UnlockQuests = { 68101, 68106 };

        public static bool Unlocked(AID aid, int level, int questProgress)
        {
            return aid switch
            {
                AID.Jinpu => level >= 4,
                AID.ThirdEye => level >= 6,
                AID.SecondWind => level >= 8,
                AID.LegSweep => level >= 10,
                AID.Bloodbath => level >= 12,
                AID.Enpi => level >= 15,
                AID.Shifu => level >= 18,
                AID.Feint => level >= 22,
                AID.Fuga => level >= 26,
                AID.Iaijutsu => level >= 30,
                AID.Higanbana => level >= 30,
                AID.Gekko => level >= 30,
                AID.ArmsLength => level >= 32,
                AID.Mangetsu => level >= 35,
                AID.Kasha => level >= 40,
                AID.TenkaGoken => level >= 40,
                AID.Oka => level >= 45,
                AID.MidareSetsugekka => level >= 50,
                AID.MeikyoShisui => level >= 50,
                AID.Yukikaze => level >= 50,
                AID.TrueNorth => level >= 50,
                AID.HissatsuShinten => level >= 52,
                AID.HissatsuGyoten => level >= 54,
                AID.HissatsuYaten => level >= 56,
                AID.Meditate => level >= 60 && questProgress > 0,
                AID.HissatsuKyuten => level >= 62,
                AID.Hagakure => level >= 68,
                AID.Ikishoten => level >= 68,
                AID.HissatsuGuren => level >= 70 && questProgress > 1,
                AID.HissatsuSenei => level >= 72,
                AID.TsubameGaeshi => level >= 76,
                AID.KaeshiHiganbana => level >= 76,
                AID.KaeshiGoken => level >= 76,
                AID.KaeshiSetsugekka => level >= 76,
                AID.Shoha => level >= 80,
                AID.Shoha2 => level >= 82,
                AID.Fuko => level >= 86,
                AID.OgiNamikiri => level >= 90,
                AID.KaeshiNamikiri => level >= 90,
                _ => true,
            };
        }

        public static bool Unlocked(TraitID tid, int level, int questProgress)
        {
            return tid switch
            {
                TraitID.KenkiMastery => level >= 52,
                TraitID.KenkiMastery2 => level >= 62,
                TraitID.WayoftheSamurai => level >= 66,
                TraitID.EnhancedIaijutsu => level >= 74,
                TraitID.EnhancedFufu => level >= 78,
                TraitID.EnhancedTsubame => level >= 84,
                TraitID.WayoftheSamurai2 => level >= 84,
                TraitID.FugaMastery => level >= 86,
                TraitID.EnhancedMeikyoShisui => level >= 88,
                TraitID.EnhancedIkishoten => level >= 90,
                _ => true,
            };
        }

        public static Dictionary<ActionID, ActionDefinition> SupportedActions;

        static Definitions()
        {
            SupportedActions = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionStr);
            SupportedActions.GCD(AID.Hakaze, 3);
            SupportedActions.GCD(AID.Jinpu, 3);
            SupportedActions.OGCD(AID.ThirdEye, 0, CDGroup.ThirdEye, 15.0f).EffectDuration = 4;
            SupportedActions.OGCD(AID.SecondWind, 0, CDGroup.SecondWind, 120.0f);
            SupportedActions.OGCD(AID.LegSweep, 3, CDGroup.LegSweep, 40.0f);
            SupportedActions.OGCD(AID.Bloodbath, 0, CDGroup.Bloodbath, 90.0f).EffectDuration = 20;
            SupportedActions.GCD(AID.Enpi, 20);
            SupportedActions.GCD(AID.Shifu, 3);
            SupportedActions.OGCD(AID.Feint, 10, CDGroup.Feint, 90.0f).EffectDuration = 10;
            SupportedActions.GCD(AID.Fuga, 8);
            SupportedActions.GCDCast(AID.Iaijutsu, 0, 1.8f);
            SupportedActions.GCDCast(AID.Higanbana, 6, 1.8f);
            SupportedActions.GCD(AID.Gekko, 3);
            SupportedActions.OGCD(AID.ArmsLength, 0, CDGroup.ArmsLength, 120.0f).EffectDuration = 6;
            SupportedActions.GCD(AID.Mangetsu, 0);
            SupportedActions.GCD(AID.Kasha, 3);
            SupportedActions.GCDCast(AID.TenkaGoken, 0, 1.8f);
            SupportedActions.GCD(AID.Oka, 0);
            SupportedActions.GCDCast(AID.MidareSetsugekka, 6, 1.8f);
            SupportedActions.OGCDWithCharges(AID.MeikyoShisui, 0, CDGroup.MeikyoShisui, 55.0f, 2).EffectDuration = 15;
            SupportedActions.GCD(AID.Yukikaze, 3);
            SupportedActions.OGCDWithCharges(AID.TrueNorth, 0, CDGroup.TrueNorth, 45.0f, 2).EffectDuration = 10;
            SupportedActions.OGCD(AID.HissatsuShinten, 3, CDGroup.HissatsuShinten, 1.0f);
            SupportedActions.OGCD(AID.HissatsuGyoten, 20, CDGroup.HissatsuGyoten, 10.0f);
            SupportedActions.OGCD(AID.HissatsuYaten, 5, CDGroup.HissatsuYaten, 10.0f, 0.800f);
            SupportedActions.OGCD(AID.Meditate, 0, CDGroup.Meditate, 60.0f);
            SupportedActions.OGCD(AID.HissatsuKyuten, 0, CDGroup.HissatsuKyuten, 1.0f);
            SupportedActions.OGCD(AID.Hagakure, 0, CDGroup.Hagakure, 5.0f);
            SupportedActions.OGCD(AID.Ikishoten, 0, CDGroup.Ikishoten, 120.0f);
            SupportedActions.OGCD(AID.HissatsuGuren, 10, CDGroup.HissatsuGuren, 120.0f);
            SupportedActions.OGCD(AID.HissatsuSenei, 3, CDGroup.HissatsuGuren, 120.0f);
            SupportedActions.OGCDWithCharges(AID.TsubameGaeshi, 0, CDGroup.TsubameGaeshi, 60.0f, 2);
            SupportedActions.OGCDWithCharges(AID.KaeshiHiganbana, 6, CDGroup.TsubameGaeshi, 60.0f, 2);
            SupportedActions.OGCDWithCharges(AID.KaeshiGoken, 0, CDGroup.TsubameGaeshi, 60.0f, 2);
            SupportedActions.OGCDWithCharges(AID.KaeshiSetsugekka, 6, CDGroup.TsubameGaeshi, 60.0f, 2);
            SupportedActions.OGCD(AID.Shoha, 3, CDGroup.Shoha, 15.0f);
            SupportedActions.OGCD(AID.Shoha2, 0, CDGroup.Shoha, 15.0f);
            SupportedActions.GCD(AID.Fuko, 0);
            SupportedActions.GCDCast(AID.OgiNamikiri, 8, 1.8f);
            SupportedActions.OGCD(AID.KaeshiNamikiri, 8, CDGroup.KaeshiNamikiri, 1.0f);
        }
    }
}
