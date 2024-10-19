namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.BLM;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 445)]
internal class OneGolemToRuleThemAll(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.OID switch
            {
                // meteor shards (boss eats them)
                0x1C82 or 0x1EB4 => 3,
                // adds
                0x1C81 => 2,
                // boss or whatever
                _ => 1
            };
    }
}

