using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic knockback component; it's a cast counter for convenience
    public class Knockback : CastCounter
    {
        private BitMask _armsLengthSurecast;
        private BitMask _innerStrength;

        public BitMask Immune => _armsLengthSurecast | _innerStrength;

        public static WPos AwayFromSource(WPos pos, WPos origin, float distance) => pos != origin ? pos + distance * (pos - origin).Normalized() : pos;
        public static WPos AwayFromSource(WPos pos, Actor? source, float distance) => source != null ? AwayFromSource(pos, source.Position, distance) : pos;

        public Knockback(ActionID aid = new()) : base(aid) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch (status.ID)
            {
                case 160: // surecast
                case 1209: // arm's length
                    _armsLengthSurecast.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case 2663: // inner strength
                    _innerStrength.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            switch (status.ID)
            {
                case 160: // surecast
                case 1209: // arm's length
                    _armsLengthSurecast.Clear(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case 2663: // inner strength
                    _innerStrength.Clear(module.Raid.FindSlot(actor.InstanceID));
                    break;
            }
        }
    }

    // generic 'knockback away from caster' component
    public class KnockbackFromCaster : Knockback
    {
        public float Distance { get; private init; }
        public bool IgnoreImmunes { get; private init; }
        public int MaxCasts { get; private init; } // used for staggered knockbacks, when showing all active would be pointless
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        public IEnumerable<Actor> ActiveCasters => _casters.Take(MaxCasts);

        public KnockbackFromCaster(ActionID aid, float distance, int maxCasts = 1, bool ignoreImmunes = false)
            : base(aid)
        {
            Distance = distance;
            IgnoreImmunes = ignoreImmunes;
            MaxCasts = maxCasts;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if ((IgnoreImmunes || !Immune[slot]) && ActiveCasters.Any(c => !module.Bounds.Contains(AwayFromSource(actor.Position, c, Distance))))
                hints.Add("About to be knocked into wall!");
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (IgnoreImmunes || !Immune[slot])
            {
                // this is really basic - implementations should probably override
                if (Distance < module.Bounds.HalfSize && ActiveCasters.Any())
                {
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(module.Bounds.Center, Distance), ActiveCasters.First().CastInfo!.FinishAt);
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (IgnoreImmunes || !Immune[pcSlot])
            {
                foreach (var c in ActiveCasters)
                {
                    var adjPos = AwayFromSource(pc.Position, c, Distance);
                    arena.Actor(adjPos, pc.Rotation, ArenaColor.Danger);
                    arena.AddLine(pc.Position, adjPos, ArenaColor.Danger);
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
