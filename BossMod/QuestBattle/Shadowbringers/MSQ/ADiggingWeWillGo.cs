namespace BossMod.QuestBattle.Shadowbringers.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 665)]
public class ADiggingWeWillGo(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        QuestObjective.Combat(ws, new Vector3(-300.51f, 385.53f, -433.57f)),
        QuestObjective.Combat(ws, new Vector3(-229.87f, 371.62f, -363.50f)),
        QuestObjective.Combat(ws, new Vector3(-106.29f, 362.27f, -354.45f)),
        QuestObjective.Combat(ws, new Vector3(-24.33f, 362.38f, -387.26f), new Vector3(-8.37f, 360.67f, -396.21f)),
        new QuestObjective(ws).CompleteOnCreated(0x29A4),
        new QuestObjective(ws).WithConnection(new Vector3(329.71f, 309.83f, -381.65f))
    ];
}
