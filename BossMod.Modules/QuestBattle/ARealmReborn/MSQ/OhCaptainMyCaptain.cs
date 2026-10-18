namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(337)]
internal class OhCaptainMyCaptain(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeTargetsByOID(0x3BC, 2);
}
