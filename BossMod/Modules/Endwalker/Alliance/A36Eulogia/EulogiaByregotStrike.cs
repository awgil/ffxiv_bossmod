namespace BossMod.Endwalker.Alliance.A36Eulogia;

class ByregotStrikeJump : Components.LocationTargetedAOEs
{
    public ByregotStrikeJump() : base(ActionID.MakeSpell(AID.ByregotStrikeJump), 8) { }
}

class ByregotStrikeKnockback : Components.KnockbackFromCastTarget
{
    public ByregotStrikeKnockback() : base(ActionID.MakeSpell(AID.ByregotStrikeKnockback), 20) { }
}

class ByregotStrikeCone : Components.GenericAOEs
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(90, 22.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ByregotStrikeKnockback)
            for (int i = 0; i < 4; ++i)
                _aoes.Add(new(_shape, caster.Position, spell.Rotation + i * 90.Degrees(), spell.NPCFinishAt));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ByregotStrikeCone)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
