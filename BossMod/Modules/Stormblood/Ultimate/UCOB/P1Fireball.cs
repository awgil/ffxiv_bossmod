namespace BossMod.Stormblood.Ultimate.UCOB
{
    class P1Fireball : Components.UniformStackSpread
    {
        public P1Fireball() : base(4, 0, 4) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Fireball)
                AddStack(actor, module.WorldState.CurrentTime.AddSeconds(5.3f));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.Fireball)
                Stacks.Clear();
        }
    }
}
