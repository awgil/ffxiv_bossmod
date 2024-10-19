namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.BLM;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 403)]
internal class TheDefiantOnes(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).WithConnection(new Vector3(434.10f, -64.64f, 208.76f))
    ];
}

