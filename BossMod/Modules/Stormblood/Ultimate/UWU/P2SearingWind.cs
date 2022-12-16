namespace BossMod.Stormblood.Ultimate.UWU
{
    class P2SearingWind : Components.StackSpread
    {
        public P2SearingWind() : base(0, 14, alwaysShowSpreads: true, includeDeadTargets: true) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.InfernoHowl)
            {
                if (SpreadMask.None())
                    ActivateAt = module.WorldState.CurrentTime.AddSeconds(8);
                SpreadMask.Set(module.Raid.FindSlot(spell.TargetID));
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.SearingWind)
            {
                var (targetSlot, target) = module.Raid.WithSlot(true).IncludedInMask(SpreadMask).Closest(spell.TargetXZ);
                if (target != null)
                {
                    var status = target.FindStatus(SID.SearingWind);
                    if (status == null || (status.Value.ExpireAt - module.WorldState.CurrentTime).TotalSeconds < 6)
                    {
                        SpreadMask.Clear(targetSlot);
                    }
                }
                ActivateAt = module.WorldState.CurrentTime.AddSeconds(6);
            }
        }
    }
}
