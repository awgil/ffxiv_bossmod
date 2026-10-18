namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.NIN;

[ZoneModuleInfo(390)]
internal class NinjaBathin(ZoneModuleInit init) : QuestBattle(init)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).WithConnection(new Vector3(213.18f, -36.40f, 313.56f))
    ];
}

