using System.Collections.Generic;

namespace BossMod.WHM
{
    public enum AID : uint
    {
        None = 0,

        // single-target damage GCDs
        Stone1 = 119, // L1, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Stone2 = 127, // L18, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Stone3 = 3568, // L54, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Stone4 = 7431, // L64, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Glare1 = 16533, // L72, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Glare3 = 25859, // L82, 1.5s cast, range 25, single-target 0/0, targets=hostile
        Aero1 = 121, // L4, instant, range 25, single-target 0/0, targets=hostile, animLock=???
        Aero2 = 132, // L46, instant, range 25, single-target 0/0, targets=hostile, animLock=???
        Dia = 16532, // L72, instant, range 25, single-target 0/0, targets=hostile
        AfflatusMisery = 16535, // L74, instant, range 25, AOE circle 5/0, targets=hostile

        // aoe damage GCDs
        Holy1 = 139, // L45, 2.5s cast, range 0, AOE circle 8/0, targets=self, animLock=???
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

    public enum CDGroup : int
    {
        LiturgyOfTheBellEnd = 0, // 1.0 max
        Assize = 5, // 45.0 max
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
        LucidDreaming = 41, // 60.0 max
        Swiftcast = 42, // 60.0 max
        Surecast = 43, // 120.0 max
        Rescue = 45, // 120.0 max
    }

    public enum MinLevel : int
    {
        // actions
        Cure1 = 2,
        Aero1 = 4,
        Repose = 8,
        Esuna = 10,
        Medica1 = 10,
        Raise = 12,
        LucidDreaming = 14,
        Stone2 = 18,
        Swiftcast = 18,
        Cure2 = 30, // unlocked by quest 65977 'In Nature's Embrace'
        PresenceOfMind = 30, // unlocked by quest 66615 'Seer Folly'
        Regen = 35, // unlocked by quest 66616 'Only You Can Prevent Forest Ire'
        Cure3 = 40, // unlocked by quest 66617 'O Brother, Where Art Thou'
        Surecast = 44,
        Holy1 = 45, // unlocked by quest 66619 'Yearn for the Urn'
        Aero2 = 46,
        Rescue = 48,
        Medica2 = 50,
        Benediction = 50, // unlocked by quest 66620 'Heart of the Forest'
        AfflatusSolace = 52,
        Asylum = 52, // unlocked by quest 67256 'A Journey of Purification'
        Stone3 = 54, // unlocked by quest 67257 'The Girl with the Dragon Tissue'
        Assize = 56, // unlocked by quest 67258 'The Dark Blight Writhes'
        ThinAir = 58, // unlocked by quest 67259 'In the Wake of Death'
        Tetragrammaton = 60, // unlocked by quest 67261 'Hands of Healing'
        Stone4 = 64,
        DivineBenison = 66,
        PlenaryIndulgence = 70, // unlocked by quest 67954 'What She Always Wanted'
        Dia = 72,
        Glare1 = 72,
        AfflatusMisery = 74,
        AfflatusRapture = 76,
        Temperance = 80,
        Glare3 = 82,
        Holy3 = 82,
        Aquaveil = 86,
        LiturgyOfTheBell = 90,

        // traits
        MaimAndMend1 = 20, // passive, heal and damage increase
        Freecure = 32, // passive, cure1 can proc a buff that makes cure2 free
        MaimAndMend2 = 40, // passive, heal and damage increase
        EnhancedAsylum = 78, // passive, asylum increases healing taken
        EnhancedHealingMagic = 85, // passive, potency increase
        EnhancedDivineBenison = 88, // passive, second charge for DB
    }

    public static class Definitions
    {
        public static QuestLockEntry[] QuestsPerLevel = {
            new(30, 65977),
            new(30, 66615),
            new(35, 66616),
            new(40, 66617),
            new(45, 66619),
            new(50, 66620),
            new(52, 67256),
            new(54, 67257),
            new(56, 67258),
            new(58, 67259),
            new(60, 67261),
            new(70, 67954),
        };

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
}
