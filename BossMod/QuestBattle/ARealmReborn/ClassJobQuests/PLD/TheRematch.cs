namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.PLD;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 318)]
internal class TheRematch(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-4.35f, -30.00f, -32.35f))
            .WithConnection(new Vector3(6.30f, -31.00f, -9.42f))
            .WithConnection(new Vector3(2.00f, -17.54f, 23.70f))
            .WithConnection(new Vector3(5.00f, -7.85f, 1.40f))
            .WithConnection(new Vector3(23.48f, -6.01f, -16.19f))
            .WithConnection(new Vector3(-21.73f, -10.01f, -15.83f))
            .PauseForCombat(false)
    ];
}