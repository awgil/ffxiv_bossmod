namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 352)]
internal class EscapeFromCastrumCentri(WorldState ws) : QuestBattle(ws)
{
    private static QuestObjective Generator(WorldState ws, Vector3 position) => GoKill(ws, position, 0x883);

    private static QuestObjective GoKill(WorldState ws, Vector3 position, uint oid)
        => new QuestObjective(ws)
            .WithConnection(position)
            .PauseForCombat(false)
            .Hints((player, hints) => hints.PrioritizeTargetsByOID(oid, 1))
            .CompleteOnKilled(oid);

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Hints((player, hints) => hints.PrioritizeAll())
            .With(obj => {
                obj.Update += () => obj.CompleteIf(World.Party.Player()?.Position.X < -470);
            }),

        Generator(ws, new Vector3(-428.83f, -3.99f, -242.18f)),
        GoKill(ws, new Vector3(-474.28f, -3.22f, -244.88f), 0x881),
        Generator(ws, new Vector3(-461.22f, -4.01f, -297.81f)),
        Generator(ws, new Vector3(-490.59f, -3.96f, -216.99f)),
        new QuestObjective(ws)
            .WithConnection(new Vector3(-493.24f, -3.22f, -269.33f))
            .PauseForCombat(false)
            .Hints((player, hints) => hints.PrioritizeAll())
    ];
}

