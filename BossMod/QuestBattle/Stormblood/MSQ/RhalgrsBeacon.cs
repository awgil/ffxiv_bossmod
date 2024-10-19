namespace BossMod.QuestBattle.Stormblood.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 466)]
internal class RhalgrsBeacon(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-184.57f, 40.62f, -10.64f))
            .WithConnection(new Vector3(-166.68f, 39.10f, 163.64f))
    ];
}
