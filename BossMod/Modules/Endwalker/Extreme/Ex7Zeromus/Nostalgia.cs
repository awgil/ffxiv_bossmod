namespace BossMod.Endwalker.Extreme.Ex7Zeromus
{
    class NostalgiaDimensionalSurge : Components.LocationTargetedAOEs
    {
        public NostalgiaDimensionalSurge() : base(ActionID.MakeSpell(AID.NostalgiaDimensionalSurge), 5) { }
    }

    class Nostalgia : Components.CastCounter
    {
        public Nostalgia() : base(default) { }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.NostalgiaBury1 or AID.NostalgiaBury2 or AID.NostalgiaBury3 or AID.NostalgiaBury4 or AID.NostalgiaRoar1 or AID.NostalgiaRoar2 or AID.NostalgiaPrimalRoar)
                ++NumCasts;
        }
    }
}
