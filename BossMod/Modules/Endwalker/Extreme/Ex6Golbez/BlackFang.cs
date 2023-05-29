namespace BossMod.Endwalker.Extreme.Ex6Golbez
{
    class BlackFang : Components.CastCounter
    {
        public BlackFang() : base(default) { }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.BlackFangAOE1 or AID.BlackFangAOE2 or AID.BlackFangAOE3)
                ++NumCasts;
        }
    }
}
