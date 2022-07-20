using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic 'puddles' component that shows circle aoes for casters that target specific location
    public class LocationTargetedAOEs : CastCounter
    {
        public AOEShapeCircle Shape { get; private init; }
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;

        public LocationTargetedAOEs(ActionID aid, float radius) : base(aid)
        {
            Shape = new(radius);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_casters.Any(c => Shape.Check(actor.Position, c.CastInfo!.LocXZ)))
                hints.Add("GTFO from puddle!");
        }

        public override void UpdateSafeZone(BossModule module, int slot, Actor actor, SafeZone zone)
        {
            foreach (var c in _casters)
                zone.ForbidZone(Shape, c.CastInfo!.LocXZ, new());
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in _casters)
                Shape.Draw(arena, c.CastInfo!.LocXZ);
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
