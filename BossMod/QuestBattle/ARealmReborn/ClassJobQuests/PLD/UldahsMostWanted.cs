namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.PLD;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 314, 253)]
internal class UldahsMostWanted(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(13.6473255f, 13.518082f, 44.521732f))
            .PauseForCombat(true)
            .CompleteAtDestination(),
        new QuestObjective(ws)
            .WithConnection(new Vector3(34.927856f, 13.266486f, 89.40259f))
            .PauseForCombat(true)
            .CompleteAtDestination(),

        new QuestObjective(ws)
            .WithConnection(new Vector3(7.823333f, 12.688625f, 32.762177f))
            .Hints((player, hints) =>
            {
                hints.PrioritizeTargetsByOID(0x274, 5);
                foreach (var e in hints.PotentialTargets)
                    if (e.Actor.OID == 0x271) // Bruce the Big
                        e.Priority = AIHints.Enemy.PriorityForbidden;

                // Stand on the far side of the captain from Bruce so the frontal AoE faces away from Bruce
                var captain = World.Actors.FirstOrDefault(x => x.OID == 0x274);
                var bruce = World.Actors.FirstOrDefault(x => x.OID == 0x271);
                if (captain != null && bruce != null)
                {
                    var dir = (captain.Position - bruce.Position).Normalized();
                    hints.GoalZones.Add(hints.GoalSingleTarget(captain.Position + dir * 3, 2));
                }
            })
            .PauseForCombat(false)
            .CompleteOnKilled(0x274) // Duskwight Freelancer Captain
    ];
}

