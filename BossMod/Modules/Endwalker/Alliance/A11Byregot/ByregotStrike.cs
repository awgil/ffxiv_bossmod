namespace BossMod.Endwalker.Alliance.A11Byregot;

class ByregotStrikeJump(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ByregotStrikeJump), 8);
class ByregotStrikeJumpCone(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ByregotStrikeJumpCone), 8);
class ByregotStrikeKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ByregotStrikeKnockback), 18);

class ByregotStrikeCone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(90, 22.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ByregotStrikeKnockback && Module.PrimaryActor.FindStatus(SID.Glow) != null)
            for (int i = 0; i < 4; ++i)
                _aoes.Add(new(_shape, caster.Position, spell.Rotation + i * 90.Degrees(), spell.NPCFinishAt));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ByregotStrikeCone)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
