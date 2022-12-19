namespace BossMod.Stormblood.Ultimate.UWU
{
    class FlamingCrush : Components.StackSpread
    {
        public FlamingCrush() : base(4, 0, 6) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.FlamingCrush)
            {
                StackMask.Set(module.Raid.FindSlot(actor.InstanceID));
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.FlamingCrush)
                StackMask = AvoidMask = new();
        }
    }

    // during P2, everyone except searing wind targets (typically two healers) should stack
    class P2FlamingCrush : FlamingCrush
    {
        public override void Init(BossModule module)
        {
            AvoidMask = module.FindComponent<P2SearingWind>()?.SpreadMask ?? new();
        }
    }

    // during P4 (annihilation), everyone should stack (except maybe ranged/caster that will handle mesohigh)
    class P4FlamingCrush : FlamingCrush { }

    // during P5 (suppression), everyone except mesohigh handler (typically tank) should stack
    class P5FlamingCrush : FlamingCrush
    {
        public override void Init(BossModule module)
        {
            AvoidMask = module.Raid.WithSlot(true).WhereActor(p => p.FindStatus(SID.ThermalLow) != null && p.Role != Role.Healer).Mask();
        }
    }
}
