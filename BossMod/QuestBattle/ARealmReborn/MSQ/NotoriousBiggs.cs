namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 351)]
internal class NotoriousBiggs(ZoneModuleArgs args) : QuestBattle(args)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PrioritizeAll();
    }
}

