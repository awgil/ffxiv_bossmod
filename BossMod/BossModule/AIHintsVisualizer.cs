using BossMod.Pathfinding;
using ImGuiNET;

namespace BossMod;

public class AIHintsVisualizer(AIHints hints, WorldState ws, Actor player, ulong targetID, Func<AIHints.Enemy?, (AIHints.Enemy? enemy, float range, Positional pos, bool tank)> targetSelect)
{
    private readonly MapVisualizer?[] _zoneVisualizers = new MapVisualizer?[hints.ForbiddenZones.Count];
    private MapVisualizer? _pathfindVisualizer;
    private readonly NavigationDecision.Context _naviCtx = new();
    private NavigationDecision _navi;

    public void Draw(UITree tree)
    {
        foreach (var _1 in tree.Node("Potential targets", hints.PotentialTargets.Count == 0))
        {
            tree.LeafNodes(hints.PotentialTargets, e => $"[{e.Priority}] {e.Actor} (str={e.AttackStrength:f2}), dist={(e.Actor.Position - player.Position).Length():f2}, tank={e.ShouldBeTanked}/{e.PreferProvoking}/{e.DesiredPosition}/{e.DesiredRotation}");
        }
        tree.LeafNode($"Forced target: {hints.ForcedTarget}");
        tree.LeafNode($"Forced movement: {hints.ForcedMovement}");
        tree.LeafNode($"Special movement: {hints.ImminentSpecialMode.mode} in {Math.Max(0, (hints.ImminentSpecialMode.activation - ws.CurrentTime).TotalSeconds):f3}s");
        foreach (var _1 in tree.Node("Forbidden zones", hints.ForbiddenZones.Count == 0))
        {
            for (int i = 0; i < hints.ForbiddenZones.Count; i++)
            {
                foreach (var _2 in tree.Node($"[{i}] activated at {Math.Max(0, (hints.ForbiddenZones[i].activation - ws.CurrentTime).TotalSeconds):f3}"))
                {
                    _zoneVisualizers[i] ??= BuildZoneVisualizer(hints.ForbiddenZones[i].shapeDistance);
                    _zoneVisualizers[i]!.Draw();
                }
            }
        }
        foreach (var _1 in tree.Node("Forbidden directions", hints.ForbiddenDirections.Count == 0))
        {
            tree.LeafNodes(hints.ForbiddenDirections, d => $"{d.center} +- {d.halfWidth}, at {Math.Max(0, (d.activation - ws.CurrentTime).TotalSeconds):f3}");
        }
        foreach (var _1 in tree.Node("Predicted damage", hints.PredictedDamage.Count == 0))
        {
            tree.LeafNodes(hints.PredictedDamage, d => $"[{string.Join(", ", ws.Party.WithSlot().IncludedInMask(d.players).Select(ia => ia.Item2.Name))}], at {Math.Max(0, (d.activation - ws.CurrentTime).TotalSeconds):f3}");
        }
        foreach (var _1 in tree.Node("Planned actions", hints.ActionsToExecute.Entries.Count == 0))
        {
            tree.LeafNodes(hints.ActionsToExecute.Entries, e => $"{e.Action} @ {e.Target} (priority {e.Priority})");
        }
        foreach (var _1 in tree.Node("Pathfinding"))
        {
            _pathfindVisualizer ??= BuildPathfindingVisualizer();
            _pathfindVisualizer!.Draw();
            ImGui.TextUnformatted($"Leeway={_navi.LeewaySeconds:f3}, ttg={_navi.TimeToGoal:f3}, dist={(_navi.Destination != null ? $"{(_navi.Destination.Value - player.Position).Length():f3}" : "---")}");
        }
    }

    private MapVisualizer BuildZoneVisualizer(Func<WPos, float> shape)
    {
        var map = new Map();
        hints.Bounds.PathfindMap(map, hints.Center);
        map.BlockPixelsInside(shape, 0, NavigationDecision.ForbiddenZoneCushion);
        return new MapVisualizer(map, player.Position, hints.Center, 3);
    }

    private MapVisualizer BuildPathfindingVisualizer()
    {
        var targetEnemy = targetID != 0 ? hints.PotentialTargets.FirstOrDefault(e => e.Actor.InstanceID == targetID) : null;
        var targeting = targetSelect(targetEnemy);
        _navi = BuildPathfind(targeting.enemy, targeting.range, targeting.pos, targeting.tank);
        return new MapVisualizer(_naviCtx.Map, player.Position, _navi.GoalPos, _navi.GoalRadius);
    }

    private NavigationDecision BuildPathfind(AIHints.Enemy? target, float range, Positional positional, bool preferTanking)
    {
        if (target == null)
            return NavigationDecision.Build(_naviCtx, ws, hints, player, null, 0, new(), Positional.Any);

        var adjRange = range + player.HitboxRadius + target.Actor.HitboxRadius;
        if (preferTanking)
        {
            // see whether we need to move target
            // TODO: think more about keeping uptime while tanking, this is tricky...
            var desiredToTarget = target.Actor.Position - target.DesiredPosition;
            if (desiredToTarget.LengthSq() > 4 /* && gcd check*/)
            {
                var dest = target.DesiredPosition - adjRange * desiredToTarget.Normalized();
                return NavigationDecision.Build(_naviCtx, ws, hints, player, dest, 0.5f, new(), Positional.Any);
            }
        }

        var adjRotation = preferTanking ? target.DesiredRotation : target.Actor.Rotation;
        return NavigationDecision.Build(_naviCtx, ws, hints, player, target.Actor.Position, adjRange, adjRotation, positional);
    }
}
