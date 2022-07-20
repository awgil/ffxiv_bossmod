using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic component that shows arbitrary shape for any number of active casters with self-targeted casts; assumed to be avoidable aoe
    public class SelfTargetedAOEs : CastCounter
    {
        public AOEShape Shape { get; private init; }
        public int MaxCasts { get; private init; } // used for staggered aoes, when showing all active would be pointless
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;

        public SelfTargetedAOEs(ActionID aid, AOEShape shape, int maxCasts = int.MaxValue) : base(aid)
        {
            Shape = shape;
            MaxCasts = maxCasts;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_casters.Take(MaxCasts).Any(c => Shape.Check(actor.Position, c)))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in _casters.Take(MaxCasts))
                Shape.Draw(arena, c);
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
