namespace BossMod.QuestBattle.Stormblood.MSQ;

[ZoneModuleInfo(472)]
public class HopeOnTheWaves(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .With(obj => {
                obj.OnEventObjectAnimation += (actor, _, _) => obj.CompleteIf(actor.OID == 0x1EA1A1 && actor.TimelineState == 0x0008);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(551.57f, 12.94f, 751.91f))
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeAll();
}
