namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.SMN;

[ZoneModuleInfo(454)]
internal class AnEgistentialCrisis(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        MoveAndWaitForCombatEnd(ws, new(-173.63245f, 23.428497f, 67.91785f)),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-81.944534f, 17.852749f, 28.01129f))
            .ThenWait(10),

        MoveAndWaitForCombatEnd(ws, new(-3.189148f, 15.807038f, -2.7008667f)),

        new QuestObjective(ws)
            .WithConnection(new Vector3(54.61206f, 11.988899f, 0.56451416f))
            .CompleteAtDestination(),

        new QuestObjective(ws)
            .Hints((_, hints) => hints.PrioritizeTargetsByOID(0x1C58, 1))
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeAll();

    private static QuestObjective MoveAndWaitForCombatEnd(WorldState ws, Vector3 destination)
        => new QuestObjective(ws)
            .WithConnection(destination)
            .With(obj =>
            {
                var arrived = false;
                obj.OnNavigationComplete += () => arrived = true;
                obj.Update += () => obj.CompleteIf(arrived && ws.Party.Player() is { InCombat: false });
            });
}
