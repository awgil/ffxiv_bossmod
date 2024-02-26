using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2Dahu
{
    class FirebreatheRotating : Components.GenericAOEs
    {
        private Angle _increment;
        private Angle _nextRotation;
        private DateTime _nextActivation;
        private static AOEShapeCone _shape = new(60, 45.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_increment != default && _nextActivation != default)
            {
                if (NumCasts < 5)
                    yield return new(_shape, module.PrimaryActor.Position, _nextRotation, _nextActivation, ArenaColor.Danger);
                if (NumCasts < 4)
                    yield return new(_shape, module.PrimaryActor.Position, _nextRotation + _increment, _nextActivation.AddSeconds(2));
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.FirebreatheRotating)
            {
                _nextRotation = spell.Rotation;
                _nextActivation = spell.NPCFinishAt.AddSeconds(0.7f);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.FirebreatheRotatingAOE)
            {
                NumCasts++;
                _nextRotation += _increment;
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            var angle = (IconID)iconID switch
            {
                IconID.FirebreatheCW => -90.Degrees(),
                IconID.FirebreatheCCW => 90.Degrees(),
                _ => default
            };
            if (angle != default)
                _increment = angle;
        }
    }
}
