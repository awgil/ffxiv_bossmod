namespace BossMod.QuestBattle.Heavensward.MSQ;

[ZoneModuleInfo(401)]
internal class FamiliarFaces(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.OID == 0x106C ? 1 : 0;
    }
}

