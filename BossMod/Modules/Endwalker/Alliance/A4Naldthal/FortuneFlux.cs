using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A4Naldthal
{
    // TODO: we can detect earlier (FiredUp cast + tether)
    class FortuneFlux : BossModule.Component
    {
        private SortedList<int, (Actor Caster, bool Knockback)> _casters = new();

        private static float _knockbackDistance = 30;
        private static AOEShapeCircle _aoeShape = new(20);

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_casters.Count == 0)
                return;
            var knockback = _casters.Values.FirstOrDefault(i => i.Knockback).Caster?.CastInfo?.LocXZ;
            var adjPos = knockback != null ? BossModule.AdjustPositionForKnockback(actor.Position, knockback.Value, _knockbackDistance) : actor.Position;
            if (!module.Arena.InBounds(adjPos))
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
                var adjPos = BossModule.AdjustPositionForKnockback(pc.Position, source.Value, _knockbackDistance);
                arena.Actor(adjPos, new(), arena.ColorDanger);
                arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            var (order, knockback) = (AID)actor.CastInfo.Action.ID switch
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
                _casters[order] = (actor, knockback);
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            int order = (AID)actor.CastInfo.Action.ID switch
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
