namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 337)]
internal class OhCaptainMyCaptain(ZoneModuleArgs args) : QuestBattle(args)
{
    public override void AddQuestAIHints(Actor player, AIHints hints) => hints.PrioritizeTargetsByOID(0x3BC, 2);
}
