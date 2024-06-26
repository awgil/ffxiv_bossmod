namespace BossMod.Heavensward.Alliance.A12IrminsulSawtooth;

class A12IrminsulSawtoothStates : StateMachineBuilder
{
    public A12IrminsulSawtoothStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WhiteBreath>()
            .ActivateOnEnter<MeanThrash>()
            .ActivateOnEnter<MeanThrashKnockback>()
            .ActivateOnEnter<MucusBomb>()
            .ActivateOnEnter<MucusSpray>()
            .ActivateOnEnter<Rootstorm>()
            .ActivateOnEnter<Ambush>()
            //.ActivateOnEnter<AmbushKnockback>()
            .ActivateOnEnter<ShockwaveStomp>();
    }
}