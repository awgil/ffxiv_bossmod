namespace BossMod.Endwalker.Alliance.A33Oschon;

class GreatWhirlwind : Components.GenericAOEs
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle circle = new(23);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        var _activation = module.WorldState.CurrentTime.AddSeconds(10.7f);

        if (index == 0x48)
        {
            if (state == 0x00200010)
                _aoe = new(circle, new WPos(-0.015f, 759.975f), activation: _activation);
            if (state == 0x00020001)
                _aoe = new(circle, new WPos(-0.015f, 739.986f), activation: _activation);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.GreatWhirlwind)
            _aoe = null;
    }
}
