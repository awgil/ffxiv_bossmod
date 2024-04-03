namespace BossMod.Endwalker.Alliance.A31Thaliak;

class RheognosisKnockback : Components.Knockback
{
    private Source? _knockback;

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_knockback);
    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Rheognosis or AID.RheognosisPetrine)
            _knockback = new(module.Bounds.Center, 25, module.WorldState.CurrentTime.AddSeconds(20.2f), Direction: spell.Rotation + 180.Degrees(), Kind: Kind.DirForward);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RheognosisKnockback)
            _knockback = null;
    }
}