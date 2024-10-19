namespace BossMod.QuestBattle.Stormblood.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 269)]
public class TheResonant(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(676.12f, 70.00f, 512.76f))
            .ThenWait(11.5f),

        new QuestObjective(ws)
            .WithConnection(new Vector3(593.39f, 70.00f, 540.88f))
            .CompleteOnKilled(0x1EB6),

        new QuestObjective(ws)
            // the piles of crates lack collision (npcs just teleport on top of them)
            .WithConnection(new Vector3(592.57f, 70.00f, 535.17f))
            .WithConnection(new Vector3(581.57f, 70.00f, 534.72f))
            .WithConnection(new Vector3(511.07f, 70.00f, 583.65f))
            .WithInteract(0x1EA74B)
            .With(obj => {
                obj.OnActorEventStateChanged += (act) => obj.CompleteIf(act.OID == 0x1EA74B && act.EventState == 7);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(581.73f, 70.00f, 533.20f))
            .WithConnection(new Vector3(655.03f, 70.00f, 527.29f))
            .WithConnection(new Vector3(748.96f, 70.00f, 510.77f))
            .WithConnection(new Vector3(754.20f, 70.00f, 429.63f))
    ];
}
