namespace BossMod.Endwalker.Extreme.Ex6Golbez;

class BlackFang(BossModule module) : Components.CastCounter(module, default)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BlackFangAOE1 or AID.BlackFangAOE2 or AID.BlackFangAOE3 or AID.BlackFangEnrageAOE1 or AID.BlackFangEnrageAOE2 or AID.BlackFangEnrageAOE3)
            ++NumCasts;
    }
}
