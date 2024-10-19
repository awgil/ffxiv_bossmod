namespace BossMod.QuestBattle.Stormblood.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 237)]
public class ItsProbablyATrap(WorldState ws) : QuestBattle(ws)
{
    enum SID : uint
    {
        Bind = 280,
    }

    private bool SmokeBomb;

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .With(obj => obj.Update += () => obj.CompleteIf(World.Client.DutyActions[0].Action.ID == 7816)),

        // have to walk up the stairs to trigger dialogue
        new QuestObjective(ws)
            .WithConnection(new Vector3(-80.82f, -3.00f, 46.31f))
            .With(obj =>
            {
                obj.OnDirectorUpdate += (op) =>
                {
                    // this op enables the Smoke Bomb duty action
                    if (op.Param1 == 14801)
                    {
                        obj.Completed = true;
                        SmokeBomb = true;
                    }
                };
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-65.45f, -3.00f, 26.26f))
            .WithConnection(new Vector3(119.86f, 12.00f, 61.87f))
            .WithConnection(new Vector3(117.25f, 12.00f, 17.32f))
            .WithConnection(new Vector3(70, -5, 17))
            .PauseForCombat(false)
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            // attacking sekiseigumi fails the mission
            if (h.Actor.OID is 0x1A6B or 0x1A66)
                h.Priority = AIHints.Enemy.PriorityForbidFully;

        if (SmokeBomb && player.FindStatus(SID.Bind) != null)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.SmokeScreen), player, ActionQueue.Priority.Medium);
    }
}
