namespace BossMod.QuestBattle.Heavensward.MSQ;

[ZoneModuleInfo(167)]
public sealed class ASpectacleForTheAges(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.FindStatus(350) == null ? 0 : 1;
    }
}
