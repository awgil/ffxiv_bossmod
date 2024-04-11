namespace BossMod.Stormblood.Ultimate.UCOB;

class P5AhkMorn(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.AkhMorn), 4)
{
    // cast is only a first hit, don't deactivate
    public override void OnCastFinished(Actor caster, ActorCastInfo spell) { }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AkhMorn or AID.AkhMornAOE)
            ++NumCasts;
    }
}
