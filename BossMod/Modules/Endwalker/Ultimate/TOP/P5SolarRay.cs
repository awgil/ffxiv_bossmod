namespace BossMod.Endwalker.Ultimate.TOP;

// TODO: not sure how exactly second target is selected, I think it is snapshotted to the current target when first cast happens?
// TODO: consider generalizing - same as P12S1 Glaukopis and others...
class P5SolarRay : Components.GenericBaitAway
{
    private static readonly AOEShapeCircle _shape = new(5);

    public P5SolarRay() : base(centerAtTarget: true) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (NumCasts == 0 && CurrentBaits.FirstOrDefault(b => b.Source.TargetID == b.Target.InstanceID) is var b && b.Source != null && actor.Role == Role.Tank)
            hints.Add(b.Source.TargetID != actor.InstanceID ? "Taunt!" : "Pass aggro!");
        base.AddHints(module, slot, actor, hints, movementHints);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.P5SolarRayM or AID.P5SolarRayF)
        {
            var target = module.WorldState.Actors.Find(spell.TargetID);
            if (target != null)
                CurrentBaits.Add(new(caster, target, _shape));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.P5SolarRayM or AID.P5SolarRayMSecond or AID.P5SolarRayF or AID.P5SolarRayFSecond)
        {
            CurrentBaits.Clear();
            if (++NumCasts < 2 && module.WorldState.Actors.Find(caster.TargetID) is var target && target != null)
                CurrentBaits.Add(new(caster, target, _shape));
        }
    }
}
