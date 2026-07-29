namespace BossMod.Dawntrail.Raid.M12NLindwurm;

[SkipLocalsInit]
sealed class M12NLindwurmStates : StateMachineBuilder
{
    public M12NLindwurmStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<TheFixer>()
            .ActivateOnEnter<SerpentineScourge>()
            .ActivateOnEnter<RavenousReach>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<VisceralBurst>()
            .ActivateOnEnter<Splattershed>()
            .ActivateOnEnter<BringDownTheHouse>()
            .ActivateOnEnter<SplitScourge>()
            .ActivateOnEnter<VenomousScourge>()
            .ActivateOnEnter<GrandEntrance>()
            .ActivateOnEnter<MindlessFlesh>()
            .ActivateOnEnter<MindlessFleshBig>()
            .ActivateOnEnter<FleshTele>()
            .ActivateOnEnter<CruelCoil>()
            .ActivateOnEnter<BurstingGrotesquerie>()
            .ActivateOnEnter<SharedGrotesquerie>()
            .ActivateOnEnter<DirectedGrotesquerie>();
    }
}
