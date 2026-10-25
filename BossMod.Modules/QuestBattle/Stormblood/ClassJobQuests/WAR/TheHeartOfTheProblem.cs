namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.WAR;

[ZoneModuleInfo(266)]
internal class TheHeartOfTheProblem(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var p in hints.PotentialTargets)
        {
            p.Priority = 0;

            if (p.Actor.FindStatus(680) != null)
                p.Priority = AIHints.Enemy.PriorityInvincible;
        }
    }
}
