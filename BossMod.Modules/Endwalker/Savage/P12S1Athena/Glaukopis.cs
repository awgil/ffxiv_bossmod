namespace BossMod.Endwalker.Savage.P12S1Athena;

// TODO: not sure how exactly second target is selected, I think it is snapshotted to the current target when first cast happens? I can definitely taunt between casts without dying...
// TODO: consider generalizing...
class Glaukopis(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect _shape = new(60, 2.5f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Any(b => b.Source.TargetID == b.Target.InstanceID) && actor.Role == Role.Tank)
            hints.Add(Module.PrimaryActor.TargetID != actor.InstanceID ? "Taunt!" : "Pass aggro!");
        base.AddHints(slot, actor, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Glaukopis)
        {
            var target = WorldState.Actors.Find(spell.TargetID);
            if (target != null)
                CurrentBaits.Add(new(caster, target, _shape));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Glaukopis or AID.GlaukopisSecond)
        {
            CurrentBaits.Clear();
            if (++NumCasts < 2 && WorldState.Actors.Find(caster.TargetID) is var target && target != null)
                CurrentBaits.Add(new(caster, target, _shape));
        }
    }
}
