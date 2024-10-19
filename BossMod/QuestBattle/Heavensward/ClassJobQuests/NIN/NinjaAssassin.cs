namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.NIN;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 421)]
internal class NinjaAssassin(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(0x11E4, 5);
    }
}

