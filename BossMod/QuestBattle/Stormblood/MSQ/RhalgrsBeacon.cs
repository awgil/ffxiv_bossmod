namespace BossMod.QuestBattle.Stormblood.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 466)]
internal class RhalgrsBeacon(ZoneModuleArgs args) : QuestBattle(args)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws, ActionDefinitions defs) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-184.57f, 40.62f, -10.64f))
            .WithConnection(new Vector3(-166.68f, 39.10f, 163.64f))
    ];
}
