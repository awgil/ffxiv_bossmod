namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

class JudgmentBlade(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var offset = (AID)spell.Action.ID switch
        {
            AID.JudgmentBladeRAOE => -10,
            AID.JudgmentBladeLAOE => +10,
            _ => 0
        };
        if (offset != 0)
            _aoe = new(new AOEShapeRect(70, 15), caster.Position + offset * spell.Rotation.ToDirection().OrthoL(), spell.Rotation, spell.NPCFinishAt);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.JudgmentBladeRAOE or AID.JudgmentBladeLAOE)
        {
            _aoe = null;
            ++NumCasts;
        }
    }
}
