using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic knockback component; it's a cast counter for convenience
    public abstract class Knockback : CastCounter
    {
        public enum Kind
        {
            AwayFromOrigin, // standard knockback - specific distance along ray from origin to target
            DirForward, // directional knockback - forward along source's direction
            DirLeft, // directional knockback - forward along source's direction + 90 degrees
        }

        public struct Source
        {
            public WPos Origin;
            public float Distance;
            public Angle Direction;
            public AOEShape? Shape; // if null, assume it is unavoidable raidwide knockback
            public DateTime Activation;
            public Kind Kind;

            public Source(WPos origin, float distance, DateTime activation = new(), AOEShape? shape = null, Angle dir = new(), Kind kind = Kind.AwayFromOrigin)
            {
                Origin = origin;
                Distance = distance;
                Direction = dir;
                Shape = shape;
                Activation = activation;
                Kind = kind;
            }
        }

        protected struct PlayerImmuneState
        {
            public DateTime ArmsLengthSurecastExpire; // 0 if not active
            public DateTime InnerStrengthExpire; // 0 if not active

            public bool ImmuneAt(DateTime time) => ArmsLengthSurecastExpire > time || InnerStrengthExpire > time;
        }

        public bool IgnoreImmunes { get; private init; }
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

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (CalculateMovements(module, slot, actor).Any(e => !module.Bounds.Contains(e.to)))
                hints.Add("About to be knocked into wall!");
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
                case (uint)WHM.SID.Surecast:
                case (uint)WAR.SID.ArmsLength:
                    var slot1 = module.Raid.FindSlot(actor.InstanceID);
                    if (slot1 >= 0)
                        PlayerImmunes[slot1].ArmsLengthSurecastExpire = status.ExpireAt;
                    break;
                case (uint)WAR.SID.InnerStrength:
                    var slot2 = module.Raid.FindSlot(actor.InstanceID);
                    if (slot2 >= 0)
                        PlayerImmunes[slot2].InnerStrengthExpire = status.ExpireAt;
                    break;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            switch (status.ID)
            {
                case (uint)WHM.SID.Surecast:
                case (uint)WAR.SID.ArmsLength:
                    var slot1 = module.Raid.FindSlot(actor.InstanceID);
                    if (slot1 >= 0)
                        PlayerImmunes[slot1].ArmsLengthSurecastExpire = new();
                    break;
                case (uint)WAR.SID.InnerStrength:
                    var slot2 = module.Raid.FindSlot(actor.InstanceID);
                    if (slot2 >= 0)
                        PlayerImmunes[slot2].InnerStrengthExpire = new();
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
                    Kind.AwayFromOrigin => from != s.Origin ? (from - s.Origin).Normalized() : new(),
                    Kind.DirForward => s.Direction.ToDirection(),
                    Kind.DirLeft => s.Direction.ToDirection().OrthoL(),
                    _ => new()
                };
                if (dir == default)
                    continue; // couldn't determine direction for some reason

                var to = from + s.Distance * dir;
                yield return (from, to);
                from = to;

                if (++count == MaxCasts)
                    break;
            }
        }
    }

    // generic 'knockback away from cast target' component
    // TODO: knockback is really applied when effectresult arrives rather than when actioneffect arrives, this is important for ai hints (they can reposition too early otherwise)
    public class KnockbackFromCastTarget : Knockback
    {
        public float Distance;
        public AOEShape? Shape;
        public Kind KnockbackKind;
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;

        public KnockbackFromCastTarget(ActionID aid, float distance, bool ignoreImmunes = false, int maxCasts = int.MaxValue, AOEShape? shape = null, Kind kind = Kind.AwayFromOrigin)
            : base(aid, ignoreImmunes, maxCasts)
        {
            Distance = distance;
            Shape = shape;
            KnockbackKind = kind;
        }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            foreach (var c in _casters)
            {
                // note that majority of knockback casts are self-targeted
                if (c.CastInfo!.TargetID == c.InstanceID)
                {
                    yield return new(c.Position, Distance, c.CastInfo.FinishAt, Shape, c.CastInfo.Rotation, KnockbackKind);
                }
                else
                {
                    var origin = module.WorldState.Actors.Find(c.CastInfo.TargetID)?.Position ?? c.CastInfo.LocXZ;
                    yield return new(origin, Distance, c.CastInfo.FinishAt, Shape, c.CastInfo.Rotation, KnockbackKind); // TODO: not sure whether rotation should be this or Angle.FromDirection(origin - c.Position)...
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
