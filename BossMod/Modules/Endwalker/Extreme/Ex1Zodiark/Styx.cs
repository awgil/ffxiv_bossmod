namespace BossMod.Endwalker.Extreme.Ex1Zodiark
{
    class Styx : Components.StackSpread
    {
        public int NumCasts { get; private set; }

        public Styx() : base(5, 0, 8) { }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.StyxAOE)
                ++NumCasts;
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Styx)
                StackMask.Set(module.Raid.FindSlot(actor.InstanceID));
        }
    }
}
