namespace BossMod.QuestBattle.Shadowbringers.RoleQuests;

public class AutoNyelbert(WorldState ws) : UnmanagedRotation(ws, 20)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget == null || Player.DistanceToHitbox(primaryTarget) > 25)
            return;

        if (World.Party.LimitBreakCur == 10000)
        {
            //Hints.RecommendedRangeToTarget = 20;
            if ((primaryTarget.Position - Player.Position).Length() < 25)
                UseAction(Roleplay.AID.FallingStar, null, 10, primaryTarget.PosRot.XYZ());
        }

        var numAOETargets = Hints.NumPriorityTargetsInAOECircle(primaryTarget.Position, 5);

        if (MP < 800)
            UseAction(Roleplay.AID.RonkanBlizzard3, primaryTarget);
        else if (MP < 1800)
            UseAction(Roleplay.AID.RonkanFlare, primaryTarget);
        else if (numAOETargets > 1)
            UseAction(Roleplay.AID.RonkanFlare, primaryTarget);
        else
        {
            if (primaryTarget.OID is 0x2975 or 0x2977)
            {
                var dotRemaining = StatusDetails(primaryTarget, Roleplay.SID.Electrocution, Player.InstanceID).Left;
                if (dotRemaining < 5)
                    UseAction(Roleplay.AID.RonkanThunder3, primaryTarget);
            }

            UseAction(Roleplay.AID.RonkanFire3, primaryTarget);
        }
    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 686)]
public class NyelbertsLament(WorldState ws) : QuestBattle(ws)
{
    private readonly AutoNyelbert _ai = new(ws);

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-440.00f, -121.67f, -676.00f))
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            if (h.Actor.InCombat)
                h.Priority = 0;

        _ai.Execute(player, hints);
    }
}
