namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(339)]
internal class LordOfTheInferno(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.OID == 0x3C7 ? 0 : 1;
    }
}
