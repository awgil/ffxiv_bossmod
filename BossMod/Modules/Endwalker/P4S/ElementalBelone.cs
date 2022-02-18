using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.P4S
{
    using static BossModule;

    // state related to elemental belone mechanic (3 of 4 corners exploding)
    class ElementalBelone : Component
    {
        enum Element : uint { Acid, Fire, Water, Lighting, Unknown }

        private P4S _module;
        private List<WorldState.Actor> _helpers;
        private uint _cornerAssignments = 0; // (x >> (2*corner-id)) & 3 == element in corner
        private Element _safeElement = Element.Unknown;
        private List<Vector3> _imminentExplodingCorners = new();

        public ElementalBelone(P4S module)
        {
            _module = module;
            _helpers = module.Enemies(OID.Helper);
        }

        public void AssignSafespotFromBloodrake()
        {
            uint forbiddenCorners = 0;
            foreach (var actor in _helpers.Tethered(TetherID.Bloodrake))
                forbiddenCorners |= 1u << CornerFromPos(actor.Position);
            int safeCorner = BitOperations.TrailingZeroCount(~forbiddenCorners);
            _safeElement = (Element)((_cornerAssignments >> (2 * safeCorner)) & 3);
        }

        public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_imminentExplodingCorners.Where(p => GeometryUtils.PointInRect(actor.Position - p, Vector3.UnitX, 10, 10, 10)).Any())
            {
                hints.Add($"GTFO from exploding square");
            }
            if (_safeElement != Element.Unknown)
            {
                hints.Add($"Safe square: {_safeElement}", false);
            }
        }

        public override void DrawArenaBackground(MiniArena arena)
        {
            foreach (var p in _imminentExplodingCorners)
            {
                arena.ZoneQuad(p, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
            }
        }

        public override void OnCastStarted(WorldState.Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo!.Action.ID)
            {
                case AID.PinaxAcid:
                    _cornerAssignments |= (uint)Element.Acid << (2 * CornerFromPos(actor.Position));
                    break;
                case AID.PinaxLava:
                    _cornerAssignments |= (uint)Element.Fire << (2 * CornerFromPos(actor.Position));
                    break;
                case AID.PinaxWell:
                    _cornerAssignments |= (uint)Element.Water << (2 * CornerFromPos(actor.Position));
                    break;
                case AID.PinaxLevinstrike:
                    _cornerAssignments |= (uint)Element.Lighting << (2 * CornerFromPos(actor.Position));
                    break;
                case AID.PeriaktoiDangerAcid:
                case AID.PeriaktoiDangerLava:
                case AID.PeriaktoiDangerWell:
                case AID.PeriaktoiDangerLevinstrike:
                    _imminentExplodingCorners.Add(actor.Position);
                    break;
            }
        }

        private int CornerFromPos(Vector3 pos)
        {
            int corner = 0;
            if (pos.X > _module.Arena.WorldCenter.X)
                corner |= 1;
            if (pos.Z > _module.Arena.WorldCenter.Z)
                corner |= 2;
            return corner;
        }
    }
}
