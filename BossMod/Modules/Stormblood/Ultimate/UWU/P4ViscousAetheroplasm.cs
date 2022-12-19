using System;
using System.Collections.Generic;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class P4ViscousAetheroplasmApply : Components.Cleave
    {
        public P4ViscousAetheroplasmApply() : base(ActionID.MakeSpell(AID.ViscousAetheroplasmApply), new AOEShapeCircle(2), (uint)OID.UltimaWeapon, originAtTarget: true) { }
    }

    // TODO: if aetheroplasm target is the same as homing laser target, assume it is being soaked solo; consider merging these two components
    class P4ViscousAetheroplasmResolve : Components.StackSpread
    {
        public P4ViscousAetheroplasmResolve() : base(4, 0, 7) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.HomingLasers)
            {
                AvoidMask.Reset();
                AvoidMask.Set(module.Raid.FindSlot(spell.TargetID)); // update avoid target to homing laser target
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ViscousAetheroplasmApply:
                    StackMask.Set(module.Raid.FindSlot(spell.MainTargetID));
                    AvoidMask = module.Raid.WithSlot(true).WhereActor(a => a.InstanceID != spell.MainTargetID && a.Role == Role.Tank).Mask();
                    break;
                case AID.ViscousAetheroplasmResolve:
                    StackMask = AvoidMask = new();
                    break;
                case AID.HomingLasers:
                    AvoidMask.Reset();
                    break;
            }
        }
    }

    class P5ViscousAetheroplasmTriple : Components.StackSpread
    {
        public int NumCasts { get; private set; }
        private List<(int slot, DateTime resolve)> _aetheroplasms = new();

        public P5ViscousAetheroplasmTriple() : base(4, 0, 8) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.ViscousAetheroplasm && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            {
                _aetheroplasms.Add((slot, status.ExpireAt));
                _aetheroplasms.SortBy(a => a.resolve);
                UpdateStackMask();
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.ViscousAetheroplasmResolve)
            {
                ++NumCasts;
                _aetheroplasms.RemoveAll(a => module.Raid[a.slot]?.InstanceID == spell.MainTargetID);
                UpdateStackMask();
            }
        }

        private void UpdateStackMask()
        {
            StackMask.Reset();
            if (_aetheroplasms.Count > 0)
            {
                StackMask.Set(_aetheroplasms[0].slot);
                ActivateAt = _aetheroplasms[0].resolve;
            }
        }
    }
}
