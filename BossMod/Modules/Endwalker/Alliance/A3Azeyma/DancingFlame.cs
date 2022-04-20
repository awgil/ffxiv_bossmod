using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.Alliance.A3Azeyma
{
    class DancingFlame : BossModule.Component
    {
        private List<Vector3> _active = new();

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (ActiveZones(module).Any(z => GeometryUtils.PointInRect(actor.Position - z.Center, Vector3.UnitX, z.HalfSize.X, z.HalfSize.X, z.HalfSize.Z)))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var z in ActiveZones(module))
                arena.ZoneRect(z.Center - z.HalfSize, z.Center + z.HalfSize, arena.ColorAOE);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.HauteAirFlare))
            {
                _active.Add(actor.Position + 40 * GeometryUtils.DirectionToVec3(actor.Rotation));
            }
        }

        public override void OnEventEnvControl(BossModule module, uint featureID, byte index, uint state)
        {
            if (featureID == 0x800375A3 && index == 27 && state == 0x00080004)
                _active.Clear();
        }

        private IEnumerable<(Vector3 Center, Vector3 HalfSize)> ActiveZones(BossModule module)
        {
            if (_active.Count == 0)
                yield break;
            yield return (module.Arena.WorldCenter, new(2.5f, 0, 30));
            yield return (module.Arena.WorldCenter, new(30, 0, 2.5f));
            foreach (var c in _active)
                yield return (c, new(15, 0, 15));
        }
    }
}
