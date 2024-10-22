namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.SCH;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 378)]
internal class TheBeastWithin(WorldState ws) : QuestBattle(ws)
{
    private void CompleteOnSpawn(QuestObjective obj, int count)
    {
        var acc = 0;
        obj.OnActorCreated += (act) =>
        {
            if (act.OID == 0x5F3 && ++acc >= count)
                obj.Completed = true;
        };
    }
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(130.52f, -18.07f, 374.95f))
            .With(obj => CompleteOnSpawn(obj, 2)),

        new QuestObjective(ws)
            .WithConnection(new Vector3(182.41f, -18.00f, 374.35f))
            .PauseForCombat(false)
            .With(obj => CompleteOnSpawn(obj, 2)),

        new QuestObjective(ws)
            .WithConnection(new Vector3(140.79f, -14.00f, 264.89f))
            .PauseForCombat(false)
            .With(obj => CompleteOnSpawn(obj, 3)),

        new QuestObjective(ws)
            .WithConnection(new Vector3(184.07f, -14.00f, 315.41f))
            .PauseForCombat(false)
            .With(obj => {
                obj.OnActorEventStateChanged += (act) => obj.CompleteIf(act.OID == 0x5F8 && act.EventState == 1);
            }),

        new QuestObjective(ws)
            .Hints((player, hints) => hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.SCH.AID.Physick), World.Actors.FirstOrDefault(x => x.OID == 0x5F8 && x.IsTargetable), ActionQueue.Priority.High))
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            if (h.Actor.OID == 0x5F3)
                h.Priority = -1;
    }
}

