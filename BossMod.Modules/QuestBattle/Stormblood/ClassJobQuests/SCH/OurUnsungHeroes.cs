namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.SCH;

[ZoneModuleInfo(265)]
internal class OurUnsungHeroes(ZoneModuleInit init) : QuestBattle(init)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).WithConnection(new Vector3(232.30f, -854.55f, 407.66f))
    ];
}

