using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class TEAStates : StateMachineBuilder
    {
        private TEA _module;

        public TEAStates(TEA module) : base(module)
        {
            _module = module;
            SimplePhase(0, Phase1LivingLiquid, "P1: Living Liquid")
                .ActivateOnEnter<P1HandOfPain>()
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.IsDead;
            SimplePhase(1, Phase2BruteJusticeCruiseChaser, "P2: BJ+CC")
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed && (_module.BruteJustice()?.IsDestroyed ?? true) && (_module.CruiseChaser()?.IsDestroyed ?? true);
        }

        private void Phase1LivingLiquid(uint id)
        {
            P1FluidSwing(id, 10.2f);
            P1Cascade(id + 0x10000, 4.1f);
            P1HandsProteansDollsCleaves(id + 0x20000, 11.1f);
            P1ProteansBoss(id + 0x30000, 10.2f);
            P1SplashDrainageCascade(id + 0x40000, 6.1f);
            P1Throttle(id + 0x50000, 4.6f);
            P1ProteansBoth(id + 0x60000, 7.7f);
            P1PainSplashCleaves(id + 0x70000, 5.1f);
            ActorCast(id + 0x80000, _module.BossP1, AID.Enrage, 3.1f, 4, true, "Enrage");
        }

        private void P1FluidSwing(uint id, float delay)
        {
            ComponentCondition<P1FluidSwing>(id, delay, comp => comp.NumCasts > 0, "Cleave")
                .ActivateOnEnter<P1FluidSwing>()
                .DeactivateOnExit<P1FluidSwing>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        // keeps cascade component active
        private void P1Cascade(uint id, float delay)
        {
            ActorCast(id, _module.BossP1, AID.Cascade, delay, 4, true, "Raidwide")
                .ActivateOnEnter<P1Cascade>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private State P1HandOfPain(uint id, float delay, int seq)
        {
            return ComponentCondition<P1HandOfPain>(id, delay, comp => comp.NumCasts >= seq, $"HP check {seq}");
        }

        private void P1HandsProteansDollsCleaves(uint id, float delay)
        {
            Condition(id, delay, () => (_module.LiquidHand()?.ModelState.ModelState ?? 0) != 0, "Hand of parting/prayer bait");
            ComponentCondition<P1ProteanWaveTornadoVisCast>(id + 1, 3.1f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<P1HandOfPartingPrayer>()
                .ActivateOnEnter<P1ProteanWaveTornadoVisBait>()
                .ActivateOnEnter<P1ProteanWaveTornadoVisCast>()
                .DeactivateOnExit<P1ProteanWaveTornadoVisBait>();
            ComponentCondition<P1JagdDolls>(id + 2, 1, comp => comp.Active)
                .ActivateOnEnter<P1JagdDolls>();
            ComponentCondition<P1HandOfPartingPrayer>(id + 3, 1, comp => comp.Resolved, "Resolve")
                .DeactivateOnExit<P1HandOfPartingPrayer>();
            ComponentCondition<P1ProteanWaveTornadoVisCast>(id + 0x10, 1, comp => comp.Casters.Count == 0)
                .ActivateOnEnter<P1FluidStrike>()
                .ActivateOnEnter<P1FluidSwing>()
                .DeactivateOnExit<P1ProteanWaveTornadoVisCast>();
            ComponentCondition<P1FluidSwing>(id + 0x20, 1.1f, comp => comp.NumCasts > 0, "Cleaves")
                .ActivateOnEnter<P1ProteanWaveTornadoInvis>()
                .DeactivateOnExit<P1FluidStrike>()
                .DeactivateOnExit<P1FluidSwing>()
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<P1ProteanWaveTornadoInvis>(id + 0x30, 1, comp => comp.NumCasts > 0)
                .DeactivateOnExit<P1ProteanWaveTornadoInvis>();
            ComponentCondition<P1JagdDolls>(id + 0x100, 1, comp => comp.NumExhausts > 0, "Exhaust 1");
            // +0.1s: hand of pain start
            // +2.0s: pressurize
            // +2.6s: embolus spawn
            P1HandOfPain(id + 0x200, 3.1f, 1);
            ComponentCondition<P1JagdDolls>(id + 0x300, 7.5f, comp => comp.NumExhausts > 1, "Exhaust 2", 0.1f);
            ComponentCondition<P1FluidSwing>(id + 0x400, 6.4f, comp => comp.NumCasts > 0, "Cleaves")
                .ActivateOnEnter<P1FluidStrike>()
                .ActivateOnEnter<P1FluidSwing>()
                .DeactivateOnExit<P1FluidStrike>()
                .DeactivateOnExit<P1FluidSwing>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void P1ProteansBoss(uint id, float delay)
        {
            ActorCast(id + 1, _module.BossP1, AID.ProteanWaveLiquidVisBoss, delay, 3, true, "Protean baited")
                .ActivateOnEnter<P1ProteanWaveLiquidVisBoss>()
                .ActivateOnEnter<P1ProteanWaveLiquidVisHelper>()
                .DeactivateOnExit<P1ProteanWaveLiquidVisBoss>()
                .DeactivateOnExit<P1ProteanWaveLiquidVisHelper>();
            P1HandOfPain(id + 0x10, 0.2f, 2)
                .ActivateOnEnter<P1ProteanWaveLiquidInvisFixed>()
                .ActivateOnEnter<P1ProteanWaveLiquidInvisBaited>()
                .ActivateOnEnter<P1Sluice>();
            ComponentCondition<P1ProteanWaveLiquidInvisFixed>(id + 0x20, 1.9f, comp => comp.NumCasts > 0, "Protean 1");
            ComponentCondition<P1ProteanWaveLiquidInvisFixed>(id + 0x30, 3.0f, comp => comp.NumCasts > 1, "Protean 2")
                .DeactivateOnExit<P1ProteanWaveLiquidInvisFixed>()
                .DeactivateOnExit<P1ProteanWaveLiquidInvisBaited>()
                .DeactivateOnExit<P1Sluice>();
        }

        private void P1SplashDrainageCascade(uint id, float delay)
        {
            ComponentCondition<P1Splash>(id, delay, comp => comp.NumCasts > 0, "Splash start")
                .ActivateOnEnter<P1Splash>()
                .ActivateOnEnter<P1Drainage>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 1, 1.1f, comp => comp.NumCasts > 1).SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 2, 1.1f, comp => comp.NumCasts > 2).SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 3, 1.1f, comp => comp.NumCasts > 3).SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 4, 1.1f, comp => comp.NumCasts > 4).SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 5, 1.1f, comp => comp.NumCasts > 5) // drainage resolves almost at the same time
                .DeactivateOnExit<P1Splash>()
                .DeactivateOnExit<P1Drainage>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ActorCastStart(id + 0x100, _module.BossP1, AID.Cascade, 1.1f, true);
            P1HandOfPain(id + 0x101, 1.3f, 3);
            ActorCastEnd(id + 0x102, _module.BossP1, 2.7f, true, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void P1Throttle(uint id, float delay)
        {
            ComponentCondition<P1Throttle>(id, delay, comp => comp.Applied, "Debuffs")
                .ActivateOnEnter<P1Throttle>()
                .DeactivateOnExit<P1Throttle>();
        }

        private void P1ProteansBoth(uint id, float delay)
        {
            ActorCast(id, _module.BossP1, AID.ProteanWaveLiquidVisBoss, delay, 3, true, "Protean baited")
                .ActivateOnEnter<P1ProteanWaveLiquidVisBoss>()
                .ActivateOnEnter<P1ProteanWaveLiquidVisHelper>()
                .DeactivateOnExit<P1ProteanWaveLiquidVisBoss>()
                .DeactivateOnExit<P1ProteanWaveLiquidVisHelper>();
            P1HandOfPain(id + 0x10, 2, 4)
                .ActivateOnEnter<P1ProteanWaveLiquidInvisFixed>()
                .ActivateOnEnter<P1ProteanWaveLiquidInvisBaited>()
                .ActivateOnEnter<P1Sluice>()
                .ActivateOnEnter<P1ProteanWaveTornadoVisBait>()
                .ActivateOnEnter<P1ProteanWaveTornadoVisCast>();
            ComponentCondition<P1ProteanWaveLiquidInvisFixed>(id + 0x11, 0.1f, comp => comp.NumCasts > 0, "Protean 1");
            ComponentCondition<P1ProteanWaveTornadoVisCast>(id + 0x12, 0.9f, comp => comp.Casters.Count > 0)
                .DeactivateOnExit<P1ProteanWaveTornadoVisBait>();
            ComponentCondition<P1ProteanWaveLiquidInvisFixed>(id + 0x20, 2.1f, comp => comp.NumCasts > 1, "Protean 2")
                .DeactivateOnExit<P1ProteanWaveLiquidInvisFixed>()
                .DeactivateOnExit<P1ProteanWaveLiquidInvisBaited>()
                .DeactivateOnExit<P1Sluice>();
            ComponentCondition<P1ProteanWaveTornadoVisCast>(id + 0x21, 0.8f, comp => comp.Casters.Count == 0)
                .DeactivateOnExit<P1ProteanWaveTornadoVisCast>();
            ComponentCondition<P1ProteanWaveTornadoInvis>(id + 0x30, 2.1f, comp => comp.NumCasts > 0)
                .ActivateOnEnter<P1ProteanWaveTornadoInvis>()
                .DeactivateOnExit<P1ProteanWaveTornadoInvis>();
            // +3.9s: pressurize
            // +4.5s: embolus spawn
            Condition(id + 0x40, 3.9f, () => (_module.LiquidHand()?.ModelState.ModelState ?? 0) != 0, "Hand of parting/prayer bait");
            ComponentCondition<P1HandOfPartingPrayer>(id + 0x41, 5.1f, comp => comp.Resolved, "Resolve")
                .ActivateOnEnter<P1HandOfPartingPrayer>()
                .DeactivateOnExit<P1HandOfPartingPrayer>();
        }

        private void P1PainSplashCleaves(uint id, float delay)
        {
            P1HandOfPain(id + 1, delay, 5)
                .DeactivateOnExit<P1HandOfPain>();
            ComponentCondition<P1Splash>(id + 0x10, 0.3f, comp => comp.NumCasts > 0, "Splash start")
                .ActivateOnEnter<P1Splash>()
                .ActivateOnEnter<P1FluidSwing>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 0x11, 1.1f, comp => comp.NumCasts > 1).SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 0x12, 1.1f, comp => comp.NumCasts > 2).SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1Splash>(id + 0x13, 1.1f, comp => comp.NumCasts > 3).SetHint(StateMachine.StateHint.Raidwide)
                .DeactivateOnExit<P1Splash>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P1FluidSwing>(id + 0x20, 1.2f, comp => comp.NumCasts > 0, "Cleave")
                .DeactivateOnExit<P1FluidSwing>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void Phase2BruteJusticeCruiseChaser(uint id)
        {
            P2Intermission(id);
            P2WhirlwindDebuffs(id + 0x10000, 5.2f);
            P2ChakramOpticalSightPhoton(id + 0x20000, 6);
            P2SpinCrusherCompressedWaterLightning(id + 0x30000, 7.2f);
            P2MissileCommand(id + 0x40000, 4.3f);
            P2VerdictGavel(id + 0x50000, 1.4f);
            // TODO: tankbusters > jumps + ray > whirlwind x2 > enrage
            SimpleState(id + 0xFF0000, 10000, "???");
        }

        private void P2Intermission(uint id)
        {
            Timeout(id, 0)
                .SetHint(StateMachine.StateHint.DowntimeStart);
            ComponentCondition<P2IntermissionHawkBlaster>(id + 0x10, 5.2f, comp => comp.NumCasts > 0, "First aoe")
                .ActivateOnEnter<P2IntermissionOrder>()
                .ActivateOnEnter<P2IntermissionHawkBlaster>();
            ComponentCondition<P2IntermissionKnockbacks>(id + 0x20, 7.4f, comp => comp.NumCasts >= 1, "Hit 1")
                .ActivateOnEnter<P2IntermissionKnockbacks>();
            ComponentCondition<P2IntermissionKnockbacks>(id + 0x30, 4.6f, comp => comp.NumCasts >= 3, "Hit 3");
            ComponentCondition<P2IntermissionKnockbacks>(id + 0x40, 4.6f, comp => comp.NumCasts >= 5, "Hit 5");
            ComponentCondition<P2IntermissionKnockbacks>(id + 0x50, 4.6f, comp => comp.NumCasts >= 7, "Hit 7");
            ComponentCondition<P2JKick>(id + 0x100, 4.6f, comp => comp.NumCasts > 0)
                .ActivateOnEnter<P2JKick>()
                .DeactivateOnExit<P2IntermissionOrder>()
                .DeactivateOnExit<P2IntermissionHawkBlaster>()
                .DeactivateOnExit<P2IntermissionKnockbacks>()
                .DeactivateOnExit<P2JKick>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ActorTargetable(id + 0x101, _module.BruteJustice, true, 3, "Intermission end")
                .SetHint(StateMachine.StateHint.DowntimeEnd);
        }

        // keeps nisi component active
        private void P2WhirlwindDebuffs(uint id, float delay)
        {
            ActorCastStart(id, _module.CruiseChaser, AID.Whirlwind, delay);
            ActorCastStart(id + 1, _module.BruteJustice, AID.JudgmentNisi, 3);
            ActorCastEnd(id + 2, _module.CruiseChaser, 1, false, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
            ActorCastEnd(id + 3, _module.BruteJustice, 3, false, "Nisi")
                .ActivateOnEnter<P2Nisi>(); // debuffs are applied ~0.8s after cast end
            ActorCast(id + 0x10, _module.BruteJustice, AID.LinkUp, 3.2f, 3, false, "Debuffs")
                .ActivateOnEnter<P2CompressedWaterLightning>(); // debuffs & icons are applied ~0.8s after cast end
        }

        private void P2ChakramOpticalSightPhoton(uint id, float delay)
        {
            ActorCastStart(id, _module.CruiseChaser, AID.OpticalSight, delay)
                .ActivateOnEnter<P2EyeOfTheChakram>();
            ActorCastEnd(id + 1, _module.CruiseChaser, 2);
            ComponentCondition<P2EyeOfTheChakram>(id + 2, 0.9f, comp => comp.NumCasts > 0, "Chakrams")
                .ActivateOnEnter<P2HawkBlasterOpticalSight>()
                .DeactivateOnExit<P2EyeOfTheChakram>();
            ActorCastStart(id + 0x10, _module.CruiseChaser, AID.Photon, 3.3f);
            ComponentCondition<P2HawkBlasterOpticalSight>(id + 0x11, 1.1f, comp => comp.NumCasts > 0, "Puddles")
                .DeactivateOnExit<P2HawkBlasterOpticalSight>();
            ActorCastEnd(id + 0x12, _module.CruiseChaser, 1.9f, false, "Photon")
                .OnEnter(() => Module.FindComponent<P2Nisi>()!.ShowPassHint = 1) // first nisi pass should happen around photon cast end
                .OnExit(() => Module.FindComponent<P2CompressedWaterLightning>()!.ResolveImminent = true) // should start moving to debuff stacks after nisi pass
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        // keeps tornado component active
        private void P2SpinCrusherCompressedWaterLightning(uint id, float delay)
        {
            ActorCast(id, _module.CruiseChaser, AID.SpinCrusher, delay, 3, false, "Baited cleave")
                .ActivateOnEnter<P2SpinCrusher>()
                .DeactivateOnExit<P2SpinCrusher>();
            ComponentCondition<P2CompressedWaterLightning>(id + 0x10, 4.6f, comp => !comp.ResolveImminent, "Water/lightning 1")
                .ActivateOnEnter<P2Drainage>(); // tornado spawns ~1s after resolve
            // +0.6s: vulns applied to previous stack targets
            // +0.7s: icons/debuffs applied to next stack targets
            // +0.8s: first sets of nisis expire
        }

        private void P2MissileCommand(uint id, float delay)
        {
            ActorCast(id, _module.BruteJustice, AID.MissileCommand, delay, 3);
            ComponentCondition<P2EarthMissileBaited>(id + 0x10, 1.1f, comp => comp.HaveCasters, "Bait missiles")
                .ActivateOnEnter<P2EarthMissileBaited>();
            ComponentCondition<P2Enumeration>(id + 0x20, 2.0f, comp => comp.Active)
                .ActivateOnEnter<P2Enumeration>();
            ComponentCondition<P2HiddenMinefield>(id + 0x30, 0.1f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<P2HiddenMinefield>();
            ComponentCondition<P2EarthMissileBaited>(id + 0x40, 1.0f, comp => !comp.HaveCasters); // voidzones appear at cast positions with a slight delay
            ComponentCondition<P2HiddenMinefield>(id + 0x50, 2.0f, comp => comp.Casters.Count == 0);
            ComponentCondition<P2Enumeration>(id + 0x60, 2.1f, comp => !comp.Active, "Enumerations + Ice")
                .ActivateOnEnter<P2EarthMissileIce>()
                .DeactivateOnExit<P2Enumeration>();
            ComponentCondition<P2EarthMissileIce>(id + 0x70, 0.8f, comp => comp.Sources(Module).Any());
            // +4.0s: ice voidzone grows
            // +5.6s: gelid gaol spawns where tornado is (assuming it is in ice voidzone)
            // +6.3s: tornado is destroyed
            // +6.3s: if any mine is not soaked, they explode now
            // +6.8s: smaller ice voidzone disappears (eventstate 7)
            // +7.7s: fire voidzones disappear (eventstate 7)
            ComponentCondition<P2EarthMissileIce>(id + 0x80, 9.8f, comp => !comp.Sources(Module).Any(), "Voidzones disappear")
                .OnEnter(() => Module.FindComponent<P2Nisi>()!.ShowPassHint = 2) // second nisi pass should happen after enumerations are resolved
                .OnEnter(() => Module.FindComponent<P2CompressedWaterLightning>()!.ResolveImminent = true) // should start moving to debuff stacks after nisi pass
                .DeactivateOnExit<P2Drainage>()
                .DeactivateOnExit<P2EarthMissileBaited>()
                .DeactivateOnExit<P2HiddenMinefield>()
                .DeactivateOnExit<P2EarthMissileIce>();
        }

        private void P2VerdictGavel(uint id, float delay)
        {
            ActorCastStart(id, _module.BruteJustice, AID.Verdict, delay);
            ComponentCondition<P2CompressedWaterLightning>(id + 0x10, 2.2f, comp => !comp.ResolveImminent, "Water/lightning 2")
                .ActivateOnEnter<P2Drainage>();
            ActorCastEnd(id + 0x20, _module.BruteJustice, 1.8f); // judgment debuffs appear ~0.8s after cast end

            ActorCast(id + 0x100, _module.CruiseChaser, AID.LimitCut, 3.2f, 2, false, "CC invuln") // note: BJ starts flarethrower cast together with CC; invuln is applied ~0.6s after cast end
                .ActivateOnEnter<P2Flarethrower>();
            ActorCastEnd(id + 0x102, _module.BruteJustice, 1.9f)
                .ActivateOnEnter<P2PlasmaShield>();
            ComponentCondition<P2Flarethrower>(id + 0x103, 0.3f, comp => comp.NumCasts > 0, "Baited flamethrower")
                .DeactivateOnExit<P2Flarethrower>() // note: tornado is normally destroyed by a flarethrower, failing to do that will cause tornado to wipe the raid later
                .OnExit(() => Module.FindComponent<P2Nisi>()!.ShowPassHint = 3) // third nisi pass should happen after flarethrower bait
                .OnExit(() => Module.FindComponent<P2CompressedWaterLightning>()!.ResolveImminent = true); // resolve stacks after nisi pass
            ActorCast(id + 0x110, _module.CruiseChaser, AID.Whirlwind, 8.0f, 4, false, "Raidwide")
                .DeactivateOnExit<P2PlasmaShield>() // it's a wipe if shield is not dealth with in time
                .SetHint(StateMachine.StateHint.Raidwide);

            ComponentCondition<P2CompressedWaterLightning>(id + 0x200, 8.7f, comp => !comp.ResolveImminent, "Water/lightning 3")
                .DeactivateOnExit<P2CompressedWaterLightning>()
                .OnExit(() => Module.FindComponent<P2Nisi>()!.ShowPassHint = 4); // fourth nisi pass should happen after last stacks, while resolving propeller wind
            // TODO: propeller wind & 4th nisi pass > gavel
        }
    }
}
