namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class A32LlymlaenStates : StateMachineBuilder
{
    public A32LlymlaenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Godsbane>()
            .ActivateOnEnter<Tempest>()
            .ActivateOnEnter<SurgingWave>()
            .ActivateOnEnter<SurgingWave>()
            .ActivateOnEnter<WindRose>()
            .ActivateOnEnter<SurgingWave>()
            .ActivateOnEnter<TorrentialTridents>()
            .ActivateOnEnter<Tridents>()
            .ActivateOnEnter<Stormwhorl>()
            .ActivateOnEnter<Stormwinds>()
            .ActivateOnEnter<Maelstrom>()
            .ActivateOnEnter<NavigatorsTridentRectAOE>()
            .ActivateOnEnter<NavigatorsTridentKnockback>()
            .ActivateOnEnter<NavigatorsTridentRaidwide>()
            .ActivateOnEnter<RightStrait>()
            .ActivateOnEnter<LeftStrait>()
            .ActivateOnEnter<SeafoamSpiral>()
            .ActivateOnEnter<DireStraits>()
            .ActivateOnEnter<FrothingSea>()
            .ActivateOnEnter<SerpentsTide>()
            .ActivateOnEnter<ToTheLast>()
            .ActivateOnEnter<DeepDive1>()
            .ActivateOnEnter<DeepDive2>()
            .ActivateOnEnter<HardWater1>()
            .ActivateOnEnter<HardWater2>()
            .ActivateOnEnter<SeaFoam>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<ShockwaveRaidwide>()
            .ActivateOnEnter<SurgingWavesArenaChange>();
    }
}
