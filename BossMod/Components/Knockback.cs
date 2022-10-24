using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic knockback component; it's a cast counter for convenience
    public class Knockback : CastCounter
    {
        public float Distance { get; private init; }
        public bool IgnoreImmunes { get; private init; }
        private BitMask _armsLengthSurecast;
        private BitMask _innerStrength;

        public BitMask Immune => _armsLengthSurecast | _innerStrength;
        public bool IsImmune(int slot) => !IgnoreImmunes && Immune[slot];

        public static WPos AwayFromSource(WPos pos, WPos origin, float distance) => pos != origin ? pos + distance * (pos - origin).Normalized() : pos;
        public static WPos AwayFromSource(WPos pos, Actor? source, float distance) => source != null ? AwayFromSource(pos, source.Position, distance) : pos;

        public static void DrawKnockback(Actor actor, WPos adjPos, MiniArena arena)
        {
            if (actor.Position != adjPos)
            {
                arena.Actor(adjPos, actor.Rotation, ArenaColor.Danger);
                arena.AddLine(actor.Position, adjPos, ArenaColor.Danger);
            }
        }

        public Knockback(float distance, ActionID aid = new(), bool ignoreImmunes = false) : base(aid)
        {
            Distance = distance;
            IgnoreImmunes = ignoreImmunes;
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch (status.ID)
            {
                case (uint)WHM.SID.Surecast:
                case (uint)WAR.SID.ArmsLength:
                    _armsLengthSurecast.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case (uint)WAR.SID.InnerStrength:
                    _innerStrength.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            switch (status.ID)
            {
                case (uint)WHM.SID.Surecast:
                case (uint)WAR.SID.ArmsLength:
                    _armsLengthSurecast.Clear(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case (uint)WAR.SID.InnerStrength:
                    _innerStrength.Clear(module.Raid.FindSlot(actor.InstanceID));
                    break;
            }
        }
    }

    // generic 'knockback away from points' component
    public abstract class KnockbackFromPoints : Knockback
    {
        public KnockbackFromPoints(float distance, ActionID aid = new(), bool ignoreImmunes = false) : base(distance, aid, ignoreImmunes) { }

        public abstract IEnumerable<WPos> Sources(BossModule module);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (!IsImmune(slot) && Sources(module).Any(s => !module.Bounds.Contains(AwayFromSource(actor.Position, s, Distance))))
                hints.Add("About to be knocked into wall!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!IsImmune(pcSlot))
            {
                foreach (var s in Sources(module))
                {
                    DrawKnockback(pc, AwayFromSource(pc.Position, s, Distance), arena);
                }
            }
        }

    }

    // generic 'knockback away from caster' component
    public class KnockbackFromCaster : KnockbackFromPoints
    {
        public int MaxCasts { get; private init; } // used for staggered knockbacks, when showing all active would be pointless
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        public IEnumerable<Actor> ActiveCasters => _casters.Take(MaxCasts);

        public KnockbackFromCaster(ActionID aid, float distance, int maxCasts = 1, bool ignoreImmunes = false)
            : base(distance, aid, ignoreImmunes)
        {
            MaxCasts = maxCasts;
        }

        public override IEnumerable<WPos> Sources(BossModule module) => ActiveCasters.Select(a => a.Position);

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (!IsImmune(slot))
            {
                // this is really basic - implementations should probably override
                if (Distance < module.Bounds.HalfSize && ActiveCasters.Any())
                {
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(module.Bounds.Center, Distance), ActiveCasters.First().CastInfo!.FinishAt);
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
