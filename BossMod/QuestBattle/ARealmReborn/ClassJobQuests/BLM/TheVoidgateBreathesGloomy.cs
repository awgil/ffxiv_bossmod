namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.BLM;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 366)]
internal class TheVoidgateBreathesGloomy(ZoneModuleArgs args) : QuestBattle(args)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeTargetsByOID(0x6B1);
}

