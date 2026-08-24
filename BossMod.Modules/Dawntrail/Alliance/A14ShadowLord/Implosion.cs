namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class Implosion(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shapeSmall = new(12, 90.Degrees());
    private static readonly AOEShapeCone _shapeLarge = new(90, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = (AID)spell.Action.ID switch
        {
            AID.ImplosionLargeL or AID.ImplosionLargeR => _shapeLarge,
            AID.ImplosionSmallL or AID.ImplosionSmallR => _shapeSmall,
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ImplosionLargeL or AID.ImplosionSmallR or AID.ImplosionLargeR or AID.ImplosionSmallL)
        {
            ++NumCasts;
            _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1) && aoe.Rotation.AlmostEqual(spell.Rotation, 0.1f));
        }
    }
}
