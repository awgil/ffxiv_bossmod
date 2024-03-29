namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class SoulGrasp : Components.GenericSharedTankbuster
{
    public SoulGrasp() : base(ActionID.MakeSpell(AID.SoulGraspAOE), 4) { }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.SoulGrasp)
        {
            Source = module.PrimaryActor;
            Target = actor;
            Activation = module.WorldState.CurrentTime.AddSeconds(5.8f);
        }
    }
}
