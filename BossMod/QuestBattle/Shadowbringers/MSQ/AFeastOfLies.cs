namespace BossMod.QuestBattle.Shadowbringers.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 664)]
public class AFeastOfLies(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(0.02f, 5.96f, -56.50f))
            .Hints((player, hints) => {
                hints.PathfindMapCenter = player.Position with { X = 0 };
                hints.PathfindMapBounds = new ArenaBoundsRect(8, 20);
            })
            .With(obj => {
                obj.OnEventObjectStateChanged += (act, state) => obj.CompleteIf(act.OID == 0x1EACEF && state == 2);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-0.00f, 6.00f, -29.00f))
            .Hints((player, hints) => {
                hints.PathfindMapCenter = new(0, -28.5f);
                hints.PathfindMapBounds = new ArenaBoundsRect(11, 16);
            })
            .With(obj => {
                var redDead = false;
                var blueDead = false;
                obj.OnActorKilled += (act) => {
                    redDead |= act.OID == 0x2959;
                    blueDead |= act.OID == 0x2958;
                    obj.CompleteIf(redDead && blueDead);
                };
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(0, 82.9f, -38))
            .CompleteOnCreated(0x295A),

        new QuestObjective(ws)
            .Hints((player, hints) => hints.ForcedMovement = new(0, 0, 1))
    ];
}
