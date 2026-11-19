namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

class Levinbolt(BossModule module) : Components.UniformStackSpread(module, 0, 5, alwaysShowSpreads: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LevinboltVisual)
            AddSpreads(Raid.WithoutSlot(), Module.CastFinishAt(spell, 0.3f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Levinbolt)
            Spreads.Clear();
    }
}
