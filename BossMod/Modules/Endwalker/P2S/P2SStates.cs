namespace BossMod.Endwalker.P2S
{
    class P2SStates : StateMachineBuilder
    {
        public P2SStates(BossModule module) : base(module)
        {
            MurkyDepths(0x00000000, 10.2f);
            DoubledImpact(0x00010000, 5.2f);

            // deluge 1
            SewageDeluge(0x00100000, 7.8f);
            Cataract(0x00110000, 15);
            Coherence(0x00120000, 8.1f);
            MurkyDepths(0x00130000, 8.2f);
            OminousBubblingShockwave(0x00140000, 3.7f);
            AvariceCataract(0x00150000, 10.9f);
            // note: deluge 1 ends here...

            Flow1(0x00200000, 8.6f);
            DoubledImpact(0x00210000, 8.2f);
            MurkyDepths(0x00220000, 5.2f);

            // deluge 2
            SewageDeluge(0x00300000, 11.7f);
            Shockwave(0x00310000, 9.6f);
            KampeosHarma(0x00320000, 4.4f);
            DoubledImpact(0x00330000, 10);
            MurkyDepths(0x00340000, 4.2f);
            Flow2(0x00350000, 8.6f);
            Cataract(0x00360000, 0.7f);
            // note: deluge 2 ends here...

            AvariceDissociationCataract(0x00400000, 15.2f);
            DissociationEruptionFloodCoherence(0x00410000, 9.8f);
            DoubledImpact(0x00420000, 7.2f);
            MurkyDepths(0x00430000, 3.2f);

            // deluge 3
            SewageDeluge(0x00500000, 12.8f);
            Flow3(0x00510000, 11.7f);
            DissociationEruption(0x00520000, 6.9f);
            OminousBubblingShockwave(0x00530000, 0.8f);
            DoubledImpact(0x00540000, 5.4f);
            MurkyDepths(0x00550000, 7.2f);
            MurkyDepths(0x00560000, 6.2f);

            Cast(0x00600000, AID.Enrage, 5.3f, 10, "Enrage");
        }

        private void MurkyDepths(uint id, float delay)
        {
            Cast(id, AID.MurkyDepths, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void DoubledImpact(uint id, float delay)
        {
            Cast(id, AID.DoubledImpact, delay, 5, "Shared Tankbuster")
                .ActivateOnEnter<DoubledImpact>()
                .DeactivateOnExit<DoubledImpact>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void SewageDeluge(uint id, float delay)
        {
            Cast(id, AID.SewageDeluge, delay, 5, "Deluge")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private State Cataract(uint id, float delay)
        {
            CastStartMulti(id, new AID[] { AID.SpokenCataract, AID.WingedCataract }, delay)
                .SetHint(StateMachine.StateHint.PositioningStart);
            return CastEnd(id + 1, 8, "Cataract")
                .ActivateOnEnter<Cataract>()
                .DeactivateOnExit<Cataract>()
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }

        private State Coherence(uint id, float delay)
        {
            Cast(id, AID.Coherence, delay, 12)
                .ActivateOnEnter<Coherence>();
            return ComponentCondition<Coherence>(id + 2, 3.2f, comp => comp.NumCasts > 0, "Coherence")
                .DeactivateOnExit<Coherence>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void Shockwave(uint id, float delay)
        {
            // TODO: some component (knockback distance=16? or just make sure autorot uses arms length?)
            Cast(id, AID.Shockwave, delay, 8, "Shockwave")
                .SetHint(StateMachine.StateHint.Knockback);
        }

        // note: this activates component, which has to be deactivated later manually
        private void PredatoryAvarice(uint id, float delay)
        {
            Cast(id, AID.PredatoryAvarice, delay, 4, "Avarice")
                .ActivateOnEnter<PredatoryAvarice>();
        }

        // note: this activates component, which has to be deactivated later manually
        private void Dissociation(uint id, float delay)
        {
            Cast(id, AID.Dissociation, delay, 4, "Dissociation")
                .ActivateOnEnter<Dissociation>();
        }

        // note: this activates component, which has to be deactivated later manually
        private State TaintedFlood(uint id, float delay)
        {
            return Cast(id, AID.TaintedFlood, delay, 3, "Flood")
                .ActivateOnEnter<TaintedFlood>();
        }

        // note: this activates component, which has to be deactivated later manually
        private State SewageEruption(uint id, float delay)
        {
            return Cast(id, AID.SewageEruption, delay, 5, "Eruption")
                .ActivateOnEnter<SewageEruption>();
        }

        private void AvariceCataract(uint id, float delay)
        {
            PredatoryAvarice(id, delay);
            Cataract(id + 0x1000, 9.8f)
                .ClearHint(StateMachine.StateHint.PositioningEnd);
            ComponentCondition<PredatoryAvarice>(id + 0x2000, 6.2f, comp => !comp.Active, "Avarice resolve")
                .DeactivateOnExit<PredatoryAvarice>()
                .SetHint(StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.Raidwide);
        }

        private void AvariceDissociationCataract(uint id, float delay)
        {
            PredatoryAvarice(id, delay);
            Dissociation(id + 0x1000, 2.4f);
            // note: we don't create separate avarice/dissociation resolve states here, since they all resolve at almost the same time
            Cataract(id + 0x2000, 10.1f)
                .DeactivateOnExit<PredatoryAvarice>()
                .DeactivateOnExit<Dissociation>()
                .SetHint(StateMachine.StateHint.Raidwide); // avarice resolve
        }

        private void DissociationEruptionFloodCoherence(uint id, float delay)
        {
            Dissociation(id, delay);
            SewageEruption(id + 0x1000, 8.2f)
                .SetHint(StateMachine.StateHint.PositioningStart);
            TaintedFlood(id + 0x2000, 2.4f)
                .DeactivateOnExit<Dissociation>(); // resolve happens mid-cast
            Coherence(id + 0x3000, 4.7f)
                .DeactivateOnExit<SewageEruption>() // resolve happens right before cast start
                .DeactivateOnExit<TaintedFlood>() // resolve happens mid-cast
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }

        private void DissociationEruption(uint id, float delay)
        {
            Dissociation(id, delay);
            SewageEruption(id + 0x1000, 8.3f)
                .SetHint(StateMachine.StateHint.PositioningStart);
            ComponentCondition<SewageEruption>(id + 0x2000, 7.6f, comp => comp.Done, "Resolve")
                .DeactivateOnExit<Dissociation>() // resolve happens at the same time as the first set of eruptions
                .DeactivateOnExit<SewageEruption>()
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }

        private void Flow1(uint id, float delay)
        {
            Cast(id, AID.ChannelingFlow, delay, 5, "Flow 1")
                .ActivateOnEnter<ChannelingFlow>()
                .SetHint(StateMachine.StateHint.PositioningStart);
            ComponentCondition<ChannelingFlow>(id + 0x1000, 14, comp => comp.NumStunned > 0)
                .SetHint(StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.DowntimeStart);
            ComponentCondition<ChannelingFlow>(id + 0x2000, 3, comp => comp.NumStunned == 0, "Flow resolve")
                .DeactivateOnExit<ChannelingFlow>()
                .SetHint(StateMachine.StateHint.DowntimeEnd | StateMachine.StateHint.Raidwide);
        }

        private void Flow2(uint id, float delay)
        {
            // flow 2: same statuses as first flow, different durations
            Cast(id, AID.ChannelingOverflow, delay, 5, "Flow 2")
                .ActivateOnEnter<ChannelingFlow>()
                .SetHint(StateMachine.StateHint.PositioningStart);
            TaintedFlood(id + 0x1000, 4.2f);
            ComponentCondition<ChannelingFlow>(id + 0x2000, 6.8f, comp => comp.NumStunned > 0);
            ComponentCondition<TaintedFlood>(id + 0x2001, 0.3f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<TaintedFlood>();
            ComponentCondition<ChannelingFlow>(id + 0x2002, 2.7f, comp => comp.NumStunned == 0, "Hit 1")
                .SetHint(StateMachine.StateHint.Raidwide);
            TaintedFlood(id + 0x3000, 2.6f);
            ComponentCondition<ChannelingFlow>(id + 0x4000, 6.4f, comp => comp.NumStunned > 0);
            ComponentCondition<TaintedFlood>(id + 0x4001, 0.6f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<TaintedFlood>();
            ComponentCondition<ChannelingFlow>(id + 0x4002, 2.4f, comp => comp.NumStunned == 0, "Hit 2")
                .DeactivateOnExit<ChannelingFlow>()
                .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd);
        }

        private void Flow3(uint id, float delay)
        {
            // flow 3: same as flow 2, but with coherence instead of floods
            Cast(id, AID.ChannelingOverflow, delay, 5, "Flow 3")
                .ActivateOnEnter<ChannelingFlow>()
                .SetHint(StateMachine.StateHint.PositioningStart);
            Coherence(id + 0x1000, 5.5f); // first hit is around coherence cast end
            ComponentCondition<ChannelingFlow>(id + 0x2000, 8.3f, comp => comp.NumStunned > 0);
            ComponentCondition<ChannelingFlow>(id + 0x3000, 3, comp => comp.NumStunned == 0, "Flow resolve") // second hit
                .DeactivateOnExit<ChannelingFlow>()
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }

        private void OminousBubblingShockwave(uint id, float delay)
        {
            Cast(id, AID.OminousBubbling, delay, 3, "TwoStacks")
                .ActivateOnEnter<OminousBubbling>()
                .SetHint(StateMachine.StateHint.PositioningStart);
            Shockwave(id + 0x1000, 2.8f);
            ComponentCondition<OminousBubbling>(id + 0x2000, 3.8f, comp => comp.NumCasts > 0, "AOE resolve")
                .DeactivateOnExit<OminousBubbling>()
                .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd);
        }

        private void KampeosHarma(uint id, float delay)
        {
            CastStart(id, AID.KampeosHarma, delay)
                .ActivateOnEnter<KampeosHarma>() // note: icons appear right before harma cast start...
                .SetHint(StateMachine.StateHint.PositioningStart);
            CastEnd(id + 1, 8.5f, "Harma");
            Targetable(id + 2, false, 0); // usually happens together with cast end
            Targetable(id + 3, true, 7.4f, "Harma resolve")
                .DeactivateOnExit<KampeosHarma>()
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }
    }
}
