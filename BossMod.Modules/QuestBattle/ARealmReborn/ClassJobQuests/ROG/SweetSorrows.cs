namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.ROG;

[ZoneModuleInfo(385)]
internal class SweetSorrows(ZoneModuleInit init) : QuestBattle(init)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .PauseForCombat(true)
            .WithConnection(new Vector3(600f, 23.94f, 453.47f))
    ];
}
