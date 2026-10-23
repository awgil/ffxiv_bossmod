namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.PGL;

[ZoneModuleInfo(322)]
internal class ReturnOfTheHolyfist(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var p in hints.PotentialTargets)
            // phase 1: force automnk to use standard rotation
            if (p.Actor.Level == 20)
                p.ForcePriority(0);
    }
}
