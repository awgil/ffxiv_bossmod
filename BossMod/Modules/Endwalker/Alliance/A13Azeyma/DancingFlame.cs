namespace BossMod.Endwalker.Alliance.A13Azeyma;

class DancingFlame(BossModule module) : Components.GenericAOEs(module, AID.DancingFlameFirst)
{
    public List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(17.5f, 17.5f, 17.5f); // 15 for diagonal 'squares' + 2.5 for central cross

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HauteAirFlare)
            AOEs.Add(new(_shape, caster.Position + 40 * caster.Rotation.ToDirection(), default, Module.CastFinishAt(spell, 1)));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 27 && state == 0x00080004)
            AOEs.Clear();
    }
}
