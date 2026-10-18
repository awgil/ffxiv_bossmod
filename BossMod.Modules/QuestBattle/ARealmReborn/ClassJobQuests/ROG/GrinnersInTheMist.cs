namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.ROG;

[ZoneModuleInfo(384)]
internal class GrinnersInTheMist(ZoneModuleInit init) : QuestBattle(init)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .PauseForCombat(false)
            .WithConnection(new Vector3(0f, 16.35f, 13.28f))
            .CompleteAtDestination(),

        new QuestObjective(ws)
            .Hints((player, hints) => hints.PrioritizeAll())
    ];
}
