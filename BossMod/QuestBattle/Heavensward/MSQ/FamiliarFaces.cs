namespace BossMod.QuestBattle.Heavensward.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 401)]
internal class FamiliarFaces(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.OID == 0x106C ? 1 : 0;
    }
}

