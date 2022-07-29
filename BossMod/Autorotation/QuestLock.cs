namespace BossMod
{
    // to deal with quest-locked abilities, we use the following strategy: we auto-generate sorted list of level/quest-id pairs, then find first non-completed quest and cap effective level to 1 below
    // this avoids having to deal with nasty permutations
    // since quest can't be 'uncompleted', we keep track of first uncompleted quest and only check it
    class QuestLockCheck
    {
        private (int Level, uint QuestID)[] _unlockData;
        private int _firstUncompletedIndex = 0;

        public QuestLockCheck((int Level, uint QuestID)[] unlockData)
        {
            _unlockData = unlockData;
        }

        public int AdjustLevel(int level)
        {
            while (_firstUncompletedIndex < _unlockData.Length && _unlockData[_firstUncompletedIndex].Level <= level && FFXIVClientStructs.FFXIV.Client.Game.QuestManager.IsQuestComplete(_unlockData[_firstUncompletedIndex].QuestID))
                ++_firstUncompletedIndex;
            return _firstUncompletedIndex < _unlockData.Length && _unlockData[_firstUncompletedIndex].Level <= level ? _unlockData[_firstUncompletedIndex].Level - 1 : level;
        }
    }
}
