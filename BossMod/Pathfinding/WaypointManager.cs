using System.Security.Cryptography;

namespace BossMod;
// for optional waypoints that have priority over automated pathfinding eg. Dohn Mheg boss 3
public class WaypointManager
{
    public readonly Queue<WPos> waypoints = new();
    private WPos? activeWaypoint;
    private DateTime activeWaypointSetTime;
    public float WaypointTimeLimit { get; set; }
    public BossModule? module;
    public WPos? CurrentWaypoint => activeWaypoint;
    public bool HasWaypoints => activeWaypoint.HasValue || waypoints.Count > 0;

    public void AddWaypoint(WPos waypoint)
    {
        waypoints.Enqueue(waypoint);
    }

    public void AddWaypointsWithRandomization(WPos[] waypointsList, float radius, int numRandomWaypoints)
    {
        for (var i = 0; i < waypointsList.Length - 1; i++)
        {
            AddWaypoint(waypointsList[i]);
            for (var j = 0; j < numRandomWaypoints; j++)
                AddWaypoint(GenerateRandomPointNearWaypoint(waypointsList[i + 1], radius));
        }
        AddWaypoint(waypointsList[^1]);
    }

    public void UpdateCurrentWaypoint(WPos actorPosition, float threshold = 0.1f)
    {
        if (activeWaypoint.HasValue)
        {
            if ((activeWaypoint.Value - actorPosition).Length() <= threshold)
            {
                activeWaypoint = waypoints.Count > 0 ? waypoints.Dequeue() : null;
                activeWaypointSetTime = module!.WorldState.CurrentTime;
            }
            else if ((module!.WorldState.CurrentTime - activeWaypointSetTime).TotalSeconds > WaypointTimeLimit)
            {
                ClearWaypoints();
                Service.Log("Waypoints cleared due to time limit");
            }
        }
        else if (waypoints.Count > 0)
        {
            activeWaypoint = waypoints.Dequeue();
            activeWaypointSetTime = module!.WorldState.CurrentTime;
        }
    }

    public void ClearWaypoints()
    {
        waypoints.Clear();
        activeWaypoint = null;
    }

    public static WPos GenerateRandomPointNearWaypoint(WPos waypoint, float radius)
    {
        using var rng = RandomNumberGenerator.Create();
        var angle = GetRandomFloat(rng) * 2 * Math.PI;
        var distance = GetRandomFloat(rng) * radius;
        var offsetX = distance * (float)Math.Cos(angle);
        var offsetZ = distance * (float)Math.Sin(angle);
        return new WPos(waypoint.X + offsetX, waypoint.Z + offsetZ);
    }

    private static float GetRandomFloat(RandomNumberGenerator rng)
    {
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        return BitConverter.ToUInt32(bytes, 0) / (float)uint.MaxValue;
    }
}
