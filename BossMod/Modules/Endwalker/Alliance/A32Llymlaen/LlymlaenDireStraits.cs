namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class DireStraits : Components.GenericAOEs
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(80, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DireStraitAltRectAOE1 or AID.DireStraitsRectAOE1)
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt.AddSeconds(9.2f)));
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DireStraitAltRectAOE2 or AID.DireStraitsRectAOE2)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}