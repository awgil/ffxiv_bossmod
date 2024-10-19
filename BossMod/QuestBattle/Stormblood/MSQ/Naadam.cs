namespace BossMod.QuestBattle.Stormblood.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 246)]
public sealed class Naadam(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(307.38f, 0.89f, 23.38f))
            .WithConnection(new Vector3(352.01f, -1.45f, 288.59f))
            .PauseForCombat(false)
            .CompleteAtDestination()
    ];
}
