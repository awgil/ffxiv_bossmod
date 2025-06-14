using BossMod.Pathfinding;

namespace BossMod.Autorotation.MiscAI;

public sealed class NormalMovement(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Destination, Range, Cast, SpecialModes, ForbiddenZoneCushion }
    public enum DestinationStrategy { None, Pathfind, Explicit }
    public enum RangeStrategy { Any, MaxRange, GreedGCDExplicit, GreedLastMomentExplicit, GreedAutomatic }
    public enum CastStrategy { Leeway, Explicit, Greedy, FinishMove, DropMove, FinishInstants, DropInstants }
    public enum ForbiddenZoneCushionStrategy { None, Small, Medium, Large }
    public enum SpecialModesStrategy { Automatic, Ignore }

    public const float GreedTolerance = 0.15f;

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Automatic movement", "Automatically move character based on pathfinding or explicit coordinates.", "AI", "veyn", RotationModuleQuality.Good, new(~0ul), 1000, 1, RotationModuleOrder.Movement, CanUseWhileRoleplaying: true);
        res.Define(Track.Destination).As<DestinationStrategy>("Destination", "Destination", 30)
            .AddOption(DestinationStrategy.None, "None", "No automatic movement")
            .AddOption(DestinationStrategy.Pathfind, "Pathfind", "Use standard pathfinding to find best position")
            .AddOption(DestinationStrategy.Explicit, "Explicit", "Move to specific point", supportedTargets: ActionTargets.Area);

        // note that these options used to be melee-specific - internal names are kept unchanged for convenience
        res.Define(Track.Range).As<RangeStrategy>("Range", "Range", 20)
            .AddOption(RangeStrategy.Any, "Any", "Go directly to destination")
            .AddOption(RangeStrategy.MaxRange, "MaxMelee", "Stay within maximum effective range of target closest to destination", supportedTargets: ActionTargets.Hostile)
            .AddOption(RangeStrategy.GreedGCDExplicit, "MeleeGreedGCDExplicit", "Stay within effective range until last GCD; ensure destination is reached by the plan entry end", supportedTargets: ActionTargets.Hostile)
            .AddOption(RangeStrategy.GreedLastMomentExplicit, "MeleeGreedLastMomentExplicit", "Stay within effective range until last possible moment; ensure destination is reached by the plan entry end", supportedTargets: ActionTargets.Hostile)
            .AddOption(RangeStrategy.GreedAutomatic, "MeleeGreedAutomatic", "Stay within effective range as long as possible; try to ensure safety is reached before mechanic resolves", supportedTargets: ActionTargets.Hostile)
            /*.AddOption(RangeStrategy.Drag, "Drag", "Drag the target to specified spot, but maintain gcd uptime", supportedTargets: ActionTargets.Hostile)*/; // TODO

        res.Define(Track.Cast).As<CastStrategy>("Cast", "Cast", 10)
            .AddOption(CastStrategy.Leeway, "Leeway", "Continue slidecasting as long as there is enough time to get to safety")
            .AddOption(CastStrategy.Explicit, "Explicit", "Continue slidecasting as long as there is enough time to reach destination by the plan entry end")
            .AddOption(CastStrategy.Greedy, "Greedy", "Don't stop casting, even when it risks getting clipped by aoes")
            .AddOption(CastStrategy.FinishMove, "FinishMove", "Start moving as soon as cast ends, use instants until destination is reached")
            .AddOption(CastStrategy.DropMove, "DropMove", "Start moving asap, interrupting casts if necessary, use instants until destination is reached")
            .AddOption(CastStrategy.FinishInstants, "FinishInstants", "Don't use any more casts after current cast ends")
            .AddOption(CastStrategy.DropInstants, "DropInstants", "Don't cast, interrupt current cast if needed");
        res.Define(Track.SpecialModes).As<SpecialModesStrategy>("SpecialModes", "Special", -1)
            .AddOption(SpecialModesStrategy.Automatic, "Automatic", "Automatically deal with special conditions (knockbacks, pyretics, etc)")
            .AddOption(SpecialModesStrategy.Ignore, "Ignore", "Ignore any special conditions (knockbacks, pyretics, etc)");
        res.Define(Track.ForbiddenZoneCushion).As<ForbiddenZoneCushionStrategy>("ForbiddenZoneCushion", "Overdodge", 25)
            .AddOption(ForbiddenZoneCushionStrategy.None, "None", "Do not use any buffer in pathfinding")
            .AddOption(ForbiddenZoneCushionStrategy.Small, "Small", "Prefer to stay 0.5y away from forbidden zones")
            .AddOption(ForbiddenZoneCushionStrategy.Medium, "Medium", "Prefer to stay 1.5y away from forbidden zones")
            .AddOption(ForbiddenZoneCushionStrategy.Large, "Large", "Prefer to stay 3y away from forbidden zones");
        return res;
    }

    private readonly NavigationDecision.Context _navCtx = new();

    public const float MeleeRange = 3;
    public const float CasterRange = 25;

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var castOpt = strategy.Option(Track.Cast);
        var castStrategy = castOpt.As<CastStrategy>();
        if (castStrategy is CastStrategy.FinishInstants or CastStrategy.DropInstants)
        {
            Hints.MaxCastTime = 0;
            Hints.ForceCancelCast |= castStrategy == CastStrategy.DropInstants;
        }

        var allowSpecialModes = strategy.Option(Track.SpecialModes).As<SpecialModesStrategy>() == SpecialModesStrategy.Automatic;
        if (allowSpecialModes)
        {
            if (Player.PendingKnockbacks.Count > 0)
                return; // do not move if there are any unresolved knockbacks - the positions are taken at resolve time, so we might fuck things up

            if (Hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Pyretic && Hints.ImminentSpecialMode.activation <= World.FutureTime(1))
            {
                Hints.ForceCancelCast = true; // this is only useful if autopyretic tweak is disabled
                return; // pyretic is imminent, do not move
            }

            if (Hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Freezing && Hints.ImminentSpecialMode.activation <= World.FutureTime(0.5f))
                Hints.WantJump = true;

            if (Hints.InteractWithTarget != null)
            {
                // strongly prefer moving towards interact target
                Hints.GoalZones.Add(p =>
                {
                    var length = (p - Hints.InteractWithTarget.Position).Length();

                    // 99% of eventobjects have an interact range of 3.5y, while the rest have a range of 2.09y
                    // checking only for the shorter range here would be fine in the vast majority of cases, but it can break interact pathfinding in the case that the target object is partially covered by a forbidden zone with a radius between 2.1 and 3.5
                    // this is specifically an issue in the metal gear thancred solo duty in endwalker
                    return length <= 2.09f ? 101 : length <= 3.5f ? 100 : 0;
                });
            }
        }

        var speed = World.Client.MoveSpeed;
        var destinationOpt = strategy.Option(Track.Destination);
        var destinationStrategy = destinationOpt.As<DestinationStrategy>();
        var cushionStrategy = strategy.Option(Track.ForbiddenZoneCushion).As<ForbiddenZoneCushionStrategy>();
        var cushionSize = cushionStrategy switch
        {
            ForbiddenZoneCushionStrategy.Small => 0.5f,
            ForbiddenZoneCushionStrategy.Medium => 1.5f,
            ForbiddenZoneCushionStrategy.Large => 3.0f,
            _ => 0f
        };
        var navi = destinationStrategy switch
        {
            DestinationStrategy.Pathfind => NavigationDecision.Build(_navCtx, World, Hints, Player, speed, forbiddenZoneCushion: cushionSize),
            DestinationStrategy.Explicit => new() { Destination = ResolveTargetLocation(destinationOpt.Value), TimeToGoal = destinationOpt.Value.ExpireIn },
            _ => default
        };
        if (navi.Destination == null)
            return; // nothing to do

        var rangeOpt = strategy.Option(Track.Range);
        var rangeStrategy = rangeOpt.As<RangeStrategy>();
        if (rangeStrategy != RangeStrategy.Any)
        {
            var rangeReference = ResolveTargetOverride(rangeOpt.Value) ?? primaryTarget;
            if (rangeReference != null)
            {
                // TODO: instead of hardcoding, is it possible to reuse goal zones for this purpose?
                // it would allow greeding AOE actions as well, but requires modification to NavigationDecision to avoid duplicating work
                var effectiveRange = Player.Role is Role.Tank or Role.Melee ? MeleeRange : CasterRange;
                var toDestination = navi.Destination.Value - rangeReference.Position;
                var maxRange = Player.HitboxRadius + rangeReference.HitboxRadius + effectiveRange - GreedTolerance;
                var range = toDestination.Length();
                if (range > maxRange)
                {
                    var uptimePosition = rangeReference.Position + maxRange / range * toDestination;
                    var uptimeToDestinationTime = (range - maxRange) / speed;
                    switch (rangeStrategy)
                    {
                        case RangeStrategy.MaxRange:
                            navi.Destination = uptimePosition;
                            navi.LeewaySeconds -= uptimeToDestinationTime; // assume we'll want to reach destination later, so leeway has to be reduced
                            break;
                        case RangeStrategy.GreedGCDExplicit:
                        case RangeStrategy.GreedLastMomentExplicit:
                            navi.LeewaySeconds = destinationOpt.Value.ExpireIn - uptimeToDestinationTime;
                            if (navi.LeewaySeconds > (rangeStrategy == RangeStrategy.GreedGCDExplicit ? GCD : 0))
                                navi.Destination = uptimePosition;
                            break;
                        case RangeStrategy.GreedAutomatic:
                            var uptimeCell = _navCtx.Map.GridToIndex(_navCtx.Map.WorldToGrid(uptimePosition));
                            var curCell = _navCtx.ThetaStar.StartNodeIndex;
                            if (navi.LeewaySeconds > 0)
                            {
                                if (_navCtx.Map.PixelMaxG[uptimeCell] >= _navCtx.Map.PixelMaxG[curCell])
                                    navi.Destination = uptimePosition;
                                else if (Player.DistanceToHitbox(primaryTarget) <= maxRange)
                                    navi.Destination = Player.Position;
                            }
                            break;
                    }
                }
                // else: destination is already in our effective range, nothing to adjust here
            }
        }

        var dir = navi.Destination.Value - Player.Position;
        var distSq = dir.LengthSq();
        if (distSq <= 0.01f)
        {
            // we're already very close to destination
            // TODO: what should we do if forced-movement is already set to something?.. not sure who could set it, some other module?..
            Hints.ForcedMovement = default;
            return;
        }

        // we want to move somewhere, check whether we're allowed to
        if (allowSpecialModes && Hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Misdirection && Hints.ImminentSpecialMode.activation <= World.CurrentTime)
        {
            // special case for misdirection
            // assume it's always fine to drop casts during misdirection (add new option to the specialmode track if it's ever not the case, i guess...)
            // we have only two options really - either move to the current forced direction, or wait (and this direction will change) - so see whether moving now brings us closer to the destination
            // if our destination is not the last one (turn != 0), we can only move if it will move us *further* from second-next point - otherwise we're moving towards the wall
            // the tolerance angle can be inferred from following consideration: in the worst case our movement should keep us at the same distance to destination (or it can move us closer)
            // so let's consider isosceles triangle with legs equal to distance to target, and base equal to distance we move over a period of time - the base angle is then our threshold
            // this means that cos(threshold) = speed * dt / 2 / distance
            // assuming we wanna move at least for a second, speed is standard 6, threshold of 60 degrees would be fine for distances >= 6
            // for micro adjusts, if we move for 1 frame (1/60s), threshold of 60 degrees would be fine for distance 0.1, which is our typical threshold
            var threshold = 30.Degrees();
            var allowMovement = World.Client.ForcedMovementDirection.AlmostEqual(Angle.FromDirection(dir), threshold.Rad);
            if (allowMovement && destinationStrategy == DestinationStrategy.Pathfind)
            {
                // if we have a map, we can try to see if current direction has long enough unobstructed path
                // TODO: maybe just check a single closest grid cell that we would intersect if we go forward?..
                allowMovement = CalculateUnobstructedPathLength(World.Client.ForcedMovementDirection) >= Math.Min(4, distSq);
            }
            Hints.ForcedMovement = allowMovement ? World.Client.ForcedMovementDirection.ToDirection().ToVec3(Player.PosRot.Y) : default;

            //var halfThreshold = Hints.MisdirectionThreshold; // even much smaller threshold seems to work fine in practice (TODO: reconsider...)
            //var idealDir = Angle.FromDirection(dir);
            //if (destinationStrategy == DestinationStrategy.Pathfind)
            //{
            //    var lenL = CalculateUnobstructedPathLength(idealDir + halfThreshold);
            //    var lenR = CalculateUnobstructedPathLength(idealDir - halfThreshold);
            //    if (lenL < 4)
            //        idealDir -= halfThreshold;
            //    if (lenR < 4)
            //        idealDir += halfThreshold;
            //}
            //var withinThreshold = World.Client.ForcedMovementDirection.AlmostEqual(idealDir, halfThreshold.Rad);
            //Hints.ForcedMovement = withinThreshold ? World.Client.ForcedMovementDirection.ToDirection().ToVec3(Player.PosRot.Y) : default;
        }
        else
        {
            // fine to move if we won't interrupt cast (or are explicitly allowed to)
            var allowMovement = Player.CastInfo == null || Player.CastInfo.EventHappened || castStrategy is CastStrategy.DropMove or CastStrategy.DropInstants;
            Hints.ForcedMovement = allowMovement ? dir.ToVec3(Player.PosRot.Y) : default;
        }

        var maxCastTime = castStrategy switch
        {
            CastStrategy.Leeway => navi.LeewaySeconds,
            CastStrategy.Explicit => castOpt.Value.ExpireIn,
            CastStrategy.Greedy => float.MaxValue,
            _ => 0,
        };
        Hints.MaxCastTime = Math.Max(0, Math.Min(Hints.MaxCastTime, maxCastTime));
        Hints.ForceCancelCast |= castStrategy == CastStrategy.DropMove;
        if (castStrategy is CastStrategy.Leeway && Player.CastInfo is { } castInfo)
        {
            var effectiveCastRemaining = Math.Max(0, castInfo.RemainingTime - 0.5f);
            if (Hints.MaxCastTime < effectiveCastRemaining)
            {
                Hints.ForceCancelCast = true;
                // no leeway, cast might have been initiated by user, keep moving
                Hints.ForcedMovement = dir.ToVec3(Player.PosRot.Y);
            }
        }
    }

    private float CalculateUnobstructedPathLength(Angle dir)
    {
        var start = _navCtx.Map.WorldToGrid(Player.Position);
        if (!_navCtx.Map.InBounds(start.x, start.y))
            return 0;

        var end = _navCtx.Map.WorldToGrid(Player.Position + 100 * dir.ToDirection());
        var startG = _navCtx.Map.PixelMaxG[_navCtx.Map.GridToIndex(start.x, start.y)];
        foreach (var p in _navCtx.Map.EnumeratePixelsInLine(start.x, start.y, end.x, end.y))
        {
            if (!_navCtx.Map.InBounds(p.x, p.y) || _navCtx.Map.PixelMaxG[_navCtx.Map.GridToIndex(p.x, p.y)] < startG)
            {
                var dest = _navCtx.Map.GridToWorld(p.x, p.y, 0.5f, 0.5f);
                return (dest - Player.Position).LengthSq();
            }
        }
        return float.MaxValue;
    }
}
