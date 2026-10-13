namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(332)]
public class LurkersInTheGrotto(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var p in hints.PotentialTargets)
            p.Priority = p.Actor.OID == 0x386 ? 0 : 1;
    }
}
