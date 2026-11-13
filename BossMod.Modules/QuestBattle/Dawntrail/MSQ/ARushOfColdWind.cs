namespace BossMod.QuestBattle.Dawntrail.MSQ;

[ZoneModuleInfo(1101, territoryID: 1371)]
internal class ARushOfColdWind(WorldState ws) : QuestBattle(ws)
{
    private bool _initialTargetSelected;

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PrioritizeAll();
        if (!_initialTargetSelected)
            _initialTargetSelected = World.Actors.Find(player.TargetID)?.OID == 19818;
        if (!_initialTargetSelected)
            hints.PrioritizeTargetsByOID(19818, 1);
    }
}
