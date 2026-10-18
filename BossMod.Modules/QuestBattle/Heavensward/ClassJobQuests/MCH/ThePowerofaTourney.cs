namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.MCH;

[ZoneModuleInfo(425)]
internal class ThePowerOfATourney(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeTargetsByOID(0x1236, 5);
}

