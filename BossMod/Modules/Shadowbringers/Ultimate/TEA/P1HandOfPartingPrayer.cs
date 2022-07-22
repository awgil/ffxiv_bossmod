namespace BossMod.Shadowbringers.Ultimate.TEA
{
    // TODO: determine when mechanic is selected; determine threshold
    class P1HandOfPartingPrayer : BossComponent
    {
        public bool Resolved { get; private set; }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.HandOfParting or AID.HandOfPrayer)
                Resolved = true;
        }
    }
}
