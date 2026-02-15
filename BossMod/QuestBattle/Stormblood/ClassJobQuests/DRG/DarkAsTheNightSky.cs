namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.DRG;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 267)]
internal class DarkAsTheNightSky(ZoneModuleArgs args) : QuestBattle(args)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws, ActionDefinitions defs) => [
        new QuestObjective(ws).WithConnection(new Vector3(-338.28f, 69.61f, -384.66f))
    ];
}

