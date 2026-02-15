using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace BossMod.Mocks;

internal class MockUnlockState : IUnlockState
{
    public event IUnlockState.UnlockDelegate? Unlock;

    public bool IsActionUnlocked(Lumina.Excel.Sheets.Action row) => true;
    public bool IsAetherCurrentCompFlgSetUnlocked(AetherCurrentCompFlgSet row) => true;
    public bool IsAetherCurrentUnlocked(AetherCurrent row) => true;
    public bool IsAozActionUnlocked(AozAction row) => true;
    public bool IsBannerBgUnlocked(BannerBg row) => true;
    public bool IsBannerConditionUnlocked(BannerCondition row) => true;
    public bool IsBannerDecorationUnlocked(BannerDecoration row) => true;
    public bool IsBannerFacialUnlocked(BannerFacial row) => true;
    public bool IsBannerFrameUnlocked(BannerFrame row) => true;
    public bool IsBannerTimelineUnlocked(BannerTimeline row) => true;
    public bool IsBuddyActionUnlocked(BuddyAction row) => true;
    public bool IsBuddyEquipUnlocked(BuddyEquip row) => true;
    public bool IsCharaMakeCustomizeUnlocked(CharaMakeCustomize row) => true;
    public bool IsChocoboTaxiStandUnlocked(ChocoboTaxiStand row) => true;
    public bool IsCompanionUnlocked(Companion row) => true;
    public bool IsCraftActionUnlocked(CraftAction row) => true;
    public bool IsCSBonusContentTypeUnlocked(CSBonusContentType row) => true;
    public bool IsEmjCostumeUnlocked(EmjCostume row) => true;
    public bool IsEmjVoiceNpcUnlocked(EmjVoiceNpc row) => true;
    public bool IsEmoteUnlocked(Emote row) => true;
    public bool IsGeneralActionUnlocked(GeneralAction row) => true;
    public bool IsGlassesUnlocked(Glasses row) => true;
    public bool IsHowToUnlocked(HowTo row) => true;
    public bool IsInstanceContentUnlocked(InstanceContent row) => true;
    public bool IsItemUnlockable(Item row) => true;
    public bool IsItemUnlocked(Item row) => true;
    public bool IsLeveCompleted(Leve row) => true;
    public bool IsMcGuffinUnlocked(McGuffin row) => true;
    public bool IsMJILandmarkUnlocked(MJILandmark row) => true;
    public bool IsMKDLoreUnlocked(MKDLore row) => true;
    public bool IsMountUnlocked(Mount row) => true;
    public bool IsNotebookDivisionUnlocked(NotebookDivision row) => true;
    public bool IsOrchestrionUnlocked(Orchestrion row) => true;
    public bool IsOrnamentUnlocked(Ornament row) => true;
    public bool IsPerformUnlocked(Perform row) => true;
    public bool IsPublicContentUnlocked(PublicContent row) => true;
    public bool IsQuestCompleted(Quest row) => true;
    public bool IsRecipeUnlocked(Recipe row) => true;
    public bool IsRowRefUnlocked(RowRef rowRef) => true;
    public bool IsRowRefUnlocked<T>(RowRef<T> rowRef) where T : struct, IExcelRow<T> => true;
    public bool IsSecretRecipeBookUnlocked(SecretRecipeBook row) => true;
    public bool IsTraitUnlocked(Trait row) => true;
    public bool IsTripleTriadCardUnlocked(TripleTriadCard row) => true;
    public bool IsUnlockLinkUnlocked(uint unlockLink) => true;
    public bool IsUnlockLinkUnlocked(ushort unlockLink) => true;
}
