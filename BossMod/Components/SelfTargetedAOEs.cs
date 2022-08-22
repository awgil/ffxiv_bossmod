using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // self-targeted aoe that happens at the end of the cast
    public class SelfTargetedAOEs : GenericAOEs
    {
        public AOEShape Shape { get; private init; }
        public int MaxCasts { get; private init; } // used for staggered aoes, when showing all active would be pointless
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        public IEnumerable<Actor> ActiveCasters => _casters.Take(MaxCasts);

        public SelfTargetedAOEs(ActionID aid, AOEShape shape, int maxCasts = int.MaxValue) : base(aid)
        {
            Shape = shape;
            MaxCasts = maxCasts;
        }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module)
        {
            return ActiveCasters.Select(c => (Shape, c.Position, c.CastInfo!.Rotation, c.CastInfo!.FinishAt));
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

    // self-targeted aoe that uses current caster's rotation instead of rotation from cast-info - used by legacy modules written before i've reversed real cast rotation
    public class SelfTargetedLegacyRotationAOEs : GenericAOEs
    {
        public AOEShape Shape { get; private init; }
        public int MaxCasts { get; private init; } // used for staggered aoes, when showing all active would be pointless
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        public IEnumerable<Actor> ActiveCasters => _casters.Take(MaxCasts);

        public SelfTargetedLegacyRotationAOEs(ActionID aid, AOEShape shape, int maxCasts = int.MaxValue) : base(aid)
        {
            Shape = shape;
            MaxCasts = maxCasts;
        }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module)
        {
            return ActiveCasters.Select(c => (Shape, c.Position, c.Rotation, c.CastInfo!.FinishAt));
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
