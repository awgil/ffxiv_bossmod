namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.NIN;

[Quest(BossModuleInfo.Maturity.Contributed, 390)]
internal class NinjaBathin(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).WithConnection(new Vector3(213.18f, -36.40f, 313.56f))
    ];
}

