using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Trials.T02Hydaelyn
{
    class WeaponTracker : Components.GenericAOEs
    {
        public enum Stance { Sword, Staff, Chakram }
        public Stance CurStance { get; private set; }
        private DateTime _activation;
        private static AOEShapeCross _aoeSword = new(40, 5);
        private static AOEShapeCircle _aoeStaff = new(10);
        private static AOEShapeDonut _aoeChakram = new(5, 40);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_activation != default)
            {
                if (CurStance == Stance.Sword)
                    yield return new(_aoeSword, module.PrimaryActor.Position, module.PrimaryActor.Rotation, activation: _activation);
                if (CurStance == Stance.Staff)
                    yield return new(_aoeStaff, module.PrimaryActor.Position, module.PrimaryActor.Rotation, activation: _activation);
                if (CurStance == Stance.Chakram)
                    yield return new(_aoeChakram, module.PrimaryActor.Position, module.PrimaryActor.Rotation, activation: _activation);
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.HydaelynsWeapon)
                _activation = module.WorldState.CurrentTime.AddSeconds(6);
            if ((SID)status.ID == SID.HydaelynsWeapon && status.Extra == 0x1B4)
                CurStance = Stance.Staff;
            if ((SID)status.ID == SID.HydaelynsWeapon && status.Extra == 0x1B5)
                CurStance = Stance.Chakram;
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.HydaelynsWeapon)
            {
                CurStance = Stance.Sword;
                _activation = module.WorldState.CurrentTime.AddSeconds(6.9f);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.Equinox2 or AID.HighestHoly or AID.Anthelion)
                _activation = default;
        }
    }
}
