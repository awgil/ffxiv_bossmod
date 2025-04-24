namespace BossMod.Endwalker.Alliance.A33Oschon;

class P2WanderingShot(BossModule module) : Components.GenericAOEs(module, AID.GreatWhirlwind)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle _shape = new(23);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        WDir offset = (AID)spell.Action.ID switch
        {
            AID.WanderingShotN or AID.WanderingVolleyN => new(0, -10),
            AID.WanderingShotS or AID.WanderingVolleyS => new(0, +10),
            _ => default
        };
        if (offset != default)
            _aoe = new(_shape, Module.Center + offset, default, Module.CastFinishAt(spell, 3.6f));
    }
}
