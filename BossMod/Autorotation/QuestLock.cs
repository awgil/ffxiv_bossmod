namespace BossMod;

// for quest-locked abilities, we maintain a sorted list of interesting quests in chain (assuming that it is slightly linear)
// since quest can't be 'uncompleted', we keep track of first uncompleted quest and only check it
class QuestLockCheck
{
    private uint[] _unlockData;
    private int _firstUncompletedIndex = 0;

    public QuestLockCheck(uint[] unlockData)
    {
        _unlockData = unlockData;
    }

    public int Progress()
    {
        while (_firstUncompletedIndex < _unlockData.Length && FFXIVClientStructs.FFXIV.Client.Game.QuestManager.IsQuestComplete(_unlockData[_firstUncompletedIndex]))
            ++_firstUncompletedIndex;
        return _firstUncompletedIndex;
    }
}
