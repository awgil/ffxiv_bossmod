namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.MCH;

[ZoneModuleInfo(426)]
internal class RiseOfTheMachinists(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .With(obj => {
                Actor? tedal = null;
                obj.OnActorCreated += (act) => {
                    if (act.OID == 0x1158)
                        tedal ??= act;
                };

                obj.AddAIHints += (player, hints) => {
                    foreach(var h in hints.PotentialTargets)
                        if (h.Actor.TargetID == tedal?.InstanceID)
                            h.Priority = 5;
                };
            })
        ];
}

