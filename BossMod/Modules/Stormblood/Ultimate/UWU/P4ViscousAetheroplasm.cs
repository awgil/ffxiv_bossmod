using System;
using System.Collections.Generic;
using System.Linq;

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
                // update avoid target to homing laser target
                AvoidTargets.Clear();
                var target = module.WorldState.Actors.Find(spell.TargetID);
                if (target != null)
                    AvoidTargets.Add(target);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ViscousAetheroplasmApply:
                    var target = module.WorldState.Actors.Find(spell.MainTargetID);
                    if (target != null)
                        StackTargets.Add(target);
                    AvoidTargets.AddRange(module.Raid.WithoutSlot(true).Where(a => a.InstanceID != spell.MainTargetID && a.Role == Role.Tank));
                    break;
                case AID.ViscousAetheroplasmResolve:
                    StackTargets.Clear();
                    break;
                case AID.HomingLasers:
                    AvoidTargets.Clear();
                    break;
            }
        }
    }

    class P5ViscousAetheroplasmTriple : Components.StackSpread
    {
        public int NumCasts { get; private set; }
        private List<(Actor target, DateTime resolve)> _aetheroplasms = new();

        public P5ViscousAetheroplasmTriple() : base(4, 0, 8) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.ViscousAetheroplasm)
            {
                _aetheroplasms.Add((actor, status.ExpireAt));
                _aetheroplasms.SortBy(a => a.resolve);
                UpdateStackTargets();
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.ViscousAetheroplasmResolve)
            {
                ++NumCasts;
                _aetheroplasms.RemoveAll(a => a.target.InstanceID == spell.MainTargetID);
                UpdateStackTargets();
            }
        }

        private void UpdateStackTargets()
        {
            StackTargets.Clear();
            if (_aetheroplasms.Count > 0)
            {
                StackTargets.Add(_aetheroplasms[0].target);
                ActivateAt = _aetheroplasms[0].resolve;
            }
        }
    }
}
