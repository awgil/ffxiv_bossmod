using System.Linq;

namespace BossMod.Stormblood.Ultimate.UCOB
{
    class P2BlockTransition : BossComponent { }

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
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || _module.Nael() is var nael && nael != null && !nael.IsTargetable && nael.HP.Cur <= 1 && Module.FindComponent<P2BlockTransition>() == null;
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
            P1Plummet(id + 0x50000, 5.8f);
            // repeat
            P1LiquidHell(id + 0x60000, 5.4f);
            P1Generate(id + 0x70000, 1.2f); // not sure about timing...
            P1LiquidHell(id + 0x80000, 3.3f);
            P1DeathSentence(id + 0x90000, 3.2f);
            P1GenerateTwister(id + 0xA0000, 4.2f);
            P1Plummet(id + 0xB0000, 5.8f);
            // hell > generate > hell > death sentence > generate+twister > plummet sequence repeats until 44%
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
            P2Heavensfall(id, 8.4f);
            P2BahamutsFavor(id + 0x10000, 2.9f);
            P2Divebombs(id + 0x20000, 5.2f);
            P2BahamutsFavorQuote(id + 0x30000, 8.8f);
            P2BahamutsFavorQuote(id + 0x40000, 3.0f);
            P2Ravensbeak(id + 0x50000, 4.1f);
            // TODO: bahamut claw > enrage
            SimpleState(id + 0xFF0000, 100, "???");
        }

        private void PhaseX(uint id)
        {
            P3SeventhUmbralEra(id, 5.3f);
            P3FlareBreath(id + 0x10000, 6.2f);
            P3Flatten(id + 0x20000, 4.1f);
            P3QuickmarchTrio(id + 0x30000, 3.1f);
            P3BlackfireTrio(id + 0x40000, 4.2f);
            P3FellruinTrio(id + 0x50000, 9.2f);
            P3HeavensfallTrio(id + 0x60000, 4.2f);
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

        private State P2BahamutsClaw(uint id, float delay)
        {
            ComponentCondition<P2BahamutsClaw>(id, delay, comp => comp.NumCasts > 0)
                .ActivateOnEnter<P2BahamutsClaw>();
            return ComponentCondition<P2BahamutsClaw>(id + 0x10, 3.2f, comp => comp.NumCasts > 4, "5-hit tankbuster end")
                .DeactivateOnExit<P2BahamutsClaw>();
        }

        private State P2Ravensbeak(uint id, float delay)
        {
            return ActorCast(id, _module.Nael, AID.Ravensbeak, delay, 4, true, "Tankbuster")
                .ActivateOnEnter<P2Ravensbeak>()
                .DeactivateOnExit<P2Ravensbeak>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void P2Heavensfall(uint id, float delay)
        {
            ComponentCondition<P2Heavensfall>(id, delay, comp => comp.NumCasts > 0, "Knockback")
                .ActivateOnEnter<P2HeavensfallDalamudDive>() // activate asap until twintania untargets current tank
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
            ComponentCondition<P2MeteorStream>(id + 0x21, 0.6f, comp => comp.NumCasts > 4, "Spread 2", 2) // sometimes it's heavily delayed...
                .DeactivateOnExit<P2MeteorStream>();
            ComponentCondition<P2ThermionicBurst>(id + 0x22, 2.4f, comp => comp.NumCasts > 8, "Cones 2")
                .ExecOnEnter<P2HeavensfallDalamudDive>(comp => comp.Show())
                .DeactivateOnExit<P2ThermionicBurst>();

            ComponentCondition<P2HeavensfallDalamudDive>(id + 0x30, 0.4f, comp => comp.NumCasts > 0, "Tankbuster")
                .DeactivateOnExit<P2HeavensfallDalamudDive>()
                .SetHint(StateMachine.StateHint.Tankbuster);
            ActorTargetable(id + 0x31, _module.Nael, true, 2, "Boss appears")
                .SetHint(StateMachine.StateHint.DowntimeEnd);

            P2BahamutsClaw(id + 0x40, 0.3f);
        }

        private void P2BahamutsFavor(uint id, float delay)
        {
            // there are 24 iceballs total; first 4 groups of 4 are 2s apart, and have fireball after each one; then the last 8 are fired in quick succession (1s apart)
            ActorCast(id, _module.Nael, AID.BahamutsFavor, delay, 3, true);
            ComponentCondition<P2BahamutsFavorChainLightning>(id + 0x10, 8, comp => comp.ActiveOrSkipped(Module))
                .ActivateOnEnter<Quote>()
                .ActivateOnEnter<P2BahamutsFavorChainLightning>();
            ComponentCondition<Quote>(id + 0x11, 0.1f, comp => comp.PendingMechanics.Count > 0); // first quote
            // +1.9s: iceball 1
            // +3.9s: iceball 2
            ComponentCondition<P2BahamutsFavorChainLightning>(id + 0x20, 5.0f, comp => !comp.Active, "Lightning spread", checkDelay: 5) // lighting target can die early, which would trigger premature transition
                .ActivateOnEnter<QuoteIronChariotLunarDynamo>()
                .DeactivateOnExit<P2BahamutsFavorChainLightning>();
            ComponentCondition<Quote>(id + 0x21, 0.1f, comp => comp.PendingMechanics.Count == 1, "In");
            // +0.8s: iceball 3
            // +2.8s: iceball 4
            ComponentCondition<Quote>(id + 0x30, 3.1f, comp => comp.PendingMechanics.Count == 0, "Out/Stack")
                .ActivateOnEnter<QuoteThermionicBeam>()
                .ActivateOnEnter<P2BahamutsFavorFireball>() // tether appears ~0.6s after in
                .DeactivateOnExit<QuoteIronChariotLunarDynamo>()
                .DeactivateOnExit<QuoteThermionicBeam>()
                .DeactivateOnExit<Quote>()
                .ExecOnExit<P2BahamutsFavorFireball>(comp => comp.Show());

            ComponentCondition<P2BahamutsFavorDeathstorm>(id + 0x100, 0.7f, comp => comp.NumDeathstorms > 0) // wings of salvation 1 bait at the same time
                .ActivateOnEnter<P2BahamutsFavorDeathstorm>(); // dooms are applied ~1.0s after deathstorm cast
            ComponentCondition<P2BahamutsFavorFireball>(id + 0x110, 2.0f, comp => !comp.Active, "Fireball 1")
                .ActivateOnEnter<P2BahamutsFavorWingsOfSalvation>()
                .DeactivateOnExit<P2BahamutsFavorFireball>();
            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x111, 0.9f, comp => comp.NumCasts > 0);
            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x120, 1.2f, comp => comp.Casters.Count > 0); // wings of salvation 2 bait
            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x121, 3.0f, comp => comp.NumCasts > 1)
                .DeactivateOnExit<P2BahamutsFavorWingsOfSalvation>();
            // +1.2s: iceball 5
            // +3.2s: iceball 6
            ComponentCondition<P2BahamutsClaw>(id + 0x130, 3.4f, comp => comp.NumCasts > 0)
                .ActivateOnEnter<P2BahamutsClaw>();
            // +0.8s: bahamut's claw 2
            ComponentCondition<P2BahamutsFavorFireball>(id + 0x132, 1.5f, comp => comp.Target != null)
                .ActivateOnEnter<P2BahamutsFavorFireball>()
                .ExecOnExit<P2BahamutsFavorFireball>(comp => comp.Show()); // show hint immediately
            // +0.2s: bahamut's claw 3
            // +0.4s: iceball 7
            // +1.0s: bahamut's claw 4
            ComponentCondition<P2BahamutsClaw>(id + 0x136, 1.7f, comp => comp.NumCasts > 4, "5-hit tankbuster end")
                .DeactivateOnExit<P2BahamutsClaw>();
            // +0.6s: iceball 8

            ComponentCondition<P2BahamutsFavorChainLightning>(id + 0x200, 2.4f, comp => comp.ActiveOrSkipped(Module))
                .ActivateOnEnter<P2BahamutsFavorChainLightning>();
            ComponentCondition<P2BahamutsFavorFireball>(id + 0x210, 1.0f, comp => !comp.Active, "Fireball 2")
                .DeactivateOnExit<P2BahamutsFavorFireball>();
            ComponentCondition<Quote>(id + 0x220, 2.4f, comp => comp.PendingMechanics.Count > 0) // second quote
                .ActivateOnEnter<Quote>();
            ComponentCondition<P2BahamutsFavorChainLightning>(id + 0x230, 1.7f, comp => !comp.Active, "Lightning spread", checkDelay: 1.7f) // lighting target can die early, which would trigger premature transition
                .DeactivateOnExit<P2BahamutsFavorChainLightning>();
            ComponentCondition<Quote>(id + 0x240, 3.4f, comp => comp.PendingMechanics.Count == 1, "Stack")
                .ActivateOnEnter<QuoteThermionicBeam>()
                .DeactivateOnExit<QuoteThermionicBeam>();
            ComponentCondition<Quote>(id + 0x250, 3.1f, comp => comp.PendingMechanics.Count == 0, "In/out")
                .ActivateOnEnter<QuoteIronChariotLunarDynamo>()
                .DeactivateOnExit<QuoteIronChariotLunarDynamo>()
                .DeactivateOnExit<Quote>();

            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x300, 0.6f, comp => comp.Casters.Count > 0) // wings of salvation 1 bait
                .ActivateOnEnter<P2BahamutsFavorWingsOfSalvation>();
            ComponentCondition<P2BahamutsFavorDeathstorm>(id + 0x301, 0.8f, comp => comp.NumDeathstorms > 1);
            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x303, 2.2f, comp => comp.NumCasts > 0);
            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x310, 1.2f, comp => comp.Casters.Count > 0); // wings of salvation 2 bait
            // +2.1s: iceball 9
            ComponentCondition<P2BahamutsFavorChainLightning>(id + 0x312, 2.8f, comp => comp.ActiveOrSkipped(Module))
                .ActivateOnEnter<P2BahamutsFavorChainLightning>();
            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x313, 0.2f, comp => comp.NumCasts > 1);
            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x320, 1.2f, comp => comp.Casters.Count > 0); // wings of salvation 3 bait
            // +0.0s: iceball 10
            // +2.0s: iceball 11
            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x323, 3.0f, comp => comp.NumCasts > 2)
                .ActivateOnEnter<P2BahamutsFavorFireball>() // tether appears ~1.5s after salvation cast start
                .DeactivateOnExit<P2BahamutsFavorWingsOfSalvation>();
            ComponentCondition<P2BahamutsFavorChainLightning>(id + 0x324, 0.7f, comp => !comp.Active, "Lightning spread", checkDelay: 0.7f) // lighting target can die early, which would trigger premature transition
                .DeactivateOnExit<P2BahamutsFavorChainLightning>()
                .ExecOnExit<P2BahamutsFavorFireball>(comp => comp.Show());
            // +0.4s: iceball 12
            ComponentCondition<P2BahamutsFavorFireball>(id + 0x350, 3.0f, comp => !comp.Active, "Fireball 3")
                .DeactivateOnExit<P2BahamutsFavorFireball>();

            P2BahamutsClaw(id + 0x400, 3.6f);

            ComponentCondition<Quote>(id + 0x500, 2.8f, comp => comp.PendingMechanics.Count > 0) // third quote
                .ActivateOnEnter<Quote>();
            // +3.0s: iceball 13
            // +5.0s: iceball 14
            ComponentCondition<Quote>(id + 0x510, 5.2f, comp => comp.PendingMechanics.Count == 1, "Spread")
                .ActivateOnEnter<QuoteRavenDive>()
                .DeactivateOnExit<QuoteRavenDive>();
            ComponentCondition<P2BahamutsFavorFireball>(id + 0x511, 1.3f, comp => comp.Target != null)
                .ActivateOnEnter<P2BahamutsFavorFireball>()
                .ActivateOnEnter<QuoteIronChariotLunarDynamo>()
                .ExecOnExit<P2BahamutsFavorFireball>(comp => comp.Show()); // show hint immediately
            // +0.5s: iceball 15
            ComponentCondition<Quote>(id + 0x520, 1.8f, comp => comp.PendingMechanics.Count == 0, "In/out")
                .DeactivateOnExit<QuoteIronChariotLunarDynamo>()
                .DeactivateOnExit<Quote>();
            // +0.9s: iceball 16
            ComponentCondition<P2BahamutsFavorChainLightning>(id + 0x530, 1.3f, comp => comp.ActiveOrSkipped(Module))
                .ActivateOnEnter<P2BahamutsFavorChainLightning>();
            ComponentCondition<P2BahamutsFavorFireball>(id + 0x540, 2.0f, comp => !comp.Active, "Fireball 4")
                .DeactivateOnExit<P2BahamutsFavorFireball>();

            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x600, 0.3f, comp => comp.Casters.Count > 0) // wings of salvation 1 bait
                .ActivateOnEnter<P2BahamutsFavorWingsOfSalvation>();
            ComponentCondition<P2BahamutsFavorDeathstorm>(id + 0x601, 0.4f, comp => comp.NumDeathstorms > 2);
            ComponentCondition<P2BahamutsFavorChainLightning>(id + 0x610, 2.3f, comp => !comp.Active, "Lightning spread", checkDelay: 2.3f) // lighting target can die early, which would trigger premature transition
                .DeactivateOnExit<P2BahamutsFavorChainLightning>();
            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x611, 0.2f, comp => comp.NumCasts > 0);
            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x620, 1.2f, comp => comp.Casters.Count > 0); // wings of salvation 2 bait
            // +1.2s: iceball 17
            // +2.3s: iceball 18
            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x623, 3.0f, comp => comp.NumCasts > 1);
            // +0.4s: iceball 19
            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x630, 1.1f, comp => comp.Casters.Count > 0); // wings of salvation 3 bait
            // +0.5s: iceball 20
            // +1.6s: iceball 21
            // +2.7s: iceball 22
            ComponentCondition<P2BahamutsFavorWingsOfSalvation>(id + 0x634, 3.0f, comp => comp.NumCasts > 2)
                .DeactivateOnExit<P2BahamutsFavorWingsOfSalvation>();
            // +0.8s: iceball 23
            // +1.9s: iceball 24

            P2Ravensbeak(id + 0x700, 6.1f)
                .DeactivateOnExit<P2BahamutsFavorDeathstorm>();
        }

        private void P2Divebombs(uint id, float delay)
        {
            ComponentCondition<Quote>(id, delay, comp => comp.PendingMechanics.Count > 0) // fourth quote
                .ActivateOnEnter<Quote>()
                .ActivateOnEnter<P2Cauterize>(); // first icon appears together with quote
            ComponentCondition<P2Cauterize>(id + 1, 4, comp => comp.NumBaitsAssigned >= 2)
                .ActivateOnEnter<P2Hypernova>()
                .ActivateOnEnter<QuoteDalamudDive>()
                .ActivateOnEnter<QuoteMeteorStream>();
            ComponentCondition<P2Hypernova>(id + 2, 1.2f, comp => comp.NumCasts >= 1);
            ComponentCondition<P2Hypernova>(id + 3, 1.6f, comp => comp.NumCasts >= 2);
            ComponentCondition<P2Cauterize>(id + 0x10, 0.5f, comp => comp.Casters.Count + comp.NumCasts >= 2, "Divebomb bait 1")
                .ActivateOnEnter<P2BlockTransition>(); // if this mechanic has started, transition won't happen until the end; nael would still briefly disappear
            ComponentCondition<P2Cauterize>(id + 0x11, 0.7f, comp => comp.NumBaitsAssigned >= 3);
            ComponentCondition<P2Hypernova>(id + 0x12, 0.4f, comp => comp.NumCasts >= 3);
            ComponentCondition<P2Hypernova>(id + 0x13, 1.6f, comp => comp.NumCasts >= 4);
            ComponentCondition<P2Cauterize>(id + 0x20, 1.3f, comp => comp.Casters.Count + comp.NumCasts >= 3, "Divebomb bait 2");
            ComponentCondition<Quote>(id + 0x30, 3.3f, comp => comp.PendingMechanics.Count == 1, "Spread/tankbuster")
                .DeactivateOnExit<QuoteMeteorStream>();
            ComponentCondition<P2Cauterize>(id + 0x40, 0.7f, comp => comp.Casters.Count + comp.NumCasts >= 5, "Divebomb bait 3")
                .ActivateOnEnter<QuoteThermionicBeam>();
            ComponentCondition<Quote>(id + 0x50, 1.6f, comp => comp.PendingMechanics.Count == 0, "Tankbuster/stack", 2)
                .DeactivateOnExit<QuoteDalamudDive>()
                .DeactivateOnExit<QuoteThermionicBeam>()
                .DeactivateOnExit<Quote>();
            ComponentCondition<P2Cauterize>(id + 0x60, 2.5f, comp => comp.NumCasts >= 5)
                .DeactivateOnExit<P2Cauterize>()
                .DeactivateOnExit<P2BlockTransition>(); // nael should be back by now

            P2BahamutsClaw(id + 0x100, 2.7f)
                .DeactivateOnExit<P2Hypernova>();
        }

        private void P2BahamutsFavorQuote(uint id, float delay)
        {
            ComponentCondition<Quote>(id, delay, comp => comp.PendingMechanics.Count > 0) // 5th/6th quote
                .ActivateOnEnter<Quote>();
            ComponentCondition<Quote>(id + 0x10, 5.1f, comp => comp.PendingMechanics.Count == 1, "In/stack/spread")
                .ActivateOnEnter<QuoteIronChariotLunarDynamo>()
                .ActivateOnEnter<QuoteThermionicBeam>()
                .ActivateOnEnter<QuoteRavenDive>()
                .DeactivateOnExit<QuoteRavenDive>();
            ComponentCondition<Quote>(id + 0x20, 3.1f, comp => comp.PendingMechanics.Count == 0, "Out/stack/in")
                .DeactivateOnExit<QuoteIronChariotLunarDynamo>()
                .DeactivateOnExit<QuoteThermionicBeam>()
                .DeactivateOnExit<Quote>();
        }

        private void P3SeventhUmbralEra(uint id, float delay)
        {
            ComponentCondition<P3SeventhUmbralEra>(id, delay, comp => comp.NumCasts > 0, "Knockback")
                .ActivateOnEnter<P3SeventhUmbralEra>()
                .DeactivateOnExit<P3SeventhUmbralEra>();
            ComponentCondition<P3CalamitousFlame>(id + 0x10, 3, comp => comp.NumCasts > 0)
                .ActivateOnEnter<P3CalamitousFlame>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P3CalamitousFlame>(id + 0x11, 1, comp => comp.NumCasts > 1)
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P3CalamitousFlame>(id + 0x12, 1, comp => comp.NumCasts > 2)
                .DeactivateOnExit<P3CalamitousFlame>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P3CalamitousBlaze>(id + 0x20, 2.9f, comp => comp.NumCasts > 0, "Tank LB")
                .ActivateOnEnter<P3CalamitousBlaze>()
                .DeactivateOnExit<P3CalamitousBlaze>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ActorTargetable(id + 0x100, _module.BahamutPrime, true, 3.0f, "Boss appears")
                .SetHint(StateMachine.StateHint.DowntimeEnd);
        }

        private State P3FlareBreath(uint id, float delay)
        {
            return ComponentCondition<P3FlareBreath>(id, delay, comp => comp.NumCasts > 0, "Cleave")
                .ActivateOnEnter<P3FlareBreath>()
                .DeactivateOnExit<P3FlareBreath>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private State P3Flatten(uint id, float delay)
        {
            return ActorCast(id, _module.BahamutPrime, AID.Flatten, delay, 4, true, "Tankbuster")
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void P3QuickmarchTrio(uint id, float delay)
        {
            ActorCast(id, _module.BahamutPrime, AID.QuickmarchTrio, delay, 4, true);
            ActorTargetable(id + 0x10, _module.BahamutPrime, false, 2, "Boss disappears (quickmarch trio)")
                .SetHint(StateMachine.StateHint.DowntimeStart);
            ComponentCondition<P3QuickmarchTrio>(id + 0x20, 1.2f, comp => comp.Active)
                .ActivateOnEnter<P3QuickmarchTrio>();

            ComponentCondition<P3MegaflareDive>(id + 0x30, 1.1f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<P3MegaflareDive>();
            ComponentCondition<P3MegaflareDive>(id + 0x31, 4, comp => comp.NumCasts > 0, "Dives")
                .ActivateOnEnter<P3TwistingDive>()
                .ActivateOnEnter<P3LunarDive>()
                .DeactivateOnExit<P3TwistingDive>()
                .DeactivateOnExit<P3LunarDive>()
                .DeactivateOnExit<P3MegaflareDive>()
                .DeactivateOnExit<P3QuickmarchTrio>();
            ComponentCondition<P3Twister>(id + 0x40, 1.4f, comp => comp.Active, "Twisters")
                .ActivateOnEnter<P3Twister>();

            ComponentCondition<P3MegaflarePuddle>(id + 0x50, 1.6f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<P3MegaflareSpreadStack>() // stack icons appear ~0.1s before puddles start
                .ActivateOnEnter<P3MegaflarePuddle>();
            ComponentCondition<P3MegaflareSpreadStack>(id + 0x51, 1, comp => comp.Spreads.Count == 0, "Spread");
            ActorTargetable(id + 0x52, _module.BahamutPrime, true, 1.2f, "Boss reappears")
                .ActivateOnEnter<P3EarthShaker>() // icons appear together with boss reappearing
                .ActivateOnEnter<P3EarthShakerVoidzone>()
                .SetHint(StateMachine.StateHint.DowntimeEnd);
            ComponentCondition<P3MegaflarePuddle>(id + 0x53, 0.8f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<P3MegaflarePuddle>();
            ComponentCondition<P3TempestWing>(id + 0x54, 0.3f, comp => comp.Active)
                .ActivateOnEnter<P3TempestWing>();
            ComponentCondition<P3Twister>(id + 0x55, 0.7f, comp => !comp.Active)
                .DeactivateOnExit<P3Twister>();
            ComponentCondition<P3MegaflareSpreadStack>(id + 0x56, 1, comp => comp.Stacks.Count == 0, "Enumeration")
                .DeactivateOnExit<P3MegaflareSpreadStack>();

            ComponentCondition<P3EarthShaker>(id + 0x60, 2.3f, comp => comp.NumCasts > 0, "Baited cones")
                .DeactivateOnExit<P3EarthShaker>();
            ComponentCondition<P3TempestWing>(id + 0x70, 2.0f, comp => comp.NumCasts > 0, "Tethers")
                .DeactivateOnExit<P3TempestWing>();

            P3FlareBreath(id + 0x1000, 4.1f);
            P3Flatten(id + 0x2000, 4.1f)
                .DeactivateOnExit<P3EarthShakerVoidzone>();
        }

        private void P3BlackfireTrio(uint id, float delay)
        {
            ActorCast(id, _module.BahamutPrime, AID.BlackfireTrio, delay, 4, true);
            ActorTargetable(id + 0x10, _module.BahamutPrime, false, 2, "Boss disappears (blackfire trio)")
                .SetHint(StateMachine.StateHint.DowntimeStart);
            ComponentCondition<P3BlackfireTrio>(id + 0x20, 1.2f, comp => comp.Active)
                .ActivateOnEnter<P3BlackfireTrio>()
                .ActivateOnEnter<P3ThermionicBeam>();
            ComponentCondition<P3MegaflareDive>(id + 0x30, 1.2f, comp => comp.Casters.Count > 0, "Dive bait")
                .ActivateOnEnter<P3MegaflareDive>()
                .ActivateOnEnter<P1LiquidHell>(); // first puddle appears ~0.1s before dive bait
            ComponentCondition<P3ThermionicBeam>(id + 0x40, 2.9f, comp => !comp.Active, "Stack")
                .DeactivateOnExit<P3ThermionicBeam>();
            // +0.5s: 4th liquid hell
            ComponentCondition<P3MegaflareDive>(id + 0x50, 1.0f, comp => comp.NumCasts > 0, "Dive")
                .DeactivateOnExit<P3MegaflareDive>()
                .DeactivateOnExit<P3BlackfireTrio>();
            // +0.7s: 5th liquid hell

            ComponentCondition<P3MegaflareTower>(id + 0x100, 2.0f, comp => comp.Towers.Count > 0)
                .ActivateOnEnter<P3MegaflareTower>();
            ComponentCondition<P3MegaflareStack>(id + 0x101, 1.0f, comp => comp.Active)
                .ActivateOnEnter<P3MegaflareStack>();
            // +3.2s: hypernova 1
            // +4.8s: hypernova 2
            ComponentCondition<P3MegaflareStack>(id + 0x110, 5.1f, comp => !comp.Active, "Enumeration")
                .ActivateOnEnter<P2Hypernova>()
                .DeactivateOnExit<P3MegaflareStack>();
            // +1.4s: hypernova 3
            ComponentCondition<P3MegaflareTower>(id + 0x120, 1.9f, comp => comp.NumCasts > 0, "Towers")
                .DeactivateOnExit<P3MegaflareTower>();

            ActorTargetable(id + 0x200, _module.BahamutPrime, true, 0.3f, "Boss reappears")
                .SetHint(StateMachine.StateHint.DowntimeEnd);
            // +0.8s: hypernova 4
            ActorCast(id + 0x210, _module.BahamutPrime, AID.Gigaflare, 0.1f, 6, true, "Raidwide")
                .DeactivateOnExit<P1LiquidHell>() // last voidzone disappears ~2.6s before cast end
                .SetHint(StateMachine.StateHint.Raidwide);

            P3FlareBreath(id + 0x1000, 9.2f)
                .DeactivateOnExit<P2Hypernova>(); // last voidzone disappears ~1.3ds
            P3FlareBreath(id + 0x1100, 2.1f);
            P3FlareBreath(id + 0x1200, 2.1f);
        }

        private void P3FellruinTrio(uint id, float delay)
        {
            ActorCast(id, _module.BahamutPrime, AID.FellruinTrio, delay, 4, true);
            ActorTargetable(id + 0x10, _module.BahamutPrime, false, 2, "Boss disappears (fellruin trio)")
                .SetHint(StateMachine.StateHint.DowntimeStart);

            ComponentCondition<Quote>(id + 0x20, 2.3f, comp => comp.PendingMechanics.Count > 0)
                .ActivateOnEnter<Quote>()
                .ActivateOnEnter<P3AethericProfusion>();
            ComponentCondition<Quote>(id + 0x30, 5.1f, comp => comp.PendingMechanics.Count == 2, "In/spread")
                .ActivateOnEnter<QuoteIronChariotLunarDynamo>()
                .ActivateOnEnter<QuoteRavenDive>();
            ComponentCondition<Quote>(id + 0x40, 3.1f, comp => comp.PendingMechanics.Count == 1, "Spread/in")
                .ActivateOnEnter<P3TempestWing>()
                .DeactivateOnExit<QuoteIronChariotLunarDynamo>()
                .DeactivateOnExit<QuoteRavenDive>();
            ComponentCondition<P3TempestWing>(id + 0x50, 2.9f, comp => comp.NumCasts > 0, "Tethers")
                .ExecOnEnter<P3AethericProfusion>(comp => comp.Active = true)
                .DeactivateOnExit<P3TempestWing>();
            ComponentCondition<P3AethericProfusion>(id + 0x60, 1.0f, comp => comp.NumCasts > 0, "Neurolinks")
                .DeactivateOnExit<P3AethericProfusion>();

            ActorTargetable(id + 0x100, _module.BahamutPrime, true, 2.2f, "Boss reappears")
                .ActivateOnEnter<QuoteMeteorStream>()
                .SetHint(StateMachine.StateHint.DowntimeEnd);
            ActorCastStart(id + 0x101, _module.BahamutPrime, AID.Gigaflare, 0.1f, true);
            ComponentCondition<Quote>(id + 0x102, 1.1f, comp => comp.PendingMechanics.Count == 0, "Spread")
                .DeactivateOnExit<QuoteMeteorStream>()
                .DeactivateOnExit<Quote>();
            ActorCastEnd(id + 0x103, _module.BahamutPrime, 4.9f, true, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);

            P3FlareBreath(id + 0x1000, 5.2f);
            P3Flatten(id + 0x2000, 5.2f);
            P3FlareBreath(id + 0x3000, 5.2f);
        }

        private void P3HeavensfallTrio(uint id, float delay)
        {
            ActorCast(id, _module.BahamutPrime, AID.HeavensfallTrio, delay, 4, true);
            ActorTargetable(id + 0x10, _module.BahamutPrime, false, 2, "Boss disappears (heavensfall trio)")
                .SetHint(StateMachine.StateHint.DowntimeStart);
            ComponentCondition<P3HeavensfallTrio>(id + 0x20, 1.2f, comp => comp.Active)
                .ActivateOnEnter<P3HeavensfallTrio>();

            ComponentCondition<P3MegaflareDive>(id + 0x30, 1.2f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<P3MegaflareDive>();
            ComponentCondition<P3MegaflareDive>(id + 0x40, 4, comp => comp.NumCasts > 0, "Dives")
                .ActivateOnEnter<P3TwistingDive>()
                .DeactivateOnExit<P3TwistingDive>()
                .DeactivateOnExit<P3MegaflareDive>()
                .DeactivateOnExit<P3HeavensfallTrio>();
            ComponentCondition<P3Twister>(id + 0x50, 1.4f, comp => comp.Active, "Twisters")
                .ActivateOnEnter<P3Twister>();

            ComponentCondition<P3HeavensfallTowers>(id + 0x60, 0.5f, comp => comp.Towers.Count > 0)
                .ActivateOnEnter<P3HeavensfallTowers>();
            ComponentCondition<P3MegaflarePuddle>(id + 0x61, 1.0f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<P3MegaflarePuddle>()
                .ActivateOnEnter<P2Heavensfall>();
            ComponentCondition<P3MegaflarePuddle>(id + 0x62, 3, comp => comp.NumCasts > 0)
                .DeactivateOnExit<P3MegaflarePuddle>();
            ComponentCondition<P2Heavensfall>(id + 0x63, 1.6f, comp => comp.NumCasts > 0, "Knockback")
                .DeactivateOnExit<P3Twister>() // twisters disappear ~0.5s before knockback
                .DeactivateOnExit<P2Heavensfall>();
            ComponentCondition<P3HeavensfallTowers>(id + 0x64, 2.4f, comp => comp.NumCasts > 0, "Towers")
                .DeactivateOnExit<P3HeavensfallTowers>();

            ComponentCondition<P2ThermionicBurst>(id + 0x70, 1.6f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<P2ThermionicBurst>();
            // +2.0s: second pair, then every 0.5s
            // TODO: pizzas resolve + hypernovas > fireball > gigaflare > 3x flare breath
        }
    }
}
