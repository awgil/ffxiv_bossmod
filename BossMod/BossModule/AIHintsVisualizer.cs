using BossMod.Autorotation.xan;
using BossMod.Pathfinding;
using Dalamud.Bindings.ImGui;

namespace BossMod;

public class AIHintsVisualizer(AIHints hints, WorldState ws, Actor player, float preferredDistance, float cushionSize)
{
    private readonly MapVisualizer?[] _zoneVisualizers = new MapVisualizer?[hints.ForbiddenZones.Count];
    private MapVisualizer? _pathfindVisualizer;
    private readonly NavigationDecision.Context _naviCtx = new();
    private NavigationDecision _navi;
    private float _naviTime;
    private readonly TrackPartyHealth _partyHealth = new(ws);

    public void Draw(UITree tree)
    {
        _partyHealth.Update(hints);

        foreach (var _1 in tree.Node("Potential targets", hints.PotentialTargets.Count == 0))
        {
            tree.LeafNodes(hints.PotentialTargets, e => $"[{e.Priority}] {e.Actor} (str={e.AttackStrength:f2}), dist={(e.Actor.Position - player.Position).Length():f2}, tank={e.ShouldBeTanked}/{e.PreferProvoking}/{e.DesiredPosition}/{e.DesiredRotation}");
        }
        tree.LeafNode($"Forced target: {hints.ForcedTarget}{((hints.ForcedTarget?.IsTargetable ?? true) ? "" : " (untargetable)")}");
        tree.LeafNode($"Forced movement: {hints.ForcedMovement} (misdirection threshold={hints.MisdirectionThreshold})");
        tree.LeafNode($"Special movement: {hints.ImminentSpecialMode.mode} in {Math.Max(0, (hints.ImminentSpecialMode.activation - ws.CurrentTime).TotalSeconds):f3}s");
        foreach (var _1 in tree.Node("Forbidden zones", hints.ForbiddenZones.Count == 0))
        {
            for (int i = 0; i < hints.ForbiddenZones.Count; i++)
            {
                foreach (var _2 in tree.Node($"[{i}] activated at {Math.Max(0, (hints.ForbiddenZones[i].activation - ws.CurrentTime).TotalSeconds):f3}"))
                {
                    _zoneVisualizers[i] ??= BuildZoneVisualizer(hints.ForbiddenZones[i].containsFn);
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
            tree.LeafNodes(hints.PredictedDamage, d => $"[{string.Join(", ", ws.Party.WithSlot().IncludedInMask(d.Players).Select(ia => ia.Item2.Name))}] ({d.Type}), at {Math.Max(0, (d.Activation - ws.CurrentTime).TotalSeconds):f3}");
        }
        foreach (var _1 in tree.Node("Party health"))
        {
            var ph = _partyHealth.PartyHealth;
            tree.LeafNode($"Total: {ph.Count}");
            tree.LeafNode($"Average (current): {ph.AvgCurrent * 100:f2} / stddev {ph.StdDevCurrent * 100:f2}");
            tree.LeafNode($"Lowest HP ally (current): {ws.Party[ph.LowestHPSlotCurrent]}");
            tree.LeafNode($"Average (predicted): {ph.AvgPredicted * 100:f2} / stddev {ph.StdDevPredicted * 100:f2}");
            tree.LeafNode($"Lowest HP ally (predicted): {ws.Party[ph.LowestHPSlotPredicted]}");
        }
        foreach (var _1 in tree.Node("Planned actions", hints.ActionsToExecute.Entries.Count == 0))
        {
            tree.LeafNodes(hints.ActionsToExecute.Entries, e => $"{e.Action} @ {e.Target} (priority {e.Priority})");
        }
        foreach (var _1 in tree.Node("Pathfinding"))
        {
            ImGui.TextUnformatted($"Center={hints.PathfindMapCenter}, Bounds={hints.PathfindMapBounds}");
            ImGui.TextUnformatted($"Obstacles={hints.PathfindMapObstacles}");
            _pathfindVisualizer ??= BuildPathfindingVisualizer();
            _pathfindVisualizer!.Draw();
            ImGui.TextUnformatted($"Rasterize Time={_naviTime:f3}");
            ImGui.TextUnformatted($"Leeway={_navi.LeewaySeconds:f3}, ttg={_navi.TimeToGoal:f3}, dist={(_navi.Destination != null ? $"{(_navi.Destination.Value - player.Position).Length():f3}" : "---")}");
        }
    }

    private MapVisualizer BuildZoneVisualizer(Func<WPos, bool> shape)
    {
        var map = new Map();
        hints.InitPathfindMap(map);
        map.BlockPixelsInside(shape, 0);
        return new MapVisualizer(map, player.Position);
    }

    private MapVisualizer BuildPathfindingVisualizer()
    {
        // TODO: remove once the similar thing in AIBehaviour.BuildNavigationDecision is removed
        if (hints.GoalZones.Count == 0 && ws.Actors.Find(player.TargetID) is var target && target != null)
            hints.GoalZones.Add(hints.GoalSingleTarget(target, preferredDistance));

        var now = DateTime.Now;
        _navi = NavigationDecision.Build(_naviCtx, ws, hints, player, forbiddenZoneCushion: cushionSize);
        _naviTime = (float)(DateTime.Now - now).TotalSeconds;

        return new MapVisualizer(_naviCtx.Map, player.Position);
    }
}
