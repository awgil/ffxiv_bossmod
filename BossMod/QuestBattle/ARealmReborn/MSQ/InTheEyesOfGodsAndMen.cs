namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 343)]
internal class InTheEyesOfGodsAndMen(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).WithInteract(0x1E8D67)
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PathfindMapBounds = new ArenaBoundsCircle(60, 1);
        hints.PathfindMapCenter = player.Position;

        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.OID switch
            {
                0x6C9 => -1,
                0x6C4 => 1,
                _ => 0
            };
    }
}
