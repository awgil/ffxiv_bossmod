namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.DRK;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 431)]
internal class HeroicReprise(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-74.51f, -24.25f, 269.76f))
            .WithInteract(0x1E9B3F)
            .With(obj => {
                obj.OnActorEventStateChanged += (act) => obj.CompleteIf(act.OID == 0x1274 && act.EventState == 0);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-124.39f, -30.14f, 302.91f))
    ];
}

