namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.NIN;

[ZoneModuleInfo(421)]
internal class NinjaAssassin(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(0x11E4, 5);
    }
}

