namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.PLD;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 435)]
internal class BigSolleretsToFill(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).Hints((player, hints) => hints.PrioritizeTargetsByOID(0x11D2, 5))
    ];
}

