namespace BossMod.Endwalker.Alliance.A33Oschon;

class TrekDraws : Components.GenericAOEs
{
    private List<AOEInstance> _aoes = [];

    private static AOEShapeCone _shape = new(65, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TrekShot2 or AID.TrekShot4 or AID.SwingingDraw2)
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TrekShot2 or AID.TrekShot4 or AID.SwingingDraw2)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}