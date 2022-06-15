using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A3Azeyma
{
    class DancingFlame : BossComponent
    {
        private List<WPos> _active = new();

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (ActiveZones(module).Any(z => actor.Position.InRect(z.Center, new WDir(1, 0), z.HalfSize.X, z.HalfSize.X, z.HalfSize.Z)))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var z in ActiveZones(module))
                arena.ZoneRect(z.Center, new WDir(1, 0), z.HalfSize.X, z.HalfSize.X, z.HalfSize.Z, ArenaColor.AOE);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.HauteAirFlare))
            {
                _active.Add(actor.Position + 40 * actor.Rotation.ToDirection());
            }
        }

        public override void OnEventEnvControl(BossModule module, uint featureID, byte index, uint state)
        {
            if (featureID == 0x800375A3 && index == 27 && state == 0x00080004)
                _active.Clear();
        }

        private IEnumerable<(WPos Center, WDir HalfSize)> ActiveZones(BossModule module)
        {
            if (_active.Count == 0)
                yield break;
            yield return (module.Bounds.Center, new(2.5f, 30));
            yield return (module.Bounds.Center, new(30, 2.5f));
            foreach (var c in _active)
                yield return (c, new(15, 15));
        }
    }
}
