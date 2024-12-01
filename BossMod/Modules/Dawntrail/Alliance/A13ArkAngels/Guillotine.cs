namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class Guillotine(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCone _shape = new(40, 120.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Guillotine)
        {
            _aoe = new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 0.6f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GuillotineAOE or AID.GuillotineAOELast)
        {
            ++NumCasts;
            if ((AID)spell.Action.ID == AID.GuillotineAOELast)
                _aoe = null;
        }
    }
}
