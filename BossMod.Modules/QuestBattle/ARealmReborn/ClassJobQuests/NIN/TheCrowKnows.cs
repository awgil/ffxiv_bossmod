namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.NIN;

[ZoneModuleInfo(391)]
internal class TheCrowKnows(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.OID switch
            {
                0xDC9 => 3,
                0xDBE => 1,
                _ => 2
            };
    }
}

