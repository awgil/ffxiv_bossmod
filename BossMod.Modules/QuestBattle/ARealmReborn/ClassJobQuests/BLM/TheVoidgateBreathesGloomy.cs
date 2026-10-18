namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.BLM;

[ZoneModuleInfo(366)]
internal class TheVoidgateBreathesGloomy(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeTargetsByOID(0x6B1);
}

