namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.ROG;

[ZoneModuleInfo(381)]
internal class StrayIntoTheShadows(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-218.03f, 4.00f, 270.63f))
            .WithInteract(0x1E96FD)
            .CompleteOnDestroyed(0x1E96FD),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-193.85f, 8.00f, 82.24f))
            .WithInteract(0x1E97BE)
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        if (!player.InCombat)
        {
            if (player.FindStatus(BossMod.NIN.SID.Hidden) == null)
                hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.NIN.AID.Hide), player, ActionQueue.Priority.VeryLow);
            hints.ActionsToExecute.Push(ActionDefinitions.IDSprint, player, ActionQueue.Priority.VeryLow);
        }

        // prevent autorot from helping v'kebbe since our job is to stealth
        foreach (var target in hints.PotentialTargets.Where(p => !p.Actor.AggroPlayer))
            target.Priority = AIHints.Enemy.PriorityUndesirable;
    }
}
