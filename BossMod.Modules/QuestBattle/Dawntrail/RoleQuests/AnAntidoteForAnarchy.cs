namespace BossMod.QuestBattle.Dawntrail.RoleQuests;

[ZoneModuleInfo(994, territoryID: 1215)]
internal class AnAntidoteForAnarchy(WorldState ws) : QuestBattle(ws)
{
    private bool _initialTargetSelected;

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PrioritizeAll();
        if (!_initialTargetSelected)
            _initialTargetSelected = World.Actors.Find(player.TargetID)?.OID == 16983;
        if (!_initialTargetSelected)
            hints.PrioritizeTargetsByOID(16983, 1);
    }
}
