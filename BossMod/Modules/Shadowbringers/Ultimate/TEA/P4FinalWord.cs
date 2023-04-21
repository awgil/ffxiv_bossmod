using System;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P4FinalWordDebuffs : P4ForcedMarchDebuffs
    {
        protected override WDir SafeSpotDirection(int slot) => Debuffs[slot] switch
        {
            Debuff.LightBeacon => new(0, -15), // N
            Debuff.DarkBeacon => new(0, 15), // S
            _ => new(0, 10), // slightly N of dark beacon
        };
    }

    class P4FinalWordStillnessMotion : Components.StayMove
    {
        private Requirement _first;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (_first != Requirement.None)
                return; // we've already seen first cast, so we no longer care - we assume stillness is always followed by motion and vice versa

            var req = (AID)spell.Action.ID switch
            {
                AID.OrdainedMotion => Requirement.Move,
                AID.OrdainedStillness => Requirement.Stay,
                _ => Requirement.None
            };
            if (req != Requirement.None)
            {
                _first = req;
                Array.Fill(Requirements, req);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.OrdainedMotionSuccess or AID.OrdainedMotionFail or AID.OrdainedStillnessSuccess or AID.OrdainedStillnessFail)
            {
                var slot = module.Raid.FindSlot(spell.MainTargetID);
                if (slot >= 0)
                {
                    Requirements[slot] = Requirements[slot] != _first ? Requirement.None : _first == Requirement.Move ? Requirement.Stay : Requirement.Move;
                }
            }
        }
    }
}
