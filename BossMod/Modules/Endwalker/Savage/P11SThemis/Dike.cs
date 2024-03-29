namespace BossMod.Endwalker.Savage.P11SThemis;

class Dike : Components.CastCounter
{
    private ulong _firstPrimaryTarget;

    public Dike() : base(ActionID.MakeSpell(AID.DikeSecond)) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (module.PrimaryActor.TargetID == _firstPrimaryTarget && actor.Role == Role.Tank)
            hints.Add(actor.InstanceID != _firstPrimaryTarget ? "Taunt!" : "Pass aggro!");
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DikeAOE1Primary)
            _firstPrimaryTarget = spell.TargetID;
    }
}
