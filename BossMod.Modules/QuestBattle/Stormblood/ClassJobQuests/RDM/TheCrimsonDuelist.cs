namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.RDM;

public enum OID : uint
{
    DeepRedXrhunTia = 0x1A8E,
    Sarameya = 0x1A7A,
    BloodshotHecteyes = 0x1A7B,
    EmptyMagus1 = 0x1A7C,
    EmptyMagus2 = 0x1A7D,
    EmptyArchmagus = 0x1A7E,
    GastricJuice = 0x1A80,
    BloodyCur = 0x1A81,
    BloodshotEye = 0x1A8D,
}

[ZoneModuleInfo(458, territoryID: 668)]
internal class TheCrimsonDuelist(WorldState ws) : QuestBattle(ws)
{
    private static readonly uint[] Hostiles = [(uint)OID.Sarameya, (uint)OID.BloodshotHecteyes, (uint)OID.EmptyMagus1, (uint)OID.EmptyMagus2,
        (uint)OID.EmptyArchmagus, (uint)OID.GastricJuice, (uint)OID.BloodyCur, (uint)OID.BloodshotEye];

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Named("Follow X'rhun and defeat Sarameya")
            .Hints((player, hints) =>
            {
                hints.PrioritizeTargetsByOID(Hostiles);
                if (hints.PotentialTargets.Any(e => e.Priority >= 0))
                    return;
                if (World.Actors.FirstOrDefault(a => a.OID == (uint)OID.DeepRedXrhunTia && !a.IsDead) is { } xrhun)
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(xrhun.Position, 5));
            })
            .CompleteOnKilled((uint)OID.Sarameya)
    ];
}
