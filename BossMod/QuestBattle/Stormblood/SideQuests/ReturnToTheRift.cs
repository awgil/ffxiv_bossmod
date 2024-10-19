using Dalamud.Game.ClientState.Conditions;

namespace BossMod.QuestBattle.Stormblood.SideQuests;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 471)]
internal class ReturnToTheRift(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(12.68f, -175.50f, 365.46f))
            .Hints((player, hints) => {
                var checker = hints.PotentialTargets.FirstOrDefault(x => x.Actor.OID == 0x21A4);
                if (checker != null)
                {
                    if (checker.Actor.InCombat)
                    {
                        checker.Priority = -1;
                        if (player.TargetID == checker.Actor.InstanceID)
                            hints.ForcedTarget = player;
                    } else
                    {
                        checker.Priority = 1;
                    }
                }
            })
            .CompleteOnKilled(0x21A4),

        new QuestObjective(ws)
            .WithInteract(0x1EA956)
            .With(obj => {
                obj.OnConditionChange += (flag, value) => obj.CompleteIf(flag == ConditionFlag.BetweenAreas && !value);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(151.57f, -89.46f, 325.45f))
            .WithConnection(new Waypoint(new Vector3(145.66f, -92.00f, 318.62f), false))
            .WithConnection(new Vector3(124.67f, -91.50f, 294.71f))
            .WithConnection(new Vector3(64.55f, -83.00f, 239.30f))
            .CompleteOnCreated(0x21A5),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-15.97f, -74.25f, 196.59f))
    ];
}
