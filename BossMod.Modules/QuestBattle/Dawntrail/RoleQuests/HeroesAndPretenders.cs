namespace BossMod.QuestBattle.Dawntrail.RoleQuests;

[ZoneModuleInfo(1001, territoryID: 1218)]
internal class HeroesAndPretenders(WorldState ws) : QuestBattle(ws)
{
    private bool _initialTargetSelected;

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PrioritizeAll();
        if (!_initialTargetSelected)
            _initialTargetSelected = World.Actors.Find(player.TargetID)?.OID == 17034;
        if (!_initialTargetSelected)
            hints.PrioritizeTargetsByOID(17034, 1);
    }
}
