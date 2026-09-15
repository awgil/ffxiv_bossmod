namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.SCH;

[ZoneModuleInfo(404)]
internal class Quarantine(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeTargetsByOID(0x1231, 5);
}

