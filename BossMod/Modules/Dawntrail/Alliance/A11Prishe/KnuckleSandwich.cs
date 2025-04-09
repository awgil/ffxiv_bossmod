namespace BossMod.Dawntrail.Alliance.A11Prishe;

class KnuckleSandwich(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // extra ai hint: stay close to the edge of the first aoe
        if (_aoes.Count == 2 && _aoes[1].Shape is AOEShapeDonut donut)
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(_aoes[0].Origin, donut.InnerRadius + 2), _aoes[0].Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.KnuckleSandwich1AOE => new AOEShapeCircle(9),
            AID.KnuckleSandwich2AOE => new AOEShapeCircle(18),
            AID.KnuckleSandwich3AOE => new AOEShapeCircle(27),
            AID.BrittleImpact1 => new AOEShapeDonut(9, 60),
            AID.BrittleImpact2 => new AOEShapeDonut(18, 60),
            AID.BrittleImpact3 => new AOEShapeDonut(27, 60),
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.KnuckleSandwich1AOE or AID.KnuckleSandwich2AOE or AID.KnuckleSandwich3AOE or AID.BrittleImpact1 or AID.BrittleImpact2 or AID.BrittleImpact3)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}
