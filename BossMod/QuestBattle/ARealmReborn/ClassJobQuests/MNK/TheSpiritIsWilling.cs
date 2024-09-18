namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.MNK;

[Quest(BossModuleInfo.Maturity.Contributed, 319)]
internal class TheSpiritIsWilling(WorldState ws) : QuestBattle(ws)
{
    public override void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime)
    {
        hints.Center = new(-240, -6);
        hints.Bounds = new ArenaBoundsSquare(15);

        // big rock
        hints.AddForbiddenZone(new AOEShapeRect(4, 3, 4), new(-229, -2));

        foreach (var h in hints.PotentialTargets)
            h.Priority = 0;
    }
}
