namespace BossMod.QuestBattle.Dawntrail.MSQ;

[ZoneModuleInfo(1009)]
internal class TheWarmthOfFamily(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(328.14f, -16.60f, -318.24f))
            .CompleteOnTargetable(0x1EBCBB, true),

        QuestObjective.StandardInteract(ws, 0x1EBCBB)
            .Named("Buffalo 1"),

        QuestObjective.StandardInteract(ws, 0x1EBCC3)
            .Named("Buffalo 2"),

        new QuestObjective(ws)
            .WithConnection(new Vector3(409.03f, -16.38f, -123.60f))
            .CompleteOnKilled(0x4637)
            .Named("Buffalo 3")
    ];
}
