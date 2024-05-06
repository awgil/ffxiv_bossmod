namespace BossMod.Endwalker.Savage.P4S2Hesperos;

class P4S2States : StateMachineBuilder
{
    public P4S2States(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        SearingStream(id, 10.1f);
        AkanthaiAct1(id + 0x010000, 10.2f);
        FarNearSight(id + 0x020000, 1);

        AkanthaiAct2(id + 0x100000, 7.1f);

        AkanthaiAct3(id + 0x200000, 8.2f);
        FarNearSight(id + 0x210000, 4.1f);
        HeartStake(id + 0x220000, 9.2f);

        AkanthaiAct4(id + 0x300000, 4.2f);
        SearingStream(id + 0x310000, 9.3f);

        AkanthaiAct5(id + 0x400000, 4.2f);
        SearingStream(id + 0x410000, 7.2f);
        DemigodDouble(id + 0x420000, 4.2f);

        AkanthaiAct6(id + 0x500000, 8.2f);
        Cast(id + 0x510000, AID.Enrage, 4.8f, 10, "Enrage");
    }

    private State SearingStream(uint id, float delay)
    {
        return Cast(id, AID.SearingStream, delay, 5, "AOE")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State UltimateImpulse(uint id, float delay)
    {
        return Cast(id, AID.UltimateImpulse, delay, 7, "AOE")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State FarNearSight(uint id, float delay)
    {
        CastStartMulti(id, new AID[] { AID.Nearsight, AID.Farsight }, delay)
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 1, 5)
            .ActivateOnEnter<NearFarSight>();
        return ComponentCondition<NearFarSight>(id + 2, 1.1f, comp => comp.CurState == NearFarSight.State.Done, "Far/nearsight")
            .DeactivateOnExit<NearFarSight>()
            .SetHint(StateMachine.StateHint.Tankbuster | StateMachine.StateHint.PositioningEnd);
    }

    private void DemigodDouble(uint id, float delay)
    {
        Cast(id, AID.DemigodDouble, delay, 5, "Shared Tankbuster")
            .ActivateOnEnter<DemigodDouble>()
            .DeactivateOnExit<DemigodDouble>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void HeartStake(uint id, float delay)
    {
        Cast(id, AID.HeartStake, delay, 5, "Tankbuster")
            .ActivateOnEnter<HeartStake>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<HeartStake>(id + 2, 3.1f, comp => comp.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<HeartStake>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void HellSting(uint id, float delay)
    {
        // timeline:
        // 0.0s: cast start (boss visual + helpers real)
        // 2.4s: visual cast end
        // 3.0s: first aoes (helpers cast end)
        // 5.5s: boss visual instant cast + helpers start cast
        // 6.1s: second aoes (helpers cast end)
        Cast(id, AID.HellsSting, delay, 2.4f)
            .ActivateOnEnter<HellsSting>();
        ComponentCondition<HellsSting>(id + 0x10, 0.6f, comp => comp.NumCasts > 0, "Cone");
        ComponentCondition<HellsSting>(id + 0x20, 3.1f, comp => comp.NumCasts > 8, "Cone")
            .DeactivateOnExit<HellsSting>();
    }

    private void AkanthaiAct1(uint id, float delay)
    {
        // 'act 1' is 4 aoes (N/S/E/W) and 8 towers; explosion order is 2 opposite aoes -> all towers -> remaining aoes
        // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers and aoes
        // aoes are at (82/118, 100) and (100, 82/118), towers are at (95.05/104.95, 95.05/104.95) and (88.69/111.31, 88.69/111.31)
        Cast(id, AID.AkanthaiAct1, delay, 5, "Act1");
        SearingStream(id + 0x1000, 4.2f);

        // timeline:
        // -0.1s: first 2 aoes tethered
        //  0.0s: wreath cast start ==> component determines order and starts showing first aoe pair
        //  3.0s: towers tethered
        //  6.0s: last 2 aoes tethered
        //  8.0s: wreath cast end
        // 10.0s: first 2 aoes start cast 27149
        // 11.0s: first 2 aoes finish cast ==> component starts showing towers
        // 13.0s: towers start cast 27150
        // 14.0s: towers finish cast ==> component starts showing second aoe pair
        // 16.0s: last 2 aoes start cast 27149
        // 17.0s: last 2 aoes finish cast ==> component is reset
        // 18.0s: boss starts casting far/nearsight
        Cast(id + 0x2000, AID.WreathOfThorns1, 6.2f, 8, "Wreath1")
            .ActivateOnEnter<WreathOfThorns1>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<WreathOfThorns1>(id + 0x3000, 3, comp => comp.CurState != WreathOfThorns1.State.FirstAOEs, "AOE 1");
        ComponentCondition<WreathOfThorns1>(id + 0x4000, 3, comp => comp.CurState != WreathOfThorns1.State.Towers, "Towers");
        ComponentCondition<WreathOfThorns1>(id + 0x5000, 3, comp => comp.CurState != WreathOfThorns1.State.LastAOEs, "AOE 2")
            .DeactivateOnExit<WreathOfThorns1>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void AkanthaiAct2(uint id, float delay)
    {
        // 'act 2' is 4 aoes and 4 towers + player pairwise tethers
        // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers and aoes
        // towers are at (96,82), (118,96), (104,118) and (82,104); aoes are at (104,82), (118,104), (96,118) and (82,96)
        Cast(id, AID.AkanthaiAct2, delay, 5, "Act2");
        DemigodDouble(id + 0x1000, 4.2f);

        // timeline:
        // -0.1s: two towers and two aoes tethered
        //  0.0s: wreath cast start ==> component determines order and starts showing first set
        //  3.0s: remaining tethers
        //  6.0s: wreath cast end
        //  6.8s: icons + tethers appear (1 dd pair and 1 tank-healer pair with fire, 1 dd pair with wind, 1 tank-healer pair with dark on healer) ==> component starts showing 'break' hint on dark pair and 'stack in center' hint on everyone else
        //  9.2s: dark design cast start
        // 11.8s: 'thornpricked' debuffs
        // 14.2s: dark design cast end ==> component starts showing 'gtfo from aoe/soak tower' hint + 'break' hint for next pair
        // 18.1s: first 2 aoes and towers start cast 27149/27150
        // 19.1s: first 2 aoes and towers finish cast => component starts showing second set
        // 25.1s: last 2 aoes and towers start cast 27149/27150
        // 26.1s: last 2 aoes and towers finish cast => component is reset
        // 26.4s: boss starts casting aoe
        // 27.8s: wind pair expires if not broken
        // 33.4s: boss finishes casting aoe
        Cast(id + 0x2000, AID.WreathOfThorns2, 4.2f, 6, "Wreath2")
            .ActivateOnEnter<WreathOfThorns2>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        Cast(id + 0x3000, AID.DarkDesign, 3.2f, 5, "DarkDesign");
        ComponentCondition<WreathOfThorns2>(id + 0x4000, 4.9f, comp => comp.CurState != WreathOfThorns2.State.FirstSet, "Resolve 1");
        ComponentCondition<WreathOfThorns2>(id + 0x5000, 7, comp => comp.CurState != WreathOfThorns2.State.SecondSet, "Resolve 2");
        UltimateImpulse(id + 0x6000, 0.3f)
            .DeactivateOnExit<WreathOfThorns2>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void AkanthaiAct3(uint id, float delay)
    {
        // 'act 3' is two sets of 4 towers + jumps and knockback from center
        // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers and knockback
        // towers are at (82.61/117.39, 104.66/95.34) and (87.27/112.73, 87.27/112.73)
        Cast(id, AID.AkanthaiAct3, delay, 5, "Act3");

        // timeline:
        // -0.1s: four towers (E/W) tethered
        //  0.0s: wreath cast start ==> component should determine order and show spots for everyone (rdd/healers to soak, some tank to bait jump)
        //  3.0s: center tether
        //  6.0s: remaining tethers
        //  8.0s: wreath cast end
        // 11.2s: kick cast start
        // 16.1s: kick cast end
        // 16.3s: first jump ==> component should switch from jump to cone mode
        // 20.0s: first towers start cast 27150
        // 20.2s: cones 1 go off ==> component should switch to second jump mode
        // 21.0s: first towers finish cast => component should show second towers
        // 22.0s: central tower starts cast 27152
        // 23.0s: central tower finishes cast
        // 26.0s: second towers start cast 27150
        // 26.4s: second jump ==> component should switch to second cone mode
        // 27.0s: second towers finish cast
        // 30.4s: second cones
        Cast(id + 0x1000, AID.WreathOfThorns3, 4.2f, 8, "Wreath3")
            .ActivateOnEnter<WreathOfThorns3>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        Cast(id + 0x2000, AID.KothornosKock, 3.2f, 4.9f, "Jump1");
        ComponentCondition<WreathOfThorns3>(id + 0x2100, 4.3f, comp => comp.NumCones > 0, "Cones1");
        ComponentCondition<WreathOfThorns3>(id + 0x2200, 0.8f, comp => comp.CurState != WreathOfThorns3.State.RangedTowers, "Towers1");
        ComponentCondition<WreathOfThorns3>(id + 0x3000, 2, comp => comp.CurState != WreathOfThorns3.State.Knockback, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<WreathOfThorns3>(id + 0x4000, 3.3f, comp => comp.NumJumps > 1, "Jump2");
        ComponentCondition<WreathOfThorns3>(id + 0x4100, 0.7f, comp => comp.CurState != WreathOfThorns3.State.MeleeTowers, "Towers2");
        ComponentCondition<WreathOfThorns3>(id + 0x4200, 3.4f, comp => comp.NumCones > 1, "Cones2")
            .DeactivateOnExit<WreathOfThorns3>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void AkanthaiAct4(uint id, float delay)
    {
        // 'act 4' is 4 towers + 4 aoes, tethered to players
        // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers and aoes
        // towers are at (82/118, 100) and (100, 82/118), aoes are at (87.27/112.73, 87.27/112.73)
        Cast(id, AID.AkanthaiAct4, delay, 5, "Act4");
        SearingStream(id + 0x1000, 4.2f);

        // timeline:
        //  0.0s: wreath cast ends
        //  0.8s: icons and tethers appear
        //  3.2s: searing stream cast start
        //  5.8s: 'thornpricked' debuffs
        //  8.2s: searing stream cast end
        // .....: blow up tethers
        // 36.4s: ultimate impulse cast start
        Cast(id + 0x2000, AID.WreathOfThorns4, 4.2f, 5, "Wreath4")
            .ActivateOnEnter<WreathOfThorns4>();
        SearingStream(id + 0x3000, 3.2f)
            .SetHint(StateMachine.StateHint.PositioningStart)
            .ExecOnExit<WreathOfThorns4>(comp => comp.ReadyToBreak = true);
        UltimateImpulse(id + 0x4000, 28.2f)
            .DeactivateOnExit<WreathOfThorns4>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void AkanthaiAct5(uint id, float delay)
    {
        // 'act 5' ('finale') is 8 staggered towers that should be soaked in correct order
        // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers
        // towers are at (88/112, 100), (100, 88/112), (91.5/108.5, 91.5/108.5)
        Cast(id, AID.AkanthaiFinale, delay, 5, "Act5");

        // timeline:
        //  0.0s: wreath cast ends
        //  0.8s: icons and tethers appear
        //  3.2s: fleeting impulse cast start
        //  5.8s: 'thornpricked' debuffs
        //  8.1s: fleeting impulse cast ends
        //  8.4s: impulse hit 1 (~0.3 from cast end)
        //  9.9s: impulse hit 2 (~1.3 from prev for each next)
        // 11.2s: impulse hit 3
        // 12.5s: impulse hit 4
        // 13.9s: impulse hit 5
        // 15.2s: impulse hit 6
        // 16.6s: impulse hit 7
        // 17.9s: impulse hit 8
        // 18.8s: 'thornpricked' disappear and some sort of instant cast happens, that does nothing if there are no fails
        // 21.6s: first tether for wreath 6; tethers switch every ~0.5s
        // 21.7s: wreath 6 cast start
        // 27.7s: wreath 6 cast end
        // 29.8s: first tower starts 27150 cast
        // 30.8s: first tower finishes cast
        // ... towers are staggered by ~1.3s
        // 38.8s: near/farsight cast start
        // 39.1s: last tower finishes cast
        Cast(id + 0x1000, AID.WreathOfThorns5, 4.2f, 5, "Wreath5")
            .ActivateOnEnter<WreathOfThorns5>();
        Cast(id + 0x2000, AID.FleetingImpulse, 3.2f, 4.9f, "Impulse");
        Cast(id + 0x3000, AID.WreathOfThorns6, 13.6f, 6, "Wreath6");
        FarNearSight(id + 0x4000, 11.2f)
            .DeactivateOnExit<WreathOfThorns5>();
    }

    private void AkanthaiAct6(uint id, float delay)
    {
        // timeline:
        //  0.0s: curtain call cast ends
        //  0.8s: icons and tethers appear
        //  5.8s: 'thornpricked' debuffs with 12/22/32/42 duration
        // 10.2s: hell sting 1 sequence start
        // 16.3s: hell sting 1 sequence end
        // 30.5s: hell sting 2 sequence start
        // 36.6s: hell sting 2 sequence end
        // 45.7s: aoe start
        // 52.7s: aoe end
        // 55.7s: 'thornpricked' debuffs
        // 59.9s: hell sting 3 sequence start
        // 66.0s: hell sting 3 sequence end
        // 80.2s: hell sting 4 sequence start
        // 86.3s: hell sting 4 sequence end
        // 95.4s: aoe start
        Cast(id, AID.AkanthaiCurtainCall, delay, 5, "Act6")
            .OnExit(Module.ActivateComponent<CurtainCall>);
        HellSting(id + 0x1000, 10.2f);
        HellSting(id + 0x2000, 14.2f);
        UltimateImpulse(id + 0x3000, 9.2f)
            .DeactivateOnExit<CurtainCall>()
            .OnExit(Module.ActivateComponent<CurtainCall>);
        HellSting(id + 0x4000, 7.2f);
        HellSting(id + 0x5000, 14.2f);
        UltimateImpulse(id + 0x6000, 9.2f)
            .DeactivateOnExit<CurtainCall>();
    }
}
