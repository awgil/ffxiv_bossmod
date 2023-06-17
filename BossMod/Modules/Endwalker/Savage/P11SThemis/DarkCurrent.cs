using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P11SThemis
{
    class DarkCurrent : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeCircle _shape = new(8);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var aoe in _aoes.Skip(3).Take(6))
                yield return aoe;
            foreach (var aoe in _aoes.Take(3))
                yield return aoe;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.DarkCurrentAOEFirst or AID.DarkCurrentAOERest)
            {
                ++NumCasts;
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
                if (_aoes.Count > 2)
                    _aoes.AsSpan()[2].Color = ArenaColor.Danger;
            }
        }

        public override void OnEventEnvControl(BossModule module, uint directorID, byte index, uint state)
        {
            if (directorID == 0x800375B3 && index is 1 or 2 && state is 0x00020001 or 0x00200010)
            {
                // index 01 => start from N/S, 02 => start from E/W
                // state 00020001 => CW => 00080004 end, 00200010 => CCW => 00800004 end
                var startingAngle = index == 2 ? 90.Degrees() : 0.Degrees();
                var rotation = state == 0x00020001 ? -22.5f.Degrees() : 22.5f.Degrees();
                for (int i = 0; i < 8; ++i)
                {
                    var offset = 13 * (startingAngle + i * rotation).ToDirection();
                    var color = i == 0 ? ArenaColor.Danger : ArenaColor.AOE;
                    _aoes.Add(new(_shape, module.Bounds.Center, default, module.WorldState.CurrentTime.AddSeconds(7.1f + i * 1.1f), color));
                    _aoes.Add(new(_shape, module.Bounds.Center + offset, default, module.WorldState.CurrentTime.AddSeconds(7.1f + i * 1.1f), color));
                    _aoes.Add(new(_shape, module.Bounds.Center - offset, default, module.WorldState.CurrentTime.AddSeconds(7.1f + i * 1.1f), color));
                }
            }
        }
    }

    class BlindingLight : Components.SpreadFromCastTargets
    {
        public BlindingLight() : base(ActionID.MakeSpell(AID.BlindingLightAOE), 6) { }
    }
}
