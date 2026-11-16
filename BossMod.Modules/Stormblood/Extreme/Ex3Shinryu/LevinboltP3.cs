namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class LevinboltP3Hint(BossModule module) : Components.UniformStackSpread(module, 0, 5, alwaysShowSpreads: true, raidwideOnResolve: false)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LevinboltVisualP3)
            AddSpreads(Raid.WithoutSlot(), Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LevinboltVisualP3)
            Spreads.Clear();
    }
}

class LevinboltP3(BossModule module) : Components.StandardAOEs(module, AID.LevinboltP3, 5);
