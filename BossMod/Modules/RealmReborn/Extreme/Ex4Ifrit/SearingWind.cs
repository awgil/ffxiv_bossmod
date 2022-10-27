namespace BossMod.RealmReborn.Extreme.Ex4Ifrit
{
    class SearingWind : Components.StackSpread
    {
        private int _searingWindsLeft;

        public SearingWind() : base(0, 10) { } // TODO: verify range

        // no-op, AI handles positioning...
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.InfernoHowl)
            {
                SpreadMask.Set(module.Raid.FindSlot(spell.TargetID));
                ActivateAt = module.WorldState.CurrentTime.AddSeconds(6);
                _searingWindsLeft = 3;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            // note: there are 3 casts total, 6s apart - last one happens ~4.8s before status expires
            if ((AID)spell.Action.ID == AID.SearingWind)
            {
                if (--_searingWindsLeft == 0)
                    SpreadMask.Reset();
                else
                    ActivateAt = module.WorldState.CurrentTime.AddSeconds(6);
            }
        }
    }
}
