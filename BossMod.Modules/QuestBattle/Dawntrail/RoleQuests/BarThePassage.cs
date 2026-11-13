using Dalamud.Game.ClientState.Conditions;

namespace BossMod.QuestBattle.Dawntrail.RoleQuests;

[ZoneModuleInfo(1016, territoryID: 1246)]
internal class BarThePassage(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws)
    {
        List<QuestObjective> objectives =
        [
            new QuestObjective(ws)
                .Named("Wait for opening event")
                .With(obj =>
                {
                    obj.Update += () => obj.CompleteIf(Service.Condition?[ConditionFlag.SufferingStatusAffliction63] == false);
                    obj.OnConditionChange += (flag, value) => obj.CompleteIf(flag == ConditionFlag.SufferingStatusAffliction63 && !value);
                })
        ];

        Vector3[] points =
        [
            new(0f, 0f, -300f),
            new(0f, 0f, -300f),
            new(0f, 0f, -300f),
            new(0f, 0f, -267f),
            new(0f, 0f, 81f),
            new(0f, 0f, 106f),
            new(0f, 0f, 364f),
        ];
        foreach (var point in points)
        {
            for (var i = 0; i < 3; ++i)
                objectives.Add(WaitThenCombatClear(ws));

            objectives.Add(new QuestObjective(ws)
                .WithConnection(point)
                .CompleteAtDestination());
        }

        objectives.Add(new QuestObjective(ws)
            .Named("Engage final enemy")
            .Hints((_, hints) => hints.PrioritizeTargetsByOID(18032, 1)));

        return objectives;
    }

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        if (CurrentObjectiveIndex > 0)
            hints.PrioritizeAll();
    }

    private static QuestObjective WaitThenCombatClear(WorldState ws)
        => new QuestObjective(ws)
            .Named("Wait 5 seconds, then for combat to end")
            .With(obj =>
            {
                DateTime? until = null;
                obj.AddAIHints += (_, _) => until ??= ws.FutureTime(5);
                obj.Update += () =>
                {
                    until ??= ws.FutureTime(5);
                    obj.CompleteIf(ws.CurrentTime >= until.Value && ws.Party.Player() is { InCombat: false });
                };
            });
}
