namespace BossMod.QuestBattle.Stormblood.MSQ;

// not a quest battle, only exists to increase priority on adds during suffering phase
[Quest(BossModuleInfo.Maturity.Contributed, 537)]
public class Tsukuyomi(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime)
    {
        foreach (var h in hints.PotentialTargets.Where(h => h.Actor.InCombat))
            h.Priority = 0;
    }
}
