namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(354)]
internal class TheLominsanWay(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(0x6EA, 1);
        hints.PrioritizeTargetsByOID(0x6E9, -1);
    }
}
