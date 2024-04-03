namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2StygimolochWarrior;

class UnrelentingCharge : Components.Knockback
{
    private Actor? _source;
    private DateTime _activation;

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        if (_source != null)
            yield return new(_source.Position, 10, _activation);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UnrelentingCharge)
        {
            _source = caster;
            _activation = spell.NPCFinishAt.AddSeconds(0.3f);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.UnrelentingChargeAOE)
        {
            ++NumCasts;
            _activation = module.WorldState.CurrentTime.AddSeconds(1.6f);
        }
    }
}
