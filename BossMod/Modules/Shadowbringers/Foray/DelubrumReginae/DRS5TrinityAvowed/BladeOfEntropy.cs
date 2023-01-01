using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5TrinityAvowed
{
    // note: instead of trying to figure out cone intersections and shit, we use the fact that clones are always positioned on grid and just check each cell
    class BladeOfEntropy : TemperatureAOE
    {
        private List<(Actor caster, WDir dir, int temperature)> _casters = new();

        private static AOEShapeRect _shapeCell = new(5, 5, 5);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            var playerTemp = Math.Clamp(Temperature(actor), -2, +2);
            for (int x = -2; x <= +2; ++x)
            {
                for (int z = -2; z <= +2; ++z)
                {
                    var cellCenter = module.Bounds.Center + 10 * new WDir(x, z);
                    int temperature = 0;
                    int numClips = 0;
                    DateTime activation = new();
                    foreach (var c in _casters)
                    {
                        activation = c.caster.CastInfo!.FinishAt;
                        if (c.dir.Dot(cellCenter - c.caster.Position) > 0)
                        {
                            temperature = c.temperature;
                            if (++numClips > 1)
                                break;
                        }
                    }

                    if (numClips > 1)
                        yield return new(_shapeCell, cellCenter, new(), activation);
                    else if (activation != default && temperature == -playerTemp)
                        yield return new(_shapeCell, cellCenter, new(), activation, ArenaColor.SafeFromAOE, false);
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.BladeOfEntropyAC11:
                case AID.BladeOfEntropyBC11:
                    _casters.Add((caster, spell.Rotation.ToDirection(), -1));
                    break;
                case AID.BladeOfEntropyAC21:
                case AID.BladeOfEntropyBC21:
                    _casters.Add((caster, spell.Rotation.ToDirection(), -2));
                    break;
                case AID.BladeOfEntropyAH11:
                case AID.BladeOfEntropyBH11:
                    _casters.Add((caster, spell.Rotation.ToDirection(), +1));
                    break;
                case AID.BladeOfEntropyAH21:
                case AID.BladeOfEntropyBH21:
                    _casters.Add((caster, spell.Rotation.ToDirection(), +2));
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.BladeOfEntropyAC11 or AID.BladeOfEntropyBC11 or AID.BladeOfEntropyAC21 or AID.BladeOfEntropyBC21 or AID.BladeOfEntropyAH11 or AID.BladeOfEntropyBH11 or AID.BladeOfEntropyAH21 or AID.BladeOfEntropyBH21)
                _casters.RemoveAll(c => c.caster == caster);
        }
    }
}
