namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

class UnluckyLotAetherialSphere(BossModule module) : Components.GenericAOEs(module, AID.UnluckyLotAetherialSphere)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OptimalOffensiveMoveSphere)
            _aoe = new(new AOEShapeCircle(20), caster.Position, default, Module.CastFinishAt(spell, 2.6f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UnluckyLotAetherialSphere)
            _aoe = null;
    }
}
