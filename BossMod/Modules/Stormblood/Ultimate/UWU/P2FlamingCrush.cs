namespace BossMod.Stormblood.Ultimate.UWU
{
    // assume everyone but healers stack
    class P2FlamingCrush : Components.StackSpread
    {
        public P2FlamingCrush() : base(4, 0, 6) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.FlamingCrush)
            {
                StackMask.Set(module.Raid.FindSlot(actor.InstanceID));
                AvoidMask = module.FindComponent<P2SearingWind>()?.SpreadMask ?? new();
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.FlamingCrush)
                StackMask = AvoidMask = new();
        }
    }
}
