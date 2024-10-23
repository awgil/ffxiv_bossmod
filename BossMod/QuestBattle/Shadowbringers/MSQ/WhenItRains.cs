namespace BossMod.QuestBattle.Shadowbringers.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 663)]
public class WhenItRains(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-111.41f, 5.76f, -99.83f))
            .CompleteOnKilled(0x28B8),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-330.46f, 2.81f, -117.58f))
            .WithConnection(new Vector3(-696.80f, 51.18f, -235.22f))
            .Hints((player, hints) => {
                if (World.Actors.FirstOrDefault(x => x.OID == 0x1EA4D5) is Actor minibossArena)
                {
                    hints.PathfindMapCenter = minibossArena.Position;
                    hints.PathfindMapBounds = new ArenaBoundsCircle(19.5f);
                }
            })
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = 0;
    }
}
