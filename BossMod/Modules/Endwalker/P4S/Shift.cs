using System;

namespace BossMod.P4S
{
    using static BossModule;

    // state related to shift mechanics
    class Shift : Component
    {
        private P4S _module;
        private Actor? _caster;
        private bool _isSword;

        private static float _coneHalfAngle = MathF.PI / 3; // not sure about this...
        private static float _knockbackRange = 30;

        public Shift(P4S module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_caster == null)
                return;

            if (_isSword && GeometryUtils.PointInCone(actor.Position - _caster.Position, _caster.Rotation, _coneHalfAngle))
            {
                hints.Add("GTFO from sword!");
            }
            else if (!_isSword && !_module.Arena.InBounds(AdjustPositionForKnockback(actor.Position, _caster, _knockbackRange)))
            {
                hints.Add("About to be knocked into wall!");
            }
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (_caster != null && _isSword)
            {
                arena.ZoneCone(_caster.Position, 0, 50, _caster.Rotation - _coneHalfAngle, _caster.Rotation + _coneHalfAngle, arena.ColorAOE);
            }
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (_caster != null && !_isSword)
            {
                arena.AddCircle(_caster.Position, 5, arena.ColorSafe);

                var adjPos = AdjustPositionForKnockback(pc.Position, _caster, _knockbackRange);
                if (adjPos != pc.Position)
                {
                    arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
                    arena.Actor(adjPos, pc.Rotation, arena.ColorDanger);
                }
            }
        }

        public override void OnCastStarted(Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.ShiftingStrikeCloak) || actor.CastInfo!.IsSpell(AID.ShiftingStrikeSword))
            {
                _caster = actor;
                _isSword = actor.CastInfo!.Action.ID == (uint)AID.ShiftingStrikeSword;
            }
        }

        public override void OnCastFinished(Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.ShiftingStrikeCloak) || actor.CastInfo!.IsSpell(AID.ShiftingStrikeSword))
                _caster = null;
        }
    }
}
