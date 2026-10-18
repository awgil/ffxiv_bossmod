namespace BossMod.QuestBattle.Stormblood.MSQ;

[ZoneModuleInfo(466)]
internal class RhalgrsBeacon(ZoneModuleInit init) : QuestBattle(init)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-184.57f, 40.62f, -10.64f))
            .WithConnection(new Vector3(-166.68f, 39.10f, 163.64f))
    ];
}
