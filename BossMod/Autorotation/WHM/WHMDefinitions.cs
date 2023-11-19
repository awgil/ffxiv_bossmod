using System.Collections.Generic;

namespace BossMod.WHM
{
    public enum AID : uint
    {
        None = 0,
        Sprint = 3,

        // single-target damage GCDs
        Stone1 = 119, // L1, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Stone2 = 127, // L18, 1.5s cast, range 25, single-target 0/0, targets=hostile
        Stone3 = 3568, // L54, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Stone4 = 7431, // L64, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Glare1 = 16533, // L72, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Glare3 = 25859, // L82, 1.5s cast, range 25, single-target 0/0, targets=hostile
        Aero1 = 121, // L4, instant, range 25, single-target 0/0, targets=hostile, animLock=???
        Aero2 = 132, // L46, instant, range 25, single-target 0/0, targets=hostile
        Dia = 16532, // L72, instant, range 25, single-target 0/0, targets=hostile
        AfflatusMisery = 16535, // L74, instant, range 25, AOE circle 5/0, targets=hostile

        // aoe damage GCDs
        Holy1 = 139, // L45, 2.5s cast, range 0, AOE circle 8/0, targets=self
        Holy3 = 25860, // L82, 2.5s cast, range 0, AOE circle 8/0, targets=self

        // single-target heal GCDs
        Cure1 = 120, // L2, 1.5s cast, range 30, single-target 0/0, targets=self/party/friendly
        Cure2 = 135, // L30, 2.0s cast, range 30, single-target 0/0, targets=self/party/friendly
        Regen = 137, // L35, instant, range 30, single-target 0/0, targets=self/party/friendly
        AfflatusSolace = 16531, // L52, instant, range 30, single-target 0/0, targets=self/party/friendly

        // aoe heal GCDs
        Medica1 = 124, // L10, 2.0s cast, range 0, AOE circle 15/0, targets=self
        Medica2 = 133, // L50, 2.0s cast, range 0, AOE circle 20/0, targets=self
        Cure3 = 131, // L40, 2.0s cast, range 30, AOE circle 10/0, targets=self/party
        AfflatusRapture = 16534, // L76, instant, range 0, AOE circle 20/0, targets=self

        // oGCDs
        Assize = 3571, // L56, instant, 45.0s CD (group 5), range 0, AOE circle 15/0, targets=self
        Asylum = 3569, // L52, instant, 90.0s CD (group 17), range 30, Ground circle 10/0, targets=area
        DivineBenison = 7432, // L66, instant, 30.0s CD (group 9) (2 charges), range 30, single-target 0/0, targets=self/party
        Tetragrammaton = 3570, // L60, instant, 60.0s CD (group 13), range 30, single-target 0/0, targets=self/party/friendly
        Benediction = 140, // L50, instant, 180.0s CD (group 23), range 30, single-target 0/0, targets=self/party/friendly
        LiturgyOfTheBell = 25862, // L90, instant, 180.0s CD (group 24), range 30, Ground circle 1/0, targets=area
        LiturgyOfTheBellEnd = 28509, // L90, instant, 1.0s CD (group 0), range 0, single-target 0/0, targets=self

        // buff CDs
        Swiftcast = 7561, // L18, instant, 60.0s CD (group 42), range 0, single-target 0/0, targets=self
        LucidDreaming = 7562, // L14, instant, 60.0s CD (group 41), range 0, single-target 0/0, targets=self
        PresenceOfMind = 136, // L30, instant, 120.0s CD (group 22), range 0, single-target 0/0, targets=self
        ThinAir = 7430, // L58, instant, 60.0s CD (group 20) (2 charges), range 0, single-target 0/0, targets=self
        PlenaryIndulgence = 7433, // L70, instant, 60.0s CD (group 10), range 0, AOE circle 20/0, targets=self
        Temperance = 16536, // L80, instant, 120.0s CD (group 21), range 0, single-target 0/0, targets=self
        Aquaveil = 25861, // L86, instant, 60.0s CD (group 11), range 30, single-target 0/0, targets=self/party
        Surecast = 7559, // L44, instant, 120.0s CD (group 43), range 0, single-target 0/0, targets=self

        // misc
        Raise = 125, // L12, 8.0s cast, range 30, single-target 0/0, targets=party/friendly, animLock=???
        Repose = 16560, // L8, 2.5s cast, range 30, single-target 0/0, targets=hostile
        Esuna = 7568, // L10, 1.0s cast, range 30, single-target 0/0, targets=self/party/friendly
        Rescue = 7571, // L48, instant, 120.0s CD (group 45), range 30, single-target 0/0, targets=party, animLock=???
    }

    public enum TraitID : uint
    {
        None = 0,
        StoneMastery1 = 179, // L18, stone1 -> stone2 upgrade
        MaimAndMend1 = 23, // L20, heal and damage increase
        Freecure = 25, // L32, cure1 can proc a buff that makes cure2 free
        MaimAndMend2 = 26, // L40, heal and damage increase
        AeroMastery1 = 180, // L46, aero1 -> aero2 upgrade
        SecretOfTheLily = 196, // L52, lily gauge
        StoneMastery2 = 181, // L54, stone2 -> stone3 upgrade
        StoneMastery3 = 182, // L64, store3 -> stone4 upgrade
        AeroMastery2 = 307, // L72, aero2 -> dia upgrade
        StoneMastery4 = 308, // L72, stone4 -> glare1 upgrade
        TranscendentAfflatus = 309, // L74, blood lily
        EnhancedAsylum = 310, // L78, healing taken buff
        GlareMastery = 487, // L82, glare1 -> glare3 upgrade
        HolyMastery = 488, // L82, holy1 -> holy3 upgrade
        EnhancedHealingMagic = 489, // L85, potency increase
        EnhancedDivineBenison = 490, // L88, second charge
    }

    public enum CDGroup : int
    {
        LiturgyOfTheBellEnd = 0, // 1.0 max
        Assize = 5, // 40.0 max
        DivineBenison = 9, // 2*30.0 max
        PlenaryIndulgence = 10, // 60.0 max
        Aquaveil = 11, // 60.0 max
        Tetragrammaton = 13, // 60.0 max
        Asylum = 17, // 90.0 max
        ThinAir = 20, // 2*60.0 max
        Temperance = 21, // 120.0 max
        PresenceOfMind = 22, // 120.0 max
        Benediction = 23, // 180.0 max
        LiturgyOfTheBell = 24, // 180.0 max
        Swiftcast = 44, // 60.0 max
        LucidDreaming = 45, // 60.0 max
        Surecast = 48, // 120.0 max
        Rescue = 49, // 120.0 max
    }

    public enum SID : uint
    {
        None = 0,
        Aero1 = 143, // applied by Aero1 to target, dot
        Aero2 = 144, // applied by Aero2 to target, dot
        Dia = 1871, // applied by Dia to target, dot
        Medica2 = 150, // applied by Medica2 to targets, hot
        Freecure = 155, // applied by Cure1 to self, next cure2 is free
        Swiftcast = 167, // applied by Swiftcast to self, next gcd is instant
        ThinAir = 1217, // applied by Thin Air to self, next gcd costs no mp
        LucidDreaming = 1204, // applied by Lucid Dreaming to self, mp regen
        DivineBenison = 1218, // applied by Divine Benison to target, shield
        Confession = 1219, // applied by Plenary Indulgence to self, heal buff
        Temperance = 1872, // applied by Temperance to self, heal and mitigate buff
        Surecast = 160, // applied by Surecast to self, knockback immune
        PresenceOfMind = 157, // applied by Presence of Mind to self, damage buff
        Regen = 158, // applied by Regen to target, hp regen
        Asylum = 1911, // applied by Asylum to target, hp regen
        Aquaveil = 2708, // applied by Aquaveil to target, mitigate
        LiturgyOfTheBell = 2709, // applied by Liturgy of the Bell to target, heal on hit
        Sleep = 3, // applied by Repose to target
    }

    public static class Definitions
    {
        public static uint[] UnlockQuests = { 65977, 66615, 66616, 66617, 66619, 66620, 67256, 67257, 67258, 67259, 67261, 67954 };

        public static bool Unlocked(AID aid, int level, int questProgress)
        {
            return aid switch
            {
                AID.Cure1 => level >= 2,
                AID.Aero1 => level >= 4,
                AID.Repose => level >= 8,
                AID.Esuna => level >= 10,
                AID.Medica1 => level >= 10,
                AID.Raise => level >= 12,
                AID.LucidDreaming => level >= 14,
                AID.Stone2 => level >= 18,
                AID.Swiftcast => level >= 18,
                AID.Cure2 => level >= 30 && questProgress > 0,
                AID.PresenceOfMind => level >= 30 && questProgress > 1,
                AID.Regen => level >= 35 && questProgress > 2,
                AID.Cure3 => level >= 40 && questProgress > 3,
                AID.Surecast => level >= 44,
                AID.Holy1 => level >= 45 && questProgress > 4,
                AID.Aero2 => level >= 46,
                AID.Rescue => level >= 48,
                AID.Benediction => level >= 50 && questProgress > 5,
                AID.Medica2 => level >= 50,
                AID.AfflatusSolace => level >= 52,
                AID.Asylum => level >= 52 && questProgress > 6,
                AID.Stone3 => level >= 54 && questProgress > 7,
                AID.Assize => level >= 56 && questProgress > 8,
                AID.ThinAir => level >= 58 && questProgress > 9,
                AID.Tetragrammaton => level >= 60 && questProgress > 10,
                AID.Stone4 => level >= 64,
                AID.DivineBenison => level >= 66,
                AID.PlenaryIndulgence => level >= 70 && questProgress > 11,
                AID.Dia => level >= 72,
                AID.Glare1 => level >= 72,
                AID.AfflatusMisery => level >= 74,
                AID.AfflatusRapture => level >= 76,
                AID.Temperance => level >= 80,
                AID.Glare3 => level >= 82,
                AID.Holy3 => level >= 82,
                AID.Aquaveil => level >= 86,
                AID.LiturgyOfTheBell => level >= 90,
                AID.LiturgyOfTheBellEnd => level >= 90,
                _ => true,
            };
        }

        public static bool Unlocked(TraitID tid, int level, int questProgress)
        {
            return tid switch
            {
                TraitID.StoneMastery1 => level >= 18,
                TraitID.MaimAndMend1 => level >= 20,
                TraitID.Freecure => level >= 32,
                TraitID.MaimAndMend2 => level >= 40,
                TraitID.AeroMastery1 => level >= 46,
                TraitID.SecretOfTheLily => level >= 52 && questProgress > 6,
                TraitID.StoneMastery2 => level >= 54 && questProgress > 7,
                TraitID.StoneMastery3 => level >= 64,
                TraitID.AeroMastery2 => level >= 72,
                TraitID.StoneMastery4 => level >= 72,
                TraitID.TranscendentAfflatus => level >= 74,
                TraitID.EnhancedAsylum => level >= 78,
                TraitID.GlareMastery => level >= 82,
                TraitID.HolyMastery => level >= 82,
                TraitID.EnhancedHealingMagic => level >= 85,
                TraitID.EnhancedDivineBenison => level >= 88,
                _ => true,
            };
        }

        public static Dictionary<ActionID, ActionDefinition> SupportedActions;
        static Definitions()
        {
            SupportedActions = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionMnd);
            SupportedActions.GCDCast(AID.Stone1, 25, 1.5f);
            SupportedActions.GCDCast(AID.Stone2, 25, 1.5f);
            SupportedActions.GCDCast(AID.Stone3, 25, 1.5f);
            SupportedActions.GCDCast(AID.Stone4, 25, 1.5f);
            SupportedActions.GCDCast(AID.Glare1, 25, 1.5f);
            SupportedActions.GCDCast(AID.Glare3, 25, 1.5f);
            SupportedActions.GCD(AID.Aero1, 25);
            SupportedActions.GCD(AID.Aero2, 25);
            SupportedActions.GCD(AID.Dia, 25);
            SupportedActions.GCD(AID.AfflatusMisery, 25);
            SupportedActions.GCDCast(AID.Holy1, 0, 2.5f);
            SupportedActions.GCDCast(AID.Holy3, 0, 2.5f);
            SupportedActions.GCDCast(AID.Cure1, 30, 1.5f);
            SupportedActions.GCDCast(AID.Cure2, 30, 2.0f);
            SupportedActions.GCD(AID.Regen, 30);
            SupportedActions.GCD(AID.AfflatusSolace, 30);
            SupportedActions.GCDCast(AID.Medica1, 0, 2.0f);
            SupportedActions.GCDCast(AID.Medica2, 0, 2.0f);
            SupportedActions.GCDCast(AID.Cure3, 30, 2.0f);
            SupportedActions.GCD(AID.AfflatusRapture, 0);
            SupportedActions.OGCD(AID.Assize, 0, CDGroup.Assize, 45.0f);
            SupportedActions.OGCD(AID.Asylum, 30, CDGroup.Asylum, 90.0f);
            SupportedActions.OGCDWithCharges(AID.DivineBenison, 30, CDGroup.DivineBenison, 30.0f, 2);
            SupportedActions.OGCD(AID.Tetragrammaton, 30, CDGroup.Tetragrammaton, 60.0f);
            SupportedActions.OGCD(AID.Benediction, 30, CDGroup.Benediction, 180.0f);
            SupportedActions.OGCD(AID.LiturgyOfTheBell, 30, CDGroup.LiturgyOfTheBell, 180.0f);
            SupportedActions.OGCD(AID.LiturgyOfTheBellEnd, 0, CDGroup.LiturgyOfTheBellEnd, 1.0f);
            SupportedActions.OGCD(AID.Swiftcast, 0, CDGroup.Swiftcast, 60.0f);
            SupportedActions.OGCD(AID.LucidDreaming, 0, CDGroup.LucidDreaming, 60.0f);
            SupportedActions.OGCD(AID.PresenceOfMind, 0, CDGroup.PresenceOfMind, 120.0f);
            SupportedActions.OGCDWithCharges(AID.ThinAir, 0, CDGroup.ThinAir, 60.0f, 2);
            SupportedActions.OGCD(AID.PlenaryIndulgence, 0, CDGroup.PlenaryIndulgence, 60.0f);
            SupportedActions.OGCD(AID.Temperance, 0, CDGroup.Temperance, 120.0f);
            SupportedActions.OGCD(AID.Aquaveil, 30, CDGroup.Aquaveil, 60.0f);
            SupportedActions.OGCD(AID.Surecast, 0, CDGroup.Surecast, 120.0f);
            SupportedActions.GCDCast(AID.Raise, 30, 8.0f);
            SupportedActions.GCDCast(AID.Repose, 30, 2.5f);
            SupportedActions.GCDCast(AID.Esuna, 30, 1.0f);
            SupportedActions.OGCD(AID.Rescue, 30, CDGroup.Rescue, 120.0f);
        }
    }
}
