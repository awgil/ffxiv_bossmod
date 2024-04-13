namespace BossMod.Endwalker.Alliance.A33Oschon;

class GreatWhirlwind(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle circle = new(23);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        var _activation = WorldState.FutureTime(10.7f);

        if (index == 0x48)
        {
            if (state == 0x00200010)
                _aoe = new(circle, new WPos(-0.015f, 759.975f), default, _activation);
            if (state == 0x00020001)
                _aoe = new(circle, new WPos(-0.015f, 739.986f), default, _activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.GreatWhirlwind)
            _aoe = null;
    }
}
