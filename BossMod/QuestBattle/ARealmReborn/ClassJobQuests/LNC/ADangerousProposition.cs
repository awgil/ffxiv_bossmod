using BossMod.QuestBattle;

namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.LNC;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 307)]
internal class ADangerousProposition(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(304.20f, -1.18f, -285.26f))
            .Hints((player, hints) => hints.PrioritizeAll())
            .CompleteOnKilled(0x21F)
    ];
}
