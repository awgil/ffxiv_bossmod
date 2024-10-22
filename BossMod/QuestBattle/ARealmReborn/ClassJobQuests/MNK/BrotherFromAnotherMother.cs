namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.MNK;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 363)]
internal class BrotherFromAnotherMother(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeTargetsByOID(0x815, 0);
}

