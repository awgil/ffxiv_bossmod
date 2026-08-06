namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.ROG;

[ZoneModuleInfo(386)]
internal class CloyingVictory(WorldState ws) : QuestBattle(ws)
{
    private static QuestObjective GoKill(WorldState ws, Vector3 position, uint OID)
        => new QuestObjective(ws)
            .WithConnection(position)
            .PauseForCombat(false)
            .Hints((player, hints) => hints.PrioritizeTargetsByOID(OID, 1))
            .CompleteOnKilled(OID);

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .PauseForCombat(true)
            .WithConnection(new Vector3(-49f, 40f, 37.29f))
            .CompleteAtDestination(),
        GoKill(ws, new (-13.95f, 44f, -34.36f), 0xD8E),
        GoKill(ws, new (-13.95f, 44f, -34.36f), 0xD8C),
        GoKill(ws, new (-34.90f, 32f, 3.65f), 0xD8E),
        GoKill(ws, new (-13.95f, 44f, -34.36f), 0xD8D),
        new QuestObjective(ws)
            .PauseForCombat(false)
            .WithConnection(new Vector3(-36f, 40f, 17f))
            .CompleteAtDestination(),
        GoKill(ws, new (16f, 40f, 37f), 0xD8E),
        GoKill(ws, new (-46f, 40f, 26f), 0xD8A),
        new QuestObjective(ws)
            .Hints((player, hints) => hints.PrioritizeAll())
    ];
}
