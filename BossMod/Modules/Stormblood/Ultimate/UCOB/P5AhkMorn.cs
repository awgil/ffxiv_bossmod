namespace BossMod.Stormblood.Ultimate.UCOB;

class P5AhkMorn : Components.CastSharedTankbuster
{
    public P5AhkMorn() : base(ActionID.MakeSpell(AID.AkhMorn), 4) { }

    // cast is only a first hit, don't deactivate
    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell) { }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AkhMorn or AID.AkhMornAOE)
            ++NumCasts;
    }
}
