namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class SoulGrasp(BossModule module) : Components.GenericSharedTankbuster(module, ActionID.MakeSpell(AID.SoulGraspAOE), 4)
{
    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.SoulGrasp)
        {
            Source = Module.PrimaryActor;
            Target = actor;
            Activation = WorldState.FutureTime(5.8f);
        }
    }
}
