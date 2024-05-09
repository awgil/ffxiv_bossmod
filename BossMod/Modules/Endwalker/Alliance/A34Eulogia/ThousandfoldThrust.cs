namespace BossMod.Endwalker.Alliance.A34Eulogia;

class ThousandfoldThrust(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private readonly static AOEShapeCone _shape = new(60, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ThousandfoldThrustAOEFirst)
            _aoe = new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ThousandfoldThrustAOEFirst or AID.ThousandfoldThrustAOERest)
        {
            ++NumCasts;
            _aoe = null;
        }
    }
}
