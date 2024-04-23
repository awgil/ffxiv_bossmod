namespace BossMod.Endwalker.Ultimate.DSW2;

class P7FlamesIceOfAscalon(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCircle _shapeOut = new(8);
    private static readonly AOEShapeDonut _shapeIn = new(8, 50);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.GenericMechanic && (OID)actor.OID == OID.DragonKingThordan)
            _aoe = new(status.Extra == 0x12B ? _shapeIn : _shapeOut, actor.Position, default, WorldState.FutureTime(6.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FlamesOfAscalon or AID.IceOfAscalon)
        {
            ++NumCasts;
            _aoe = null;
        }
    }
}
