using Lumina.Excel.GeneratedSheets;

namespace BossMod
{
    // elements of the bozja holster
    public enum BozjaHolsterID : byte
    {
        None,
        LostParalyze,
        LostBanish,
        LostManawall,
        LostDispel,
        LostStealth,
        LostSpellforge,
        LostSteelsting,
        LostSwift,
        LostProtect1,
        LostShell1,
        LostReflect,
        LostStoneskin1,
        LostBravery,
        LostFocus,
        LostFontOfMagic,
        LostFontOfSkill,
        LostFontOfPower,
        LostSlash,
        LostDeath,
        BannerNobleEnds,
        BannerHonoredSacrifice,
        BannerTirelessConviction,
        BannerFirmResolve,
        BannerSolemnClarity,
        BannerHonedAcuity,
        LostCure1,
        LostCure2,
        LostCure3,
        LostCure4,
        LostArise,
        LostIncense,
        LostFairTrade,
        Mimic,
        DynamisDice,
        ResistancePhoenix,
        ResistanceReraiser,
        ResistancePotionKit,
        ResistanceEtherKit,
        ResistanceMedikit,
        ResistancePotion,
        EssenceAetherweaver,
        EssenceMartialist,
        EssenceSavior,
        EssenceVeteran,
        EssencePlatebearer,
        EssenceGuardian,
        EssenceOrdained,
        EssenceSkirmisher,
        EssenceWatcher,
        EssenceProfane,
        EssenceIrregular,
        EssenceBreathtaker,
        EssenceBloodsucker,
        EssenceBeast,
        EssenceTemplar,
        DeepEssenceAetherweaver,
        DeepEssenceMartialist,
        DeepEssenceSavior,
        DeepEssenceVeteran,
        DeepEssencePlatebearer,
        DeepEssenceGuardian,
        DeepEssenceOrdained,
        DeepEssenceSkirmisher,
        DeepEssenceWatcher,
        DeepEssenceProfane,
        DeepEssenceIrregular,
        DeepEssenceBreathtaker,
        DeepEssenceBloodsucker,
        DeepEssenceBeast,
        DeepEssenceTemplar,
        LostPerception,
        LostSacrifice,
        PureEssenceGambler,
        PureEssenceElder,
        PureEssenceDuelist,
        PureEssenceFiendhunter,
        PureEssenceIndomitable,
        PureEssenceDivine,
        LostFlareStar,
        LostRendArmor,
        LostSeraphStrike,
        LostAethershield,
        LostDervish,
        Lodestone,
        LostStoneskin2,
        LostBurst,
        LostRampage,
        LightCurtain,
        LostReraise,
        LostChainspell,
        LostAssassination,
        LostProtect2,
        LostShell2,
        LostBubble,
        LostImpetus,
        LostExcellence,
        LostFullCure,
        LostBloodRage,
        ResistanceElixir,

        Count
    }

    // holster -> action id mapping
    public static class BozjaActionID
    {
        private static ActionID[] _actions = new ActionID[(int)BozjaHolsterID.Count];

        public static ActionID Get(BozjaHolsterID id) => _actions[(int)id];

        static BozjaActionID()
        {
            var sheet = Service.LuminaGameData?.GetExcelSheet<MYCTemporaryItem>();
            for (int i = 0; i < _actions.Length; i++)
            {
                var row = sheet?.GetRow((uint)i);
                if (row != null)
                {
                    _actions[i] = new(ActionType.Spell, row.Action.Row);
                }
            }
        }
    }
}
