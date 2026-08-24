namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class NorthernCross(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance? AOE;

    private static readonly AOEShapeRect _shape = new(25, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(AOE);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 3)
            return;
        var offset = state switch
        {
            0x00200010 => -90.Degrees(),
            0x00020001 => 90.Degrees(),
            _ => default
        };
        if (offset != default)
            AOE = new(_shape, Module.Center, -127.Degrees() + offset, WorldState.FutureTime(9.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NorthernCrossL or AID.NorthernCrossR)
        {
            ++NumCasts;
            AOE = null;
        }
    }
}
