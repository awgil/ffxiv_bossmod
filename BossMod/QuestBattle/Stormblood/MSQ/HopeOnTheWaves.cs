namespace BossMod.QuestBattle.Stormblood.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 472)]
public class HopeOnTheWaves(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .With(obj => {
                obj.OnEventObjectStateChanged += (actor, state) => obj.CompleteIf(actor.OID == 0x1EA1A1 && state == 0x0008);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(551.57f, 12.94f, 751.91f))
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeAll();
}
