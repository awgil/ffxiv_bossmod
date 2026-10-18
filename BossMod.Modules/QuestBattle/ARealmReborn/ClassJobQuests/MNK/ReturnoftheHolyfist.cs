namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.MNK;

[ZoneModuleInfo(323)]
internal class ReturnOfTheHolyfist(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeAll();
}
