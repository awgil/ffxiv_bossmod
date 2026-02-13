namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.BLM;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 444)]
internal class ShadesOfShatotto(ZoneModuleArgs args) : QuestBattle(args)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.OID == 0x1AE8 ? 0 : 1;
    }
}

