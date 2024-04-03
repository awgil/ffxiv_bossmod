namespace BossMod.Endwalker.Alliance.A32Llymlaen
{
    class A32LlymlaenStates : StateMachineBuilder
    {           
        public A32LlymlaenStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<WindRoseAOE>()
                .ActivateOnEnter<SurgingWaveAOE>()
                .ActivateOnEnter<LandingAOE>()
                .ActivateOnEnter<StormwhorlLocAOE>()
                .ActivateOnEnter<StormwindsSpread>()
                .ActivateOnEnter<MaelstromLocAOE>()
                .ActivateOnEnter<NavigatorsTridentRectAOE>()
                .ActivateOnEnter<NavigatorsTridentKnockback>()
                .ActivateOnEnter<RightStraitCone>()
                .ActivateOnEnter<LeftStraitCone>()
                .ActivateOnEnter<SeafoamSpiralDonut>()
                .ActivateOnEnter<DireStraits>()
                .ActivateOnEnter<FrothingSeaRectAOE>()
                .ActivateOnEnter<SerpentsTideRectAOE1>()
                .ActivateOnEnter<SerpentsTideRectAOE2>()
                .ActivateOnEnter<SerpentsTideRectAOE3>()
                .ActivateOnEnter<SerpentsTideRectAOE4>()
                .ActivateOnEnter<SerpentsTideRectAOE5>()
                .ActivateOnEnter<SerpentsTideRectAOE6>()
                .ActivateOnEnter<ToTheLastRectAOE>()
                .ActivateOnEnter<DeepDiveStack>()
                .ActivateOnEnter<HardWaterStack>()
                .ActivateOnEnter<SphereShatter>()
                .ActivateOnEnter<SurgingWaveKnockback>();
        }
    }
}
