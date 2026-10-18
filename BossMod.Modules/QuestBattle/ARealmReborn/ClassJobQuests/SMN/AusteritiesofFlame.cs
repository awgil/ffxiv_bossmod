namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.SMN;

[ZoneModuleInfo(372)]
internal class AusteritiesOfFlame(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeTargetsByOID(0x5EC, 5);
}

