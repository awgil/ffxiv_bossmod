namespace BossMod.QuestBattle.Shadowbringers.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 724)]
public class ASleepDisturbed(WorldState ws) : QuestBattle(ws)
{
    private static readonly ICollection<int> InteractTargets =
    [
        // opo statue (first correct answer)
        0x1EAF81,
        // wolf statue (second correct answer)
        // easier to just pick wolf twice than figure out what state transition makes the serpent statue the right choice
        0x1EAF7E,
        // list of all the correct cards
        .. Enumerable.Range(0x1EAF8C, 6),
    ];

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Hints((player, hints) => {
                hints.InteractWithTarget = World.Actors.Where(a => InteractTargets.Contains((int)a.OID) && a.IsTargetable).OrderBy(a => a.OID).FirstOrDefault();
            })
    ];
}
