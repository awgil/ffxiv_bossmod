namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.SAM;

[ZoneModuleInfo(250, territoryID: 710)]
internal class MatsubaMayhem(WorldState ws) : QuestBattle(ws)
{
    private bool _initialTargetSelected;

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PrioritizeAll();
        if (!_initialTargetSelected)
            _initialTargetSelected = World.Actors.Find(player.TargetID)?.OID == 7154;
        if (!_initialTargetSelected)
            hints.PrioritizeTargetsByOID(7154, 1);
    }
}
