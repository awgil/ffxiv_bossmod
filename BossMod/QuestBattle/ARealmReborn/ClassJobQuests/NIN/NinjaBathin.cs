namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.NIN;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 390)]
internal class NinjaBathin(ZoneModuleArgs args) : QuestBattle(args)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).WithConnection(new Vector3(213.18f, -36.40f, 313.56f))
    ];
}

