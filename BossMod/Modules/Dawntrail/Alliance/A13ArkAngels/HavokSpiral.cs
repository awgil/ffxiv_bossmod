namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class HavokSpiral(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;

    private static readonly AOEShapeCone _shape = new(30, 15.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var increment = (IconID)iconID switch
        {
            IconID.RotateCW => -30.Degrees(),
            IconID.RotateCCW => 30.Degrees(),
            _ => default
        };
        if (increment != default)
            _increment = increment;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HavocSpiralFirst)
        {
            Sequences.Add(new(_shape, caster.Position, spell.Rotation, _increment, Module.CastFinishAt(spell), 1.2f, 8));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HavocSpiralFirst or AID.HavocSpiralRest)
        {
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
        }
    }
}

class SpiralFinish(BossModule module) : Components.KnockbackFromCastTarget(module, AID.SpiralFinishAOE, 16)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(Module.Center, 9), Module.CastFinishAt(Casters[0].CastInfo));
    }
}
