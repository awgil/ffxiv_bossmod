namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.AST;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 412)]
internal class SlingsAndArrows(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-36.12f, 40.34f, -25.96f))
            .WithConnection(new Vector3(-46.72f, 40.00f, 60.58f))
            .With(obj => {
                obj.AddAIHints += (player, hints) => {
                    hints.InteractWithTarget = World.Actors.FirstOrDefault(x => x.OID == 0x1E9B5B && x.Position.AlmostEqual(new WPos(-40.36f, 64.93f), 5));
                };
            })
            .CompleteOnKilled(0x12A0),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-14.61f, 32.00f, -35.02f))
            .CompleteAtDestination(),

        new QuestObjective(ws)
            .WithInteract(0x1E9C1C)
            .CompleteOnKilled(0x12A0),

        new QuestObjective(ws)
            .WithConnection(new Vector3(12.76f, 32.00f, 36.32f))
            .CompleteAtDestination(),

        new QuestObjective(ws)
            .WithInteract(0x1E9C1D)
            .CompleteOnKilled(0x12A0),

        new QuestObjective(ws)
            .With(obj => {
                obj.OnActorTargetableChanged += (actor) => obj.CompleteIf(actor.OID == 0x12A1 && !actor.IsTargetable);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-173.99f, 41.00f, 179.72f))
            .PauseForCombat(false)
    ];
}

