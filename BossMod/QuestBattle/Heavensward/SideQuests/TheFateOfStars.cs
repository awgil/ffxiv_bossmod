namespace BossMod.QuestBattle.Heavensward.SideQuests;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 173)]
internal class ABloodyReunion(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(288.62f, 222.20f, 271.82f))
            .Hints((player, hints) => {
                if (World.Actors.FirstOrDefault(x => x.OID == 0x1EA080 && x.EventState != 7) is Actor shield)
                    hints.AddForbiddenZone(ShapeContains.InvertedCircle(shield.Position, 5));
            })
    ];
}
