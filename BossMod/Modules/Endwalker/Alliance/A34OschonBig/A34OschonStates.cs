namespace BossMod.Endwalker.Alliance.A34OschonBig;

class A34OschonStates : StateMachineBuilder
{
    public A34OschonStates(BossModule module) : base(module)
    {
        TrivialPhase()
                        .ActivateOnEnter<PitonPullAOE>()
                        .ActivateOnEnter<AltitudeAOE>()
                        .ActivateOnEnter<GreatWhirlwindAOE>()
                        .ActivateOnEnter<DownhillSmallAOE>()
                        .ActivateOnEnter<DownhillBigAOE>()
                        //.ActivateOnEnter<ArrowTrail>()
                        .ActivateOnEnter<WanderingVolley>()
                        .ActivateOnEnter<WanderingVolleyAOE>()
                        .ActivateOnEnter<BigFlintedFoehn>()
                        .ActivateOnEnter<TheArrowTankbuster>();
    }
}