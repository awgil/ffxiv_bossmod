namespace BossMod.Endwalker.Unreal.Un3Sophia;

class ArmsOfWisdom : Components.Knockback
{
    public ArmsOfWisdom() : base(ActionID.MakeSpell(AID.ArmsOfWisdom)) { }

    private Actor? _caster;

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        if (_caster?.CastInfo?.TargetID == actor.InstanceID)
            yield return new(_caster.Position, 5, _caster.CastInfo.NPCFinishAt);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);

        if (_caster != null && _caster.CastInfo?.TargetID == _caster.TargetID)
        {
            // tank swap hints
            if (_caster.TargetID == actor.InstanceID)
                hints.Add("Pass aggro to co-tank!");
            else if (actor.Role == Role.Tank)
                hints.Add("Taunt!");
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _caster = caster;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _caster = null;
    }
}
