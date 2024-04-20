using static BossMod.Components.Knockback;

namespace BossMod.Components;

// generic knockback/attract component; it's a cast counter for convenience
public abstract class Knockback(BossModule module, ActionID aid = new(), bool ignoreImmunes = false, int maxCasts = int.MaxValue, bool stopAtWall = false, bool stopAfterWall = false, IEnumerable<SafeWall>? safeWalls = null) : CastCounter(module, aid)
{
    public enum Kind
    {
        None,
        AwayFromOrigin, // standard knockback - specific distance along ray from origin to target
        TowardsOrigin, // standard pull - "knockback" to source - forward along source's direction + 180 degrees
        DirForward, // directional knockback - forward along source's direction
        DirLeft, // directional knockback - forward along source's direction + 90 degrees
        DirRight, // directional knockback - forward along source's direction - 90 degrees
    }

    public record struct Source(
        WPos Origin,
        float Distance,
        DateTime Activation = default,
        AOEShape? Shape = null, // if null, assume it is unavoidable raidwide knockback/attract
        Angle Direction = default, // irrelevant for non-directional knockback/attract
        Kind Kind = Kind.AwayFromOrigin,
        float MinDistance = 0 // irrelevant for knockbacks
    );

    public record struct SafeWall(
        // for line segments
        WPos Vertex1 = default,
        WPos Vertex2 = default,
        // for circle segments
        WPos Center = default,
        float Radius = default,
        Angle StartAngle = default,
        Angle EndAngle = default
    );

    protected struct PlayerImmuneState
    {
        public DateTime RoleBuffExpire; // 0 if not active
        public DateTime JobBuffExpire; // 0 if not active
        public DateTime DutyBuffExpire; // 0 if not active

        public readonly bool ImmuneAt(DateTime time) => RoleBuffExpire > time || JobBuffExpire > time || DutyBuffExpire > time;
    }

    public IEnumerable<SafeWall> SafeWalls { get; init; } = safeWalls ?? [];
    public bool IgnoreImmunes { get; init; } = ignoreImmunes;
    public bool StopAtWall = stopAtWall; // use if wall is solid rather than deadly
    public bool StopAfterWall = stopAfterWall; // use if the wall is a polygon where you need to check for intersections
    public int MaxCasts = maxCasts; // use to limit number of drawn knockbacks
    protected PlayerImmuneState[] PlayerImmunes = new PlayerImmuneState[PartyState.MaxAllianceSize];

    public bool IsImmune(int slot, DateTime time) => !IgnoreImmunes && PlayerImmunes[slot].ImmuneAt(time);

    public static WPos AwayFromSource(WPos pos, WPos origin, float distance) => pos != origin ? pos + distance * (pos - origin).Normalized() : pos;
    public static WPos AwayFromSource(WPos pos, Actor? source, float distance) => source != null ? AwayFromSource(pos, source.Position, distance) : pos;

    public static void DrawKnockback(WPos from, WPos to, Angle rot, MiniArena arena)
    {
        if (from != to)
        {
            arena.Actor(to, rot, ArenaColor.Danger);
            if (arena.Config.ShowOutlinesAndShadows)
                arena.AddLine(from, to, 0xFF000000, 2);
            arena.AddLine(from, to, ArenaColor.Danger);
        }
    }
    public static void DrawKnockback(Actor actor, WPos adjPos, MiniArena arena) => DrawKnockback(actor.Position, adjPos, actor.Rotation, arena);

    // note: if implementation returns multiple sources, it is assumed they are applied sequentially (so they should be pre-sorted in activation order)
    public abstract IEnumerable<Source> Sources(int slot, Actor actor);

    // called to determine whether we need to show hint
    public virtual bool DestinationUnsafe(int slot, Actor actor, WPos pos) => StopAtWall ? false : !Module.Bounds.Contains(pos);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CalculateMovements(slot, actor).Any(e => DestinationUnsafe(slot, actor, e.to)))
            hints.Add("About to be knocked into danger!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var e in CalculateMovements(pcSlot, pc))
            DrawKnockback(e.from, e.to, pc.Rotation, Arena);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case 3054: //Guard in PVP
            case (uint)WHM.SID.Surecast:
            case (uint)WAR.SID.ArmsLength:
                var slot1 = Raid.FindSlot(actor.InstanceID);
                if (slot1 >= 0)
                    PlayerImmunes[slot1].RoleBuffExpire = status.ExpireAt;
                break;
            case 1722: //Bluemage Diamondback
            case (uint)WAR.SID.InnerStrength:
                var slot2 = Raid.FindSlot(actor.InstanceID);
                if (slot2 >= 0)
                    PlayerImmunes[slot2].JobBuffExpire = status.ExpireAt;
                break;
            case 2345: //Lost Manawall in Bozja
                var slot3 = Raid.FindSlot(actor.InstanceID);
                if (slot3 >= 0)
                    PlayerImmunes[slot3].DutyBuffExpire = status.ExpireAt;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case 3054: //Guard in PVP
            case (uint)WHM.SID.Surecast:
            case (uint)WAR.SID.ArmsLength:
                var slot1 = Raid.FindSlot(actor.InstanceID);
                if (slot1 >= 0)
                    PlayerImmunes[slot1].RoleBuffExpire = new();
                break;
            case 1722: //Bluemage Diamondback
            case (uint)WAR.SID.InnerStrength:
                var slot2 = Raid.FindSlot(actor.InstanceID);
                if (slot2 >= 0)
                    PlayerImmunes[slot2].JobBuffExpire = new();
                break;
            case 2345: //Lost Manawall in Bozja
                var slot3 = Raid.FindSlot(actor.InstanceID);
                if (slot3 >= 0)
                    PlayerImmunes[slot3].DutyBuffExpire = new();
                break;
        }
    }

    protected IEnumerable<(WPos from, WPos to)> CalculateMovements(int slot, Actor actor)
    {
        if (MaxCasts <= 0)
            yield break;

        var from = actor.Position;
        int count = 0;
        foreach (var s in Sources(slot, actor))
        {
            if (IsImmune(slot, s.Activation))
                continue; // this source won't affect player due to immunity
            if (s.Shape != null && !s.Shape.Check(from, s.Origin, s.Direction))
                continue; // this source won't affect player due to being out of aoe

            WDir dir = s.Kind switch
            {
                Kind.AwayFromOrigin => from != s.Origin ? (from - s.Origin).Normalized() : default,
                Kind.TowardsOrigin => from != s.Origin ? (s.Origin - from).Normalized() : default,
                Kind.DirForward => s.Direction.ToDirection(),
                Kind.DirLeft => s.Direction.ToDirection().OrthoL(),
                Kind.DirRight => s.Direction.ToDirection().OrthoR(),
                _ => default
            };
            if (dir == default)
                continue; // couldn't determine direction for some reason

            var distance = s.Distance;
            if (s.Kind is Kind.TowardsOrigin)
                distance = MathF.Min(s.Distance, (s.Origin - from).Length() - s.MinDistance);
            if (distance <= 0)
                continue; // this could happen if attract starts from < min distance

            if (StopAtWall)
                distance = MathF.Min(distance, Module.Bounds.IntersectRay(from, dir) - actor.HitboxRadius);
            if (StopAfterWall)
                distance = Math.Min(distance, Module.Bounds.IntersectRay(from, dir) + 0.1f);

            if (SafeWalls.Any())
            {
                float distanceToWall = float.MaxValue;
                foreach (var wall in SafeWalls)
                {
                    float t = float.MaxValue;

                    if (wall.Vertex1 != default && wall.Vertex2 != default) // Line segment
                        t = Intersect.RaySegment(from, dir, wall.Vertex1, wall.Vertex2);
                    else if (wall.Center != default && wall.Radius != 0) // Circle segment
                    {
                        t = Intersect.RayCircle(from, dir, wall.Center, wall.Radius);
                        if (t < float.MaxValue)
                        {
                            WPos intersection = from + t * dir;
                            Angle intersectionAngle = Angle.FromDirection(intersection - wall.Center).Normalized();

                            bool isWithinAngles;
                            if (wall.StartAngle <= wall.EndAngle)
                                isWithinAngles = intersectionAngle >= wall.StartAngle && intersectionAngle <= wall.EndAngle;
                            else // Angle wraps around at 0°
                                isWithinAngles = intersectionAngle >= wall.StartAngle || intersectionAngle <= wall.EndAngle;
                            if (!isWithinAngles)
                                t = float.MaxValue;
                        }
                    }
                    if (t < distanceToWall && t <= s.Distance)
                        distanceToWall = t;
                }
                if (distanceToWall < float.MaxValue)
                    distance = Math.Min(distance, distanceToWall - actor.HitboxRadius);
                else
                    distance = Math.Min(distance, Module.Bounds.IntersectRay(from, dir) + 0.1f);
            }
            var to = from + distance * dir;
            yield return (from, to);
            from = to;

            if (++count == MaxCasts)
                break;
        }
    }
}

// generic 'knockback from/attract to cast target' component
// TODO: knockback is really applied when effectresult arrives rather than when actioneffect arrives, this is important for ai hints (they can reposition too early otherwise)
public class KnockbackFromCastTarget(BossModule module, ActionID aid, float distance, bool ignoreImmunes = false, int maxCasts = int.MaxValue, AOEShape? shape = null, Kind kind = Kind.AwayFromOrigin, float minDistance = 0, bool minDistanceBetweenHitboxes = false, bool stopAtWall = false, bool stopAfterWall = false, IEnumerable<SafeWall>? safeWalls = null)
    : Knockback(module, aid, ignoreImmunes, maxCasts, stopAtWall, stopAfterWall, safeWalls)
{
    public float Distance = distance;
    public AOEShape? Shape = shape;
    public Kind KnockbackKind = kind;
    public float MinDistance = minDistance;
    public bool MinDistanceBetweenHitboxes = minDistanceBetweenHitboxes;
    public readonly List<Actor> Casters = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in Casters)
        {
            // note that majority of knockback casts are self-targeted
            var minDist = MinDistance + (MinDistanceBetweenHitboxes ? actor.HitboxRadius + c.HitboxRadius : 0);
            if (c.CastInfo!.TargetID == c.InstanceID)
            {
                yield return new(c.Position, Distance, c.CastInfo.NPCFinishAt, Shape, c.CastInfo.Rotation, KnockbackKind, minDist);
            }
            else
            {
                var origin = WorldState.Actors.Find(c.CastInfo.TargetID)?.Position ?? c.CastInfo.LocXZ;
                yield return new(origin, Distance, c.CastInfo.NPCFinishAt, Shape, Angle.FromDirection(origin - c.Position), KnockbackKind, minDist);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }
}
