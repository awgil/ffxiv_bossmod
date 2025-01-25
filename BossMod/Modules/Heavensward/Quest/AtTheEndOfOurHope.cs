using BossMod.QuestBattle;

namespace BossMod.Heavensward.Quest;

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 416)]
public class AtTheEndOfOurHope(WorldState ws) : QuestBattle.QuestBattle(ws)
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
