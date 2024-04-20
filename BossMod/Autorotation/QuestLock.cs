namespace BossMod;

// for quest-locked abilities, we maintain a sorted list of interesting quests in chain (assuming that it is slightly linear)
// since quest can't be 'uncompleted', we keep track of first uncompleted quest and only check it
class QuestLockCheck(uint[] unlockData)
{
    private int _firstUncompletedIndex;

    public int Progress()
    {
        while (_firstUncompletedIndex < unlockData.Length && FFXIVClientStructs.FFXIV.Client.Game.QuestManager.IsQuestComplete(unlockData[_firstUncompletedIndex]))
            ++_firstUncompletedIndex;
        return _firstUncompletedIndex;
    }
}
