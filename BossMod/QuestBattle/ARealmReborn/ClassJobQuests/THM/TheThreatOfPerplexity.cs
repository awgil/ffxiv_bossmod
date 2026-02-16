using BossMod.QuestBattle;

namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.THM;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 325)]
internal class TheThreatOfPerplexity(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Hints((player, hints) => hints.PrioritizeTargetsByOID(0x2A4, 0x2A5))
    ];
}
