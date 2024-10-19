namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 313)]
internal class VictoryInPeril(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(182.51f, 8.97f, 604.66f))
            .WithConnection(new Vector3(239.49f, 14.10f, 611.79f))
    ];
}

