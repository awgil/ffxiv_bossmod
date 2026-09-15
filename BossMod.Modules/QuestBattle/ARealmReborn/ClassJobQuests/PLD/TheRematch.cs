namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.PLD;

[ZoneModuleInfo(318)]
internal class TheRematch(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws) // Run to Bridge, Fight Until Gigirya Defeated
            .WithConnection(new Vector3(-0.95f, -10.02f, -16.34f))
            .PauseForCombat(false)
            .Hints((player, hints) =>
            {
                foreach (var e in hints.PotentialTargets)
                    if (e.Actor.OID == 0x52C) // Ignore Leavold
                        e.Priority = AIHints.Enemy.PriorityForbidden;
            })
            .CompleteOnKilled(0x290),
        new QuestObjective(ws) // Beginning mobs may have been skipped. They're needed to complete duty.
            .WithConnection(new Vector3(5.2286663f, -31f, -8.183876f))
            .PauseForCombat(false)
            .Hints((player, hints) =>
            {
                foreach (var e in hints.PotentialTargets)
                    if (e.Actor.OID == 0x52C) // Ignore Leavold
                        e.Priority = AIHints.Enemy.PriorityForbidden;
            }),
    ];
}
