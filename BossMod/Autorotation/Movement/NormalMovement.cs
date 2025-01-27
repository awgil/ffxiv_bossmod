using BossMod.Pathfinding;

namespace BossMod.Autorotation;

// TODO: deal with casters...
public sealed class NormalMovement(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Destination, Range, SpecialModes }
    public enum DestinationStrategy { None, Pathfind, Explicit }
    public enum RangeStrategy { Any, MaxMelee, MeleeGreedGCDLeeway, MeleeGreedLastMomentLeeway, MeleeGreedGCDExplicit, MeleeGreedLastMomentExplicit }
    public enum SpecialModesStrategy { Automatic, Ignore }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Default Movement", "Automatically move character. Make sure this is ordered after standard rotation modules!", "Movement", "veyn", RotationModuleQuality.WIP, new(~0ul), 100);
        res.Define(Track.Destination).As<DestinationStrategy>("Destination", "Destination")
            .AddOption(DestinationStrategy.None, "None", "No automatic movement")
            .AddOption(DestinationStrategy.Pathfind, "Pathfind", "Use standard pathfinding to find best position")
            .AddOption(DestinationStrategy.Explicit, "Explicit", "Move to specific point", supportedTargets: ActionTargets.Area);
        res.Define(Track.Range).As<RangeStrategy>("Range", "Range")
            .AddOption(RangeStrategy.Any, "Any", "Go directly to destination")
            .AddOption(RangeStrategy.MaxMelee, "MaxMelee", "Stay within max-melee of target closest to destination", supportedTargets: ActionTargets.Hostile)
            .AddOption(RangeStrategy.MeleeGreedGCDLeeway, "MeleeGreedGCDLeeway", "Melee greed, wait until last gcd to move based on pathfinding leeway", supportedTargets: ActionTargets.Hostile)
            .AddOption(RangeStrategy.MeleeGreedLastMomentLeeway, "MeleeGreedLastMomentLeeway", "Melee greed, wait until last moment to move based on pathfinding leeway", supportedTargets: ActionTargets.Hostile)
            .AddOption(RangeStrategy.MeleeGreedGCDExplicit, "MeleeGreedGCDExplicit", "Melee greed, wait until last gcd to move, ensure destination is reached by the plan entry end", supportedTargets: ActionTargets.Hostile)
            .AddOption(RangeStrategy.MeleeGreedLastMomentExplicit, "MeleeGreedLastMomentExplicit", "Melee greed, wait until last moment to move, ensure destination is reached by the plan entry end", supportedTargets: ActionTargets.Hostile)
            /*.AddOption(RangeStrategy.Drag, "Drag", "Drag the target to specified spot, but maintain gcd uptime", supportedTargets: ActionTargets.Hostile)*/; // TODO
        res.Define(Track.SpecialModes).As<SpecialModesStrategy>("SpecialModes", "Special")
            .AddOption(SpecialModesStrategy.Automatic, "Automatic", "Automatically deal with special conditions (knockbacks, pyretics, etc)")
            .AddOption(SpecialModesStrategy.Ignore, "Ignore", "Ignore any special conditions (knockbacks, pyretics, etc)");
        return res;
    }

    private readonly NavigationDecision.Context _navCtx = new();

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (strategy.Option(Track.SpecialModes).As<SpecialModesStrategy>() == SpecialModesStrategy.Automatic)
        {
            if (Player.PendingKnockbacks.Count > 0)
                return; // do not move if there are any unresolved knockbacks - the positions are taken at resolve time, so we might fuck things up

            if (Hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Pyretic && Hints.ImminentSpecialMode.activation <= World.FutureTime(1))
                return; // pyretic is imminent, do not move
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
        var rangeReference = ResolveTargetOverride(rangeOpt.Value) ?? primaryTarget;
        if (rangeReference != null)
        {
            navi.Destination = rangeOpt.As<RangeStrategy>() switch
            {
                RangeStrategy.MaxMelee => ClosestInMelee(navi.Destination.Value, rangeReference),
                RangeStrategy.MeleeGreedGCDLeeway => MeleeGreedLeeway(navi.Destination.Value, rangeReference, navi.LeewaySeconds, GCD),
                RangeStrategy.MeleeGreedLastMomentLeeway => MeleeGreedLeeway(navi.Destination.Value, rangeReference, navi.LeewaySeconds, 0),
                RangeStrategy.MeleeGreedGCDExplicit => MeleeGreedExplicit(navi.Destination.Value, rangeReference, destinationOpt.Value.ExpireIn, GCD, speed),
                RangeStrategy.MeleeGreedLastMomentExplicit => MeleeGreedExplicit(navi.Destination.Value, rangeReference, destinationOpt.Value.ExpireIn, 0, speed),
                _ => navi.Destination
            };
        }

        var dir = navi.Destination.Value - Player.Position;
        Hints.ForcedMovement = dir.LengthSq() > 0.01f ? dir.ToVec3(Player.PosRot.Y) : default;
        // TODO: set max-cast and force-cancel-cast hints
        // TODO: misdirection support
    }

    private WPos ClosestInRange(WPos pos, WPos target, float maxRange)
    {
        var toDest = pos - target;
        var range = toDest.Length();
        return range <= maxRange ? pos : target + maxRange * toDest.Normalized();
    }

    private WPos ClosestInMelee(WPos pos, Actor target, float tolerance = 0.15f) => ClosestInRange(pos, target.Position, target.HitboxRadius + 3 - tolerance);
    private WPos MeleeGreedLeeway(WPos original, Actor target, float leeway, float minLeeway) => leeway > minLeeway ? ClosestInMelee(original, target) : original;
    private WPos MeleeGreedExplicit(WPos original, Actor target, float expire, float minLeeway, float speed) => ClosestInMelee(original, target) is var uptime && expire > minLeeway + (uptime - original).Length() / speed ? uptime : original;
}
