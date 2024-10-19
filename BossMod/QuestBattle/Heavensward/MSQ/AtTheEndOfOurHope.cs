namespace BossMod.QuestBattle.Heavensward.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 416)]
public sealed class AtTheEndOfOurHope(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws).WithConnections(
            // doorway
            new Vector3(455.42f, 164.31f, -542.78f),
            // basement
            new Vector3(456.10f, 157.41f, -554.90f)
        )
        .WithInteract(0x1E9B5A)
        .PauseForCombat(false)
    ];
}
