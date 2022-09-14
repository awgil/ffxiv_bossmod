using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A4Naldthal
{
    // TODO: we can detect earlier (FiredUp cast + tether)
    class FortuneFlux : BossComponent
    {
        private SortedList<int, (Actor Caster, bool Knockback)> _casters = new();

        private static float _knockbackDistance = 30;
        private static AOEShapeCircle _aoeShape = new(20);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_casters.Count == 0)
                return;
            var knockback = _casters.Values.FirstOrDefault(i => i.Knockback).Caster?.CastInfo?.LocXZ;
            var adjPos = knockback != null ? Components.Knockback.AwayFromSource(actor.Position, knockback.Value, _knockbackDistance) : actor.Position;
            if (!module.Bounds.Contains(adjPos))
            {
                hints.Add("About to be knocked into wall!");
            }
            if (_casters.Values.Any(c => !c.Knockback && _aoeShape.Check(adjPos, c.Caster)))
            {
                hints.Add("GTFO from aoe!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (caster, _) in _casters.Values.Where(i => !i.Knockback))
                _aoeShape.Draw(arena, caster);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var source = _casters.Values.FirstOrDefault(i => i.Knockback).Caster?.CastInfo?.LocXZ;
            if (source != null)
            {
                var adjPos = Components.Knockback.AwayFromSource(pc.Position, source.Value, _knockbackDistance);
                arena.Actor(adjPos, pc.Rotation, ArenaColor.Danger);
                arena.AddLine(pc.Position, adjPos, ArenaColor.Danger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var (order, knockback) = (AID)spell.Action.ID switch
            {
                AID.FortuneFluxKnockback1 => (1, true),
                AID.FortuneFluxKnockback2 => (2, true),
                AID.FortuneFluxKnockback3 => (3, true),
                AID.FortuneFluxAOE1 => (1, false),
                AID.FortuneFluxAOE2 => (2, false),
                AID.FortuneFluxAOE3 => (3, false),
                _ => (0, false)
            };
            if (order != 0)
                _casters[order] = (caster, knockback);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            int order = (AID)spell.Action.ID switch
            {
                AID.FortuneFluxKnockback1 => 1,
                AID.FortuneFluxKnockback2 => 2,
                AID.FortuneFluxKnockback3 => 3,
                AID.FortuneFluxAOE1 => 1,
                AID.FortuneFluxAOE2 => 2,
                AID.FortuneFluxAOE3 => 3,
                _ => 0
            };
            if (order != 0)
                _casters.Remove(order);
        }
    }
}
