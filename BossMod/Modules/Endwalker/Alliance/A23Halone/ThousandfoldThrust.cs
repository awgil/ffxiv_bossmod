namespace BossMod.Endwalker.Alliance.A23Halone;

class ThousandfoldThrust(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ThousandfoldThrustAOEFirst)
            _aoe = new(new AOEShapeCone(30, 90.Degrees()), caster.Position, spell.Rotation, spell.NPCFinishAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ThousandfoldThrustAOEFirst or AID.ThousandfoldThrustAOERest)
            ++NumCasts;
    }
}
