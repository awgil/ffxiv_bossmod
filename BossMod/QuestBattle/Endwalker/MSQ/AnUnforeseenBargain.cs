using RID = BossMod.Roleplay.AID;

namespace BossMod.QuestBattle.Endwalker.MSQ;

class ZeroAI(WorldState ws) : UnmanagedRotation(ws, 3)
{
    protected override void Exec(Actor? primaryTarget)
    {
        UseAction(RID.Communio, primaryTarget);

        var zones = Hints.GoalAOECircle(5);
        if (primaryTarget != null)
            zones = Hints.GoalCombined(Hints.GoalSingleTarget(primaryTarget, 3), zones, 3);

        Hints.GoalZones.Add(zones);

        var numAOETargets = Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);

        switch (ComboAction)
        {
            case RID.WaxingSlice:
                UseAction(RID.InfernalSlice, primaryTarget);
                break;
            case RID.Slice:
                UseAction(RID.WaxingSlice, primaryTarget);
                break;
            case RID.SpinningScythe:
                UseAction(RID.NightmareScythe, Player);
                break;
            default:
                if (numAOETargets > 2)
                    UseAction(RID.SpinningScythe, Player);
                else
                    UseAction(RID.Slice, primaryTarget);
                break;
        }

        UseAction(RID.Engravement, primaryTarget, -100);
        if (Player.InCombat)
            UseAction(RID.Bloodbath, Player, -100);
    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 910)]
internal class AnUnforeseenBargain(WorldState ws) : QuestBattle(ws)
{
    private readonly ZeroAI _zero = new(ws);

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        hints.PathfindMapCenter = new(97.85f, 286);
        hints.PathfindMapBounds = new ArenaBoundsCircle(19.5f);

        foreach (var h in hints.PotentialTargets)
            if (h.Actor.CastInfo is { Action.ID: 33042 } ci)
                hints.ForbiddenDirections.Add((player.AngleTo(h.Actor), 45.Degrees(), World.FutureTime(ci.NPCRemainingTime)));

        // walk north to engage enemies
        if (!player.InCombat)
            hints.ForcedMovement = new(0, 0, -1);

        if (player.FindStatus(Roleplay.SID.RolePlaying) != null)
            _zero.Execute(player, hints);
    }
}
