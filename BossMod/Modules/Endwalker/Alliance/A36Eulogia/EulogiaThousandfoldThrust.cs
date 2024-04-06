namespace BossMod.Endwalker.Alliance.A36Eulogia;

class ThousandfoldThrust : Components.GenericAOEs
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ThousandfoldThrustAOEFirst)
            _aoe = new(new AOEShapeCone(60, 90.Degrees()), caster.Position, spell.Rotation, spell.NPCFinishAt);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ThousandfoldThrustAOERest)
        {
            ++NumCasts;
            if (NumCasts % 5 == 0)
                _aoe = null;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ThousandfoldThrustAOEFirst)
            ++NumCasts;
    }
}
