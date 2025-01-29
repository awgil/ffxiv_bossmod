using BossMod.Pathfinding;

namespace BossMod.Autorotation.MiscAI;

public sealed class NormalMovement(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Destination, Range, Cast, SpecialModes }
    public enum DestinationStrategy { None, Pathfind, Explicit }
    public enum RangeStrategy { Any, MaxMelee, MeleeGreedGCDExplicit, MeleeGreedLastMomentExplicit }
    public enum CastStrategy { Leeway, Explicit, Greedy, FinishMove, DropMove, FinishInstants, DropInstants }
    public enum SpecialModesStrategy { Automatic, Ignore }

    public const float MeleeGreedTolerance = 0.15f;

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Automatic movement", "Automatically move character based on pathfinding or explicit coordinates.", "AI", "veyn", RotationModuleQuality.WIP, new(~0ul), 1000, 1, RotationModuleOrder.Movement);
        res.Define(Track.Destination).As<DestinationStrategy>("Destination", "Destination", 30)
            .AddOption(DestinationStrategy.None, "None", "No automatic movement")
            .AddOption(DestinationStrategy.Pathfind, "Pathfind", "Use standard pathfinding to find best position")
            .AddOption(DestinationStrategy.Explicit, "Explicit", "Move to specific point", supportedTargets: ActionTargets.Area);
        res.Define(Track.Range).As<RangeStrategy>("Range", "Range", 20)
            .AddOption(RangeStrategy.Any, "Any", "Go directly to destination")
            .AddOption(RangeStrategy.MaxMelee, "MaxMelee", "Stay within max-melee of target closest to destination", supportedTargets: ActionTargets.Hostile)
            .AddOption(RangeStrategy.MeleeGreedGCDExplicit, "MeleeGreedGCDExplicit", "Melee greed, wait until last gcd to move, ensure destination is reached by the plan entry end", supportedTargets: ActionTargets.Hostile)
            .AddOption(RangeStrategy.MeleeGreedLastMomentExplicit, "MeleeGreedLastMomentExplicit", "Melee greed, wait until last moment to move, ensure destination is reached by the plan entry end", supportedTargets: ActionTargets.Hostile)
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
        return res;
    }

    private readonly NavigationDecision.Context _navCtx = new();

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

            if (Hints.InteractWithTarget != null)
                Hints.GoalZones.Add(Hints.GoalSingleTarget(Hints.InteractWithTarget.Position, 2, 100)); // strongly prefer moving towards interact target
        }

        var speed = Player.FindStatus(ClassShared.SID.Sprint) != null ? 7.8f : 6;
        var destinationOpt = strategy.Option(Track.Destination);
        var destinationStrategy = destinationOpt.As<DestinationStrategy>();
        var navi = destinationStrategy switch
        {
            DestinationStrategy.Pathfind => NavigationDecision.Build(_navCtx, World, Hints, Player, speed),
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
                var toDestination = navi.Destination.Value - rangeReference.Position;
                var maxRange = Player.HitboxRadius + rangeReference.HitboxRadius + 3 - MeleeGreedTolerance;
                var range = toDestination.Length();
                if (range > maxRange)
                {
                    var uptimePosition = rangeReference.Position + maxRange / range * toDestination;
                    var uptimeToDestinationTime = (range - maxRange) / speed;
                    switch (rangeStrategy)
                    {
                        case RangeStrategy.MaxMelee:
                            navi.Destination = uptimePosition;
                            navi.LeewaySeconds -= uptimeToDestinationTime; // assume we'll want to reach destination later, so leeway has to be reduced
                            break;
                        case RangeStrategy.MeleeGreedGCDExplicit:
                        case RangeStrategy.MeleeGreedLastMomentExplicit:
                            navi.LeewaySeconds = destinationOpt.Value.ExpireIn - uptimeToDestinationTime;
                            if (navi.LeewaySeconds > (rangeStrategy == RangeStrategy.MeleeGreedGCDExplicit ? GCD : 0))
                                navi.Destination = uptimePosition;
                            break;
                    }
                }
                // else: destination is already in melee range, nothing to adjust here
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
        Hints.MaxCastTime = Math.Min(Hints.MaxCastTime, maxCastTime);
        Hints.ForceCancelCast |= castStrategy == CastStrategy.DropMove;
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
