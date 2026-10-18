namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.MNK;

[ZoneModuleInfo(364)]
internal class FiveEasyPieces(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.OID == 0x640 ? 0 : 1;
    }
}

