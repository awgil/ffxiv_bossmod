namespace BossMod.Stormblood.Alliance.A34UltimaP1;

class A34UltimaP1States : StateMachineBuilder
{
    public A34UltimaP1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HolyIVBait>()
            .ActivateOnEnter<HolyIVSpread>()
            .ActivateOnEnter<AuralightAOE>()
            .ActivateOnEnter<AuralightRect>()
            .ActivateOnEnter<GrandCrossAOE>()
            .ActivateOnEnter<TimeEruption>()
            .ActivateOnEnter<Eruption2>()
            .ActivateOnEnter<ControlTower2>()
            .ActivateOnEnter<ExtremeEdge1>()
            .ActivateOnEnter<ExtremeEdge2>()
            .ActivateOnEnter<CrushWeapon>()
            .ActivateOnEnter<Searchlight>()
            .ActivateOnEnter<HallowedBolt>();
    }
}
