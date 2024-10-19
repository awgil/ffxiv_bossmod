namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.MCH;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 425)]
internal class ThePowerOfATourney(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeTargetsByOID(0x1236, 5);
}

