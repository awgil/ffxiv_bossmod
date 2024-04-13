namespace BossMod.Endwalker.Alliance.A23Halone;

class Tetrapagos(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return new(_aoes[0].Shape, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, ArenaColor.Danger);
        if (_aoes.Count > 1)
            yield return new(_aoes[1].Shape, _aoes[1].Origin, _aoes[1].Rotation, _aoes[1].Activation, Risky: false);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.TetrapagosHailstormPrepare => new AOEShapeCircle(10),
            AID.TetrapagosSwirlPrepare => new AOEShapeDonut(10, 30),
            AID.TetrapagosRightrimePrepare or AID.TetrapagosLeftrimePrepare => new AOEShapeCone(30, 90.Degrees()),
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, caster.Position, caster.Rotation, spell.NPCFinishAt.AddSeconds(7.3f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TetrapagosHailstormAOE or AID.TetrapagosSwirlAOE or AID.TetrapagosRightrimeAOE or AID.TetrapagosLeftrimeAOE)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}
