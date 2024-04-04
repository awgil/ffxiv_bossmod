namespace BossMod.Endwalker.Alliance.A33Oschon;

class A33OschonStates : StateMachineBuilder
{
    private readonly A33Oschon _module;
    public A33OschonStates(A33Oschon module) : base(module)
    {
        _module = module;
        DeathPhase(0, id => { SimpleState(id, 10000, "Enrage"); })
            .ActivateOnEnter<Downhill>()
            .ActivateOnEnter<ClimbingShot>()
            .ActivateOnEnter<ClimbingShot2>()
            .ActivateOnEnter<ClimbingShot3>()
            .ActivateOnEnter<ClimbingShot4>()
            .ActivateOnEnter<SoaringMinuet1>()
            .ActivateOnEnter<SoaringMinuet2>()
            .ActivateOnEnter<FlintedFoehnP1>()
            .ActivateOnEnter<TheArrow>()
            .ActivateOnEnter<TrekDraws>();
        SimplePhase(1, id => { SimpleState(id, 10000, "Enrage"); }, "P2")
            .ActivateOnEnter<PitonPullAOE>()
            .ActivateOnEnter<AltitudeAOE>()
            .ActivateOnEnter<GreatWhirlwindAOE>()
            .ActivateOnEnter<DownhillSmallAOE>()
            .ActivateOnEnter<DownhillBigAOE>()
            //.ActivateOnEnter<ArrowTrail>()
            .ActivateOnEnter<WanderingVolley>()
            .ActivateOnEnter<WanderingVolleyAOE>()
            .ActivateOnEnter<FlintedFoehnP2>()
            .ActivateOnEnter<TheArrowP2>()
            .Raw.Update = () => _module.OschonP2() is var OschonP2 && _module.OschonP1() is var OschonP1 && OschonP2 != null && OschonP1 != null && !OschonP1.IsTargetable;
    }
}