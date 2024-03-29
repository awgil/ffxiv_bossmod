namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class Flameviper : Components.CastCounter
{
    private ulong _firstTarget;

    public Flameviper() : base(ActionID.MakeSpell(AID.FlameviperSecond)) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (actor.Role != Role.Tank)
            return;

        if (module.PrimaryActor.TargetID == _firstTarget)
            hints.Add(actor.InstanceID == _firstTarget ? "Pass aggro to co-tank" : "Taunt!");
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Flameviper)
            _firstTarget = spell.TargetID;
    }
}
