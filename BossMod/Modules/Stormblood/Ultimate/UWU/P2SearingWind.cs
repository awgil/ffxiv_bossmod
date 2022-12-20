namespace BossMod.Stormblood.Ultimate.UWU
{
    class P2SearingWind : Components.StackSpread
    {
        public P2SearingWind() : base(0, 14, alwaysShowSpreads: true, includeDeadTargets: true) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.InfernoHowl)
            {
                if (SpreadTargets.Count == 0)
                    ActivateAt = module.WorldState.CurrentTime.AddSeconds(8);
                if (module.WorldState.Actors.Find(spell.TargetID) is var target && target != null)
                    SpreadTargets.Add(target);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.SearingWind)
            {
                var target = SpreadTargets.Closest(spell.TargetXZ);
                if (target != null)
                {
                    var status = target.FindStatus(SID.SearingWind);
                    if (status == null || (status.Value.ExpireAt - module.WorldState.CurrentTime).TotalSeconds < 6)
                    {
                        SpreadTargets.Remove(target);
                    }
                }
                ActivateAt = module.WorldState.CurrentTime.AddSeconds(6);
            }
        }
    }
}
