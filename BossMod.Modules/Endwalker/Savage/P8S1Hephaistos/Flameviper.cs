namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class Flameviper(BossModule module) : Components.CastCounter(module, AID.FlameviperSecond)
{
    private ulong _firstTarget;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.Role != Role.Tank)
            return;

        if (Module.PrimaryActor.TargetID == _firstTarget)
            hints.Add(actor.InstanceID == _firstTarget ? "Pass aggro to co-tank" : "Taunt!");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Flameviper)
            _firstTarget = spell.TargetID;
    }
}
