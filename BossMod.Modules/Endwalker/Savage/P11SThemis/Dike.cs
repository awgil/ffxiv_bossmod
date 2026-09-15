namespace BossMod.Endwalker.Savage.P11SThemis;

class Dike(BossModule module) : Components.CastCounter(module, AID.DikeSecond)
{
    private ulong _firstPrimaryTarget;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.PrimaryActor.TargetID == _firstPrimaryTarget && actor.Role == Role.Tank)
            hints.Add(actor.InstanceID != _firstPrimaryTarget ? "Taunt!" : "Pass aggro!");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DikeAOE1Primary)
            _firstPrimaryTarget = spell.TargetID;
    }
}
