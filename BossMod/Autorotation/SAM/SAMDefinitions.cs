using System.Collections.Generic;
using System.Runtime.Versioning;
using BossMod.Endwalker.HuntA.Petalodus;

namespace BossMod.SAM
{
    public enum AID : uint
    {
        None = 0,
        Sprint = 3,

        // single target GCDs
        Hakaze = 7477, // L1, instant, range 3, single-target 0/0, targets=hostile
        Jinpu = 7478, // L4, instant, range 3, single-target 0/0, targets=hostile
        Shifu = 7479, // L18, instant, range 3, single-target 0/0, targets=hostile
        Gekko = 7481, // L30, instant, range 3, single-target 0/0, targets=hostile
        Kasha = 7482, // L40, instant, range 3, single-target 0/0, targets=hostile
        Yukikaze = 7480, // L50, instant, range 3, single-target 0/0, targets=hostile
        Higanbana = 7489, // L30, 1.5s, range 6, single-target 0/0, targets=hostile
        MidareSetsugekka = 7487, // L50, 1.5s, range 6, single-target 0/0, targets=hostile
        KaeshiHiganbana = 16484, // L76, instant, 60.0s CD (group 10) (2 charges), range 6, single-target 0/0, targets=hostile
        KaeshiSetsugekka = 16486, // L76, instant, 60.0s CD (group 10) (2 charges), range 6, single-target 0/0, targets=hostile

        // aoe GCDs
        Fuga = 7483, // L26, instant, range 8, AOE cone 8/8, targets=hostile
        Mangetsu = 7484, // L35, instant, range 0, AOE circle 5/0, targets=self
        TenkaGoken = 7488, // L40, 1.5s, range 0, AOE circle 8/0, targets=self
        Oka = 7485, // L45, instant, range 0, AOE circle 5/0, targets=self
        KaeshiGoken = 16485, // L76, instant, 60.0s CD (group 10) (2 charges), range 0, AOE circle 8/0, targets=self
        Fuko = 25780, // L86, instant, range 0, AOE circle 5/0, targets=self
        OgiNamikiri = 25781, // L90, 1.3s, range 8, AOE cone 8/8, targets=hostile
        KaeshiNamikiri = 25782, // L90, instant, range 8, AOE cone 8/8, targets=hostile

        // oGCDs
        HissatsuShinten = 7490, // L52, instant, 1.0s CD (group 1), range 3, single-target 0/0, targets=hostile
        HissatsuGyoten = 7492, // L54, instant, 10.0s CD (group 5), range 20, single-target 0/0, targets=hostile
        HissatsuKyuten = 7491, // L62, instant, 1.0s CD (group 1), range 0, AOE circle 5/0, targets=self
        HissatsuGuren = 7496, // L70, instant, 120.0s CD (group 21), range 10, AOE rect 10/10, targets=hostile
        HissatsuSenei = 16481, // L72, instant, 120.0s CD (group 21), range 3, single-target 0/0, targets=hostile
        Shoha = 16487, // L80, instant, 15.0s CD (group 8), range 3, single-target 0/0, targets=hostile
        Shoha2 = 25779, // L82, instant, 15.0s CD (group 8), range 0, AOE circle 5/0, targets=self

        // offensive CDs
        MeikyoShisui = 7499, // L50, instant, 55.0s CD (group 20) (2 charges), range 0, single-target 0/0, targets=self
        Meditate = 7497, // L60, instant, 60.0s CD (group 11), range 0, single-target 0/0, targets=self
        Hagakure = 7495, // L68, instant, 5.0s CD (group 4), range 0, single-target 0/0, targets=self
        Ikishoten = 16482, // L68, instant, 120.0s CD (group 19), range 0, single-target 0/0, targets=self

        // defensive CDs
        ThirdEye = 7498, // L6, instant, 14s CD (group 7), range 0, single-target 0/0, targets=self
        SecondWind = 7541, // L8, instant, 120.0s CD (group 40), range 0, single-target 0/0, targets=self
        Bloodbath = 7542, // L12, instant, 90.0s CD (group 42), range 0, single-target 0/0, targets=self
        Feint = 7549, // L22, instant, 90.0s CD (group 43), range 10, single-target 0/0, targets=hostile
        ArmsLength = 7548, // L32, instant, 120.0s CD (group 46), range 0, single-target 0/0, targets=self

        // misc
        Enpi = 7486, // L15, instant, range 20, single-target 0/0, targets=hostile
        TrueNorth = 7546, // L50, instant, 45.0s CD (group 44) (2 charges), range 0, single-target 0/0, targets=self
        LegSweep = 7863, // L10, instant, 40.0s CD (group 41), range 3, single-target 0/0, targets=hostile
        HissatsuYaten = 7493, // L56, instant, 10.0s CD (group 6), range 5, single-target 0/0, targets=hostile
    }

    public enum TraitID : uint
    {
        None = 0,
        KenkiMastery = 215, // L52, kenki gauge
        KenkiMastery2 = 208, // L62
        WayoftheSamurai = 520, // L66
        EnhancedIaijutsu = 277, // L74, iaijutsu cast 1.5s -> 1.3s
        EnhancedFufu = 278, // L78
        EnhancedTsubame = 442, // L84, second charge
        WayoftheSamurai2 = 521, // L84
        FugaMastery = 519, // L86, unlocks Fuko
        EnhancedMeikyoShisui = 443, // L88, second charge
        EnhancedIkishoten = 514, // L90, unlocks ogi
    }

    public enum CDGroup : int
    {
        HissatsuShinten = 1,
        Hagakure = 4, // 5.0 max
        HissatsuGyoten = 5, // 10.0 max
        HissatsuYaten = 6, // 10.0 max
        ThirdEye = 7, // 15.0 max
        Shoha = 8, // 15.0 max
        Tsubame = 10, // 2*60.0 max
        Meditate = 11, // 60.0 max
        Ikishoten = 19, // 120.0 max
        MeikyoShisui = 20, // 2*55.0 max
        HissatsuSenei = 21, // 120.0 max
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
        Feint = 1195, // applied by Feint to target, -10% phys and -5% magic damage dealt
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
                AID.Enpi => level >= 15,
                AID.Shifu => level >= 18,
                AID.Fuga => level >= 26,
                AID.Gekko => level >= 30,
                AID.Higanbana => level >= 30,
                AID.Mangetsu => level >= 35,
                AID.Kasha => level >= 40,
                AID.TenkaGoken => level >= 40,
                AID.Oka => level >= 45,
                AID.Yukikaze => level >= 50,
                AID.MidareSetsugekka => level >= 50,
                AID.MeikyoShisui => level >= 50,
                AID.HissatsuShinten => level >= 52,
                AID.HissatsuGyoten => level >= 54,
                AID.HissatsuYaten => level >= 56,
                AID.Meditate => level >= 60 && questProgress > 0,
                AID.HissatsuKyuten => level >= 62,
                AID.Ikishoten => level >= 68,
                AID.Hagakure => level >= 68,
                AID.HissatsuGuren => level >= 70 && questProgress > 1,
                AID.HissatsuSenei => level >= 72,
                AID.KaeshiHiganbana => level >= 76,
                AID.KaeshiSetsugekka => level >= 76,
                AID.KaeshiGoken => level >= 76,
                AID.Shoha => level >= 80,
                AID.Shoha2 => level >= 82,
                AID.Fuko => level >= 86,
                AID.OgiNamikiri => level >= 90,
                AID.KaeshiNamikiri => level >= 90,
                _ => true
            };
        }

        public static bool Unlocked(TraitID tid, int level)
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
                _ => true
            };
        }

        public static Dictionary<ActionID, ActionDefinition> SupportedActions;
        static Definitions()
        {
            SupportedActions = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionStr);
            SupportedActions.GCD(AID.Hakaze, 3);
            SupportedActions.GCD(AID.Jinpu, 3);
            SupportedActions.GCD(AID.Shifu, 3);
            SupportedActions.GCD(AID.Gekko, 3);
            SupportedActions.GCD(AID.Kasha, 3);
            SupportedActions.GCD(AID.Yukikaze, 3);
            SupportedActions.GCDCast(AID.Higanbana, 6, 1.3f);
            SupportedActions.GCDCast(AID.TenkaGoken, 8, 1.3f);
            SupportedActions.GCDCast(AID.MidareSetsugekka, 6, 1.3f);
            SupportedActions.GCDCast(AID.OgiNamikiri, 8, 1.3f);
            SupportedActions.GCDWithCharges(AID.KaeshiSetsugekka, 6, CDGroup.Tsubame, 60, 2);
            SupportedActions.GCD(AID.KaeshiNamikiri, 8);
            SupportedActions.GCD(AID.Fuga, 8);
            SupportedActions.GCD(AID.Fuko, 5);
            SupportedActions.GCD(AID.Mangetsu, 5);
            SupportedActions.GCD(AID.Oka, 5);
            SupportedActions.GCDWithCharges(AID.KaeshiGoken, 8, CDGroup.Tsubame, 60, 2);
            SupportedActions.OGCD(AID.Hagakure, 0, CDGroup.Hagakure, 5);
            SupportedActions.OGCD(AID.HissatsuShinten, 3, CDGroup.HissatsuShinten, 1);
            SupportedActions.OGCD(AID.HissatsuKyuten, 5, CDGroup.HissatsuShinten, 1);
            SupportedActions.OGCD(AID.HissatsuSenei, 3, CDGroup.HissatsuSenei, 120.0f);
            SupportedActions.OGCD(AID.HissatsuGuren, 10, CDGroup.HissatsuSenei, 1);
            SupportedActions.OGCD(AID.Shoha, 3, CDGroup.Shoha, 15.0f);
            SupportedActions.OGCD(AID.Shoha2, 0, CDGroup.Shoha, 15.0f);
            SupportedActions.OGCD(AID.Ikishoten, 0, CDGroup.Ikishoten, 120.0f);
            SupportedActions.OGCD(AID.SecondWind, 0, CDGroup.SecondWind, 120.0f);
            SupportedActions.OGCD(AID.Bloodbath, 0, CDGroup.Bloodbath, 90.0f);
            SupportedActions.OGCD(AID.Feint, 10, CDGroup.Feint, 90.0f).EffectDuration = 10;
            SupportedActions.OGCD(AID.ArmsLength, 0, CDGroup.ArmsLength, 120.0f).EffectDuration = 6;
            SupportedActions.OGCD(AID.ThirdEye, 0, CDGroup.ThirdEye, 15.0f).EffectDuration = 4;
            SupportedActions.OGCDWithCharges(AID.TrueNorth, 0, CDGroup.TrueNorth, 45.0f, 2);
            SupportedActions.OGCDWithCharges(AID.MeikyoShisui, 0, CDGroup.MeikyoShisui, 55.0f, 2);
            SupportedActions.OGCD(AID.LegSweep, 3, CDGroup.LegSweep, 40.0f);
        }
    }
}
