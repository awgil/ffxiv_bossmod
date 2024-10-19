namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.PLD;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 318)]
internal class TheRematch(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-0.95f, -10.02f, -16.34f))
            .PauseForCombat(false)
    ];
}
