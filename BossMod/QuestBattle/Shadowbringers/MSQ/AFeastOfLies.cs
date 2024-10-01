namespace BossMod.QuestBattle.Shadowbringers.MSQ;

[Quest(BossModuleInfo.Maturity.Contributed, 664)]
public class AFeastOfLies(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(0.02f, 5.96f, -56.50f))
            .Hints((player, hints) => {
                hints.Center = player.Position with { X = 0 };
                hints.Bounds = new ArenaBoundsRect(8, 20);
            })
            .With(obj => {
                obj.OnEventObjectStateChanged += (act, state) => obj.CompleteIf(act.OID == 0x1EACEF && state == 2);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-0.00f, 6.00f, -29.00f))
            .Hints((player, hints) => {
                hints.Center = new(0, -28.5f);
                hints.Bounds = new ArenaBoundsRect(11, 16);
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
            .WithConnection(new Vector3(0, 82.00f, 18))
    ];
}
