namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class A13ArkAngelsStates : StateMachineBuilder
{
    private readonly A13ArkAngels _module;

    public A13ArkAngelsStates(A13ArkAngels module) : base(module)
    {
        _module = module;
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        DecisiveBattle(id, 0.2f);
        Cloudsplitter(id + 0x10000, 4.2f);
        MeikyoShisui(id + 0x20000, 4.6f);
        Meteor(id + 0x30000, 2.1f);
        HavocSpiral(id + 0x40000, 3.1f);
        Dragonfall(id + 0x50000, 5.6f);
        Guillotine(id + 0x60000, 2.4f);

        Intermission(id + 0x100000, 10.2f);
        UtsusemiDominionSlash(id + 0x110000, 3.2f);
        Holy(id + 0x120000, 4.0f);
        MijinGakure(id + 0x130000, 8.1f);
        Rampage(id + 0x140000, 5.8f);
        Guillotine(id + 0x150000, 6.0f);
        MeikyoShisuiCrossReaverMeteor(id + 0x160000, 4.3f);
        ArroganceIncarnate(id + 0x170000, 4.3f);
        Cloudsplitter(id + 0x180000, 1.0f);
        Rampage(id + 0x190000, 13.8f);
        Guillotine(id + 0x1A0000, 0.6f);
        CriticalReaver(id + 0x1B0000, 6.9f);

        // TODO: rest is random? seen dominion slash and havok spiral...
        SimpleState(id + 0xFF0000, 10000, "???")
            .ActivateOnEnter<Cloudsplitter>()
            .ActivateOnEnter<TachiYukikaze>()
            .ActivateOnEnter<TachiGekko>()
            .ActivateOnEnter<TachiKasha>()
            .ActivateOnEnter<ConcertedDissolution>()
            .ActivateOnEnter<LightsChain>()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<HavokSpiral>()
            .ActivateOnEnter<SpiralFinish>()
            .ActivateOnEnter<Dragonfall>()
            .ActivateOnEnter<Guillotine>()
            .ActivateOnEnter<DominionSlash>()
            .ActivateOnEnter<CrossReaver>()
            .ActivateOnEnter<Rampage>()
            .ActivateOnEnter<ArroganceIncarnate>();
    }

    private void DecisiveBattle(uint id, float delay)
    {
        ActorCast(id, _module.BossMR, AID.DecisiveBattleMR, delay, 4, true, "Assign target")
            .ActivateOnEnter<DecisiveBattle>();
    }

    private void Cloudsplitter(uint id, float delay)
    {
        ActorCast(id, _module.BossMR, AID.Cloudsplitter, delay, 5, true)
            .ActivateOnEnter<Cloudsplitter>();
        ComponentCondition<Cloudsplitter>(id + 2, 0.5f, comp => comp.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<Cloudsplitter>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void MeikyoShisui(uint id, float delay)
    {
        ActorCast(id, _module.BossGK, AID.MeikyoShisui, delay, 4, true);
        ComponentCondition<TachiYukikaze>(id + 0x10, 1, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<TachiYukikaze>()
            .ActivateOnEnter<TachiGekko>()
            .ActivateOnEnter<TachiKasha>();
        ComponentCondition<TachiYukikaze>(id + 0x11, 3, comp => comp.NumCasts > 0, "Criss-cross 1");
        ComponentCondition<TachiYukikaze>(id + 0x12, 1.6f, comp => comp.Casters.Count > 0);
        ComponentCondition<TachiGekko>(id + 0x13, 2.4f, comp => comp.NumCasts > 0, "Gaze")
            .DeactivateOnExit<TachiGekko>();
        ComponentCondition<TachiYukikaze>(id + 0x14, 0.6f, comp => comp.NumCasts > 10, "Criss-cross 2")
            .DeactivateOnExit<TachiYukikaze>();
        ComponentCondition<ConcertedDissolution>(id + 0x15, 1.5f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<ConcertedDissolution>();
        ComponentCondition<TachiKasha>(id + 0x16, 2.9f, comp => comp.NumCasts > 0, "Out")
            .DeactivateOnExit<TachiKasha>();
        ComponentCondition<LightsChain>(id + 0x17, 0.6f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<LightsChain>();
        ComponentCondition<ConcertedDissolution>(id + 0x18, 2.5f, comp => comp.NumCasts > 0, "Cones")
            .DeactivateOnExit<ConcertedDissolution>();
        ComponentCondition<LightsChain>(id + 0x19, 5.5f, comp => comp.NumCasts > 0, "Donut")
            .DeactivateOnExit<LightsChain>();
    }

    private void Meteor(uint id, float delay)
    {
        ActorCast(id, _module.BossTT, AID.Meteor, delay, 11, true, "Interrupt", true)
            .ActivateOnEnter<Meteor>()
            .DeactivateOnExit<Meteor>();
    }

    private void HavocSpiral(uint id, float delay)
    {
        ActorCastStart(id, _module.BossMR, AID.HavocSpiral, delay, true)
            .ActivateOnEnter<HavokSpiral>();
        ActorCastEnd(id + 1, _module.BossMR, 5);
        ComponentCondition<HavokSpiral>(id + 2, 0.5f, comp => comp.NumCasts > 0, "Rotation start");
        ActorCast(id + 0x10, _module.BossMR, AID.SpiralFinish, 0.6f, 11, true)
            .ActivateOnEnter<SpiralFinish>()
            .DeactivateOnExit<HavokSpiral>();
        ComponentCondition<SpiralFinish>(id + 0x12, 0.5f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<SpiralFinish>();
    }

    private void Dragonfall(uint id, float delay)
    {
        ActorCast(id, _module.BossGK, AID.Dragonfall, delay, 9, true)
            .ActivateOnEnter<Dragonfall>();
        ComponentCondition<Dragonfall>(id + 2, 0.4f, comp => comp.NumCasts > 0, "Stack 1");
        ComponentCondition<Dragonfall>(id + 3, 2.3f, comp => comp.NumCasts > 1, "Stack 2");
        ComponentCondition<Dragonfall>(id + 4, 2.3f, comp => comp.NumCasts > 2, "Stack 3")
            .DeactivateOnExit<Dragonfall>();
    }

    private void Guillotine(uint id, float delay)
    {
        ActorCast(id, _module.BossTT, AID.Guillotine, delay, 10.5f, true)
            .ActivateOnEnter<Guillotine>();
        ComponentCondition<Guillotine>(id + 2, 0.6f, comp => comp.NumCasts > 0, "Cone start");
        ComponentCondition<Guillotine>(id + 0x10, 3.4f, comp => comp.NumCasts >= 4, "Cone resolve")
            .DeactivateOnExit<Guillotine>();
    }

    private void Intermission(uint id, float delay)
    {
        Targetable(id, false, delay, "Bosses disappear")
            .DeactivateOnExit<DecisiveBattle>();
        ActorTargetable(id + 1, _module.BossHM, true, 4.3f, "Bosses appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void UtsusemiDominionSlash(uint id, float delay)
    {
        ActorCast(id, _module.BossHM, AID.Utsusemi, delay, 3);
        ActorCastEnd(id + 2, _module.BossEV, 2, false, "Raidwide") // dominion slash starts at the same time
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DominionSlash>(id + 3, 0.7f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<DominionSlash>();
        ActorCast(id + 0x10, _module.BossHM, AID.MightyStrikesClones, 0.5f, 5)
            .ActivateOnEnter<Utsusemi>();
        ActorCast(id + 0x20, _module.BossHM, AID.CrossReaver, 6.3f, 3);
        ComponentCondition<CrossReaver>(id + 0x22, 1, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<CrossReaver>();
        ComponentCondition<DominionSlash>(id + 0x23, 2.3f, comp => comp.AOEs.Count == 0)
            .DeactivateOnExit<DominionSlash>();
        ComponentCondition<CrossReaver>(id + 0x24, 3.7f, comp => comp.NumCasts > 0, "Cross")
            .DeactivateOnExit<CrossReaver>()
            .DeactivateOnExit<Utsusemi>();
    }

    private void Holy(uint id, float delay)
    {
        ActorCast(id, _module.BossEV, AID.Holy, delay, 5, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MijinGakure(uint id, float delay)
    {
        ActorTargetable(id, _module.BossEV, false, delay, "Shield")
            .ActivateOnEnter<ArkShield>();
        ActorCast(id + 0x10, _module.BossHM, AID.MijinGakure, 1, 30, true, "Interrupt", true)
            .DeactivateOnExit<ArkShield>();
    }

    private void Rampage(uint id, float delay)
    {
        ActorCast(id, _module.BossMR, AID.Rampage, delay, 8, true)
            .ActivateOnEnter<Rampage>();
        ComponentCondition<Rampage>(id + 0x10, 0.2f, comp => comp.NumCasts > 0, "Charges start");
        ComponentCondition<Rampage>(id + 0x20, 5.2f, comp => comp.NumCasts > 4, "Charges resolve")
            .DeactivateOnExit<Rampage>();
    }

    private void MeikyoShisuiCrossReaverMeteor(uint id, float delay)
    {
        ActorCast(id, _module.BossGK, AID.MeikyoShisui, delay, 4, true);
        ComponentCondition<TachiYukikaze>(id + 0x10, 1, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<TachiYukikaze>()
            .ActivateOnEnter<TachiGekko>()
            .ActivateOnEnter<TachiKasha>();
        ComponentCondition<TachiYukikaze>(id + 0x11, 3, comp => comp.NumCasts > 0, "Criss-cross 1");
        ComponentCondition<TachiYukikaze>(id + 0x12, 1.6f, comp => comp.Casters.Count > 0);
        ComponentCondition<TachiGekko>(id + 0x13, 2.4f, comp => comp.NumCasts > 0, "Gaze")
            .DeactivateOnExit<TachiGekko>();
        ComponentCondition<TachiYukikaze>(id + 0x14, 0.6f, comp => comp.NumCasts > 10, "Criss-cross 2")
            .DeactivateOnExit<TachiYukikaze>();
        ComponentCondition<ConcertedDissolution>(id + 0x15, 1.5f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<ConcertedDissolution>();
        ComponentCondition<TachiKasha>(id + 0x16, 2.9f, comp => comp.NumCasts > 0, "Out")
            .DeactivateOnExit<TachiKasha>();
        ComponentCondition<LightsChain>(id + 0x17, 0.7f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<LightsChain>();

        ActorCastStart(id + 0x20, _module.BossHM, AID.CrossReaver, 1.9f, true);
        ComponentCondition<ConcertedDissolution>(id + 0x21, 0.5f, comp => comp.NumCasts > 0, "Cones")
            .DeactivateOnExit<ConcertedDissolution>();
        ActorCastEnd(id + 0x22, _module.BossHM, 2.5f, true);
        ComponentCondition<CrossReaver>(id + 0x23, 1, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<CrossReaver>();
        ComponentCondition<LightsChain>(id + 0x24, 2.1f, comp => comp.NumCasts > 0, "Donut")
            .DeactivateOnExit<LightsChain>();

        ActorCastStart(id + 0x30, _module.BossTT, AID.Meteor, 1, true);
        ComponentCondition<CrossReaver>(id + 0x31, 2.9f, comp => comp.NumCasts > 0, "Cross")
            .ActivateOnEnter<Meteor>()
            .DeactivateOnExit<CrossReaver>();
        ActorCastEnd(id + 0x32, _module.BossTT, 8.1f, true, "Interrupt", true)
            .DeactivateOnExit<Meteor>();
    }

    private void ArroganceIncarnate(uint id, float delay)
    {
        ActorCastStart(id, _module.BossEV, AID.ArroganceIncarnate, delay, true)
            .ActivateOnEnter<ArroganceIncarnate>();
        ActorCastEnd(id + 1, _module.BossEV, 5, true);
        ComponentCondition<ArroganceIncarnate>(id + 2, 0.7f, comp => comp.NumFinishedStacks > 0, "Stack 1");
        ComponentCondition<ArroganceIncarnate>(id + 0x10, 4.3f, comp => comp.NumFinishedStacks >= 5, "Stack 5")
            .DeactivateOnExit<ArroganceIncarnate>();
    }

    private void CriticalReaver(uint id, float delay)
    {
        ActorCast(id, _module.BossHM, AID.MightyStrikesBoss, delay, 5, true);
        ComponentCondition<CriticalReaverRaidwide>(id + 0x10, 2.1f, comp => comp.NumCasts >= 1, "Raidwide 1")
            .ActivateOnEnter<CriticalReaverRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<CriticalReaverRaidwide>(id + 0x11, 2.1f, comp => comp.NumCasts >= 2, "Raidwide 2")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<CriticalReaverRaidwide>(id + 0x12, 2.1f, comp => comp.NumCasts >= 3, "Raidwide 3")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<CriticalReaverRaidwide>(id + 0x13, 2.1f, comp => comp.NumCasts >= 4, "Raidwide 4")
            .DeactivateOnExit<CriticalReaverRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorCast(id + 0x20, _module.BossHM, AID.CriticalReaverEnrage, 2.1f, 10, true, "Interrupt", true)
            .ActivateOnEnter<CriticalReaverEnrage>()
            .DeactivateOnExit<CriticalReaverEnrage>();
    }
}
