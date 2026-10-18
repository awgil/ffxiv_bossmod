namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.LNC;

[ZoneModuleInfo(307)]
internal class ADangerousProposition(ZoneModuleInit init) : QuestBattle(init)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(304.20f, -1.18f, -285.26f))
            .Hints((player, hints) => hints.PrioritizeAll())
            .CompleteOnKilled(0x21F)
    ];
}
