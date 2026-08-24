namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

class Petrai(BossModule module) : Components.GenericSharedTankbuster(module, AID.PetraiAOE, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Petrai)
        {
            Source = caster;
            Target = WorldState.Actors.Find(spell.TargetID);
            Activation = Module.CastFinishAt(spell, 1);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            Source = Target = null;
        }
    }
}
