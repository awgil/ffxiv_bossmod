using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic knockback/attract component; it's a cast counter for convenience
    public abstract class Knockback : CastCounter
    {
        public enum Kind
        {
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

            public bool ImmuneAt(DateTime time) => RoleBuffExpire > time || JobBuffExpire > time || DutyBuffExpire > time;
        }

        public bool IgnoreImmunes { get; private init; }
        public bool StopAtWall; // use if wall is solid rather than deadly
        public int MaxCasts; // use to limit number of drawn knockbacks
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

        public Knockback(ActionID aid = new(), bool ignoreImmunes = false, int maxCasts = int.MaxValue) : base(aid)
        {
            IgnoreImmunes = ignoreImmunes;
            MaxCasts = maxCasts;
        }

        // note: if implementation returns multiple sources, it is assumed they are applied sequentially (so they should be pre-sorted in activation order)
        public abstract IEnumerable<Source> Sources(BossModule module, int slot, Actor actor);

        // called to determine whether we need to show hint
        public virtual bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => StopAtWall ? false : !module.Bounds.Contains(pos);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (CalculateMovements(module, slot, actor).Any(e => DestinationUnsafe(module, slot, actor, e.to)))
                hints.Add("About to be knocked into danger!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var e in CalculateMovements(module, pcSlot, pc))
                DrawKnockback(e.from, e.to, pc.Rotation, arena);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch (status.ID)
            {
                case 3054: //Guard in PVP
                case (uint)WHM.SID.Surecast:
                case (uint)WAR.SID.ArmsLength:
                    var slot1 = module.Raid.FindSlot(actor.InstanceID);
                    if (slot1 >= 0)
                        PlayerImmunes[slot1].RoleBuffExpire = status.ExpireAt;
                    break;
                case 1722: //Bluemage Diamondback
                case (uint)WAR.SID.InnerStrength:
                    var slot2 = module.Raid.FindSlot(actor.InstanceID);
                    if (slot2 >= 0)
                        PlayerImmunes[slot2].JobBuffExpire = status.ExpireAt;
                    break;
                case 2345: //Lost Manawall in Bozja
                    var slot3 = module.Raid.FindSlot(actor.InstanceID);
                    if (slot3 >= 0)
                        PlayerImmunes[slot3].DutyBuffExpire = status.ExpireAt;
                    break;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            switch (status.ID)
            {
                case 3054: //Guard in PVP
                case (uint)WHM.SID.Surecast:
                case (uint)WAR.SID.ArmsLength:
                    var slot1 = module.Raid.FindSlot(actor.InstanceID);
                    if (slot1 >= 0)
                        PlayerImmunes[slot1].RoleBuffExpire = new();
                    break;
                case 1722: //Bluemage Diamondback
                case (uint)WAR.SID.InnerStrength:
                    var slot2 = module.Raid.FindSlot(actor.InstanceID);
                    if (slot2 >= 0)
                        PlayerImmunes[slot2].JobBuffExpire = new();
                    break;
                case 2345: //Lost Manawall in Bozja
                    var slot3 = module.Raid.FindSlot(actor.InstanceID);
                    if (slot3 >= 0)
                        PlayerImmunes[slot3].DutyBuffExpire = new();
                    break;
            }
        }

        protected IEnumerable<(WPos from, WPos to)> CalculateMovements(BossModule module, int slot, Actor actor)
        {
            if (MaxCasts <= 0)
                yield break;

            var from = actor.Position;
            int count = 0;
            foreach (var s in Sources(module, slot, actor))
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
                    _ => new()
                };
                if (dir == default)
                    continue; // couldn't determine direction for some reason

                var distance = s.Distance;
                if (s.Kind is Kind.TowardsOrigin)
                    distance = Math.Min(s.Distance, (s.Origin - from).Length() - s.MinDistance);
                if (distance <= 0)
                    continue; // this could happen if attract starts from < min distance

                if (StopAtWall)
                    distance = Math.Min(distance, module.Bounds.IntersectRay(from, dir) - 0.001f);

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
    public class KnockbackFromCastTarget : Knockback
    {
        public float Distance;
        public AOEShape? Shape;
        public Kind KnockbackKind;
        public float MinDistance;
        public bool MinDistanceBetweenHitboxes;
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;

        public KnockbackFromCastTarget(ActionID aid, float distance, bool ignoreImmunes = false, int maxCasts = int.MaxValue, AOEShape? shape = null, Kind kind = Kind.AwayFromOrigin, float minDistance = 0, bool minDistanceBetweenHitboxes = false)
            : base(aid, ignoreImmunes, maxCasts)
        {
            Distance = distance;
            Shape = shape;
            KnockbackKind = kind;
            MinDistance = minDistance;
            MinDistanceBetweenHitboxes = minDistanceBetweenHitboxes;
        }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            foreach (var c in _casters)
            {
                // note that majority of knockback casts are self-targeted
                var minDist = MinDistance + (MinDistanceBetweenHitboxes ? actor.HitboxRadius + c.HitboxRadius : 0);
                if (c.CastInfo!.TargetID == c.InstanceID)
                {
                    yield return new(c.Position, Distance, c.CastInfo.NPCFinishAt, Shape, c.CastInfo.Rotation, KnockbackKind, minDist);
                }
                else
                {
                    var origin = module.WorldState.Actors.Find(c.CastInfo.TargetID)?.Position ?? c.CastInfo.LocXZ;
                    yield return new(origin, Distance, c.CastInfo.NPCFinishAt, Shape, Angle.FromDirection(origin - c.Position), KnockbackKind, minDist);
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Remove(caster);
        }
    }
}
