namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class BladeOfDarkness(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeDonutSector _shapeIn = new(12, 60, 75.Degrees());
    private static readonly AOEShapeCone _shapeOut = new(30, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.BladeOfDarknessLAOE or AID.BladeOfDarknessRAOE => _shapeIn,
            AID.BladeOfDarknessCAOE => _shapeOut,
            _ => null
        };
        if (shape != null)
            _aoe = new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BladeOfDarknessLAOE or AID.BladeOfDarknessRAOE or AID.BladeOfDarknessCAOE)
        {
            _aoe = null;
            ++NumCasts;
        }
    }
}
