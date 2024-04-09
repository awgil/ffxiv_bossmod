namespace BossMod.Endwalker.Unreal.Un5Thordan;

// note: we don't use simple 'spread from cast targets', because casts are staggered a bit, which is ugly
class LightningStorm(BossModule module) : Components.UniformStackSpread(module, 0, 5, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.LightningStorm)
            AddSpread(actor, WorldState.FutureTime(4));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LightningStormAOE)
            Spreads.Clear();
    }
}
