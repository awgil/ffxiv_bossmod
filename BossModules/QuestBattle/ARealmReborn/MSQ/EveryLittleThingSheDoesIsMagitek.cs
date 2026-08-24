namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(358)]
internal class EveryLittleThingSheDoesIsMagitek(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeAll();
}

