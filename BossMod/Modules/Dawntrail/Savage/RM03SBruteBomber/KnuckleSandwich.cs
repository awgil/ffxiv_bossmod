namespace BossMod.Dawntrail.Savage.RM03SBruteBomber;

class KnuckleSandwich(BossModule module) : Components.GenericSharedTankbuster(module, default, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.KnuckleSandwich)
        {
            Source = caster;
            Target = WorldState.Actors.Find(spell.TargetID);
            Activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.KnuckleSandwich or AID.KnuckleSandwichAOE)
        {
            ++NumCasts;
        }
    }
}
