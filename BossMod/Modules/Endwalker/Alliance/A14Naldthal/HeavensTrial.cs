using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A14Naldthal
{
    class HeavensTrial : BossComponent
    {
        private Actor? _stackTarget;
        private List<(Actor Caster, Actor Target)> _cones = new();

        private static float _stackRadius = 6;
        private static AOEShapeCone _coneShape = new(60, 15.Degrees());

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
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

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return player == _stackTarget ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
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
                arena.AddCircle(_stackTarget.Position, _stackRadius, ArenaColor.Safe);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.HeavensTrialAOE)
                _stackTarget = module.WorldState.Actors.Find(spell.TargetID);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.HeavensTrialAOE)
                _stackTarget = null;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.HeavensTrialConeStart:
                    var target = module.WorldState.Actors.Find(spell.MainTargetID);
                    if (target != null)
                        _cones.Add((caster, target));
                    break;
                case AID.HeavensTrialSmelting:
                    _cones.RemoveAll(i => i.Caster.InstanceID == caster.InstanceID);
                    break;
            }
        }
    }
}
