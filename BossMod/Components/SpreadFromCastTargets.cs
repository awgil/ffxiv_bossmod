using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic 'spread from target' component that shows circles around actors that are target of a specific cast
    public class SpreadFromCastTargets : CastCounter
    {
        public float Radius { get; private init; }
        private List<(Actor Caster, Actor Target)> _active = new();

        public SpreadFromCastTargets(ActionID aid, float radius) : base(aid)
        {
            Radius = radius;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_active.Any(e => e.Target != actor && e.Target.Position.InCircle(actor.Position, Radius)))
                hints.Add("Spread!");
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _active.Any(ct => ct.Target == player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var e in _active)
            {
                arena.AddCircle(e.Target.Position, Radius, ArenaColor.Danger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                var target = module.WorldState.Actors.Find(spell.TargetID);
                if (target != null)
                {
                    _active.Add((caster, target));
                }
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _active.RemoveAll(e => e.Caster == caster);
            }
        }
    }
}
