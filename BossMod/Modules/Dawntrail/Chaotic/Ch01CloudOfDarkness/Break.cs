namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class Break(BossModule module) : Components.GenericGaze(module)
{
    private readonly List<Eye> _eyes = [];

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor) => _eyes;//_casters.Where(c => c.CastInfo?.TargetID != actor.InstanceID).Select(c => new Eye(EyePosition(c), Module.CastFinishAt(c.CastInfo), Range: range));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BreakBoss or AID.BreakEye)
            _eyes.Add(new(caster.Position, Module.CastFinishAt(spell, 0.9f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BreakBossAOE or AID.BreakEyeAOE)
            _eyes.RemoveAll(eye => eye.Position.AlmostEqual(caster.Position, 1));
    }
}
