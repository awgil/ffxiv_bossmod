namespace BossMod.QuestBattle.Heavensward.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 167)]
public sealed class ASpectacleForTheAges(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.FindStatus(350) == null ? 0 : 1;
    }
}
