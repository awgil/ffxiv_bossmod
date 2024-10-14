namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.MNK;

[Quest(BossModuleInfo.Maturity.Contributed, 323)]
internal class ReturnOfTheHolyfist(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeAll();
}
