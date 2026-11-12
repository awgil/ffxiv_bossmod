using static BossMod.Components.Knockback;

namespace BossMod.Components;

// generic knockback/attract component; it's a cast counter for convenience
public abstract class Knockback(BossModule module, Enum? aid = default, bool ignoreImmunes = false, int maxCasts = int.MaxValue, bool stopAtWall = false) : CastCounter(module, aid)
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

    protected struct PlayerImmuneState
    {
        public DateTime RoleBuffExpire; // 0 if not active
        public DateTime JobBuffExpire; // 0 if not active
        public DateTime DutyBuffExpire; // 0 if not active

        public readonly bool ImmuneAt(DateTime time) => RoleBuffExpire > time || JobBuffExpire > time || DutyBuffExpire > time;
    }

    public bool IgnoreImmunes { get; init; } = ignoreImmunes;
    public bool StopAtWall = stopAtWall; // use if wall is solid rather than deadly
    public int MaxCasts = maxCasts; // use to limit number of drawn knockbacks
    protected PlayerImmuneState[] PlayerImmunes = new PlayerImmuneState[PartyState.MaxAllies];

    public bool IsImmune(int slot, DateTime time) => !IgnoreImmunes && PlayerImmunes[slot].ImmuneAt(time);

    public static WPos AwayFromSource(WPos pos, WPos origin, float distance) => pos != origin ? pos + distance * (pos - origin).Normalized() : pos;
    public static WVec AwayFromSource(WVec pos, WPos origin, float distance) => pos + distance * (pos - origin).Normalized();
    public static WPos AwayFromSource(WPos pos, Actor? source, float distance) => source != null ? AwayFromSource(pos, source.Position, distance) : pos;

    public static void DrawKnockback(WPos from, WPos to, Angle rot, MiniArena arena)
    {
        if (from != to)
        {
            arena.ActorProjected(from, to, rot, ArenaColor.Danger);
            if (arena.Config.ShowOutlinesAndShadows)
                arena.AddLine(from, to, 0xFF000000, 2);
            arena.AddLine(from, to, ArenaColor.Danger);
        }
    }
    public static void DrawKnockback(Actor actor, WPos adjPos, MiniArena arena) => DrawKnockback(actor.Position, adjPos, actor.Rotation, arena);

    // note: if implementation returns multiple sources, it is assumed they are applied sequentially (so they should be pre-sorted in activation order)
    public abstract IEnumerable<Source> Sources(int slot, Actor actor);

    // called to determine whether we need to show hint
    public bool DestinationUnsafe(int slot, Actor actor, WPos pos) => DestinationUnsafeImpl(slot, actor).CallOnce(pos);

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

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)ClassShared.SID.GuardPvP:
            case (uint)ClassShared.SID.Surecast:
            case (uint)WAR.SID.ArmsLength:
                if (Raid.TryFindSlot(actor, out var slot1))
                    PlayerImmunes[slot1].RoleBuffExpire = status.ExpireAt;
                break;
            case (uint)WAR.SID.InnerStrength:
            case (uint)BLU.SID.Diamondback:
            case (uint)BST.SID.Vileskin:
            case (uint)BST.SID.Covered:
                if (Raid.TryFindSlot(actor, out var slot2))
                    PlayerImmunes[slot2].JobBuffExpire = status.ExpireAt;
                break;
            case 2345: //Lost Manawall in Bozja
                if (Raid.TryFindSlot(actor, out var slot3))
                    PlayerImmunes[slot3].DutyBuffExpire = status.ExpireAt;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)ClassShared.SID.GuardPvP:
            case (uint)ClassShared.SID.Surecast:
            case (uint)WAR.SID.ArmsLength:
                if (Raid.TryFindSlot(actor, out var slot1))
                    PlayerImmunes[slot1].RoleBuffExpire = new();
                break;
            case (uint)WAR.SID.InnerStrength:
            case (uint)BLU.SID.Diamondback:
            case (uint)BST.SID.Vileskin:
            case (uint)BST.SID.Covered:
                if (Raid.TryFindSlot(actor, out var slot2))
                    PlayerImmunes[slot2].JobBuffExpire = new();
                break;
            case 2345: //Lost Manawall in Bozja
                if (Raid.TryFindSlot(actor, out var slot3))
                    PlayerImmunes[slot3].DutyBuffExpire = new();
                break;
        }
    }

    protected IEnumerable<(WPos from, WPos to)> CalculateMovements(int slot, Actor actor)
    {
        if (MaxCasts <= 0)
            yield break;

        var from = actor.Position;
        var count = 0;
        foreach (var s in Sources(slot, actor))
        {
            if (IsImmune(slot, s.Activation))
                continue; // this source won't affect player due to immunity
            if (s.Shape?.Check(from, s.Origin, s.Direction) == false)
                continue; // this source won't affect player due to being out of aoe

            var to = CalculateMovement(s, new(from))[0];
            yield return (from, to);
            from = to;

            if (++count == MaxCasts)
                break;
        }
    }

    protected WVec CalculateMovement(Source src, WVec starting)
    {
        starting = WVec.Select(src.Shape?.Check(starting, src.Origin, src.Direction) ?? VBool.AllBitsSet, starting, WVec.Invalid);

        WVec dir;
        var distance = VV.Create(src.Distance);

        void clamp(WVec dir) => distance = VV.Min(distance, CalculateDistance(starting, dir));

        switch (src.Kind)
        {
            case Kind.AwayFromOrigin:
                clamp(dir = (starting - src.Origin).Normalized());
                break;
            case Kind.TowardsOrigin:
                var off = src.Origin - starting;
                dir = off.Normalized();
                distance = VV.Clamp(off.Length() - src.MinDistance, VFloat.Zero, VV.Create(src.Distance));
                break;
            case Kind.DirForward:
                clamp(dir = new(src.Direction.ToDirection()));
                break;
            case Kind.DirLeft:
                clamp(dir = new(src.Direction.ToDirection().OrthoL()));
                break;
            case Kind.DirRight:
                clamp(dir = new(src.Direction.ToDirection().OrthoR()));
                break;
            default:
                return WVec.Invalid;
        }

        return starting + dir * distance;
    }

    /// <summary>
    /// <para>Check if the provided point is "dangerous", i.e. inside an AOE.</para>
    /// <remarks>The returned function is not expected to check if its input is in bounds. Bounds checking is handled by <c cref="DestinationOutOfBounds(int, Actor)">DestinationOutOfBounds</c>.</remarks>
    /// </summary>
    /// <returns></returns>
    public virtual PointTest DestinationUnsafe(int slot, Actor actor) => ShapeDistance.Union([.. Module.Components.OfType<GenericAOEs>().SelectMany(a => a.ActiveAOEs(slot, actor).Select(inst => inst.DistV))]).Discretize();
    /// <summary>
    /// <para>Check if the provided point is in bounds.</para>
    /// <remarks>Default implementation does nothing if <c cref="StopAtWall">StopAtWall</c> is true (since all points are expected to be in bounds).</remarks>
    /// </summary>
    /// <returns></returns>
    public virtual PointTest DestinationOutOfBounds(int slot, Actor actor) => StopAtWall ? PointTest.Never : ((PointTest)Module.InBounds).Inverted();
    /// <summary>
    /// <para>Calculate how far a player starting at <paramref name="position"/> would move in <paramref name="direction"/>.</para>
    /// <remarks>This value will be capped to the current <c cref="Source">Source</c>'s maximum distance, so if there is no applicable wall, you can just return a large positive number. Useful for temporary obstacles.</remarks>
    /// </summary>
    /// <param name="position">Position vector.</param>
    /// <param name="direction">Direction vector (normalized).</param>
    /// <returns></returns>
    public virtual VFloat CalculateDistance(WVec position, WVec direction) => StopAtWall ? Module.Arena.IntersectRayBounds(position, direction) - 0.1f : VV.Create(float.MaxValue);

    private PointTest DestinationUnsafeImpl(int slot, Actor actor) => DestinationUnsafe(slot, actor) | DestinationOutOfBounds(slot, actor);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
        {
            if (IsImmune(slot, src.Activation))
                continue;

            var isUnsafe = DestinationUnsafeImpl(slot, actor);

            hints.AddForbiddenZone(Sdf.Discrete(p =>
            {
                var proj = CalculateMovement(src, p);
                return proj.IsValid() & isUnsafe(proj);
            }), src.Activation);
        }
    }
}

// generic 'knockback from/attract to cast target' component
// TODO: knockback is really applied when effectresult arrives rather than when actioneffect arrives, this is important for ai hints (they can reposition too early otherwise)
public class KnockbackFromCastTarget(BossModule module, Enum aid, float distance, bool ignoreImmunes = false, int maxCasts = int.MaxValue, AOEShape? shape = null, Kind kind = Kind.AwayFromOrigin, float minDistance = 0, bool minDistanceBetweenHitboxes = false, bool stopAtWall = false)
    : Knockback(module, aid, ignoreImmunes, maxCasts, stopAtWall)
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
                yield return new(c.CastInfo.LocXZ, Distance, Module.CastFinishAt(c.CastInfo), Shape, c.CastInfo.Rotation, KnockbackKind, minDist);
            }
            else
            {
                var origin = WorldState.Actors.Find(c.CastInfo.TargetID)?.Position ?? c.CastInfo.LocXZ;
                yield return new(origin, Distance, Module.CastFinishAt(c.CastInfo), Shape, Angle.FromDirection(origin - c.Position), KnockbackKind, minDist);
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
