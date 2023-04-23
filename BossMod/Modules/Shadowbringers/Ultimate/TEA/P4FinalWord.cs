using System;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P4FinalWordDebuffs : P4ForcedMarchDebuffs
    {
        protected override WDir SafeSpotDirection(int slot) => Debuffs[slot] switch
        {
            Debuff.LightBeacon => new(0, -15), // N
            Debuff.DarkBeacon => new(0, 13), // S
            _ => new(0, 10), // slightly N of dark beacon
        };

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.FinalWordContactProhibition:
                    AssignDebuff(module, actor, Debuff.LightFollow);
                    break;
                case SID.FinalWordContactRegulation:
                    AssignDebuff(module, actor, Debuff.LightBeacon);
                    LightBeacon = actor;
                    break;
                case SID.FinalWordEscapeProhibition:
                    AssignDebuff(module, actor, Debuff.DarkFollow);
                    break;
                case SID.FinalWordEscapeDetection:
                    AssignDebuff(module, actor, Debuff.DarkBeacon);
                    DarkBeacon = actor;
                    break;
                case SID.ContactProhibitionOrdained:
                case SID.ContactRegulationOrdained:
                case SID.EscapeProhibitionOrdained:
                case SID.EscapeDetectionOrdained:
                    Done = true;
                    break;
            }
        }

        private void AssignDebuff(BossModule module, Actor actor, Debuff debuff)
        {
            var slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                Debuffs[slot] = debuff;
        }
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
