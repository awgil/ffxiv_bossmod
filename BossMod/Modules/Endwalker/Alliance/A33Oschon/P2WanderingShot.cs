namespace BossMod.Endwalker.Alliance.A33Oschon;

class P2WanderingShot(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.GreatWhirlwind))
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle _shape = new(23);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        WPos coords = (AID)spell.Action.ID switch
        {
            AID.WanderingShotN or AID.WanderingVolleyN => new(-0.015f, 739.986f),
            AID.WanderingShotS or AID.WanderingVolleyS => new(-0.015f, 759.975f),
            _ => default
        };
        if (coords != default)
            _aoe = new(_shape, coords, default, spell.NPCFinishAt.AddSeconds(3.6f));
    }
}
