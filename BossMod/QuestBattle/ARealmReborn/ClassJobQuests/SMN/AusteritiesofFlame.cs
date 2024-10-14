namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.SMN;

[Quest(BossModuleInfo.Maturity.Contributed, 372)]
internal class AusteritiesOfFlame(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeTargetsByOID(0x5EC, 5);
}

