namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.AST;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 414)]
internal class TheHandsOfFate(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            if (h.Actor.OID is >= 0x1268 and <= 0x126D)
                h.Priority = 5;
    }
}

