namespace BossMod.Stormblood.Ultimate.UWU
{
    class P4ViscousAetheroplasmApply : Components.Cleave
    {
        public P4ViscousAetheroplasmApply() : base(ActionID.MakeSpell(AID.ViscousAetheroplasmApply), new AOEShapeCircle(2), (uint)OID.UltimaWeapon, originAtTarget: true) { }
    }

    // TODO: this assumes it is always paired with homing lasers...
    class P4ViscousAetheroplasmResolve : Components.StackSpread
    {
        public P4ViscousAetheroplasmResolve() : base(4, 0, 7) { }

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
                    //AvoidMask.Set(module.Raid.FindSlot(spell.MainTargetID)); // update avoid target to homing laser target
                    break;
            }
        }
    }
}
