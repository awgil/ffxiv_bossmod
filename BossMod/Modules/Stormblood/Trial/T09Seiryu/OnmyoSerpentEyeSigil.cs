namespace BossMod.Stormblood.Trial.T09Seiryu;

class OnmyoSerpentEyeSigil(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeDonut donut = new(6.95f, 30.05f); //adjusted sizes slightly since cast is done by helper with non identical position
    private static readonly AOEShapeCircle circle = new(12.05f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        var activation = Module.WorldState.FutureTime(5.6f);
        if (modelState == 32)
            _aoe = new(circle, Module.PrimaryActor.Position, default, activation);
        if (modelState == 33)
            _aoe = new(donut, Module.PrimaryActor.Position, default, activation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.OnmyoSigil2 or AID.SerpentEyeSigil2)
            _aoe = null;
    }
}
