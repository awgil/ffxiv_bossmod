namespace BossMod.QuestBattle.ARealmReborn.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 387)]
internal class TheReasonRoaille(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
        {
            if (h.Actor.HPMP.CurHP == 1)
                h.Priority = 0;
            else if (h.Actor.OID == 0xDB7)
                h.Priority = 2;
            else
                h.Priority = 1;
        }
    }
}

