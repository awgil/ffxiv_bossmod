namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 354)]
internal class TheLominsanWay(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(0x6EA, 1);
        hints.PrioritizeTargetsByOID(0x6E9, -1);
    }
}
