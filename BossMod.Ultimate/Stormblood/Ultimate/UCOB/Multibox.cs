using BossMod.Autorotation;

namespace BossMod.Stormblood.Ultimate.UCOB;

class Multibox(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Auto-UCOB", "It does what it says", "AI", "xan", RotationModuleQuality.Basic, new(~0ul), 70, 70, RelatedBossModule: typeof(UCOB));
    }

    private readonly PartyRolesConfig partyRolesConfig = Service.Config.Get<PartyRolesConfig>();

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (World.Party.WithoutSlot(includeDead: true).Any(p => p.IsDead) && Bossmods.ActiveModule!.StateMachine.ActivePhaseIndex < 4)
            Hints.ForcedMovement = -Player.DirectionTo(new WPos(0, 0)).ToVec3();

        var playerAssignment = partyRolesConfig[World.Party.Members[0].ContentId];

        if (!Player.InCombat && World.Client.CountdownRemaining > 0)
        {
            if (!(Player.FindStatus(48, DateTime.MaxValue)?.ExpireAt > World.FutureTime(600)))
            {
                var food = Player.Role switch
                {
                    Role.Healer => ActionDefinitions.IDFruitcake,
                    Role.Tank => ActionDefinitions.IDClamCake,
                    _ => ActionDefinitions.IDPopcorn
                };
                Hints.ActionsToExecute.Push(food, Player, ActionQueue.Priority.High);
            }

            if (primaryTarget != null)
            {
                var destination = primaryTarget.Position + primaryTarget.DirectionTo(Player) * (14 + primaryTarget.HitboxRadius + 0.5f);

                var sh = ShapeDistance.PrecisePosition(destination, new(0, 1), 0.5f, Player.Position, 0.1f);

                Hints.GoalZones.Add(p => sh(p) > 0 ? 5 : 0);
            }
        }
    }

    void SetStance(bool enabled)
    {
        var (stance, stanceBuff) = Player.Class switch
        {
            Class.WAR => (ActionID.MakeSpell(WAR.AID.Defiance), (uint)WAR.SID.Defiance),
            Class.PLD => (ActionID.MakeSpell(PLD.AID.IronWill), (uint)PLD.SID.IronWill),
            Class.DRK => (ActionID.MakeSpell(DRK.AID.Grit), (uint)DRK.SID.Grit),
            Class.GNB => (ActionID.MakeSpell(GNB.AID.RoyalGuard), (uint)GNB.SID.RoyalGuard),
            _ => (default, default)
        };

        if (stanceBuff == 0)
            return;

        var haveStance = Player.FindStatus(stanceBuff) != null;
        if (enabled != haveStance)
            Hints.ActionsToExecute.Push(stance, Player, ActionQueue.Priority.Medium);
    }
}
