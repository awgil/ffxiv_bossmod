namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.ROG;

[ZoneModuleInfo(385)]
internal class SweetSorrows(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
        {
            h.Priority = h.Actor.OID switch
            {
                0xD82 => 2,
                _ => 1
            };
        }
    }

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .PauseForCombat(true)
            .WithConnection(new Vector3(600f, 23.94f, 453.47f))
    ];
}
