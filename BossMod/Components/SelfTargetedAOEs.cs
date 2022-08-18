using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic component that shows arbitrary shape for any number of active casters with self-targeted casts; assumed to be avoidable aoe
    public abstract class GenericSelfTargetedAOEs : CastCounter
    {
        public AOEShape Shape { get; private init; }

        public GenericSelfTargetedAOEs(ActionID aid, AOEShape shape) : base(aid)
        {
            Shape = shape;
        }

        public abstract IEnumerable<(WPos, Angle, DateTime)> ImminentCasts(BossModule module);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (ImminentCasts(module).Any(c => Shape.Check(actor.Position, c.Item1, c.Item2)))
                hints.Add("GTFO from aoe!");
        }

        public override void UpdateSafeZone(BossModule module, int slot, Actor actor, SafeZone zone)
        {
            foreach (var (pos, rot, time) in ImminentCasts(module))
                zone.ForbidZone(Shape, pos, rot, time);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (pos, rot, _) in ImminentCasts(module))
                Shape.Draw(arena, pos, rot);
        }
    }

    // self-targeted aoe that happens at the end of the cast
    public class SelfTargetedAOEs : GenericSelfTargetedAOEs
    {
        public int MaxCasts { get; private init; } // used for staggered aoes, when showing all active would be pointless
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        public IEnumerable<Actor> ActiveCasters => _casters.Take(MaxCasts);

        public SelfTargetedAOEs(ActionID aid, AOEShape shape, int maxCasts = int.MaxValue) : base(aid, shape)
        {
            MaxCasts = maxCasts;
        }

        public override IEnumerable<(WPos, Angle, DateTime)> ImminentCasts(BossModule module)
        {
            return ActiveCasters.Select(c => (c.Position, c.Rotation, c.CastInfo!.FinishAt));
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
