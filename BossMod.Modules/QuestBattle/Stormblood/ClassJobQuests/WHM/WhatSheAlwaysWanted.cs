namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.WHM;

[ZoneModuleInfo(457)]
internal class WhatSheAlwaysWanted(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var target in World.Actors.Where(a => a.OID == 0x1CF1 && a.FindStatus(774) != null && a.PendingHPRatio < 1))
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.WHM.AID.Benediction), target, ActionQueue.Priority.Medium);
            hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.WHM.AID.AfflatusSolace), target, ActionQueue.Priority.High);
        }

        // we can't even deal half of this thing's HP in very easy mode
        foreach (var target in hints.PotentialTargets.Where(t => t.Actor.OID == 0x1CF0))
            target.Priority = AIHints.Enemy.PriorityPointless;
    }
}
