namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(358)]
internal class EveryLittleThingSheDoesIsMagitek(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeAll();
}

