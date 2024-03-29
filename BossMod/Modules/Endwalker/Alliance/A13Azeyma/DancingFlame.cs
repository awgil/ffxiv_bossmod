namespace BossMod.Endwalker.Alliance.A13Azeyma;

class DancingFlame : Components.GenericAOEs
{
    public List<AOEInstance> AOEs = new();

    private static readonly AOEShapeRect _shape = new(17.5f, 17.5f, 17.5f); // 15 for diagonal 'squares' + 2.5 for central cross

    public DancingFlame() : base(ActionID.MakeSpell(AID.DancingFlameFirst)) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => AOEs;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HauteAirFlare)
            AOEs.Add(new(_shape, caster.Position + 40 * caster.Rotation.ToDirection(), default, spell.NPCFinishAt.AddSeconds(1)));
    }

    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        if (index == 27 && state == 0x00080004)
            AOEs.Clear();
    }
}
