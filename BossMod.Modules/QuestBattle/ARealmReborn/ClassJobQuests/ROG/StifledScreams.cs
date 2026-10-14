namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.ROG;

[ZoneModuleInfo(382)]
internal class StifledScreams(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .PauseForCombat(true)
            .WithConnection(new Vector3(306.04f, -36.06f, 318.09f))
            .WithConnection(new Vector3(323.55f, -35.95f, 314.95f))
            .WithInteract(0x1E9753)
            .CompleteOnCreated(0x1E9755),

        new QuestObjective(ws)
            .PauseForCombat(true)
            .WithConnection(new Vector3(323.16f, -31.90f, 268.45f))
            .WithInteract(0x1E9755)
            .CompleteOnCreated(0x1E9757),

        new QuestObjective(ws)
            .PauseForCombat(true)
            .WithConnection(new Vector3(328.5f, -25f, 251.88f))
            .WithInteract(0x1E9757)
            .CompleteOnDestroyed(0x1E9757),

        new QuestObjective(ws)
            .WithInteract(0x1E9758)
            .CompleteOnDestroyed(0x1E9758),

        new QuestObjective(ws)
            .Hints((player, hints) => hints.PrioritizeTargetsByOID(0xD47))
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        if (!player.InCombat)
        {
            if (player.FindStatus(BossMod.NIN.SID.Hidden) == null)
                hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.NIN.AID.Hide), player, ActionQueue.Priority.VeryLow);
            hints.ActionsToExecute.Push(ActionDefinitions.IDSprint, player, ActionQueue.Priority.VeryLow);
        }

        // stupid ass complicated ass room layouts
        if (World.Actors.Find(player.TargetID) is { Visibility: Visibility.Blocked } t)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(t.Position, 1));
    }
}

