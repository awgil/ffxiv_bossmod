namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2StygimolochWarrior;

class UnrelentingCharge(BossModule module) : Components.Knockback(module)
{
    private Actor? _source;
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_source != null)
            yield return new(_source.Position, 10, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UnrelentingCharge)
        {
            _source = caster;
            _activation = spell.NPCFinishAt.AddSeconds(0.3f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.UnrelentingChargeAOE)
        {
            ++NumCasts;
            _activation = WorldState.FutureTime(1.6f);
        }
    }
}
