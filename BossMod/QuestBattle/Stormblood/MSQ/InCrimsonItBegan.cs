namespace BossMod.QuestBattle.Stormblood.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 464)]
public sealed class InCrimsonItBegan(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-33.61f, 13.63f, 112.63f))
            .WithConnection(new Vector3(11.37f, -1.31f, 60.38f))
            .WithConnection(new Vector3(65.57f, 0.00f, -3.53f))
            .WithConnection(new Vector3(76.48f, 0.31f, -73.51f))
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeAll();
}
