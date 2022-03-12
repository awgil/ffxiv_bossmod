using System;

namespace BossMod.P4S1
{
    using static BossModule;

    // state related to shift mechanics
    class Shift : Component
    {
        private P4S1 _module;
        private AOEShapeCone _swordAOE = new(50, MathF.PI / 3); // not sure about half-angle...
        private Actor? _swordCaster;
        private Actor? _cloakCaster;

        private static float _knockbackRange = 30;

        public Shift(P4S1 module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_swordAOE.Check(actor.Position, _swordCaster))
            {
                hints.Add("GTFO from sword!");
            }
            else if (_cloakCaster != null && !_module.Arena.InBounds(AdjustPositionForKnockback(actor.Position, _cloakCaster, _knockbackRange)))
            {
                hints.Add("About to be knocked into wall!");
            }
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            _swordAOE.Draw(arena, _swordCaster);
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (_cloakCaster != null)
            {
                arena.AddCircle(_cloakCaster.Position, 5, arena.ColorSafe);

                var adjPos = AdjustPositionForKnockback(pc.Position, _cloakCaster, _knockbackRange);
                if (adjPos != pc.Position)
                {
                    arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
                    arena.Actor(adjPos, pc.Rotation, arena.ColorDanger);
                }
            }
        }

        public override void OnCastStarted(Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.ShiftingStrikeCloak))
                _cloakCaster = actor;
            else if (actor.CastInfo!.IsSpell(AID.ShiftingStrikeSword))
                _swordCaster = actor;
        }

        public override void OnCastFinished(Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.ShiftingStrikeCloak))
                _cloakCaster = null;
            else if (actor.CastInfo!.IsSpell(AID.ShiftingStrikeSword))
                _swordCaster = null;
        }
    }
}
