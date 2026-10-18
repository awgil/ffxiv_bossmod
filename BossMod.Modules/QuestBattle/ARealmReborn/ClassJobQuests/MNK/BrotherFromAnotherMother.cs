namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.MNK;

[ZoneModuleInfo(363)]
internal class BrotherFromAnotherMother(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeTargetsByOID(0x815, 0);
}

