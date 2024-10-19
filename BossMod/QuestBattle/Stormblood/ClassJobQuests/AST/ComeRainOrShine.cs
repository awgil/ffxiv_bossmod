namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.AST;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 442)]
internal class ComeRainOrShrine(WorldState ws) : QuestBattle(ws)
{
    private void HealBond(QuestObjective obj)
    {
        obj.AddAIHints += (player, hints) =>
        {
            if (World.Actors.FirstOrDefault(x => x.OID == 0x1ADA && x.IsTargetable && x.FindStatus(835) == null) is Actor t)
                hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.AST.AID.AspectedBenefic), t, ActionQueue.Priority.High);
        };
        obj.OnStatusGain += (act, stat) => obj.CompleteIf(act.OID == 0x1ADA && stat.ID == 835);
    }

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithInteract(0x1EA30F)
            .With(HealBond),

        new QuestObjective(ws)
            .WithConnection(new Vector3(41.53f, 8.02f, 190.47f))
            .With(obj => {
                obj.Update += () => obj.CompleteIf(World.Actors.Any(x => x.OID == 0x1A6E && x.Position.AlmostEqual(new(40, 191.2f), 1)));
            }),

        new QuestObjective(ws)
            .WithInteract(0x1EA310)
            .With(HealBond),

        new QuestObjective(ws)
            .WithConnection(new Vector3(117.76f, 8.02f, 122.13f))
            .WithConnection(new Vector3(157.24f, 25.00f, -23.10f))
            .WithInteract(0x1EA311)
            .With(HealBond),

        new QuestObjective(ws)
            .WithConnection(new Vector3(144.55f, 18.00f, -58.19f))
            .WithConnection(new Vector3(47.98f, 3.00f, -60.09f))
            .WithConnection(new Vector3(22.43f, 10.50f, -209.94f))
            .WithInteract(0x1EA312)
            .With(HealBond)
    ];
}

