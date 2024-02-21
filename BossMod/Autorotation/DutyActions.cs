﻿using System.Collections.Generic;
using Lumina.Excel.GeneratedSheets;

namespace BossMod
{
    public enum LostActionID : uint
    {
        None = 0,
        LostParalyzeIII = 20701,
        LostBanishIII = 20702,
        LostManawall = 20703,
        LostDispel = 20704,
        LostStealth = 20705,
        LostSpellforge = 20706,
        LostSteelsting = 20707,
        LostSwift = 20708,
        LostProtect = 20709,
        LostShell = 20710,
        LostReflect = 20711,
        LostStoneskin = 20712,
        LostBravery = 20713,
        LostFocus = 20714,
        LostFontofMagic = 20715,
        LostFontofSkill = 20716,
        LostFontofPower = 20717,
        LostSlash = 20718,
        LostDeath = 20719,
        BannerNobleEnds = 20720,
        BannerHonoredSacrifice = 20721,
        BannerTirelessConviction = 20722,
        BannerFirmResolve = 20723,
        BannerSolemnClarity = 20724,
        BannerHonedAcuity = 20725,
        LostCure = 20726,
        LostCureII = 20727,
        LostCureIII = 20728,
        LostCureIV = 20729,
        LostArise = 20730,
        LostIncense = 20731,
        LostFairTrade = 20732,
        Mimic = 20733,
        DynamisDice = 20734,
        ResistancePhoenix = 20735,
        ResistanceReraiser = 20736,
        ResistancePotionKit = 20737,
        ResistanceEtherKit = 20738,
        ResistanceMedikit = 20739,
        ResistancePotion = 20740,
        EssenceAetherweaver = 20741,
        EssenceMartialist = 20742,
        EssenceSavior = 20743,
        EssenceVeteran = 20744,
        EssencePlatebearer = 20745,
        EssenceGuardian = 20746,
        EssenceOrdained = 20747,
        EssenceSkirmisher = 20748,
        EssenceWatcher = 20749,
        EssenceProfane = 20750,
        EssenceIrregular = 20751,
        EssenceBreathtaker = 20752,
        EssenceBloodsucker = 20753,
        EssenceBeast = 20754,
        EssenceTemplar = 20755,
        DeepEssenceAetherweaver = 20756,
        DeepEssenceMartialist = 20757,
        DeepEssenceSavior = 20758,
        DeepEssenceVeteran = 20759,
        DeepEssencePlatebearer = 20760,
        DeepEssenceGuardian = 20761,
        DeepEssenceOrdained = 20762,
        DeepEssenceSkirmisher = 20763,
        DeepEssenceWatcher = 20764,
        DeepEssenceProfane = 20765,
        DeepEssenceIrregular = 20766,
        DeepEssenceBreathtaker = 20767,
        DeepEssenceBloodsucker = 20768,
        DeepEssenceBeast = 20769,
        DeepEssenceTemplar = 20770,
        LostPerception = 22344,
        LostSacrifice = 22345,
        PureEssenceGambler = 22346,
        PureEssenceElder = 22347,
        PureEssenceDuelist = 22348,
        PureEssenceFiendhunter = 22349,
        PureEssenceIndomitable = 22350,
        PureEssenceDivine = 22351,
        LostFlareStar = 22352,
        LostRendArmor = 22353,
        LostSeraphStrike = 22354,
        LostAethershield = 22355,
        LostDervish = 22356,
        Lodestone = 23907,
        LostStoneskinII = 23908,
        LostBurst = 23909,
        LostRampage = 23910,
        LightCurtain = 23911,
        LostReraise = 23912,
        LostChainspell = 23913,
        LostAssassination = 23914,
        LostProtectII = 23915,
        LostShellII = 23916,
        LostBubble = 23917,
        LostImpetus = 23918,
        LostExcellence = 23919,
        LostFullCure = 23920,
        LostBloodRage = 23921,
        ResistanceElixir = 23922,
    }

    // TODO: add eureka actions...i guess...if anybody cares about that
    public static class DutyActions
    {
        // lost actions have their own bozja-specific action ID that are shown by the SimpleTweaks action ID tweak,
        // from 1-99; the list below is in that order.
        // the Reminiscence status effect reflects what lost actions a player has equipped; the value of the status param
        // is (duty action 1 ID) + (duty action 2 ID << 8)
        private static LostActionID[] ALL =
        [
            LostActionID.None,
            LostActionID.LostParalyzeIII,
            LostActionID.LostBanishIII,
            LostActionID.LostManawall,
            LostActionID.LostDispel,
            LostActionID.LostStealth,
            LostActionID.LostSpellforge,
            LostActionID.LostSteelsting,
            LostActionID.LostSwift,
            LostActionID.LostProtect,
            LostActionID.LostShell,
            LostActionID.LostReflect,
            LostActionID.LostStoneskin,
            LostActionID.LostBravery,
            LostActionID.LostFocus,
            LostActionID.LostFontofMagic,
            LostActionID.LostFontofSkill,
            LostActionID.LostFontofPower,
            LostActionID.LostSlash,
            LostActionID.LostDeath,
            LostActionID.BannerNobleEnds,
            LostActionID.BannerHonoredSacrifice,
            LostActionID.BannerTirelessConviction,
            LostActionID.BannerFirmResolve,
            LostActionID.BannerSolemnClarity,
            LostActionID.BannerHonedAcuity,
            LostActionID.LostCure,
            LostActionID.LostCureII,
            LostActionID.LostCureIII,
            LostActionID.LostCureIV,
            LostActionID.LostArise,
            LostActionID.LostIncense,
            LostActionID.LostFairTrade,
            LostActionID.Mimic,
            LostActionID.DynamisDice,
            LostActionID.ResistancePhoenix,
            LostActionID.ResistanceReraiser,
            LostActionID.ResistancePotionKit,
            LostActionID.ResistanceEtherKit,
            LostActionID.ResistanceMedikit,
            LostActionID.ResistancePotion,
            LostActionID.EssenceAetherweaver,
            LostActionID.EssenceMartialist,
            LostActionID.EssenceSavior,
            LostActionID.EssenceVeteran,
            LostActionID.EssencePlatebearer,
            LostActionID.EssenceGuardian,
            LostActionID.EssenceOrdained,
            LostActionID.EssenceSkirmisher,
            LostActionID.EssenceWatcher,
            LostActionID.EssenceProfane,
            LostActionID.EssenceIrregular,
            LostActionID.EssenceBreathtaker,
            LostActionID.EssenceBloodsucker,
            LostActionID.EssenceBeast,
            LostActionID.EssenceTemplar,
            LostActionID.DeepEssenceAetherweaver,
            LostActionID.DeepEssenceMartialist,
            LostActionID.DeepEssenceSavior,
            LostActionID.DeepEssenceVeteran,
            LostActionID.DeepEssencePlatebearer,
            LostActionID.DeepEssenceGuardian,
            LostActionID.DeepEssenceOrdained,
            LostActionID.DeepEssenceSkirmisher,
            LostActionID.DeepEssenceWatcher,
            LostActionID.DeepEssenceProfane,
            LostActionID.DeepEssenceIrregular,
            LostActionID.DeepEssenceBreathtaker,
            LostActionID.DeepEssenceBloodsucker,
            LostActionID.DeepEssenceBeast,
            LostActionID.DeepEssenceTemplar,
            LostActionID.LostPerception,
            LostActionID.LostSacrifice,
            LostActionID.PureEssenceGambler,
            LostActionID.PureEssenceElder,
            LostActionID.PureEssenceDuelist,
            LostActionID.PureEssenceFiendhunter,
            LostActionID.PureEssenceIndomitable,
            LostActionID.PureEssenceDivine,
            LostActionID.LostFlareStar,
            LostActionID.LostRendArmor,
            LostActionID.LostSeraphStrike,
            LostActionID.LostAethershield,
            LostActionID.LostDervish,
            LostActionID.Lodestone,
            LostActionID.LostStoneskinII,
            LostActionID.LostBurst,
            LostActionID.LostRampage,
            LostActionID.LightCurtain,
            LostActionID.LostReraise,
            LostActionID.LostChainspell,
            LostActionID.LostAssassination,
            LostActionID.LostProtectII,
            LostActionID.LostShellII,
            LostActionID.LostBubble,
            LostActionID.LostImpetus,
            LostActionID.LostExcellence,
            LostActionID.LostFullCure,
            LostActionID.LostBloodRage,
            LostActionID.ResistanceElixir,
        ];

        public static void Register(ref Dictionary<ActionID, ActionDefinition> res)
        {
            var actions = Service.LuminaGameData!.GetExcelSheet<Action>()!;
            foreach (var actionID in ALL)
            {
                var actSheet = actions.GetRow((uint)actionID)!;
                // this is not exactly accurate for Rend and Seraph Strike because they're dash actions and the
                // animation lock is increased with distance from target, just like Onslaught
                (var actType, var animLock) =
                    actSheet.ActionCategory.Row == 5 ? (ActionType.Item, 1.100f) : (ActionType.Spell, 0.600f);
                var actId = new ActionID(actType, (uint)actionID);
                res[actId] = new(
                    actSheet.Range,
                    actSheet.Cast100ms * 0.1f,
                    actSheet.CooldownGroup - 1,
                    actSheet.Recast100ms * 0.1f,
                    System.Math.Max((int)actSheet.MaxCharges, 1),
                    animLock
                );
            }
        }

        public static uint GetRealIdFromBozjaId(uint id) => (uint)ALL[id];
    }
}
