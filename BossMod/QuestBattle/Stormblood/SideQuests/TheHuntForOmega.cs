using Dalamud.Game.ClientState.Conditions;

namespace BossMod.QuestBattle.Stormblood.SideQuests;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 275)]
internal class TheHuntForOmega(WorldState ws) : QuestBattle(ws)
{
    public override unsafe List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Waypoint(new Vector3(0.11f, -272.00f, 432.31f), false))
            .With(obj => {
                obj.OnConditionChange += (flag, value) =>
                    obj.CompleteIf(flag == ConditionFlag.BeingMoved && !value);
            }),

        new QuestObjective(ws)
            .WithConnection(new Waypoint(new Vector3(-0.46f, -258.10f, 415.93f), false))
            .WithConnection(new Vector3(-13.42f, -256.50f, 388.53f))
            .Hints((player, hints) => {
                var checker = hints.PotentialTargets.FirstOrDefault(x => x.Actor.OID == 0x1E3D);
                if (checker != null)
                {
                    if (checker.Actor.InCombat)
                    {
                        checker.Priority = -1;
                        if (player.TargetID == checker.Actor.InstanceID)
                            hints.ForcedTarget = player;
                    } else
                        checker.Priority = 1;
                }
            })
            .CompleteOnKilled(0x1E3D),

        new QuestObjective(ws)
            .WithInteract(0x1EA5B5)
            .With(obj => {
                obj.OnConditionChange += (flag, value) => obj.CompleteIf(flag == ConditionFlag.Jumping61 && !value);
            }),

        new QuestObjective(ws)
            .Hints((player, hints) => {
                foreach(var h in hints.PotentialTargets)
                    h.Priority = h.Actor.OID == 0x1E3E ? -1 : 1;
            })
            .WithInteract(0x1E84B0)
            .With(obj => {
                obj.OnConditionChange += (flag, value) => obj.CompleteIf(flag == ConditionFlag.Jumping61 && !value);
            }),

        new QuestObjective(ws)
            .Hints((player, hints) => {
                foreach(var h in hints.PotentialTargets)
                    h.Priority = h.Actor.OID is 0x1E3D or 0x1E3E ? -1 : 1;
            })
    ];
}
