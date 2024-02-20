using System.Linq;

namespace BossMod.Stormblood.Ultimate.UCOB
{
    class UCOBStates : StateMachineBuilder
    {
        private UCOB _module;

        public UCOBStates(UCOB module) : base(module)
        {
            _module = module;
            SimplePhase(0, Phase1Twintania1, "Twintania pre neurolink 1 (100%-74%)")
                .ActivateOnEnter<P1Hatch>()
                .ActivateOnEnter<P1Twister>()
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.FindComponent<P1Hatch>()?.NumNeurolinkSpawns > 0;
            SimplePhase(1, Phase1Twintania2, "Twintania pre neurolink 2 (74%-44%)")
                .ActivateOnEnter<P1LiquidHell>()
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.FindComponent<P1Hatch>()?.NumNeurolinkSpawns > 1;
            SimplePhase(2, Phase1Twintania3, "Twintania pre neurolink 3 (44%-0%)")
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !Module.PrimaryActor.IsTargetable;
            SimplePhase(3, Phase2, "Nael")
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed;
            DeathPhase(4, PhaseX);
        }

        private void Phase1Twintania1(uint id)
        {
            P1Plummet(id, 6.2f);
            P1TwisterFireball(id + 0x10000, 4.1f);
            P1DeathSentence(id + 0x20000, 4.1f);
            // repeat
            P1Plummet(id + 0x30000, 3.2f);
            P1TwisterFireball(id + 0x40000, 3.2f);
            P1DeathSentence(id + 0x50000, 4.1f); // not sure about timing...
            // plummet > twister/fireball > death sentence repeats until 74%
            SimpleState(id + 0xFF0000, 10, "???");
        }

        private void Phase1Twintania2(uint id)
        {
            P1LiquidHell(id, 9.3f);
            P1Generate(id + 0x10000, 1.2f);
            P1LiquidHell(id + 0x20000, 3.3f);
            P1DeathSentence(id + 0x30000, 3.2f);
            P1GenerateTwister(id + 0x40000, 4.2f);
            // TODO: plummet?
            // sequence repeats from start until 44% ?
            SimpleState(id + 0xFF0000, 10, "???");
        }

        private void Phase1Twintania3(uint id)
        {
            P1LiquidHell(id, 9.4f);
            P1Generate(id + 0x10000, 1.2f);
            P1LiquidHellFireball(id + 0x20000, 3.3f);
            P1DeathSentence(id + 0x30000, 5.1f);
            P1Plummet(id + 0x40000, 3.2f);
            P1GenerateTwister(id + 0x50000, 4.1f);
            P1Plummet(id + 0x60000, 4.9f);
            // repeat
            P1LiquidHell(id + 0x70000, 2.3f);
            P1Generate(id + 0x80000, 1.2f);
            P1LiquidHellFireball(id + 0x90000, 3.3f);
            P1DeathSentence(id + 0xA0000, 5.1f);
            P1Plummet(id + 0xB0000, 3.2f);
            P1GenerateTwister(id + 0xC0000, 4.1f);
            P1Plummet(id + 0xD0000, 4.9f);
            // liquid hell > generate > liquid hell/fireball > death sentence > plummet > generate/twister > plummet repeats until enrage
            SimpleState(id + 0xFF0000, 10, "???");
        }

        private void Phase2(uint id)
        {
            P2Heavensfall(id, 8.3f);
            SimpleState(id + 0xFF0000, 100, "???");
        }

        private void PhaseX(uint id)
        {
            SimpleState(id + 0xFF0000, 100, "???");
        }

        private void P1Plummet(uint id, float delay)
        {
            ComponentCondition<P1Plummet>(id, delay, comp => comp.NumCasts > 0, "Cleave")
                .ActivateOnEnter<P1Plummet>()
                .DeactivateOnExit<P1Plummet>();
        }

        private void P1DeathSentence(uint id, float delay)
        {
            ActorCast(id, _module.Twintania, AID.DeathSentence, delay, 4, true, "Tankbuster")
                .ActivateOnEnter<P1DeathSentence>()
                .DeactivateOnExit<P1DeathSentence>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void P1FireballResolve(uint id, float delay)
        {
            ComponentCondition<P1Fireball>(id, delay, comp => comp.ActiveStacks.Count() == 0, "Stack", 1, delay - 0.2f) // note that if target dies, fireball won't happen
                .DeactivateOnExit<P1Fireball>();
        }

        private void P1Twister(uint id, float delay, bool withFireball = false)
        {
            ActorCastStart(id, _module.Twintania, AID.Twister, delay, true)
                .ActivateOnEnter<P1Fireball>(withFireball); // icon appears ~0.1s before cast start
            ActorCastEnd(id + 1, _module.Twintania, 2, true);
            ComponentCondition<P1Twister>(id + 2, 0.3f, comp => comp.Active, "Twisters");
        }

        private void P1TwisterFireball(uint id, float delay)
        {
            P1Twister(id, delay, true);
            P1FireballResolve(id + 0x10, 2.8f);
        }

        private void P1LiquidHell(uint id, float delay, bool withFireball = false)
        {
            ComponentCondition<P1LiquidHell>(id, delay, comp => comp.NumCasts >= 1, "Puddle 1")
                .ActivateOnEnter<P1Fireball>(withFireball)
                .ExecOnEnter<P1LiquidHell>(comp => comp.Reset());
            ComponentCondition<P1LiquidHell>(id + 1, 1.2f, comp => comp.NumCasts >= 2);
            ComponentCondition<P1LiquidHell>(id + 2, 1.2f, comp => comp.NumCasts >= 3);
            ComponentCondition<P1LiquidHell>(id + 3, 1.2f, comp => comp.NumCasts >= 4);
            ComponentCondition<P1LiquidHell>(id + 4, 1.2f, comp => comp.NumCasts >= 5, "Puddle 5");
        }

        private void P1LiquidHellFireball(uint id, float delay)
        {
            P1LiquidHell(id, delay, true);
            P1FireballResolve(id + 0x10, 2.2f);
        }

        private void P1Generate(uint id, float delay)
        {
            ActorCast(id, _module.Twintania, AID.Generate, delay, 3, true, "Hatch"); // icon appears ~0.1s before cast start
        }

        private void P1GenerateTwister(uint id, float delay)
        {
            P1Generate(id, delay);
            P1Twister(id + 0x10, 1.1f);
        }

        private void P2Heavensfall(uint id, float delay)
        {
            ComponentCondition<P2Heavensfall>(id, delay, comp => comp.NumCasts > 0, "Knockback")
                .ActivateOnEnter<P2DalamudDive>() // activate asap until twintania untargets current tank
                .ActivateOnEnter<P2Heavensfall>()
                .DeactivateOnExit<P2Heavensfall>()
                .DeactivateOnExit<P1Hatch>() // clean up p1 components...
                .DeactivateOnExit<P1Twister>()
                .DeactivateOnExit<P1LiquidHell>();

            ComponentCondition<P2ThermionicBurst>(id + 0x10, 4, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<P2ThermionicBurst>()
                .ActivateOnEnter<P2MeteorStream>();
            ComponentCondition<P2MeteorStream>(id + 0x11, 1.6f, comp => comp.NumCasts > 0, "Spread 1");
            ComponentCondition<P2ThermionicBurst>(id + 0x12, 1.4f, comp => comp.NumCasts > 0, "Cones 1");

            ComponentCondition<P2ThermionicBurst>(id + 0x20, 1.2f, comp => comp.Casters.Count > 0);
            ComponentCondition<P2MeteorStream>(id + 0x21, 0.7f, comp => comp.NumCasts > 4, "Spread 2")
                .DeactivateOnExit<P2MeteorStream>();
            ComponentCondition<P2ThermionicBurst>(id + 0x22, 2.3f, comp => comp.NumCasts > 8, "Cones 2")
                .ExecOnEnter<P2DalamudDive>(comp => comp.Show())
                .DeactivateOnExit<P2ThermionicBurst>();

            ComponentCondition<P2DalamudDive>(id + 0x30, 0.4f, comp => comp.NumCasts > 0, "Tankbuster")
                .DeactivateOnExit<P2DalamudDive>()
                .SetHint(StateMachine.StateHint.Tankbuster);
            ActorTargetable(id + 0x31, _module.Nael, true, 2, "Boss appears")
                .SetHint(StateMachine.StateHint.DowntimeEnd);
            ComponentCondition<P2BahamutsClaw>(id + 0x40, 0.3f, comp => comp.NumCasts > 0)
                .ActivateOnEnter<P2BahamutsClaw>();
            ComponentCondition<P2BahamutsClaw>(id + 0x50, 3.3f, comp => comp.NumCasts > 4, "5-hit tankbuster end")
                .DeactivateOnExit<P2BahamutsClaw>();
        }
    }
}
