namespace BossMod.QuestBattle.Heavensward.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 395)]
public sealed class ASeriesOfUnfortunateEvents(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
           .Named("Free Emmanellain")
           .WithConnection(new Vector3(657.58f, -65.54f, -123.75f))
           .WithInteract(0x1E9ACE)
           .With(obj =>
           {
               obj.OnModelStateChanged += (act) => obj.CompleteIf(act.OID == 0x1003 && act.ModelState.ModelState == 0);
           }),

        new QuestObjective(ws).Named("Escort Emmanellain to safety")
            .Hints((player, hints) =>
            {
                hints.PathfindMapBounds = OverworldBounds;

                var emmanellain = World.Actors.FirstOrDefault(i => i.OID == 0x1003);
                if (emmanellain != null)
                {
                    foreach (var h in hints.PotentialTargets)
                        if (h.Actor.TargetID == emmanellain.InstanceID)
                            h.Priority = 0;

                    if (!player.InCombat && player.DistanceToHitbox(emmanellain) > 50)
                        hints.ForcedMovement = player.DirectionTo(emmanellain).ToVec3();
                }
            })
    ];
}
