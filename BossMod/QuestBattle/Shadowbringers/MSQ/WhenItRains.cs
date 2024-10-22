namespace BossMod.QuestBattle.Shadowbringers.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 663)]
public class WhenItRains(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(226.01f, 10.03f, 105.23f))
            .WithConnection(new Vector3(149.92f, 7.67f, 97.04f))
            .WithConnection(new Vector3(58.82f, 3.14f, -19.36f))
            .WithConnection(new Vector3(-10.54f, 4.59f, -47.66f))
            .WithConnection(new Vector3(-110.38f, 5.76f, -94.28f))
            .WithConnection(new Vector3(-133.96f, 5.76f, -164.13f))
            .CompleteAtDestination(),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-171.87f, 5.75f, -170.05f))
            .WithConnection(new Vector3(-189.67f, 5.76f, -148.71f))
            .WithConnection(new Vector3(-193.42f, 5.76f, -112.89f))
            .WithConnection(new Vector3(-178.40f, 6.00f, -98.59f))
            .WithConnection(new Vector3(-165.58f, 5.78f, -91.05f))
            .PauseForCombat(false)
            .With(obj => {
                var navComplete = false;
                var minibossDead = false;

                obj.OnNavigationComplete += () => navComplete = true;
                obj.OnActorKilled += (act) => minibossDead |= act.OID == 0x28B8;
                obj.Update += () => obj.CompleteIf(navComplete && minibossDead);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-372.99f, 4.49f, -114.72f))
            .WithConnection(new Vector3(-455.64f, 8.23f, -195.11f))
            .WithConnection(new Vector3(-528.04f, 14.35f, -243.61f))
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
