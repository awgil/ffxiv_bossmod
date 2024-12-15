using BossMod.AI;
using BossMod.Autorotation;
using BossMod.Pathfinding;

namespace BossMod.Dawntrail.Ultimate.FRU;

// note: this assumes the positions my static was using
sealed class FRUAI(RotationModuleManager manager, Actor player) : AIRotationModule(manager, player)
{
    public enum Track { Movement }
    public enum MovementStrategy { None, Pathfind, Prepull, DragToCenter, MaxMeleeNearest, ClockSpot }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("AI Experiment", "Experimental encounter-specific rotation", "Encounter AI", "veyn", RotationModuleQuality.WIP, new(~1ul), 100, 1, typeof(FRU));
        res.Define(Track.Movement).As<MovementStrategy>("Movement", "Movement")
            .AddOption(MovementStrategy.None, "None", "No automatic movement")
            .AddOption(MovementStrategy.Pathfind, "Pathfind", "Use standard pathfinding to move")
            .AddOption(MovementStrategy.Prepull, "Prepull", "Pre-pull position: as close to the clock-spot as possible")
            .AddOption(MovementStrategy.DragToCenter, "DragToCenter", "Drag boss to the arena center")
            .AddOption(MovementStrategy.MaxMeleeNearest, "MaxMeleeNearest", "Move to nearest spot in max-melee")
            .AddOption(MovementStrategy.ClockSpot, "ClockSpot", "Move to role-based clock-spot");
        return res;
    }

    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (Bossmods.ActiveModule is FRU module && module.Raid.FindSlot(Player.InstanceID) is var playerSlot && playerSlot >= 0)
        {
            SetForcedMovement(CalculateDestination(module, primaryTarget, strategy.Option(Track.Movement).As<MovementStrategy>(), Service.Config.Get<PartyRolesConfig>()[module.Raid.Members[playerSlot].ContentId]));
        }
    }

    private WPos CalculateDestination(FRU module, Actor? primaryTarget, MovementStrategy strategy, PartyRolesConfig.Assignment assignment) => strategy switch
    {
        MovementStrategy.Pathfind => PathfindPosition(),
        MovementStrategy.Prepull => PrepullPosition(module, assignment),
        MovementStrategy.DragToCenter => DragToCenterPosition(module),
        MovementStrategy.MaxMeleeNearest => primaryTarget != null ? primaryTarget.Position + 7.5f * (Player.Position - primaryTarget.Position).Normalized() : Player.Position,
        MovementStrategy.ClockSpot => ClockSpotPosition(module, assignment, 6),
        _ => Player.Position
    };

    // TODO: account for leeway for casters
    private WPos PathfindPosition()
    {
        var res = NavigationDecision.Build(NavigationContext, World, Hints, Player, Speed());
        return res.Destination ?? Player.Position;
    }

    // assumption: pull range is 12; hitbox is 5, so maxmelee is 8, meaning we have approx 4m to move during pull - with sprint, speed is 7.8, accel is 30 => over 0.26s accel period we move 1.014m, then need another 0.38s to reach boss (but it also moves)
    private WPos PrepullPosition(FRU module, PartyRolesConfig.Assignment assignment)
    {
        var safeRange = 12.5f;
        var pos = ClockSpotPosition(module, assignment, assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 ? 5 : 10);
        var off = pos - module.PrimaryActor.Position;
        var distSq = off.LengthSq();
        if (distSq >= safeRange * safeRange)
            return pos;
        off /= MathF.Sqrt(distSq);
        return module.PrimaryActor.Position + off * safeRange;
    }

    // notes on boss movement: boss top speed is ~8.5m, it moves up to distance 7.5m (both hitboxes + 2m)
    // empyrically, if i stand still, i can start moving when boss is ~11m away and it will still be dragged to intended spot
    private WPos DragToCenterPosition(FRU module)
    {
        if (module.PrimaryActor.Position.Z >= module.Center.Z - 1)
            return module.Center - new WDir(0, 6); // boss is positioned, go to N clockspot
        var dragDistance = module.PrimaryActor.HitboxRadius + Player.HitboxRadius + 2.25f; // we need to stay approx here, it's fine to overshoot a little bit - then when boss teleports, it won't turn
        var meleeDistance = module.PrimaryActor.HitboxRadius + Player.HitboxRadius + 2.75f; // -0.25 is a small extra leeway
        var dragDir = (module.Center - module.PrimaryActor.Position).Normalized();
        var dragSpot = module.Center + dragDistance * dragDir;
        var timeToMelee = ((dragSpot - module.PrimaryActor.Position).Length() - meleeDistance) / (Speed() + 8.5f); // assume 8.5 boss speed...
        return GCD > timeToMelee + 0.1f ? dragSpot : module.PrimaryActor.Position + meleeDistance * dragDir;
    }

    private WPos ClockSpotPosition(FRU module, PartyRolesConfig.Assignment assignment, float range)
    {
        var dir = _config.P1CyclonicBreakSpots[assignment];
        return dir >= 0 ? module.Center + range * (180 - 45 * dir).Degrees().ToDirection() : Player.Position;
    }
}
