namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.PLD;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 435)]
internal class BigSolleretsToFill(ZoneModuleArgs args) : QuestBattle(args)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws, ActionDefinitions defs) => [
        new QuestObjective(ws).Hints((player, hints) => hints.PrioritizeTargetsByOID(0x11D2, 5))
    ];
}

