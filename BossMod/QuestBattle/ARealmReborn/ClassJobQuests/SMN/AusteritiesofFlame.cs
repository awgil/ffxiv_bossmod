namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.SMN;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 372)]
internal class AusteritiesOfFlame(ZoneModuleArgs args) : QuestBattle(args)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeTargetsByOID(0x5EC, 5);
}

