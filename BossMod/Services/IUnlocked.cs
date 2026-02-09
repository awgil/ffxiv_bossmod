namespace BossMod.Services;

public interface IUnlocked
{
    public bool Check(uint questLink);
}

internal class NativeUnlocked : IUnlocked
{
    public bool Check(uint questLink)
    {
        unsafe
        {
            // see ActionManager.IsActionUnlocked
            var gameMain = FFXIVClientStructs.FFXIV.Client.Game.GameMain.Instance();
            return questLink == 0
                || Service.LuminaRow<Lumina.Excel.Sheets.TerritoryType>(gameMain->CurrentTerritoryTypeId)?.TerritoryIntendedUse.RowId == 31 // deep dungeons check is hardcoded in game
                || FFXIVClientStructs.FFXIV.Client.Game.UI.UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(questLink);
        }
    }
}

internal class MockUnlocked : IUnlocked
{
    public bool Check(uint questLink) => true;
}
