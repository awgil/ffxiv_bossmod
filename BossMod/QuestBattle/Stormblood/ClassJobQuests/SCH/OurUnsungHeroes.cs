namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.SCH;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 265)]
internal class OurUnsungHeroes(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).WithConnection(new Vector3(232.30f, -854.55f, 407.66f))
    ];
}

