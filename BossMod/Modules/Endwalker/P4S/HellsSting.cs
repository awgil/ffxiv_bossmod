using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.P4S
{
    using static BossModule;

    // state related to hell's sting mechanic (part of curtain call sequence)
    class HellsSting : Component
    {
        public int NumCasts { get; private set; } = 0;

        private P4S _module;
        private List<float> _directions = new();

        private static float _coneHalfAngle = MathF.PI / 12; // not sure about this...

        public HellsSting(P4S module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var boss = _module.Boss2();
            if (NumCasts >= _directions.Count * 2 || boss == null)
                return;

            var offset = actor.Position - boss.Position;
            if (ConeDirections().Any(x => GeometryUtils.PointInCone(offset, x, _coneHalfAngle)))
            {
                hints.Add("GTFO from cone!");
            }
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            var boss = _module.Boss2();
            if (NumCasts >= _directions.Count * 2 || boss == null)
                return;

            foreach (var dir in ConeDirections())
            {
                arena.ZoneCone(boss.Position, 0, 50, dir - _coneHalfAngle, dir + _coneHalfAngle, arena.ColorAOE);
            }
        }

        public override void OnCastStarted(Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.HellsStingAOE1))
                _directions.Add(actor.Rotation);
        }

        public override void OnCastFinished(Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.HellsStingAOE1) || actor.CastInfo!.IsSpell(AID.HellsStingAOE2))
                ++NumCasts;
        }

        private IEnumerable<float> ConeDirections()
        {
            return NumCasts < _directions.Count ? _directions : _directions.Select(x => x + MathF.PI / 8);
        }
    }
}
