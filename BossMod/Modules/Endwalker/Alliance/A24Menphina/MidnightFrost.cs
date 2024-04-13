namespace BossMod.Endwalker.Alliance.A24Menphina;

class MidnightFrostWaxingClaw(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(60, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MidnightFrostShortNormalFrontAOE or AID.MidnightFrostShortNormalBackAOE or
            AID.MidnightFrostShortMountedFrontAOE or AID.MidnightFrostShortMountedBackAOE or
            AID.MidnightFrostLongMountedFrontAOE or AID.MidnightFrostLongMountedBackAOE or
            AID.MidnightFrostLongDismountedFrontAOE or AID.MidnightFrostLongDismountedBackAOE or
            AID.WaxingClawRight or AID.WaxingClawLeft)
        {
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MidnightFrostShortNormalFrontAOE or AID.MidnightFrostShortNormalBackAOE or
            AID.MidnightFrostShortMountedFrontAOE or AID.MidnightFrostShortMountedBackAOE or
            AID.MidnightFrostLongMountedFrontAOE or AID.MidnightFrostLongMountedBackAOE or
            AID.MidnightFrostLongDismountedFrontAOE or AID.MidnightFrostLongDismountedBackAOE or
            AID.WaxingClawRight or AID.WaxingClawLeft)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}
