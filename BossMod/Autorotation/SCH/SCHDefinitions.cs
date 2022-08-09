using System.Collections.Generic;

namespace BossMod.SCH
{
    public enum AID : uint
    {
        None = 0,

        // single-target damage GCDs
        Ruin1 = 17869, // L1, 1.5s cast, range 25, single-target 0/0, targets=hostile
        Broil1 = 3584, // L54, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Broil2 = 7435, // L64, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Broil3 = 16541, // L72, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Broil4 = 25865, // L82, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Bio1 = 17864, // L2, instant, range 25, single-target 0/0, targets=hostile
        Bio2 = 17865, // L26, instant, range 25, single-target 0/0, targets=hostile
        Biolysis = 16540, // L72, instant, range 25, single-target 0/0, targets=hostile, animLock=???
        Ruin2 = 17870, // L38, instant, range 25, single-target 0/0, targets=hostile, animLock=???

        // aoe damage GCDs
        ArtOfWar1 = 16539, // L46, instant, range 0, AOE circle 5/0, targets=self, animLock=???
        ArtOfWar2 = 25866, // L82, instant, range 0, AOE circle 5/0, targets=self, animLock=???

        // single-target heal GCDs
        Physick = 190, // L4, 1.5s cast, range 30, single-target 0/0, targets=self/party/friendly
        Adloquium = 185, // L30, 2.0s cast, range 30, single-target 0/0, targets=self/party/friendly

        // aoe heal GCDs
        Succor = 186, // L35, 2.0s cast, range 0, AOE circle 15/0, targets=self, animLock=???

        // summons
        SummonEos = 17215, // L4, 1.5s cast, range 0, single-target 0/0, targets=self
        SummonSelene = 17216, // L4, 1.5s cast, range 0, single-target 0/0, targets=self

        // damage oGCDs
        EnergyDrain = 167, // L45, instant, 1.0s CD (group 3), range 25, single-target 0/0, targets=hostile, animLock=???

        // heal oGCDs
        WhisperingDawn = 16537, // L20, instant, 60.0s CD (group 13), range 0, single-target 0/0, targets=self
        Lustrate = 189, // L45, instant, 1.0s CD (group 0), range 30, single-target 0/0, targets=self/party/friendly, animLock=???
        SacredSoil = 188, // L50, instant, 30.0s CD (group 5), range 30, Ground circle 10/0, targets=area, animLock=???
        Indomitability = 3583, // L52, instant, 30.0s CD (group 6), range 0, AOE circle 15/0, targets=self, animLock=???
        DeploymentTactics = 3585, // L56, instant, 120.0s CD (group 19), range 30, AOE circle 15/0, targets=self/party, animLock=???
        EmergencyTactics = 3586, // L58, instant, 15.0s CD (group 4), range 0, single-target 0/0, targets=self, animLock=???
        Excogitation = 7434, // L62, instant, 45.0s CD (group 8), range 30, single-target 0/0, targets=self/party, animLock=???
        Aetherpact = 7437, // L70, instant, 3.0s CD (group 2), range 30, single-target 0/0, targets=self/party, animLock=???
        DissolveUnion = 7869, // L70, instant, 1.0s CD (group 1), range 0, single-target 0/0, targets=self, animLock=???
        FeyBlessing = 16543, // L76, instant, 60.0s CD (group 10), range 0, single-target 0/0, targets=self, animLock=???
        Consolation = 16546, // L80, instant, 30.0s CD (group 9) (2 charges), range 0, single-target 0/0, targets=self, animLock=???

        // buff CDs
        LucidDreaming = 7562, // L14, instant, 60.0s CD (group 41), range 0, single-target 0/0, targets=self
        Swiftcast = 7561, // L18, instant, 60.0s CD (group 42), range 0, single-target 0/0, targets=self
        FeyIllumination = 16538, // L40, instant, 120.0s CD (group 21), range 0, single-target 0/0, targets=self, animLock=???
        Surecast = 7559, // L44, instant, 120.0s CD (group 43), range 0, single-target 0/0, targets=self, animLock=???
        Aetherflow = 166, // L45, instant, 60.0s CD (group 12), range 0, single-target 0/0, targets=self, animLock=???
        Dissipation = 3587, // L60, instant, 180.0s CD (group 24), range 0, single-target 0/0, targets=self, animLock=???
        ChainStratagem = 7436, // L66, instant, 120.0s CD (group 20), range 25, single-target 0/0, targets=hostile, animLock=???
        Recitation = 16542, // L74, instant, 90.0s CD (group 14), range 0, single-target 0/0, targets=self, animLock=???
        SummonSeraph = 16545, // L80, instant, 120.0s CD (group 22), range 0, single-target 0/0, targets=self, animLock=???
        Protraction = 25867, // L86, instant, 60.0s CD (group 11), range 30, single-target 0/0, targets=self/party, animLock=???
        Expedient = 25868, // L90, instant, 120.0s CD (group 23), range 0, AOE circle 15/0, targets=self, animLock=???

        // misc
        Resurrection = 173, // L12, 8.0s cast, range 30, single-target 0/0, targets=party/friendly, animLock=???
        Repose = 16560, // L8, 2.5s cast, range 30, single-target 0/0, targets=hostile
        Esuna = 7568, // L10, 1.0s cast, range 30, single-target 0/0, targets=self/party/friendly
        Rescue = 7571, // L48, instant, 120.0s CD (group 45), range 30, single-target 0/0, targets=party, animLock=???

        // pet abilities
        PetEmbrace = 802, // L1, instant, 3.0s CD (group 76), range 30, single-target 0/0, targets=self/party/friendly, animLock=???
        PetSeraphicVeil = 16548, // L80, instant, 3.0s CD (group 76), range 30, single-target 0/0, targets=self/party, animLock=???
        PetWhisperingDawn = 803, // L20, instant, 0.0s CD (group -1), range 0, AOE circle 15/0, targets=self, animLock=???
        PetAngelsWhisper = 16550, // L80, instant, 0.0s CD (group -1), range 0, AOE circle 15/0, targets=self, animLock=???
        PetFeyIllumination = 805, // L40, instant, 0.0s CD (group -1), range 0, AOE circle 15/0, targets=self, animLock=???
        PetSeraphicIllumination = 16551, // L80, instant, 0.0s CD (group -1), range 0, AOE circle 15/0, targets=self, animLock=???
        PetFeyUnion = 7438, // L70, instant, 0.0s CD (group -1), range 30, single-target 0/0, targets=party, animLock=???
        PetFeyBlessing = 16544, // L76, instant, 0.0s CD (group -1), range 0, AOE circle 20/0, targets=self, animLock=???
        PetConsolation = 16547, // L80, instant, 0.0s CD (group -1), range 0, AOE circle 20/0, targets=self, animLock=???
    }

    public enum CDGroup : int
    {
        Lustrate = 0, // 1.0 max
        DissolveUnion = 1, // 1.0 max
        Aetherpact = 2, // 3.0 max
        EnergyDrain = 3, // 1.0 max
        EmergencyTactics = 4, // 15.0 max
        SacredSoil = 5, // 30.0 max
        Indomitability = 6, // 30.0 max
        Excogitation = 8, // 45.0 max
        Consolation = 9, // 2*30.0 max
        FeyBlessing = 10, // 60.0 max
        Protraction = 11, // 60.0 max
        Aetherflow = 12, // 60.0 max
        WhisperingDawn = 13, // 60.0 max
        Recitation = 14, // 90.0 max
        DeploymentTactics = 19, // 120.0 max
        ChainStratagem = 20, // 120.0 max
        FeyIllumination = 21, // 120.0 max
        SummonSeraph = 22, // 120.0 max
        Expedient = 23, // 120.0 max
        Dissipation = 24, // 180.0 max
        LucidDreaming = 41, // 60.0 max
        Swiftcast = 42, // 60.0 max
        Surecast = 43, // 120.0 max
        Rescue = 45, // 120.0 max
        PetEmbrace = 76, // 3.0 max, shared by Embrace, Seraphic Veil
    }

    public enum MinLevel : int
    {
        // actions
        Bio1 = 2,
        SummonFairy = 4, // includes summon eos and selene
        Physick = 4,
        Repose = 8,
        Esuna = 10,
        Resurrection = 12,
        LucidDreaming = 14,
        Swiftcast = 18,
        WhisperingDawn = 20,
        Bio2 = 26,
        Adloquium = 30, // unlocked by quest 66633 'Forgotten but Not Gone'
        Succor = 35, // unlocked by quest 66634 'The Last Remnants'
        Ruin2 = 38,
        FeyIllumination = 40,
        Surecast = 44,
        AetherflowEnergyDrain = 45, // includes aetherflow & energy drain
        Lustrate = 45, // unlocked by quest 66637 'For Your Fellow Man'
        ArtOfWar1 = 46,
        Rescue = 48,
        SacredSoil = 50, // unlocked by quest 66638 'The Beast Within'
        Indomitability = 52, // unlocked by quest 67208 'Quarantine'
        Broil1 = 54, // unlocked by quest 67209 'False Friends'
        DeploymentTactics = 56, // unlocked by quest 67210 'Ooh Rah'
        EmergencyTactics = 58, // unlocked by quest 67211 'Unseen'
        Dissipation = 60, // unlocked by quest 67212 'Forward, the Royal Marines'
        Excogitation = 62,
        Broil2 = 64,
        ChainStratagem = 66,
        Aetherpact = 70, // unlocked by quest 68463 'Our Unsung Heroes'; also includes dissolve union
        Biolysis = 72,
        Broil3 = 72,
        Recitation = 74,
        FeyBlessing = 76,
        SummonSeraph = 80, // also includes consolation
        Broil4 = 82,
        ArtOfWar2 = 82,
        Protraction = 86,
        Expedient = 90,

        // traits
        MaimAndMend1 = 20, // passive, damage & healing increase
        MaimAndMend2 = 40, // passive, damage & healing increase
        EnhancedSacredSoil = 78, // passive, sacred soil now also provides hot
        EnhancedHealingMagic = 85, // passive, potency increase
        EnhancedDeploymentTactics = 88, // passive, cd reduction
    }

    public static class Definitions
    {
        public static QuestLockEntry[] QuestsPerLevel = {
            new(30, 66633),
            new(35, 66634),
            new(45, 66637),
            new(50, 66638),
            new(52, 67208),
            new(54, 67209),
            new(56, 67210),
            new(58, 67211),
            new(60, 67212),
            new(70, 68463),
        };

        public static Dictionary<ActionID, ActionDefinition> SupportedActions;
        static Definitions()
        {
            SupportedActions = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionMnd);
            SupportedActions.GCDCast(AID.Ruin1, 25, 1.5f);
            SupportedActions.GCDCast(AID.Broil1, 25, 1.5f);
            SupportedActions.GCDCast(AID.Broil2, 25, 1.5f);
            SupportedActions.GCDCast(AID.Broil3, 25, 1.5f);
            SupportedActions.GCDCast(AID.Broil4, 25, 1.5f);
            SupportedActions.GCD(AID.Bio1, 25);
            SupportedActions.GCD(AID.Bio2, 25);
            SupportedActions.GCD(AID.Biolysis, 25);
            SupportedActions.GCD(AID.Ruin2, 25);
            SupportedActions.GCD(AID.ArtOfWar1, 0);
            SupportedActions.GCD(AID.ArtOfWar2, 0);
            SupportedActions.GCDCast(AID.Physick, 30, 1.5f);
            SupportedActions.GCDCast(AID.Adloquium, 30, 2.0f);
            SupportedActions.GCDCast(AID.Succor, 0, 2.0f);
            SupportedActions.GCDCast(AID.SummonEos, 0, 1.5f);
            SupportedActions.GCDCast(AID.SummonSelene, 0, 1.5f);
            SupportedActions.OGCD(AID.EnergyDrain, 25, CDGroup.EnergyDrain, 1.0f);
            SupportedActions.OGCD(AID.WhisperingDawn, 0, CDGroup.WhisperingDawn, 60.0f);
            SupportedActions.OGCD(AID.Lustrate, 30, CDGroup.Lustrate, 1.0f);
            SupportedActions.OGCD(AID.SacredSoil, 30, CDGroup.SacredSoil, 30.0f);
            SupportedActions.OGCD(AID.Indomitability, 0, CDGroup.Indomitability, 30.0f);
            SupportedActions.OGCD(AID.DeploymentTactics, 30, CDGroup.DeploymentTactics, 120.0f);
            SupportedActions.OGCD(AID.EmergencyTactics, 0, CDGroup.EmergencyTactics, 15.0f);
            SupportedActions.OGCD(AID.Excogitation, 30, CDGroup.Excogitation, 45.0f);
            SupportedActions.OGCD(AID.Aetherpact, 30, CDGroup.Aetherpact, 3.0f);
            SupportedActions.OGCD(AID.DissolveUnion, 0, CDGroup.DissolveUnion, 1.0f);
            SupportedActions.OGCD(AID.FeyBlessing, 0, CDGroup.FeyBlessing, 60.0f);
            SupportedActions.OGCDWithCharges(AID.Consolation, 0, CDGroup.Consolation, 30.0f, 2);
            SupportedActions.OGCD(AID.LucidDreaming, 0, CDGroup.LucidDreaming, 60.0f);
            SupportedActions.OGCD(AID.Swiftcast, 0, CDGroup.Swiftcast, 60.0f);
            SupportedActions.OGCD(AID.FeyIllumination, 0, CDGroup.FeyIllumination, 120.0f);
            SupportedActions.OGCD(AID.Surecast, 0, CDGroup.Surecast, 120.0f);
            SupportedActions.OGCD(AID.Aetherflow, 0, CDGroup.Aetherflow, 60.0f);
            SupportedActions.OGCD(AID.Dissipation, 0, CDGroup.Dissipation, 180.0f);
            SupportedActions.OGCD(AID.ChainStratagem, 25, CDGroup.ChainStratagem, 120.0f);
            SupportedActions.OGCD(AID.Recitation, 0, CDGroup.Recitation, 90.0f);
            SupportedActions.OGCD(AID.SummonSeraph, 0, CDGroup.SummonSeraph, 120.0f);
            SupportedActions.OGCD(AID.Protraction, 30, CDGroup.Protraction, 60.0f);
            SupportedActions.OGCD(AID.Expedient, 0, CDGroup.Expedient, 120.0f);
            SupportedActions.GCDCast(AID.Resurrection, 30, 8.0f);
            SupportedActions.GCDCast(AID.Repose, 30, 2.5f);
            SupportedActions.GCDCast(AID.Esuna, 30, 1.0f);
            SupportedActions.OGCD(AID.Rescue, 30, CDGroup.Rescue, 120.0f);
        }
    }

    public enum SID : uint
    {
        None = 0,
        Bio1 = 179, // applied by Bio1 to target, dot
        Bio2 = 189, // applied by Bio2 to target, dot
        Biolysis = 0xFFFFFF, // TODO!
        Galvanize = 297, // applied by Adloquium to target, shield
        LucidDreaming = 1204, // applied by Lucid Dreaming to self
        Swiftcast = 167, // applied by Swiftcast to self
        Sleep = 3, // applied by Repose to target
    }
}
