using Dalamud.Game.ClientState.Conditions;

namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.RDM;

[ZoneModuleInfo(461)] // Territory 676, QuestBattle 166 (JobRdm650_02585).
internal class Nightkin(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        MoveAndWaitForCombatEnd(ws, new(296.57343f, -23.999998f, -46.911057f)),
        MoveAndWaitForCombatEnd(ws, new(252.63516f, -24f, -30.802874f)),
        MoveAndWaitForCombatEnd(ws, new(292.96347f, -40.000004f, -58.68649f)),

        SearchBooks(ws, 6828551, new(368.9174f, -39.0174f, -67.61273f)),
        SearchBooks(ws, 6828550, new(364.1259f, -39.0174f, -61.63129f)),
        SearchBooks(ws, 6828552, new(376.7352f, -39f, -72.19662f)),
        SearchBooks(ws, 6828553, new(376.9069f, -39f, -47.19531f)),
        SearchBooks(ws, 6828554, new(384.6841f, -39f, -51.21257f)),

        new QuestObjective(ws)
            .Named("Wait for voidsphere")
            .With(obj => obj.Update += () => obj.CompleteIf(ws.Actors.Any(a => a.OID == 7075))),

        MoveAndWaitForCombatEnd(ws, new(368.9174f, -39.0174f, -67.61273f)),
        SearchBooks(ws, 6828555, new(376.36014f, -39f, -60.74992f))
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

    private static QuestObjective SearchBooks(WorldState ws, uint layoutID, Vector3 destination)
        => new QuestObjective(ws)
            .WithConnection(destination)
            .With(obj =>
            {
                var arrived = false;
                var interactionRequested = false;
                Actor? target = null;
                DateTime? completeAfter = null;
                obj.OnNavigationComplete += () => arrived = true;
                obj.AddAIHints += (player, hints) =>
                {
                    if (!arrived || !InteractionIdle(ws.Party.Player()) || obj.Completed)
                        return;

                    if (!interactionRequested && (target == null || target.IsDestroyed || !target.IsTargetable))
                        target = ws.Actors.FirstOrDefault(a => a.OID == 2007939 && a.LayoutID == layoutID && a.IsTargetable);
                    if (target == null || target.IsDestroyed || !target.IsTargetable)
                        return;

                    hints.GoalZones.Add(AIHints.GoalSingleTarget(target.Position, 1));
                    if (player.DistanceToHitbox(target) <= 2)
                    {
                        hints.InteractWithTarget = target;
                        interactionRequested = true;
                    }
                };

                obj.Update += () =>
                {
                    var current = target != null ? ws.Actors.Find(target.InstanceID) : null;
                    var unavailable = target is { OID: 2007939 } && target.LayoutID == layoutID
                        && (current == target ? !target.IsTargetable : current == null && target.IsDestroyed);
                    if (!interactionRequested || !InteractionIdle(ws.Party.Player()) || !unavailable)
                    {
                        completeAfter = null;
                        return;
                    }

                    completeAfter ??= ws.FutureTime(0.2f);
                    obj.CompleteIf(ws.CurrentTime >= completeAfter.Value);
                };
            });

    private static bool InteractionIdle(Actor? player)
        => player is { InCombat: false, CastInfo: null }
            && Service.Condition is { } condition
            && !condition[ConditionFlag.Casting]
            && !condition[ConditionFlag.OccupiedInEvent]
            && !condition[ConditionFlag.OccupiedInQuestEvent];
}
