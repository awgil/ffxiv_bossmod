namespace BossMod.Dawntrail.Savage.RM07SBruteAbominator;

class P2BrutishSwing(BossModule module) : Components.GenericAOEs(module)
{
    public bool Risky;

    private readonly List<(Actor Caster, bool In)> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.P2BrutishSwingIn:
                Casters.Add((caster, true));
                break;
            case AID.P2BrutishSwingOut:
                Casters.Add((caster, false));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.P2BrutishSwingIn or AID.P2BrutishSwingOut)
        {
            Casters.RemoveAll(c => c.Caster == caster);
            NumCasts++;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(c.In ? new AOEShapeDonut(21.712f, 88) : new AOEShapeCircle(25), c.Caster.CastInfo!.LocXZ, c.Caster.CastInfo!.Rotation, Module.CastFinishAt(c.Caster.CastInfo), Risky: Risky));
}

class P3BrutishSwing(BossModule module) : Components.GenericAOEs(module)
{
    public bool Risky;

    private readonly List<(Actor Caster, bool In)> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.P3BrutishSwingIn:
                Casters.Add((caster, true));
                break;
            case AID.P3BrutishSwingOut:
                Casters.Add((caster, false));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.P3BrutishSwingIn or AID.P3BrutishSwingOut)
        {
            Casters.RemoveAll(c => c.Caster == caster);
            NumCasts++;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(c.In ? new AOEShapeDonut(21.712f, 88) : new AOEShapeCircle(25), c.Caster.CastInfo!.LocXZ, c.Caster.CastInfo!.Rotation, Module.CastFinishAt(c.Caster.CastInfo), Risky: Risky));
}
