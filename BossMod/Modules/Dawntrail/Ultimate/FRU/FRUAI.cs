using BossMod.AI;
using BossMod.Autorotation;

namespace BossMod.Dawntrail.Ultimate.FRU;

// note: this assumes the positions my static was using
sealed class FRUAI(RotationModuleManager manager, Actor player) : AIRotationModule(manager, player)
{
    public enum Track { Movement }
    public enum MovementStrategy { None, Prepull, DragToCenter, MaxMeleeNearest, ClockSpot }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("AI Experiment", "Experimental encounter-specific rotation", "Encounter AI", "veyn", RotationModuleQuality.WIP, new(~1ul), 100, 1, typeof(FRU));
        res.Define(Track.Movement).As<MovementStrategy>("Movement", "Movement")
            .AddOption(MovementStrategy.None, "None", "No automatic movement")
            .AddOption(MovementStrategy.Prepull, "Prepull", "Pre-pull position: as close to the clock-spot as possible")
            .AddOption(MovementStrategy.DragToCenter, "DragToCenter", "Drag boss to the arena center")
            .AddOption(MovementStrategy.MaxMeleeNearest, "MaxMeleeNearest", "Move to nearest spot in max-melee")
            .AddOption(MovementStrategy.ClockSpot, "ClockSpot", "Move to role-based clock-spot");
        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (Bossmods.ActiveModule is FRU module && module.Raid.FindSlot(Player.InstanceID) is var playerSlot && playerSlot >= 0)
        {
            SetForcedMovement(CalculateDestination(module, primaryTarget, strategy.Option(Track.Movement).As<MovementStrategy>(), Service.Config.Get<PartyRolesConfig>()[module.Raid.Members[playerSlot].ContentId]));
        }
    }

    private WPos CalculateDestination(FRU module, Actor? primaryTarget, MovementStrategy strategy, PartyRolesConfig.Assignment assignment) => strategy switch
    {
        MovementStrategy.Prepull => PrepullPosition(module, assignment),
        MovementStrategy.DragToCenter => DragToCenterPosition(module),
        MovementStrategy.MaxMeleeNearest => primaryTarget != null ? ClosestInMelee(Player.Position, primaryTarget) : Player.Position,
        MovementStrategy.ClockSpot => ClockSpotPosition(module, assignment, 6),
        _ => Player.Position
    };

    // assumption: pull range is 12; hitbox is 5, so maxmelee is 8, meaning we have approx 4m to move during pull - with sprint, speed is 7.8, accel is 30 => over 0.26s accel period we move 1.014m, then need another 0.38s to reach boss (but it also moves)
    // TODO: R1/R2 can stand close to clockspots, H1/H2 can stand directly on clockspots
    private WPos PrepullPosition(FRU module, PartyRolesConfig.Assignment assignment) => assignment switch
    {
        _ => module.PrimaryActor.Position + new WDir(0, 12.5f)
    };

    private WPos DragToCenterPosition(FRU module)
    {
        if (module.PrimaryActor.Position.Z >= module.Center.Z - 1)
            return module.Center - new WDir(0, 6); // boss is positioned, go to N clockspot
        var dragSpot = module.Center + new WDir(0, 7.75f); // we need to stay approx here, it's fine to overshoot a little bit - then when boss teleports, it won't turn
        var meleeSpot = ClosestInMelee(dragSpot, module.PrimaryActor);
        return UptimeDowntimePos(dragSpot, meleeSpot, 0, GCD);
    }

    private WPos ClockSpotPosition(FRU module, PartyRolesConfig.Assignment assignment, float range) => assignment switch
    {
        PartyRolesConfig.Assignment.MT => module.Center + range * 180.Degrees().ToDirection(),
        PartyRolesConfig.Assignment.OT => module.Center + range * 0.Degrees().ToDirection(),
        PartyRolesConfig.Assignment.H1 => module.Center + range * (-90).Degrees().ToDirection(),
        PartyRolesConfig.Assignment.H2 => module.Center + range * 90.Degrees().ToDirection(),
        PartyRolesConfig.Assignment.M1 => module.Center + range * (-45).Degrees().ToDirection(),
        PartyRolesConfig.Assignment.M2 => module.Center + range * 45.Degrees().ToDirection(),
        PartyRolesConfig.Assignment.R1 => module.Center + range * (-135).Degrees().ToDirection(),
        PartyRolesConfig.Assignment.R2 => module.Center + range * 135.Degrees().ToDirection(),
        _ => Player.Position
    };
}
