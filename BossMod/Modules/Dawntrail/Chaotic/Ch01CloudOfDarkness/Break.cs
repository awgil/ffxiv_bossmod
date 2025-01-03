namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class Break(BossModule module) : Components.GenericGaze(module)
{
    public readonly List<Eye> Eyes = [];

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor) => Eyes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BreakBoss or AID.BreakEye)
            Eyes.Add(new(caster.Position, Module.CastFinishAt(spell, 0.9f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BreakBossAOE or AID.BreakEyeAOE)
            Eyes.RemoveAll(eye => eye.Position.AlmostEqual(caster.Position, 1));
    }
}
