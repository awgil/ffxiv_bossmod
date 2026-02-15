namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.BLM;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 403)]
internal class TheDefiantOnes(ZoneModuleArgs args) : QuestBattle(args)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws, ActionDefinitions defs) => [
        new QuestObjective(ws).WithConnection(new Vector3(434.10f, -64.64f, 208.76f))
    ];
}

