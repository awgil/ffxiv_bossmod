namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(351)]
internal class NotoriousBiggs(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PrioritizeAll();
    }
}

