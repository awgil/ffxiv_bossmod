namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.DRK;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 436)]
internal class IshgardianJustice(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).CompleteOnCreated(0x1223),
        new QuestObjective(ws)
            .Hints((player, hints) => {
                if (World.Actors.FirstOrDefault(x => x.OID == 0x1223 && x.EventState == 1) is Actor d)
                    hints.ForcedMovement = player.DirectionTo(d).ToVec3();
            })
            .With(obj => {
                obj.OnStatusLose += (act, status) => obj.CompleteIf(status.ID == 625);
            }),
        new QuestObjective(ws)
            .WithConnection(new Vector3(118.36f, 14.95f, -148.58f))
            .WithConnection(new Vector3(134.09f, 14.55f, -148.98f))
            .WithInteract(0x1E9C6D)
    ];
}
