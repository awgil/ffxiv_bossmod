using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A13Azeyma
{
    class SolarFlair : BossComponent
    {
        private Dictionary<ulong, WPos?> _sunstorms = new(); // null = cast finished, otherwise expected position

        private static float _kickDistance = 18;
        private static AOEShapeCircle _aoe = new(15);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (ActiveSunstorms(module).Any(s => _aoe.Check(actor.Position, s)))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var s in ActiveSunstorms(module))
                _aoe.Draw(arena, s);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.HauteAirWings)
            {
                var closestSunstorm = module.Enemies(OID.Sunstorm).Closest(caster.Position);
                if (closestSunstorm != null)
                {
                    _sunstorms[closestSunstorm.InstanceID] = closestSunstorm.Position + _kickDistance * caster.Rotation.ToDirection();
                }
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.SolarFlair)
                _sunstorms[caster.InstanceID] = null;
        }

        private IEnumerable<WPos> ActiveSunstorms(BossModule module)
        {
            foreach (var s in module.Enemies(OID.Sunstorm))
            {
                if (!_sunstorms.ContainsKey(s.InstanceID))
                    _sunstorms[s.InstanceID] = s.Position;
                var pos = _sunstorms[s.InstanceID];
                if (pos != null)
                    yield return pos.Value;
            }
        }
    }
}
