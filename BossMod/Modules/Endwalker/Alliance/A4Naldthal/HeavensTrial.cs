using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A4Naldthal
{
    class HeavensTrial : BossModule.Component
    {
        private Actor? _stackTarget;
        private List<(Actor Caster, Actor Target)> _cones = new();

        private static float _stackRadius = 6;
        private static AOEShapeCone _coneShape = new(60, 15.Degrees());

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_stackTarget != null && _stackTarget != actor && !_cones.Any(c => c.Target == actor))
            {
                hints.Add("Stack!", !actor.Position.InCircle(_stackTarget.Position, _stackRadius));
            }
            if (_cones.Any(cone => actor != cone.Target && _coneShape.Check(actor.Position, cone.Caster.Position, Angle.FromDirection(cone.Target.Position - cone.Caster.Position))))
            {
                hints.Add("GTFO from cone!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var cone in _cones)
                _coneShape.Draw(arena, cone.Caster.Position, Angle.FromDirection(cone.Target.Position - cone.Caster.Position));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_stackTarget != null)
            {
                arena.Actor(_stackTarget, ArenaColor.Danger);
                arena.AddCircle(_stackTarget.Position, _stackRadius, ArenaColor.Safe);
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.HeavensTrialAOE))
                _stackTarget = module.WorldState.Actors.Find(actor.CastInfo.TargetID);
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.HeavensTrialAOE))
                _stackTarget = null;
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.IsSpell(AID.HeavensTrialConeStart))
            {
                var caster = module.WorldState.Actors.Find(info.CasterID);
                var target = module.WorldState.Actors.Find(info.MainTargetID);
                if (caster != null && target != null)
                    _cones.Add((caster, target));
            }
            else if (info.IsSpell(AID.HeavensTrialSmelting))
            {
                _cones.RemoveAll(i => i.Caster.InstanceID == info.CasterID);
            }
        }
    }
}
