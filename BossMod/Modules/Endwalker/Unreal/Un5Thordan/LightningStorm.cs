namespace BossMod.Endwalker.Unreal.Un5Thordan;

// note: we don't use simple 'spread from cast targets', because casts are staggered a bit, which is ugly
class LightningStorm : Components.UniformStackSpread
{
    public LightningStorm() : base(0, 5, alwaysShowSpreads: true) { }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.LightningStorm)
            AddSpread(actor, module.WorldState.CurrentTime.AddSeconds(4));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LightningStormAOE)
            Spreads.Clear();
    }
}
