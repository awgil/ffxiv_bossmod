namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.PLD;

[ZoneModuleInfo(435)]
internal class BigSolleretsToFill(ZoneModuleInit init) : QuestBattle(init)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).Hints((player, hints) => hints.PrioritizeTargetsByOID(0x11D2, 5))
    ];
}

