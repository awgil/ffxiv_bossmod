namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.AST;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 411)]
internal class FortuneFavorsTheBole(WorldState ws) : QuestBattle(ws)
{
    private QuestObjective Card(WorldState ws, Vector3 pos, uint oid)
        => new QuestObjective(ws).WithConnection(pos).WithInteract(oid).CompleteOnState7(oid);

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        Card(ws, new(141.36f, 18.58f, 73.59f), 0x1E9BC7),
        Card(ws, new(98.61f, 22.82f, 172.71f), 0x1E9BC8),
        Card(ws, new(51.44f, 24.93f, 135.83f), 0x1E9BC9),
        Card(ws, new(79.41f, 23.67f, 199.89f), 0x1E9BCA),
        Card(ws, new(51.20f, 19.37f, 217.16f), 0x1E9BCB),
        Card(ws, new(52.92f, 12.10f, 278.88f), 0x1E9BCC),

        new QuestObjective(ws)
            .WithConnection(new Vector3(34.35f, 3.68f, 306.75f))
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            if (h.Actor.InCombat)
                h.Priority = h.Actor.OID == 0x10D0 ? 1 : 0;
    }
}

