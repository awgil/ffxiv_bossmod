namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.MCH;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 423)]
internal class MasterOfMarksmanship(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-333.91f, 50.23f, -292.74f))
            .Hints((player, hints) => hints.PrioritizeTargetsByOID(0x10BF, 5))
            .PauseForCombat(false)
            .CompleteOnKilled(0x10BF, 3),

        new QuestObjective(ws).WithConnection(new Vector3(-258.50f, 63.77f, -291.17f)).CompleteAtDestination().PauseForCombat(false),
        new QuestObjective(ws).Hints((player, hints) => hints.PrioritizeTargetsByOID(0x10C3, 5)).CompleteOnKilled(0x10C3),
        new QuestObjective(ws).WithConnection(new Vector3(-269.74f, 63.67f, -310.10f)).CompleteAtDestination().PauseForCombat(false),
        new QuestObjective(ws).Hints((player, hints) => hints.PrioritizeTargetsByOID(0x10C3, 5)).CompleteOnKilled(0x10C3),
        new QuestObjective(ws).WithConnection(new Vector3(-351.23f, 64.37f, -318.57f)).CompleteAtDestination().PauseForCombat(false),
        new QuestObjective(ws).Hints((player, hints) => hints.PrioritizeTargetsByOID(0x10C3, 5)).CompleteOnCreated(0x10C6),

        new QuestObjective(ws).WithConnection(new Vector3(-338.41f, 37.62f, -456.20f)).PauseForCombat(false).CompleteOnKilled(0x10C6, 2).Hints((player, hints) => hints.PrioritizeTargetsByOID(0x10C6, 5)),
        new QuestObjective(ws).WithConnection(new Vector3(-310.11f, 32.31f, -446.69f)).PauseForCombat(false).CompleteOnKilled(0x10C7, 2).Hints((player, hints) => hints.PrioritizeTargetsByOID(0x10C7, 5)),

        new QuestObjective(ws).WithConnection(new Vector3(-294.63f, 5.84f, -556.29f))
    ];
}

