namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(337)]
internal class OhCaptainMyCaptain(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeTargetsByOID(0x3BC, 2);
}
