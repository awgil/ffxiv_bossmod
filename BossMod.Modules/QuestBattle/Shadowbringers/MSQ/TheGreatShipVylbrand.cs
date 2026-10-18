namespace BossMod.QuestBattle.Shadowbringers.MSQ;

[ZoneModuleInfo(764)]
internal class TheGreatShipVylbrand(ZoneModuleInit init) : QuestBattle(init)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).WithInteract(0x1EB0F7)
    ];
}

