namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.RDM.VermilionVendetta;

public enum OID : uint
{
    DeepRedXrhunTia = 0x1ADF,
    FearGorta = 0x1AE1,
    EmptyArchmagus = 0x1AE2,
    DeadMansMoan = 0x1AE3,
    EmptyMagus = 0x1AE4,
    ShadowSprite = 0x1AE6,
}

[ZoneModuleInfo(459, territoryID: 675)]
internal class AVermilionVendetta(WorldState ws) : QuestBattle(ws)
{
    private static readonly uint[] Hostiles = [(uint)OID.FearGorta, (uint)OID.EmptyArchmagus, (uint)OID.DeadMansMoan, (uint)OID.EmptyMagus];

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Named("Follow X'rhun and defeat Fear Gorta")
            .Hints((player, hints) =>
            {
                foreach (var e in hints.PotentialTargets)
                    if (e.Actor.EventState == 0 && Hostiles.Contains(e.Actor.OID))
                        e.Priority = Math.Max(e.Priority, 0);
                hints.PrioritizeTargetsByOID((uint)OID.ShadowSprite, 1);
                if (hints.PotentialTargets.Any(e => e.Priority >= 0))
                    return;
                if (World.Actors.FirstOrDefault(a => a.OID == (uint)OID.DeepRedXrhunTia && !a.IsDead) is { } xrhun)
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(xrhun.Position, 5));
            })
            .CompleteOnKilled((uint)OID.FearGorta)
    ];
}
