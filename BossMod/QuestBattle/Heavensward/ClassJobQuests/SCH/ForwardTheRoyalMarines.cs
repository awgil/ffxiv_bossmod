namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.SCH;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 394)]
internal class ForwardTheRoyalMarines(WorldState ws) : QuestBattle(ws)
{
    private QuestObjective Trace(Vector3 dest, uint oid) => new QuestObjective(World).WithConnection(dest).WithInteract(oid).CompleteOnState7(oid);

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        Trace(new Vector3(409.02f, 3.61f, 74.67f), 0x1E9B56),
        Trace(new Vector3(466.37f, 3.61f, 81.82f), 0x1E9B54),
        Trace(new Vector3(465.51f, 3.61f, 107.16f), 0x1E9B55),
        Trace(new Vector3(430.70f, 7.61f, 127.83f), 0x1E9B53),
        new QuestObjective(ws)
            .WithConnection(new Vector3(472.40f, 15.07f, 83.76f))
            .PauseForCombat(false)
            .CompleteOnCreated(0x10A4)
    ];
}

