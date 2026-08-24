namespace BossMod.Endwalker.Alliance.A23Halone;

class FurysAegis(BossModule module) : Components.CastCounter(module, default)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Shockwave or AID.FurysAegisAOE1 or AID.FurysAegisAOE2 or AID.FurysAegisAOE3 or AID.FurysAegisAOE4 or AID.FurysAegisAOE5 or AID.FurysAegisAOE6)
            ++NumCasts;
    }
}
