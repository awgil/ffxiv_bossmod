namespace BossMod.QuestBattle.Shadowbringers.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 681)]
public class LegendsOfTheNotSoHiddenTemple(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws)
    {
        // executioners patrolling the perimeter of a square CCW - once one enters this lane, they won't have LoS on us and the other will be far behind
        static bool inRoom1SafeLane(WPos pos) => pos.InRect(new(15, 15), new WDir(0, -1), 12, 12, 2);

        var preStealth1 = new QuestObjective(ws)
            .WithConnection(new Vector3(14.57f, 0.07f, 40.00f))
            .With(obj =>
            {
                var waitForWorthlessNPCs = DateTime.MaxValue;
                obj.OnNavigationComplete += () =>
                {
                    Service.Log($"delaying npcs");
                    waitForWorthlessNPCs = World.FutureTime(8);
                };
                obj.Update += () =>
                {
                    if (obj.Completed || World.CurrentTime < waitForWorthlessNPCs)
                        return;

                    var executioners = World.Actors.Where(x => x.OID == 0x2954 && !x.IsDead);
                    obj.CompleteIf(executioners.Any(ex => inRoom1SafeLane(ex.Position)));
                };
            });

        return [
            preStealth1,
            new QuestObjective(ws)
                .WithConnection(new Vector3(-13.92f, 0.67f, -18.39f))
                .WithInteract(0x1EAC5A)
                .CompleteOnState7(0x1EAC5A),

            new QuestObjective(ws)
                .WithConnection(new Vector3(-11.89f, 1.12f, -81.84f))
                .WithInteract(0x1EAC5B)
                .CompleteOnState7(0x1EAC5B),

            new QuestObjective(ws)
                .WithConnection(new Vector3(8.24f, 2.15f, -98.69f))
                .With(obj => {
                    obj.OnActorCombatChanged += (act) => obj.CompleteIf(act.OID == 0x2955 && act.InCombat);
                })
                .PauseForCombat(false),

            new QuestObjective(ws)
                .WithConnection(new Vector3(8.18f, 2.42f, -137.54f))
                .WithInteract(0x1EAC5E, allowInCombat: true)
                .PauseForCombat(false)
                .CompleteOnState7(0x1EAC5E),

            new QuestObjective(ws)
                .WithConnection(new Vector3(8.34f, 3.80f, -173.86f))
                .With(obj => {
                    obj.OnStatusGain += (act, status) => obj.CompleteIf(act.OID == 0 && status.ID == 19);
                }),

            new QuestObjective(ws)
                .WithConnection(new Waypoint(new(7.5f, 6, -213), false))
                .WithConnection(new Waypoint(new(2.3f, 6, -228), false))
                .WithConnection(new Waypoint(new(-7.6f, 6, -233), false))
                .WithConnection(new Waypoint(new(-12.6f, 6, -218), false))
                .With(obj => {
                    obj.OnStatusLose += (act, status) => obj.CompleteIf(act.OID == 0 && status.ID == 19);
                }),

            new QuestObjective(ws)
                .WithConnection(new Vector3(168.53f, 2.53f, -231.32f))
                .ThenWait(3f),

            new QuestObjective(ws)
                .WithConnection(new Vector3(159.91f, -26.11f, -398.98f))
                .WithConnection(new Waypoint(new Vector3(159.99f, -100.82f, -415.27f), false))
                .With(obj => {
                    obj.OnDirectorUpdate += (op) => {
                        // called when the npcs are finished yapping in the tunnel
                        obj.CompleteIf(op.Param1 == 22672);
                    };
                }),

            new QuestObjective(ws)
                .WithConnection(new Vector3(162.47f, -106.21f, -489.79f))
                .WithConnection(new Vector3(199.61f, -105.36f, -491.51f))
                .WithInteract(0x1EAC5C)
                .CompleteOnState7(0x1EAC5C),

            new QuestObjective(ws)
                .WithConnection(new Vector3(230.15f, -105.26f, -491.60f))
                .Hints((player, hints) => {
                    hints.PrioritizeTargetsByOID([0x2952, 0x2953]);
                })
                .With(obj => {
                    obj.OnActorTargetableChanged += (act) => obj.CompleteIf(act.OID == 0x2952 && !act.IsTargetable);
                }),

            new QuestObjective(ws)
                .WithConnection(new Vector3(232.41f, -105.31f, -511.72f))
                .PauseForCombat(false)
                .WithInteract(0x1EAC5D, allowInCombat: true)
                .CompleteOnState7(0x1EAC5D),

            new QuestObjective(ws)
                .WithConnection(new Vector3(231.8f, -105.31f, -547.7f))
                .PauseForCombat(false)
                .WithInteract(0x1EAC5F, allowInCombat: true)
        ];
    }

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            if (h.Actor.OID == 0x2955)
                h.Priority = AIHints.Enemy.PriorityForbidFully;
    }
}
