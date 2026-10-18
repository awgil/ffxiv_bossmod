namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.MNK;

[ZoneModuleInfo(321)]
internal class StarcrossedRivals(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
        {
            h.Priority = h.Actor.OID switch
            {
                0x2BD or 0x2BE => 2,
                0x2BA => 0,
                _ => 1
            };
        }
    }
}
