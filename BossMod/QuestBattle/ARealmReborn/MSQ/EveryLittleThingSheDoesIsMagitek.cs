namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 358)]
internal class EveryLittleThingSheDoesIsMagitek(ZoneModuleArgs args) : QuestBattle(args)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeAll();
}

