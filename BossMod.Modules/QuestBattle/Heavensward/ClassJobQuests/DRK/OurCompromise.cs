namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.DRK;

[ZoneModuleInfo(272)]
internal class OurCompromise(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PathfindMapBounds = new ArenaBoundsCircle(19.5f);
        hints.PathfindMapCenter = new(-615.237f, -471.459f);

        hints.PrioritizeAll();

        foreach (var v in hints.PotentialTargets)
            if (v.Actor.OID == 0x1C80)
                hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.DRK.AID.Souleater), v.Actor, ActionQueue.Priority.VeryHigh);
    }
}
