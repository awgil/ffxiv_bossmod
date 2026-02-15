namespace BossMod.QuestBattle.Shadowbringers.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 764)]
internal class TheGreatShipVylbrand(ZoneModuleArgs args) : QuestBattle(args)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws, ActionDefinitions defs) => [
        new QuestObjective(ws).WithInteract(0x1EB0F7)
    ];
}

