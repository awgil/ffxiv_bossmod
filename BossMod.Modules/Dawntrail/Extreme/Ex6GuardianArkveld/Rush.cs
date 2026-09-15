namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

class WyvernsRadianceQuake(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(8), new AOEShapeDonut(8, 14), new AOEShapeDonut(14, 20), new AOEShapeDonut(20, 26)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WyvernsRadianceQuake1)
            AddSequence(caster.Position, Module.CastFinishAt(spell), default);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.WyvernsRadianceQuake1 => 0,
            AID.WyvernsRadianceQuake2 => 1,
            AID.WyvernsRadianceQuake3 => 2,
            AID.WyvernsRadianceQuake4 => 3,
            _ => -1
        };
        if (order >= 0)
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
    }
}

class Rush(BossModule module) : Components.GenericAOEs(module, AID.RushCast)
{
    private readonly List<(WPos From, WPos To, DateTime Activation)> _charges = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _charges)
            yield return new(new AOEShapeRect((c.To - c.From).Length(), 6), c.From, (c.To - c.From).ToAngle(), c.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RushTelegraph)
        {
            _charges.Add((caster.Position, spell.LocXZ, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RushCast or AID.RushInstant)
        {
            NumCasts++;
            _charges.RemoveAll(c => c.To.AlmostEqual(spell.TargetXZ, 1));
        }
    }
}
