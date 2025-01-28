using BossMod.Pathfinding;
using BossMod.Stormblood.Dungeon.D15GhimlytDark.D151MarkIIIBMagitekColossus;
using Dalamud.Utility;

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
        var res = new RotationModuleDefinition("Misc AI: Movement", "Automatically move character. Make sure this is ordered after standard rotation modules!", "Movement", "veyn", RotationModuleQuality.WIP, new(~0ul), 100);
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

        if (strategy.Option(Track.SpecialModes).As<SpecialModesStrategy>() == SpecialModesStrategy.Automatic)
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
        var navi = destinationOpt.As<DestinationStrategy>() switch
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
        if (dir.LengthSq() > 0.01f)
        {
            Hints.ForcedMovement = dir.ToVec3(Player.PosRot.Y);

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
        else
        {
            // we're already very close to destination
            // TODO: what should we do if forced-movement is already set to something?.. not sure who could set it, some other module?..
            Hints.ForcedMovement = default;
        }

        // TODO: misdirection support
    }
}
