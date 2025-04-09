namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.SCH;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 453)]
internal class InLovingMemory(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(75.00f, 48.06f, -9.25f))
            .WithInteract(0x1EA572)
            .With(obj => {
                obj.OnActorDestroyed += (act) => obj.CompleteIf(act.OID == 0x1EA572 && World.Party.Player()?.DistanceToHitbox(act) < 10);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(212.32f, 44.13f, -16.14f))
            .Hints((player, hints) => hints.PrioritizeTargetsByOID(0x1E30))
            .CompleteOnCreated(0x1EA53D),

        new QuestObjective(ws)
            .WithInteract(0x1EA53D)
            .With(obj => {
                obj.OnEventObjectStateChanged += (act, state) => obj.CompleteIf(act.OID == 0x1EA53D && state == 0x20);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(199.60f, 44.00f, -153.20f))
            .Hints((player, hints) => {
                if (player.InCombat)
                    foreach (var p in World.Party.WithoutSlot().Exclude(player))
                        hints.AddForbiddenZone(ShapeContains.Circle(p.Position, 6), World.FutureTime(1));

                foreach(var e in World.Actors)
                    if (e.OID == 0x1E43)
                        hints.AddForbiddenZone(ShapeContains.Circle(e.Position, 6));
            })
            .WithInteract(0x1EA732)
    ];
}

