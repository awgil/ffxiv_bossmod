using BossMod.QuestBattle;

namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.ARC;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 302)]
internal class ViolatorsWillBeShot(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(404.64f, -5.45f, 68.67f))
            .PauseForCombat(false)
            .Hints((player, hints) => hints.PrioritizeTargetsByOID(0x5AB, 1))
            .CompleteOnKilled(0x5AB)
    ];
}
