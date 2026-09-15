namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

class SavageBarbery(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, AOEShape Shape)> _casts = [];
    public int NumActiveCasts => _casts.Count;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _casts.Select(cs => new AOEInstance(cs.Shape, cs.Caster.Position, cs.Caster.CastInfo!.Rotation, Module.CastFinishAt(cs.Caster.CastInfo)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.SavageBarberyDonutAOE => new AOEShapeDonut(6, 20),
            AID.SavageBarberyRectAOE => new AOEShapeRect(20, 6, 20),
            AID.SavageBarberyDonutSword or AID.SavageBarberyRectSword => new AOEShapeCircle(20),
            _ => null
        };
        if (shape != null)
            _casts.Add((caster, shape));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SavageBarberyDonutAOE or AID.SavageBarberyRectAOE or AID.SavageBarberyDonutSword or AID.SavageBarberyRectSword)
            _casts.RemoveAll(cs => cs.Caster == caster);
    }
}

class HairRaid(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, AOEShape Shape)> _casts = [];
    public int NumActiveCasts => _casts.Count;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _casts.Select(cs => new AOEInstance(cs.Shape, cs.Caster.Position, cs.Caster.CastInfo!.Rotation, Module.CastFinishAt(cs.Caster.CastInfo)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.HairRaidConeAOE => new AOEShapeCone(40, 60.Degrees()), // TODO: verify angle
            AID.HairRaidDonutAOE => new AOEShapeDonut(6, 20),
            _ => null
        };
        if (shape != null)
            _casts.Add((caster, shape));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HairRaidConeAOE or AID.HairRaidDonutAOE)
            _casts.RemoveAll(cs => cs.Caster == caster);
    }
}

class HairSprayDeadlyTwist(BossModule module) : Components.CastStackSpread(module, AID.DeadlyTwist, AID.HairSpray, 6, 5, 4);
