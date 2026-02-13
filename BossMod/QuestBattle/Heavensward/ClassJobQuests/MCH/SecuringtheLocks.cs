namespace BossMod.QuestBattle.Heavensward.ClassJobQuests.MCH;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 424)]
internal class SecuringTheLocks(ZoneModuleArgs args) : QuestBattle(args)
{
    private static readonly WPos Center = new(231.29f, 124.36f);

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PathfindMapCenter = Center;
        hints.PathfindMapBounds = new ArenaBoundsCircle(120, 2);

        if (player.TargetID == 0)
        {
            var closest = hints.PotentialTargets.MinBy(p => p.Actor.DistanceToHitbox(player));
            if (closest != null)
                hints.GoalZones.Add(hints.GoalSingleTarget(closest.Actor, 25));
            else
                hints.GoalZones.Add(hints.GoalSingleTarget(Center, 5));
        }
    }
}

