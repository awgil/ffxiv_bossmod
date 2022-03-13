using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.P4S2
{
    using static BossModule;

    // state related to hell's sting mechanic (part of curtain call sequence)
    class HellsSting : Component
    {
        public int NumCasts { get; private set; } = 0;

        private P4S2 _module;
        private AOEShapeCone _cone = new(50, MathF.PI / 12); // not sure about half-width...
        private List<float> _directions = new();

        public HellsSting(P4S2 module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (NumCasts >= _directions.Count * 2)
                return;

            var offset = actor.Position - _module.PrimaryActor.Position;
            if (ConeDirections().Any(x => GeometryUtils.PointInCone(offset, x, _cone.HalfAngle)))
                hints.Add("GTFO from cone!");
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (NumCasts >= _directions.Count * 2)
                return;

            foreach (var dir in ConeDirections())
                _cone.Draw(arena, _module.PrimaryActor.Position, dir);
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
