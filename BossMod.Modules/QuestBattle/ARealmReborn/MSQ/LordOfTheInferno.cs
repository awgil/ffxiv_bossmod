namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(339)]
internal class LordOfTheInferno(ZoneModuleInit init) : QuestBattle(init)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.OID == 0x3C7 ? 0 : 1;
    }
}
