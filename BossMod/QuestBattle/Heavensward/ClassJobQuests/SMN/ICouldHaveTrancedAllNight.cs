namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.SMN;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 439)]
internal class ICouldHaveTrancedAllNight(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.OID == 0x1321 ? 0 : 1;

        hints.InteractWithTarget = World.Actors.FirstOrDefault(x => x.OID is >= 0x1E9D09 and <= 0x1E9D0E && x.IsTargetable);
    }
}

